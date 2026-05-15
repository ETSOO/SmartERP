import {
  ButtonLink,
  HBox,
  IconButtonLink,
  MenuButton,
  ViewContainer
} from "@etsoo/materialui";
import { GridDataType } from "@etsoo/react";
import {
  OrderViewData,
  Permissions,
  PromotionCodeCalculation
} from "@etsoo/smarterp-crm";
import { app } from "../../../app/MyApp";
import Badge from "@mui/material/Badge";
import IconButton from "@mui/material/IconButton";
import CelebrationIcon from "@mui/icons-material/Celebration";
import EditIcon from "@mui/icons-material/Edit";
import AddIcon from "@mui/icons-material/Add";
import CalculateIcon from "@mui/icons-material/Calculate";
import { EntityStatus } from "@etsoo/appscript";
import Button from "@mui/material/Button";
import Typography from "@mui/material/Typography";
import { OrderUIUtils } from "./OrderUIUtils";

export type OrderViewUIProps = {
  data: OrderViewData;
  refresh: () => Promise<void>;
};

export function OrderViewUI(props: OrderViewUIProps) {
  // Destruct
  const { data, refresh } = props;

  const labels = app.getLabels(
    "addOrderLine",
    "confirmAction",
    "edit",
    "promotions",
    "recalculate"
  );

  const moneyProps = { currency: data.currency };

  const formatAmount = (amount: number) =>
    app.formatMoney(amount, undefined, moneyProps);

  const editable = app.owns(Permissions.Order.Edit);

  return (
    <ViewContainer
      data={data}
      refresh={refresh}
      fields={[
        {
          data: (item) => (
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
              {editable && (
                <IconButtonLink
                  href={`./../../edit/${item.id}`}
                  title={labels.edit}
                  size="small"
                >
                  <EditIcon />
                </IconButtonLink>
              )}
            </HBox>
          ),
          singleRow: true
        },
        {
          data: (item) =>
            `${app.formatNumber(item.lines)} / ${app.formatNumber(item.items)}`,
          label: "orderLines"
        },
        {
          data: (item) =>
            item.lineDiscount === 0
              ? undefined
              : formatAmount(-item.lineDiscount),
          label: "orderLineDiscount"
        },
        {
          data: (item) => {
            if (item.discount === 0) return undefined;
            const promotions = item.promotions ?? [];

            const titleFormatter = (data: PromotionCodeCalculation) =>
              `${data.title} (${formatAmount(data.amount)})`;

            return (
              <HBox>
                <Typography variant="body2" sx={{ fontWeight: "bold" }}>
                  {formatAmount(-item.discount)}
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
        ["amount", GridDataType.Money, moneyProps],
        {
          data: (item) =>
            item.approvedDiscount === 0
              ? undefined
              : formatAmount(-item.approvedDiscount),
          label: "approvedDiscount"
        },
        {
          data: (item) =>
            item.taxAmount === 0 ? undefined : formatAmount(item.taxAmount),
          label: "taxAmount"
        },
        {
          data: "paidAmount",
          label: "amountPaid",
          renderProps: moneyProps,
          dataType: GridDataType.Money
        },
        {
          data: "source",
          label: "orderSource"
        },
        {
          data: "sourceId",
          label: "orderSourceId"
        },
        "assignedId",
        {
          data: (item) => (
            <ButtonLink
              href={`./../../../contact/view/${item.userId}`}
              size="small"
              variant="outlined"
            >
              {item.user}
            </ButtonLink>
          ),
          label: "user"
        },
        {
          data: "tags",
          singleRow: "medium",
          horizontal: true
        },
        {
          data: (item) => (
            <HBox
              spacing={1}
              sx={{ justifyContent: "center", flexWrap: "wrap" }}
            >
              {app.owns(Permissions.Order.Manage) &&
                item.status < EntityStatus.Inactivated && (
                  <Button
                    startIcon={<CalculateIcon />}
                    variant="outlined"
                    onClick={() => {
                      app.notifier.confirm(
                        labels.confirmAction.format(labels.recalculate),
                        undefined,
                        async (ok) => {
                          if (!ok) return;

                          const result = await app.orderApi.recalculate(
                            item.id
                          );
                          if (result == null) return;

                          if (result.ok) {
                            refresh();
                            return;
                          }

                          app.alertResult(result);
                        }
                      );
                    }}
                  >
                    {labels.recalculate}
                  </Button>
                )}
              {editable && (
                <ButtonLink
                  startIcon={<EditIcon />}
                  variant="outlined"
                  href={`./../../edit/${item.id}`}
                >
                  {labels.edit}
                </ButtonLink>
              )}
              {editable && (
                <Button
                  startIcon={<AddIcon />}
                  variant="outlined"
                  onClick={() =>
                    OrderUIUtils.addOrderLine(
                      {
                        customerId: item.customerId,
                        orderId: item.id,
                        currency: item.currency
                      },
                      refresh
                    )
                  }
                >
                  {labels.addOrderLine}
                </Button>
              )}
            </HBox>
          ),
          singleRow: true
        },
        {
          data: "description",
          singleRow: true,
          horizontal: true
        },
        {
          data: "addressFormatted",
          label: "deliveryAddress",
          singleRow: true
        },
        {
          data: "delivery",
          label: "orderDelivery",
          singleRow: "small"
        },
        {
          data: "deliveryInstruction",
          singleRow: "large"
        },
        {
          data: "payment",
          label: "orderPayment",
          singleRow: "small"
        },
        {
          data: "paymentInstruction",
          singleRow: "large"
        },
        {
          data: (item) => (
            <ButtonLink
              href={`./../../../contact/view/${item.customerId}`}
              size="small"
              variant="outlined"
              disabled={!app.owns(Permissions.Customer.View)}
            >
              {item.customerName}
            </ButtonLink>
          ),
          singleRow: "large",
          label: "customer"
        },
        ["startDate", GridDataType.DateTime],
        ["endDate", GridDataType.DateTime],
        {
          data: (item) => app.getStatusLabel(item.status),
          label: "status"
        },
        ["creation", GridDataType.DateTime]
      ]}
    />
  );
}
