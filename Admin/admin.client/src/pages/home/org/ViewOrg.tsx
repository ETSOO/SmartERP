import { GridDataType, useParamsEx } from "@etsoo/react";
import { BusinessTax } from "@etsoo/appscript";
import { ButtonLink, HBox, ViewPage } from "@etsoo/materialui";
import { Typography } from "@mui/material";
import { app } from "../../../app/MyApp";
import { usePageData } from "@etsoo/smarterp-core";
import React from "react";
import { useNavigate } from "react-router-dom";
import { ReadOrgData } from "../../../api/dto/query/ReadOrgDto";

export default function ViewOrg() {
  // Route
  const { id = 0 } = useParamsEx({ id: "number" });

  // Route
  const navigate = useNavigate();

  // Load data
  const loadData = React.useCallback(() => {
    return app.queryApi.readOrg(id);
  }, [id]);

  // Labels
  const labels = app.getLabels("view");

  // Tax
  const tax = BusinessTax.getById(app.region);

  // Page data hook
  usePageData(app, labels.view, [loadData]);

  return (
    <ViewPage<ReadOrgData>
      paddings={0}
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
          data: "brand",
          label: "brand",
          singleRow: false
        },
        {
          data: "pin",
          label: app.get(tax?.labelKey ?? "taxId"),
          singleRow: false
        },
        {
          data: (item) =>
            item.parentName ? (
              <ButtonLink
                href={`./../${item.parentId}`}
                size="small"
                variant="outlined"
              >
                {item.parentName}
              </ButtonLink>
            ) : undefined,
          label: "parentOrg"
        },
        {
          data: "ownerName",
          label: "owner"
        },
        {
          data: (item) => app.getStatusLabel(item.status),
          label: "status"
        },
        ["creation", GridDataType.DateTime]
      ]}
      loadData={loadData}
      actions={(data, _refresh) => <React.Fragment></React.Fragment>}
    ></ViewPage>
  );
}
