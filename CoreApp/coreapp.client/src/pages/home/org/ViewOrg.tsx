import { GridDataType, useParamsEx } from "@etsoo/react";
import { BusinessTax } from "@etsoo/appscript";
import { ButtonLink, HBox, IconButtonLink, ViewPage } from "@etsoo/materialui";
import EditIcon from "@mui/icons-material/Edit";
import ApiIcon from "@mui/icons-material/Api";
import NotInterestedIcon from "@mui/icons-material/NotInterested";
import LabelIcon from "@mui/icons-material/Label";
import { app } from "../../../app/MyApp";
import {
  AvatarState,
  CoreUtils,
  OrgReadDto,
  usePageDataEmpty
} from "@etsoo/smarterp-core";
import React from "react";
import { useNavigate } from "react-router-dom";
import Button from "@mui/material/Button";
import Typography from "@mui/material/Typography";

export default function ViewOrg() {
  // Route
  const { id = 0 } = useParamsEx({ id: "number" });

  // Admin permission
  const adminPermission = app.isAdminUser();

  // Route
  const navigate = useNavigate();

  // Load data
  const loadData = React.useCallback(() => {
    return app.core.orgApi.read(id);
  }, [id]);

  // Labels
  const labels = app.getLabels(
    "confirmAction",
    "customResources",
    "edit",
    "editLogo",
    "externalApis",
    "leaveOrg",
    "logo"
  );

  // Tax
  const tax = BusinessTax.getById(app.region);

  // Page data hook
  usePageDataEmpty(app);

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
        <HBox>
          <img
            src={item.logo}
            alt={labels.logo}
            style={CoreUtils.avatarStyles(true)}
          />
          {item.isOwner && (
            <IconButtonLink<AvatarState>
              href={`./../../avatar/${item.id}`}
              state={{ title: item.name, avatar: item.logo }}
              title={labels.editLogo}
              size="small"
            >
              <EditIcon />
            </IconButtonLink>
          )}
        </HBox>
      )}
      fields={[
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
        ["userExpiry", GridDataType.DateTime],
        ["creation", GridDataType.DateTime]
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
          {adminPermission && (
            <React.Fragment>
              <ButtonLink
                variant="outlined"
                href={`./../../customresource/${id}`}
                startIcon={<LabelIcon />}
              >
                {labels.customResources}
              </ButtonLink>
              <ButtonLink
                variant="outlined"
                href={`./../../apis/${id}`}
                startIcon={<ApiIcon />}
              >
                {labels.externalApis}
              </ButtonLink>
            </React.Fragment>
          )}
        </React.Fragment>
      )}
    ></ViewPage>
  );
}
