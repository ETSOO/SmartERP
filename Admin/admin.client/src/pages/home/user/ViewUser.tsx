import { GridDataType, useParamsEx } from "@etsoo/react";
import { HBox, ViewPage } from "@etsoo/materialui";
import { Typography } from "@mui/material";
import { app } from "../../../app/MyApp";
import { usePageData } from "@etsoo/smarterp-core";
import React from "react";
import { ReadUserDto } from "../../../api/dto/query/ReadUserDto";

export default function ViewUser() {
  // Route
  const { id = 0 } = useParamsEx({ id: "number" });

  // Load data
  const loadData = React.useCallback(() => {
    return app.queryApi.readUser(id);
  }, [id]);

  // Labels
  const labels = app.getLabels("logo", "view");

  // Page data hook
  usePageData(app, labels.view, [loadData]);

  return (
    <ViewPage<ReadUserDto>
      fields={[
        {
          data: (item) => (
            <HBox justifyContent="center" alignItems="center">
              <Typography
                variant="subtitle2"
                textAlign="center"
                paddingRight={2}
              >
                {item.name}
              </Typography>
            </HBox>
          ),
          singleRow: true
        },
        {
          data: (item) => (
            <HBox>
              <img
                src={item.avatar}
                alt={labels.logo}
                style={{
                  width: "160px",
                  height: "160px",
                  border: "1px solid #666"
                }}
              />
            </HBox>
          ),
          singleRow: false
        },
        ["creation", GridDataType.DateTime]
      ]}
      loadData={loadData}
    ></ViewPage>
  );
}
