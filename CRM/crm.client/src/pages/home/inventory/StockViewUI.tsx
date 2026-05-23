import {
  HBox,
  InputField,
  LinkEx,
  VBox,
  ViewContainer,
  ViewPageFieldType
} from "@etsoo/materialui";
import { GridDataType } from "@etsoo/react";
import {
  Permissions,
  StockKind,
  StockUpdateRQ,
  StockViewData
} from "@etsoo/smarterp-crm";
import { app } from "../../../app/MyApp";
import DeleteIcon from "@mui/icons-material/Delete";
import EditIcon from "@mui/icons-material/Edit";
import ThumbUpIcon from "@mui/icons-material/ThumbUp";
import Typography from "@mui/material/Typography";
import Button from "@mui/material/Button";
import IconButton from "@mui/material/IconButton";
import { DomUtils, Utils } from "@etsoo/shared";
import { useNavigate } from "react-router-dom";

export function StockViewUI({
  data,
  refresh
}: {
  data: StockViewData;
  refresh: () => Promise<void>;
}) {
  // Route
  const navigate = useNavigate();

  // Labels
  const labels = app.getLabels(
    "confirmAction",
    "delete",
    "deleteConfirm1",
    "description",
    "edit",
    "noChanges",
    "order",
    "po",
    "receiving",
    "title",
    "trackingNumber"
  );

  const editable = app.owns(Permissions.Inventory.Edit);
  const hasManage = app.owns(Permissions.Inventory.Manage);

  function doEdit() {
    // Show
    app.showInputDialog({
      title: labels.edit,
      message: "",
      callback: async (form) => {
        // Cancelled
        if (form == null) return;

        if (!form.reportValidity()) {
          return false;
        }

        // Form data
        const rq: StockUpdateRQ = {
          id: data.id,
          ...DomUtils.dataAs(new FormData(form), {
            title: "string",
            description: "string",
            trackingNumber: "string"
          })
        };

        if (rq.title == null) {
          return;
        }

        // Changed fields
        const fields = Utils.getDataChanges(rq, data);
        if (fields.length === 0) {
          return labels.noChanges;
        }
        rq.changedFields = fields;

        const result = await app.stockApi.update(rq);
        if (result == null) return;

        if (result.ok) {
          refresh();
          return true;
        } else {
          return app.formatResult(result);
        }
      },
      inputs: (
        <VBox spacing={2} sx={{ marginTop: 1 }}>
          <InputField
            name="title"
            label={labels.title}
            fullWidth
            required
            defaultValue={data.title}
            slotProps={{ htmlInput: { maxLength: 128 } }}
          />
          <InputField
            name="description"
            label={labels.description}
            multiline
            rows={2}
            fullWidth
            defaultValue={data.description ?? ""}
            slotProps={{ htmlInput: { maxLength: 1280 } }}
          />
          <InputField
            name="trackingNumber"
            label={labels.trackingNumber}
            fullWidth
            defaultValue={data.trackingNumber ?? ""}
            slotProps={{ htmlInput: { maxLength: 20 } }}
          />
        </VBox>
      )
    });
  }

  function doDelete() {
    app.notifier.confirm(
      labels.deleteConfirm1.format(data.title),
      undefined,
      async (ok) => {
        if (!ok) return;

        const result = await app.stockApi.delete(data.id);
        if (result == null) return;

        if (result.ok) {
          navigate("./../..");
          return;
        }

        app.alertResult(result);
      }
    );
  }

  function doReceive() {
    app.notifier.prompt(
      labels.trackingNumber,
      async (trackingNumber) => {
        if (trackingNumber == null) return;

        if (trackingNumber === "" || trackingNumber == data.trackingNumber) {
          trackingNumber = undefined;
        }

        const result = await app.stockApi.receive(
          { id: data.id, trackingNumber },
          { showLoading: false }
        );

        if (result == null) return false;

        if (result.ok) {
          refresh();
        } else {
          return app.formatResult(result);
        }
      },
      labels.receiving,
      {
        inputProps: {
          type: "input",
          defaultValue: data.trackingNumber,
          required: false, // default is true
          slotProps: {
            htmlInput: { maxLength: 20 }
          }
        }
      }
    );
  }

  const orders =
    data.orders == null
      ? []
      : data.orders.map<ViewPageFieldType<StockViewData>>((o, index) => {
          return {
            data: () =>
              app.owns(
                data.kind === StockKind.PO
                  ? Permissions.PO.View
                  : Permissions.Order.View
              ) ? (
                <LinkEx
                  variant="body2"
                  to={`./../../../${data.kind === StockKind.PO ? "po" : "order"}/view/${o.id}`}
                >
                  {o.label}
                </LinkEx>
              ) : (
                o.label
              ),
            label: `${data.kind === StockKind.PO ? labels.po : labels.order} ${index + 1}`
          };
        });

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
                <IconButton onClick={doEdit} title={labels.edit} size="small">
                  <EditIcon />
                </IconButton>
              )}
            </HBox>
          ),
          singleRow: true
        },
        {
          data: (item) => app.stock.getKind(item.kind),
          label: "type"
        },
        {
          data: "locationFrom",
          label: "shippingWarehouse"
        },
        {
          data: "locationTo",
          label: "receivingWarehouse"
        },
        "trackingNumber",
        {
          data: (item) =>
            `${app.formatNumber(item.totalLines)} / ${app.formatNumber(item.totalQty)}`,
          label: "orderLines"
        },
        ...orders,
        {
          data: (item) => (
            <LinkEx
              variant="body2"
              to={`./../../../contact/view/${item.personId}`}
            >
              {item.personName}
            </LinkEx>
          ),
          label: "relatedTarget"
        },
        {
          data: (item) => (
            <HBox
              spacing={1}
              sx={{ justifyContent: "center", flexWrap: "wrap" }}
            >
              {item.isDeletable && (
                <Button
                  startIcon={<DeleteIcon />}
                  variant="outlined"
                  onClick={doDelete}
                >
                  {labels.delete}
                </Button>
              )}
              {hasManage && data.receiptTime == null && (
                <Button
                  startIcon={<ThumbUpIcon />}
                  variant="outlined"
                  onClick={doReceive}
                >
                  {labels.receiving}
                </Button>
              )}
              {editable && (
                <Button
                  startIcon={<EditIcon />}
                  variant="outlined"
                  onClick={doEdit}
                >
                  {labels.edit}
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
          data: (item) =>
            app.owns(Permissions.User.View) ? (
              <LinkEx
                variant="body2"
                to={`./../../../contact/view/${item.userId}`}
              >
                {item.userName}
              </LinkEx>
            ) : (
              item.userName
            ),
          label: "user"
        },
        ["receiptTime", GridDataType.DateTime],
        ["creation", GridDataType.DateTime]
      ]}
    />
  );
}
