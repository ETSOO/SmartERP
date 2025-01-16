import { GridDataType, useParamsEx } from "@etsoo/react";
import { BusinessTax } from "@etsoo/appscript";
import { ButtonLink, HBox, IconButtonLink, ViewPage } from "@etsoo/materialui";
import { Typography } from "@mui/material";
import EditIcon from "@mui/icons-material/Edit";
import { app } from "../../../app/MyApp";
import { OrgReadDto, usePageData } from "@etsoo/smarterp-core";
import React from "react";

export default function ViewOrg() {
  // Route
  const { id = 0 } = useParamsEx({ id: "number" });

  // Load data
  const loadData = React.useCallback(() => {
    return app.core.orgApi.read(id);
  }, [id]);

  // Labels
  const labels = app.getLabels("edit", "editLogo", "logo", "view");

  // Tax
  const tax = BusinessTax.getById(app.region);

  // Page data hook
  usePageData(app, labels.view, [loadData]);

  return (
    <ViewPage<OrgReadDto>
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
              {item.isOwner && (
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
          data: (item) => (
            <HBox>
              <img
                src={item.logo}
                alt={labels.logo}
                style={{
                  width: "160px",
                  height: "80px",
                  border: "1px solid #666"
                }}
              />
              {item.isOwner && (
                <IconButtonLink
                  href={`./../../avatar/${item.id}`}
                  state={item.logo}
                  title={labels.editLogo}
                  size="small"
                >
                  <EditIcon />
                </IconButtonLink>
              )}
            </HBox>
          ),
          singleRow: false
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
    ></ViewPage>
  );
}
