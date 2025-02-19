import {
  GridDataType,
  NotificationMessageType,
  useParamsEx
} from "@etsoo/react";
import {
  ButtonLink,
  HBox,
  IconButtonLink,
  TooltipClick,
  ViewPage
} from "@etsoo/materialui";
import { Button, Stack, Typography } from "@mui/material";
import EditIcon from "@mui/icons-material/Edit";
import KeyIcon from "@mui/icons-material/Key";
import ShoppingCartIcon from "@mui/icons-material/ShoppingCart";
import { app } from "../../../app/MyApp";
import { AppReadDto, usePageData } from "@etsoo/smarterp-core";
import React from "react";
import { EntityStatus } from "@etsoo/appscript";

export default function ViewApp() {
  // Route
  const { id = 0 } = useParamsEx({ id: "number" });

  // Permissions
  const editPermission = app.isAdminUser();

  // Load data
  const loadData = React.useCallback(() => {
    return app.core.appApi.read(id);
  }, [id]);

  // Labels
  const labels = app.getLabels(
    "appKey",
    "appKeyTip",
    "appSecret",
    "completeTip",
    "copy",
    "createApiKey",
    "edit",
    "renew",
    "view"
  );

  // Page data hook
  usePageData(app, labels.view, [loadData]);

  return (
    <ViewPage<AppReadDto>
      fields={[
        {
          data: (item) => (
            <HBox justifyContent="center" alignItems="center">
              <Typography
                variant="subtitle2"
                textAlign="center"
                paddingRight={2}
                title={item.name}
              >
                {app.core.getAppName(item)}
              </Typography>
              <IconButtonLink
                href={`./../../edit/${item.id}`}
                title={labels.edit}
                size="small"
              >
                <EditIcon />
              </IconButtonLink>
            </HBox>
          ),
          singleRow: true
        },
        { data: "name", label: "fullName", singleRow: true },
        { data: "appKey", label: "appKey", singleRow: true },
        {
          data: (item) => {
            const urls = (item.localUrls ?? item.urls).map((u) => u.web);
            return urls.length < 1 ? undefined : (
              <Stack spacing={1} direction="row">
                {urls.map((a) => (
                  <a href={a} key={a} target="_blank" rel="noreferrer">
                    {a}
                  </a>
                ))}
              </Stack>
            );
          },
          label: "appWebUrl",
          singleRow: "medium"
        },
        {
          data: (item) => {
            const urls = (item.localUrls ?? item.urls)
              .map((u) => u.help)
              .filter((u): u is string => u != null);
            return urls.length < 1 ? undefined : (
              <Stack spacing={1} direction="row">
                {urls.map((a) => (
                  <a href={a} key={a} target="_blank" rel="noreferrer">
                    {a}
                  </a>
                ))}
              </Stack>
            );
          },
          label: "appHelpUrl",
          singleRow: "medium"
        },
        {
          data: (item) => {
            const urls = (item.localUrls ?? item.urls).map((u) => u.api);
            return urls.length < 1 ? undefined : (
              <Stack spacing={1} direction="row">
                {urls.map((a) => (
                  <a href={a} key={a} target="_blank" rel="noreferrer">
                    {a}
                  </a>
                ))}
              </Stack>
            );
          },
          label: "appApiUrl",
          singleRow: true
        },
        ["expiry", GridDataType.DateTime],
        { data: "expiryDays", label: "days" },
        {
          data: (item) => app.core.getIdentityLabel(item.identityType),
          label: "identityType"
        },
        {
          data: (item) => app.getStatusLabel(item.status),
          label: "status"
        },
        ["creation", GridDataType.DateTime]
      ]}
      loadData={loadData}
      actions={(data, refresh) => (
        <React.Fragment>
          <Button variant="outlined" startIcon={<ShoppingCartIcon />}>
            {labels.renew}
          </Button>
          {editPermission && (
            <ButtonLink
              variant="outlined"
              startIcon={<EditIcon />}
              href={`./../../edit/${data.id}`}
            >
              {labels.edit}
            </ButtonLink>
          )}
          {editPermission && data.status == EntityStatus.Normal && (
            <Button
              variant="outlined"
              startIcon={<KeyIcon />}
              onClick={() => {
                app.notifier.confirm(
                  labels.appKeyTip,
                  `${data.localName ?? app.get(`app${data.appId}`)} (${
                    data.name
                  })`,
                  async (ok) => {
                    if (!ok) return;
                    const result = await app.core.appApi.createApiKey(
                      { id: data.id, deviceId: app.deviceId },
                      {
                        showLoading: false
                      }
                    );
                    if (result == null) return;
                    if (!result.ok || result.data == null) {
                      app.alertResult(result);
                      return;
                    }

                    const key = result.data;
                    const appSecret = app.decrypt(key.appSecret);
                    if (appSecret == null) {
                      app.warning("Failed to decrypt the app secret.");
                      return;
                    }

                    app.notifier
                      .alert(
                        <React.Fragment>
                          <Typography component="span">
                            {labels.appKey}: <b>{key.appKey}</b>,{" "}
                            {labels.appSecret}:{" "}
                          </Typography>
                          <TooltipClick
                            title={labels.completeTip.format(labels.copy)}
                          >
                            {(openTooltip) => (
                              <Button
                                variant="outlined"
                                size="small"
                                onClick={() => {
                                  navigator.clipboard?.writeText(appSecret);
                                  openTooltip();
                                }}
                              >
                                {labels.copy}
                              </Button>
                            )}
                          </TooltipClick>
                        </React.Fragment>,
                        undefined,
                        NotificationMessageType.Success
                      )
                      .dismiss(180);

                    refresh();
                  }
                );
              }}
            >
              {labels.createApiKey}
            </Button>
          )}
        </React.Fragment>
      )}
    ></ViewPage>
  );
}
