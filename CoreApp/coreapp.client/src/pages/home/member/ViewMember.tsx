import { GridDataType, useParamsEx } from "@etsoo/react";
import { ButtonLink, HBox, IconButtonLink, ViewPage } from "@etsoo/materialui";
import EditIcon from "@mui/icons-material/Edit";
import { app } from "../../../app/MyApp";
import { MemberReadDto, usePageDataEmpty } from "@etsoo/smarterp-core";
import React from "react";
import Typography from "@mui/material/Typography";
import { ImagePreviewButton } from "./ImagePreviewButton";
import ButtonGroup from "@mui/material/ButtonGroup";

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
  const editPermission = app.isHRUser();

  // Page data hook
  usePageDataEmpty(app);

  return (
    <ViewPage<MemberReadDto>
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
        <ButtonGroup
          sx={{ justifyContent: { xs: "center", sm: "flex-start" } }}
        >
          <ImagePreviewButton
            size={160}
            image={item.localAvatar ?? item.avatar}
          />
          {editPermission && (
            <ButtonLink
              title={labels.editAvatar}
              href={`./../../avatar/${item.id}`}
              state={{ title: item.name, avatar: item.localAvatar }}
            >
              <EditIcon />
            </ButtonLink>
          )}
        </ButtonGroup>
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
