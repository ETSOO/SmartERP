import { GridDataType, useParamsEx } from "@etsoo/react";
import { BusinessTax } from "@etsoo/appscript";
import {
  ButtonLink,
  HBox,
  IconButtonLink,
  ImagePreviewButton,
  ViewPage
} from "@etsoo/materialui";
import EditIcon from "@mui/icons-material/Edit";
import ApiIcon from "@mui/icons-material/Api";
import ArticleIcon from "@mui/icons-material/Article";
import NotInterestedIcon from "@mui/icons-material/NotInterested";
import LabelIcon from "@mui/icons-material/Label";
import { app } from "../../../app/MyApp";
import {
  AvatarState,
  OrgReadDto,
  usePageDataEmpty
} from "@etsoo/smarterp-core";
import React from "react";
import { useNavigate } from "react-router-dom";
import Button from "@mui/material/Button";
import Typography from "@mui/material/Typography";
import BarChartIcon from "@mui/icons-material/BarChart";
import { MyUtils } from "../../../app/MyUtils";
import ButtonGroup from "@mui/material/ButtonGroup";

export default function ViewOrg() {
  // Route
  const { id = 0 } = useParamsEx({ id: "number" });

  // Route
  const navigate = useNavigate();

  // Load data
  const loadData = React.useCallback(() => app.core.orgApi.read(id), [id]);

  // Labels
  const labels = app.getLabels(
    "confirmAction",
    "customResources",
    "documentTemplates",
    "edit",
    "editLogo",
    "externalApis",
    "leaveOrg",
    "logo",
    "usageReport"
  );

  // Tax
  const tax = BusinessTax.getById(app.region);

  // Page data hook
  usePageDataEmpty(app);

  React.useEffect(() => {
    MyUtils.checkOrg(id);
  }, [id]);

  return (
    <ViewPage<OrgReadDto>
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
      )}
      leftContainerLines={2}
      leftContainer={(item) => (
        <ButtonGroup
          sx={{ justifyContent: { xs: "center", sm: "flex-start" } }}
        >
          <ImagePreviewButton size={[160, 80]} image={item.logo} />
          {item.isOwner && (
            <ButtonLink<AvatarState>
              title={labels.editLogo}
              href={`./../../avatar/${item.id}`}
              state={{ title: item.name, avatar: item.logo }}
            >
              <EditIcon />
            </ButtonLink>
          )}
        </ButtonGroup>
      )}
      fields={[
        {
          data: "slogan",
          singleRow: "large"
        },
        "brand",
        {
          data: "pin",
          label: app.get(tax?.labelKey ?? "taxId"),
          singleRow: false
        },
        {
          data: (item) => `${item.users} / ${item.persons}`,
          label: "members"
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
          singleRow: true
        },
        {
          data: "ownerName",
          label: "owner"
        },
        {
          data: (item) => app.getStatusLabel(item.status),
          label: "status"
        },
        {
          data: (item) => app.getStatusLabel(item.userStatus),
          label: "userStatus"
        },
        "timeZone",
        ["userExpiry", GridDataType.DateTime],
        ["creation", GridDataType.DateTime],
        {
          data: (item) => (
            <ButtonGroup>
              <ImagePreviewButton size={[60, 60]} image={item.companySeal} />
              {item.isOwner && (
                <ButtonLink<AvatarState>
                  title={labels.edit}
                  href={`./../../companySeal/${item.id}`}
                  state={{ title: item.name, avatar: item.companySeal }}
                >
                  <EditIcon />
                </ButtonLink>
              )}
            </ButtonGroup>
          ),
          label: "companySeal"
        }
      ]}
      loadData={loadData}
      actions={(data, _refresh) => (
        <React.Fragment>
          {!data.isOwner && (
            <Button
              variant="outlined"
              startIcon={<NotInterestedIcon />}
              onClick={() => {
                app.notifier.confirm(
                  labels.confirmAction.format(labels.leaveOrg),
                  data.name,
                  async (ok) => {
                    if (!ok) return;
                    const result = await app.core.orgApi.leave(id);
                    if (result == null) return;
                    if (result.ok) {
                      navigate("./../");
                      return;
                    }
                    app.alertResult(result);
                  }
                );
              }}
            >
              {labels.leaveOrg}
            </Button>
          )}
          <ButtonLink
            variant="outlined"
            href={`./../../customresource/${id}`}
            startIcon={<LabelIcon />}
          >
            {labels.customResources}
          </ButtonLink>
          <ButtonLink
            variant="outlined"
            href={`./../../document/${id}`}
            startIcon={<ArticleIcon />}
          >
            {labels.documentTemplates}
          </ButtonLink>
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
