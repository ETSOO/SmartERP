import { GridDataType, useParamsEx } from "@etsoo/react";
import {
  ButtonLink,
  CustomFieldViewUI,
  HBox,
  IconButtonLink,
  InputField,
  MenuButton,
  MoneyInputField,
  OptionBool,
  VBox,
  ViewPage
} from "@etsoo/materialui";
import { app } from "../../../app/MyApp";
import { usePageDataEmpty } from "@etsoo/smarterp-core";
import React from "react";
import Typography from "@mui/material/Typography";
import {
  OrderLineViewData,
  Permissions,
  PromotionCodeCalculation
} from "@etsoo/smarterp-crm";
import EditIcon from "@mui/icons-material/Edit";
import StartIcon from "@mui/icons-material/Start";
import DoneAllIcon from "@mui/icons-material/DoneAll";
import CelebrationIcon from "@mui/icons-material/Celebration";
import RestoreIcon from "@mui/icons-material/Restore";
import Button from "@mui/material/Button";
import { DataTypes, DateUtils, DomUtils } from "@etsoo/shared";
import { UserTiplist } from "@etsoo/smarterp-core/components";
import IconButton from "@mui/material/IconButton";
import Badge from "@mui/material/Badge";
import { AssetList, SupplierList } from "@etsoo/smarterp-crm/components";
import { EntityStatus } from "@etsoo/appscript";

function CompleteUI({
  data,
  requiresAsset
}: {
  data: OrderLineViewData;
  requiresAsset: boolean;
}) {
  // labels
  const labels = app.getLabels(
    "add",
    "asset",
    "costPrice",
    "description",
    "expiry",
    "sn",
    "supplier"
  );

  const addAsset = async (data: OrderLineViewData) => {
    app.showInputDialog({
      title: labels.asset,
      message: "",
      callback: async (form) => {
        // Cancelled
        if (form == null) {
          return;
        }

        // Validate form
        if (!form.reportValidity()) {
          return false;
        }

        // Form data
        const { sn, expiry, description } = DomUtils.dataAs(
          new FormData(form),
          {
            sn: "string",
            expiry: "date",
            description: "string"
          }
        );

        if (!sn || !expiry) {
          return false;
        }

        const result = await app.assetApi.create({
          personId: data.customerId,
          productId: data.productId,
          sn,
          expiry,
          description
        });

        if (result == null) return;

        if (!result.ok) {
          return app.formatResult(result);
        }
      },
      inputs: (
        <VBox spacing={2} sx={{ paddingTop: 1 }}>
          <InputField
            fullWidth
            required
            name="sn"
            slotProps={{ htmlInput: { maxLength: 256 } }}
            label={labels.sn}
          />
          <InputField
            fullWidth
            required
            name="expiry"
            type="date"
            defaultValue={DateUtils.formatForInput(new Date())}
            label={labels.expiry}
          />

          <InputField
            fullWidth
            name="description"
            slotProps={{
              htmlInput: { maxLength: 1280 }
            }}
            label={labels.description}
            multiline
            rows={2}
          />
        </VBox>
      )
    });
  };

  const [supplierId, setSupplierId] = React.useState<number>();

  return (
    <VBox spacing={2} sx={{ paddingTop: 1 }}>
      {requiresAsset && (
        <HBox spacing={1}>
          <AssetList
            fullWidth
            inputRequired
            rq={{ personId: data.customerId, productId: data.productId }}
          />
          <Button onClick={() => addAsset(data)} variant="outlined">
            {labels.add}
          </Button>
        </HBox>
      )}
      <SupplierList
        fullWidth
        onValueChange={(item) => setSupplierId(item?.id)}
        rq={{ productId: data.productId }}
      />
      {supplierId && (
        <MoneyInputField
          fullWidth
          required
          name="costPrice"
          label={labels.costPrice}
        />
      )}
    </VBox>
  );
}

