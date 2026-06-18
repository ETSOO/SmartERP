import { GridDataType, useParamsEx } from "@etsoo/react";
import { BusinessTax } from "@etsoo/appscript";
import { ButtonLink, HBox, LinkEx, ViewPage } from "@etsoo/materialui";
import SupportIcon from "@mui/icons-material/Support";
import ApiIcon from "@mui/icons-material/Api";
import BarChartIcon from "@mui/icons-material/BarChart";
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
  const labels = app.getLabels(
    "adminSupport",
    "externalApis",
    "logo",
    "usageReport"
  );

  // Tax
  const tax = BusinessTax.getById(app.region);

  // Page data hook
  usePageDataEmpty(app);

  return (
    <ViewPage<ReadOrgData>
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
            align="center"
            sx={{ paddingRight: 2 }}
          >
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
        "timeZone",
        "apps",
        "users",
        "persons",
        "orders",
        {
          data: (item) => (
            <LinkEx to={`./../../../user/view/${item.ownerId}`} variant="body2">
              {item.ownerName}
            </LinkEx>
          ),
          label: "owner"
        },
        {
          data: (item) =>
            item.parentName ? (
              <LinkEx to={`./../${item.parentId}`} variant="body2">
                {item.parentName}
              </LinkEx>
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
          <ButtonLink
            variant="outlined"
            href={`./../../apis/${id}`}
            startIcon={<ApiIcon />}
          >
            {labels.externalApis}
          </ButtonLink>
          <ButtonLink
            variant="outlined"
            href={`./../../usage/${id}`}
            startIcon={<BarChartIcon />}
          >
            {labels.usageReport}
          </ButtonLink>
        </React.Fragment>
      )}
    ></ViewPage>
  );
}
