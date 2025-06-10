import { GridDataType, useParamsEx } from "@etsoo/react";
import { BusinessTax } from "@etsoo/appscript";
import { ButtonLink, HBox, ViewPage } from "@etsoo/materialui";
import SupportIcon from "@mui/icons-material/Support";
import { app } from "../../../app/MyApp";
import { CoreUtils, usePageDataEmpty } from "@etsoo/smarterp-core";
import React from "react";
import { ReadOrgData } from "../../../api/dto/query/ReadOrgDto";
import { AppUtils } from "../../../components/AppUtils";
import Button from "@mui/material/Button";
import Typography from "@mui/material/Typography";

export default function ViewOrg() {
  // Route
  const { id = 0 } = useParamsEx({ id: "number" });

  // Load data
  const loadData = React.useCallback(() => {
    return app.queryApi.readOrg(id);
  }, [id]);

  // Labels
  const labels = app.getLabels("adminSupport", "logo");

  // Tax
  const tax = BusinessTax.getById(app.region);

  // Page data hook
  usePageDataEmpty(app);

  return (
    <ViewPage<ReadOrgData>
      paddings={0}
      titleBar={(item) => (
        <HBox justifyContent="center" alignItems="center" marginBottom={2}>
          <Typography variant="subtitle2" textAlign="center" paddingRight={2}>
            {item.name}
          </Typography>
        </HBox>
      )}
      leftContainerLines={2}
      leftContainer={(item) =>
        item.logo ? (
          <HBox>
            <img
              src={item.logo}
              alt={labels.logo}
              style={CoreUtils.avatarStyles(true)}
            />
          </HBox>
        ) : undefined
      }
      fields={[
        "brand",
        "region",
        {
          data: "pin",
          label: app.get(tax?.labelKey ?? "taxId"),
          singleRow: false
        },
        "apps",
        "users",
        "persons",
        "orders",
        {
          data: (item) => (
            <ButtonLink
              href={`./../../../user/view/${item.ownerId}`}
              size="small"
              variant="outlined"
            >
              {item.ownerName}
            </ButtonLink>
          ),
          label: "owner"
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
          label: "parentOrg",
          singleRow: "medium"
        },
        {
          data: (item) => app.getStatusLabel(item.status),
          label: "status"
        },
        ["creation", GridDataType.DateTime]
      ]}
      loadData={loadData}
      actions={(data, _refresh) => (
        <React.Fragment>
          <Button
            variant="contained"
            startIcon={<SupportIcon />}
            onClick={() => AppUtils.adminSupport(data)}
          >
            {labels.adminSupport}
          </Button>
        </React.Fragment>
      )}
    ></ViewPage>
  );
}
