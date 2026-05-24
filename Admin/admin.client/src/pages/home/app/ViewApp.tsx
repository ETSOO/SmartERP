import { GridDataType, useParamsEx } from "@etsoo/react";
import { HBox, LinkEx, ViewPage } from "@etsoo/materialui";
import HandymanIcon from "@mui/icons-material/Handyman";
import { app } from "../../../app/MyApp";
import { usePageDataEmpty } from "@etsoo/smarterp-core";
import React from "react";
import { ReadAppDto } from "../../../api/dto/query/ReadAppDto";
import { AppUtils } from "../../../components/AppUtils";
import Typography from "@mui/material/Typography";
import Stack from "@mui/material/Stack";
import Button from "@mui/material/Button";

export default function ViewApp() {
  // Route
  const { id = 0 } = useParamsEx({ id: "number" });

  // Load data
  const loadData = React.useCallback(() => {
    return app.queryApi.readApp(id);
  }, [id]);

  // Labels
  const labels = app.getLabels("adminRenew", "view");

  // Page data hook
  usePageDataEmpty(app);

  return (
    <ViewPage<ReadAppDto>
      paddings={0}
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
                align="center"
                sx={{ paddingRight: 2 }}
                title={item.name}
              >
                {item.name}
              </Typography>
            </HBox>
          ),
          singleRow: true
        },
        {
          data: (item) => (
            <LinkEx to={`./../../../org/view/${item.orgId}`} variant="body2">
              {item.orgName}
            </LinkEx>
          ),
          label: "org",
          singleRow: "medium"
        },
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
          data: (item) => `${item.id} (${item.appId})`,
          label: "id"
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
          <Button
            variant="contained"
            startIcon={<HandymanIcon />}
            onClick={() => AppUtils.renewApp(data, refresh)}
          >
            {labels.adminRenew}
          </Button>
        </React.Fragment>
      )}
    ></ViewPage>
  );
}