export default function ViewOrderLine() {
  // Route
  const { id = 0 } = useParamsEx({ id: "number" });

  // Labels
  const labels = app.getLabels(
    "confirmAction",
    "completeExecution",
    "edit",
    "promotions",
    "restart",
    "restore",
    "startExecution",
    "supplier"
  );

  // Load data
  const loadData = React.useCallback(
    async () => app.orderLineApi.read(id),
    [id]
  );

  const editable = app.owns(Permissions.Order.Edit);

  const start = async (
    data: OrderLineViewData,
    refresh: () => PromiseLike<void>
  ) => {
    app.showInputDialog({
      title: labels.startExecution,
      message: "",
      callback: async (form) => {
        // Cancelled
        if (form == null) {
          return;
        }

        // Validate form
        if (!form.reportValidity()) {
          return false;
        }

        // Form data
        const { userId, initStart } = DomUtils.dataAs(new FormData(form), {
          userId: "number",
          initStart: "boolean"
        });

        const result = await app.orderLineApi.start({
          id: data.id,
          userId,
          initStart
        });

        if (result == null) return;

        if (result.ok) {
          await refresh();
          return;
        }

        return app.formatResult(result);
      },
      inputs: (
        <VBox spacing={2} sx={{ paddingTop: 1 }}>
          <UserTiplist name="userId" fullWidth />
          <OptionBool fullWidth name="initStart" label={labels.restart} />
        </VBox>
      )
    });
  };

  const complete = async (
    data: OrderLineViewData,
    refresh: () => PromiseLike<void>
  ) => {
    // Required asset
    const requiresAsset = data.assetQty > 0 && data.assetId == null;

    // Show dialog
    app.showInputDialog({
      title: labels.completeExecution,
      message: "",
      callback: async (form) => {
        // Cancelled
        if (form == null) {
          return;
        }

        // Validate form
        if (!form.reportValidity()) {
          return false;
        }

        // Form data
        const { assetId, supplierId, costPrice } = DomUtils.dataAs(
          new FormData(form),
          {
            assetId: "number",
            supplierId: "number",
            costPrice: "number"
          }
        );

        if (requiresAsset && assetId == null) {
          DomUtils.setFocus("assetIdInput", form);
          return false;
        }

        if (supplierId != null && costPrice == null) {
          DomUtils.setFocus("costPrice", form);
          return false;
        }

        const result = await app.orderLineApi.complete({
          id: data.id,
          assetId,
          supplierId,
          costPrice
        });

        if (result == null) return;

        if (result.ok) {
          await refresh();
          return;
        }

        return app.formatResult(result);
      },
      inputs: <CompleteUI data={data} requiresAsset={requiresAsset} />
    });
  };

  const restore = async (
    data: OrderLineViewData,
    refresh: () => PromiseLike<void>
  ) => {
    app.notifier.confirm(
      labels.confirmAction.format(labels.restore),
      undefined,
      async (confirmed) => {
        if (!confirmed) return;

        const result = await app.orderLineApi.rollback(data.id);
        if (result == null) return;

        if (result.ok) {
          await refresh();
          return;
        }

        return app.formatResult(result);
      }
    );
  };

  // Page data hook
  usePageDataEmpty(app);

  return (
    <ViewPage<OrderLineViewData>
      paddings={0}
      titleBar={(item) => (
        <HBox
          sx={{
            justifyContent: "center",
            alignItems: "center",
            marginBottom: 2
          }}
        >
          <Typography
            variant="subtitle2"
            sx={{ textAlign: "center", paddingRight: 2 }}
          >
            {item.title}
          </Typography>
          {editable && item.orderStatus < EntityStatus.Inactivated && (
            <IconButtonLink
              href={`./../../editline/${item.id}`}
              title={labels.edit}
              size="small"
            >
              <EditIcon />
            </IconButtonLink>
          )}
        </HBox>
      )}
      fields={[
        {
          data: (item) => (
            <ButtonLink
              href={`./../../../order/view/${item.orderId}`}
              size="small"
              variant="outlined"
              disabled={!app.owns(Permissions.Order.View)}
            >
              {item.orderTitle}
            </ButtonLink>
          ),
          singleRow: "large",
          label: "order"
        },
        {
          data: (item) => (
            <ButtonLink
              href={`./../../../product/view/${item.productId}`}
              size="small"
              variant="outlined"
              disabled={!app.owns(Permissions.Product.View)}
            >
              {item.productName}
            </ButtonLink>
          ),
          singleRow: "large",
          label: "product"
        },
        {
          data: "currency"
        },
        ["price", GridDataType.Money],
        ["qty", GridDataType.Number],
        {
          data: (item) =>
            item.qtyDelivered == null
              ? undefined
              : `${app.formatNumber(item.qtyDelivered)} (${((item.qtyDelivered * 100.0) / item.qty).toExact(1)}%)`,
          label: "qtyDelivered"
        },
        {
          data: (item) => {
            if (item.discount === 0) return undefined;
            const promotions = item.promotions ?? [];

            const titleFormatter = (data: PromotionCodeCalculation) =>
              `${data.title} (${app.formatNumber(data.amount)})`;

            return (
              <HBox>
                <Typography variant="body2" sx={{ fontWeight: "bold" }}>
                  {app.formatMoney(-item.discount, undefined, {
                    currency: item.currency
                  })}
                </Typography>
                <MenuButton<PromotionCodeCalculation>
                  items={promotions}
                  labelField={titleFormatter}
                  button={(clickHandler) => {
                    return (
                      <IconButton
                        onClick={clickHandler}
                        size="small"
                        title={[
                          labels.promotions,
                          ...promotions.map(titleFormatter)
                        ].join("\n")}
                      >
                        <Badge
                          badgeContent={promotions.length}
                          color="secondary"
                        >
                          <CelebrationIcon color="action" fontSize="small" />
                        </Badge>
                      </IconButton>
                    );
                  }}
                />
              </HBox>
            );
          },
          label: "discount"
        },
        ["amount", GridDataType.Money],
        ["originalPrice", GridDataType.Money],
        ["costPrice", GridDataType.Money],
        ["assetQty", GridDataType.Number],
        {
          data: (item) =>
            item.assetId ? (
              <ButtonLink
                href={`./../../../customer/asset/view/${item.assetId}`}
                size="small"
                variant="outlined"
              >
                {item.assetSn}
              </ButtonLink>
            ) : undefined,
          label: "asset",
          singleRow: "large"
        },
        {
          data: (item) =>
            item.supplierId ? (
              <ButtonLink
                href={`./../../../contact/view/${item.supplierId}`}
                size="small"
                variant="outlined"
              >
                {item.supplierName}
              </ButtonLink>
            ) : undefined,
          label: "supplier",
          singleRow: "large"
        },
        {
          data: "description",
          singleRow: true
        },
        {
          data: (item) => app.getStatusLabel(item.status),
          label: "status"
        },
        {
          data: "startTime",
          label: "orderLineStartTime",
          dataType: GridDataType.DateTime
        },
        ["endTime", GridDataType.DateTime],
        ["creation", GridDataType.DateTime],
        {
          data: (item) =>
            item.userId ? (
              <ButtonLink
                href={`./../../../person/view/${item.userId}`}
                size="small"
                variant="outlined"
                disabled={!app.owns(Permissions.User.View)}
              >
                {item.userName}
              </ButtonLink>
            ) : undefined,
          label: "user"
        }
      ]}
      loadData={loadData}
      actions={(data, refresh) => (
        <React.Fragment>
          {editable && data.orderStatus < EntityStatus.Inactivated && (
            <ButtonLink
              startIcon={<EditIcon />}
              variant="outlined"
              href={`./../../editline/${data.id}`}
            >
              {labels.edit}
            </ButtonLink>
          )}
          {data.isRestorable && (
            <Button
              startIcon={<RestoreIcon />}
              variant="outlined"
              onClick={() => restore(data, refresh)}
            >
              {labels.restore}
            </Button>
          )}
          {data.isStartable && (
            <Button
              startIcon={<StartIcon />}
              variant="outlined"
              onClick={() => start(data, refresh)}
            >
              {labels.startExecution}
            </Button>
          )}
          {data.isCompletable && (
            <Button
              startIcon={<DoneAllIcon />}
              variant="outlined"
              onClick={() => complete(data, refresh)}
            >
              {labels.completeExecution}
            </Button>
          )}
        </React.Fragment>
      )}
    >
      {(item) => (
        <React.Fragment>
          {item.modifiers != null && item.modifiers.length > 0 && (
            <CustomFieldViewUI
              fields={item.modifiers}
              data={item.data?.modifiers as DataTypes.StringRecord}
              refresh={async () => {
                loadData();
              }}
            />
          )}
        </React.Fragment>
      )}
    </ViewPage>
  );
}
