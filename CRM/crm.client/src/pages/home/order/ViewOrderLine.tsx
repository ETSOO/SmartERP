import { GridDataType, useParamsEx } from "@etsoo/react";
import { ButtonLink, HBox, IconButtonLink, ViewPage } from "@etsoo/materialui";
import { app } from "../../../app/MyApp";
import { usePageDataEmpty } from "@etsoo/smarterp-core";
import React from "react";
import Typography from "@mui/material/Typography";
import { OrderLineViewData, Permissions } from "@etsoo/smarterp-crm";
import EditIcon from "@mui/icons-material/Edit";
import StartIcon from "@mui/icons-material/Start";
import DoneAllIcon from "@mui/icons-material/DoneAll";
import Button from "@mui/material/Button";

export default function ViewOrderLine() {
  // Route
  const { id = 0 } = useParamsEx({ id: "number" });

  // Labels
  const labels = app.getLabels("completeExecution", "edit", "startExecution");

  // Load data
  const loadData = React.useCallback(
    async () => app.orderLineApi.read(id),
    [id]
  );

  const editable = app.owns(Permissions.Order.Edit);

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
          {editable && (
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
        ["price", GridDataType.Money],
        ["qty", GridDataType.Number],
        ["discount", GridDataType.Money],
        ["amount", GridDataType.Money],
        ["originalPrice", GridDataType.Money],
        ["costPrice", GridDataType.Money],
        ["assetQty", GridDataType.Number],
        {
          data: "description",
          singleRow: true
        },
        {
          data: (item) => app.getStatusLabel(item.status),
          label: "status"
        },
        ["startTime", GridDataType.DateTime],
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
          {editable && (
            <ButtonLink
              startIcon={<EditIcon />}
              variant="outlined"
              href={`./../../editline/${data.id}`}
            >
              {labels.edit}
            </ButtonLink>
          )}
          <Button startIcon={<StartIcon />} variant="outlined">
            {labels.startExecution}
          </Button>
          <Button startIcon={<DoneAllIcon />} variant="outlined">
            {labels.completeExecution}
          </Button>
        </React.Fragment>
      )}
    ></ViewPage>
  );
}
