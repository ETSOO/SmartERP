import {
  ButtonLink,
  HBox,
  IconButtonLink,
  ViewContainer
} from "@etsoo/materialui";
import { GridDataType } from "@etsoo/react";
import { Permissions, StockViewData } from "@etsoo/smarterp-crm";
import { app } from "../../../app/MyApp";
import DeleteIcon from "@mui/icons-material/Delete";
import EditIcon from "@mui/icons-material/Edit";
import Typography from "@mui/material/Typography";
import Button from "@mui/material/Button";
import IconButton from "@mui/material/IconButton";

export function StockViewUI({
  data,
  refresh
}: {
  data: StockViewData;
  refresh: () => Promise<void>;
}) {
  // Labels
  const labels = app.getLabels("confirmAction", "delete", "edit");

  const editable = app.owns(Permissions.Inventory.Edit);

  function doEdit() {}

  function doDelete() {}

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
        {
          data: (item) => (
            <ButtonLink
              href={`./../../../contact/view/${item.personId}`}
              size="small"
              variant="outlined"
            >
              {item.personName}
            </ButtonLink>
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
          data: (item) => (
            <ButtonLink
              href={`./../../../contact/view/${item.userId}`}
              size="small"
              variant="outlined"
              disabled={!app.owns(Permissions.User.View)}
            >
              {item.userName}
            </ButtonLink>
          ),
          label: "user"
        },
        ["receiptTime", GridDataType.DateTime],
        ["creation", GridDataType.DateTime]
      ]}
    />
  );
}
