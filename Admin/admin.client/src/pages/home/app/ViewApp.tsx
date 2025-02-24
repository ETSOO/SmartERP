import { GridDataType, useParamsEx } from "@etsoo/react";
import { HBox, ViewPage } from "@etsoo/materialui";
import { Stack, Typography } from "@mui/material";
import { app } from "../../../app/MyApp";
import { usePageData } from "@etsoo/smarterp-core";
import React from "react";
import { ReadAppDto } from "../../../api/dto/query/ReadAppDto";

export default function ViewApp() {
  // Route
  const { id = 0 } = useParamsEx({ id: "number" });

  // Load data
  const loadData = React.useCallback(() => {
    return app.queryApi.readApp(id);
  }, [id]);

  // Labels
  const labels = app.getLabels("view");

  // Page data hook
  usePageData(app, labels.view, [loadData]);

  return (
    <ViewPage<ReadAppDto>
      paddings={0}
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
                {item.name}
              </Typography>
            </HBox>
          ),
          singleRow: true
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
          data: (item) => app.getStatusLabel(item.status),
          label: "status"
        },
        ["creation", GridDataType.DateTime]
      ]}
      loadData={loadData}
      actions={(data, refresh) => <React.Fragment></React.Fragment>}
    ></ViewPage>
  );
}
