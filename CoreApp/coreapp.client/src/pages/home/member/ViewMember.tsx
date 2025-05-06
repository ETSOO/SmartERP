import { GridDataType, useParamsEx } from "@etsoo/react";
import { HBox, IconButtonLink, ViewPage } from "@etsoo/materialui";
import EditIcon from "@mui/icons-material/Edit";
import { app } from "../../../app/MyApp";
import {
  AvatarState,
  MemberReadDto,
  usePageDataEmpty
} from "@etsoo/smarterp-core";
import React from "react";
import Typography from "@mui/material/Typography";

export default function ViewMember() {
  // Route
  const { id = 0 } = useParamsEx({ id: "number" });

  // Load data
  const loadData = React.useCallback(() => {
    return app.core.memberApi.read(id);
  }, [id]);

  // Labels
  const labels = app.getLabels("edit", "editAvatar", "logo");

  // Permissions
  const editPermission = app.isAdminUser();

  // Page data hook
  usePageDataEmpty(app);

  return (
    <ViewPage<MemberReadDto>
      paddings={0}
      titleBar={(item) => (
        <HBox justifyContent="center" alignItems="center" marginBottom={2}>
          <Typography variant="subtitle2" textAlign="center" paddingRight={2}>
            {item.localName ? `${item.localName} (${item.name})` : item.name}
          </Typography>
          {editPermission && (
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
      leftContainerLines={3}
      leftContainer={(item) => (
        <HBox justifyContent={{ xs: "center", sm: "flex-start" }}>
          <img
            src={item.localAvatar ?? item.avatar}
            alt={labels.logo}
            style={{
              width: "160px",
              height: "160px",
              border: "1px solid #666"
            }}
          />
          {editPermission && (
            <IconButtonLink<AvatarState>
              href={`./../../avatar/${item.id}`}
              state={{ title: item.name, avatar: item.localAvatar }}
              title={labels.editAvatar}
              size="small"
            >
              <EditIcon />
            </IconButtonLink>
          )}
        </HBox>
      )}
      fields={[
        {
          data: (item) => app.getRoleLabel(item.userRole),
          label: "role"
        },
        "reportTo",
        "assignedId",
        ["expiry", GridDataType.DateTime],
        {
          data: (item) => app.core.getIdentityLabel(item.identityType),
          label: "identityType"
        },
        "inviter",
        {
          data: (item) => app.getStatusLabel(item.status),
          label: "status"
        },
        ["refreshTime", GridDataType.DateTime],
        ["creation", GridDataType.DateTime]
      ]}
      loadData={loadData}
    ></ViewPage>
  );
}
