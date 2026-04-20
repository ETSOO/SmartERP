import { AssetViewData, Permissions } from "@etsoo/smarterp-crm";
import { app } from "../../../app/MyApp";
import {
  ButtonLink,
  HBox,
  TooltipClick,
  ViewContainer
} from "@etsoo/materialui";
import Typography from "@mui/material/Typography";
import { GridDataType, NotificationMessageType } from "@etsoo/react";
import EditIcon from "@mui/icons-material/Edit";
import KeyIcon from "@mui/icons-material/Key";
import Button from "@mui/material/Button";
import React from "react";

export type AssetViewUIProps = {
  data: AssetViewData;
  refresh: () => Promise<void>;
};

export function AssetViewUI(props: AssetViewUIProps) {
  // Destruct
  const { data, refresh } = props;

  const labels = app.getLabels(
    "completeTip",
    "copy",
    "edit",
    "sensitiveData",
    "view"
  );

  const defaultCurrency = app.system.getDefaultCurrency();

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
                {item.sn} - {item.productName}
              </Typography>
              <ButtonLink
                startIcon={<EditIcon />}
                variant="outlined"
                href={`./../../edit/${item.id}`}
              >
                {labels.edit}
              </ButtonLink>
            </HBox>
          ),
          singleRow: true
        },
        {
          data: (item) => (
            <ButtonLink
              href={`./../../../../contact/view/${item.personId}`}
              size="small"
              variant="outlined"
            >
              {item.personName}
            </ButtonLink>
          ),
          label: "relatedTarget",
          singleRow: "large"
        },
        {
          data: (item) => (
            <ButtonLink
              href={`./../../../../product/view/${item.productId}`}
              size="small"
              variant="outlined"
            >
              {item.productName}
            </ButtonLink>
          ),
          label: "product",
          singleRow: "large"
        },
        [
          "amount",
          GridDataType.Money,
          app.getMoneyFormatProps(defaultCurrency)
        ],
        ["times", GridDataType.Number],
        ["expiry", GridDataType.DateTime],
        {
          data: (item) =>
            item.sensitiveData ? (
              <Button
                variant="outlined"
                startIcon={<KeyIcon />}
                disabled={!app.owns(Permissions.Org.Manage)}
                onClick={async () => {
                  const result = await app.assetApi.readSensitiveData(item.id);
                  if (result == null) return;

                  const data = app.decrypt(result, item.id.toString());
                  if (data == null) {
                    app.warning("Failed to decrypt the app secret.");
                    return;
                  }

                  app.notifier
                    .alert(
                      <Typography>
                        {labels.sensitiveData}: {data}
                      </Typography>,
                      undefined,
                      NotificationMessageType.Success
                    )
                    .dismiss(180);
                }}
              >
                {labels.view}
              </Button>
            ) : undefined,
          label: "sensitiveData"
        },
        {
          data: "description",
          singleRow: true,
          horizontal: true
        },
        {
          data: "healthCheckUrl",
          singleRow: true,
          horizontal: true
        },
        {
          data: (item) =>
            item.supplierId ? (
              <ButtonLink
                href={`./../../../../contact/view/${item.supplierId}`}
                size="small"
                variant="outlined"
              >
                {item.supplierName}
              </ButtonLink>
            ) : undefined,
          label: "supplier",
          singleRow: "large"
        },
        ["healthCheckSchedule", GridDataType.DateTime],
        {
          data: (item) => app.getStatusLabel(item.status),
          label: "status"
        },
        ["creation", GridDataType.DateTime]
      ]}
    />
  );
}
