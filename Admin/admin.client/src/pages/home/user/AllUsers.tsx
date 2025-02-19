import { EntityStatus } from "@etsoo/appscript";
import {
  MUGlobal,
  ResponsivePage,
  SearchField,
  ComboBox,
  IconButtonLink,
  MobileListItemRenderer,
  Switch
} from "@etsoo/materialui";
import { BoxProps, Fab, Typography } from "@mui/material";
import PersonAddAlt1Icon from "@mui/icons-material/PersonAddAlt1";
import EditIcon from "@mui/icons-material/Edit";
import ArticleIcon from "@mui/icons-material/Article";
import React from "react";
import {
  GridCellRendererProps,
  GridDataType,
  GridDeletedCellBoxStyle,
  ScrollerListForwardRef
} from "@etsoo/react";
import { useNavigate } from "react-router-dom";
import { app } from "../../../app/MyApp";
import { MemberQueryDto, usePageDataEmpty } from "@etsoo/smarterp-core";
import { DataTypes } from "@etsoo/shared";
import { DefaultUI } from "@etsoo/smarterp-core/components";
import { AppUtils } from "../components/AppUtils";

const template = {
  name: "string",
  userRole: "number",
  assignedId: "string",
  enabled: "boolean"
} as const satisfies DataTypes.BasicTemplate;

export default function AllUsers() {
  // Route
  const navigate = useNavigate();

  // Roles
  const roles = app.getRoles();

  // Edit permission
  const editPermission = app.isAdminUser();

  // Labels
  const labels = app.getLabels(
    "actions",
    "assignedId",
    "confirmAction",
    "creation",
    "edit",
    "entityStatus",
    "inviteMember",
    "name",
    "role",
    "statusNormal",
    "view"
  );

  // Refs
  const ref = React.useRef<ScrollerListForwardRef<MemberQueryDto>>();

  // Load data
  const reloadData = () => ref.current?.reset();

  const margin = MUGlobal.pagePaddings;

  // Page data hook
  usePageDataEmpty(app);

  return (
    <ResponsivePage<MemberQueryDto, typeof template>
      {...DefaultUI.createProps({
        onRefresh: reloadData,
        fabButtons: (
          <React.Fragment>
            {editPermission && (
              <Fab
                title={labels.inviteMember}
                size="medium"
                color="primary"
                onClick={() => AppUtils.inviteMember(() => reloadData())}
              >
                <PersonAddAlt1Icon />
              </Fab>
            )}
          </React.Fragment>
        )
      })}
      mRef={ref}
      defaultOrderBy={[{ field: "creation", desc: true }]}
      quickAction={(data) => navigate(`./../view/${data.id}`)}
      fieldTemplate={template}
      fields={(data) => [
        <SearchField
          label={labels.name}
          name="keywords"
          defaultValue={data.name}
          slotProps={{ input: { sx: { width: "120px" } } }}
        />,
        <ComboBox
          options={roles}
          name="role"
          label={labels.role}
          search
          idValue={data.userRole}
        />,
        <SearchField
          label={labels.assignedId}
          name="assignedId"
          minChars={3}
          defaultValue={data.assignedId}
        />,
        <Switch
          label={labels.statusNormal}
          name="enabled"
          checked={data.enabled ?? true}
        />
      ]}
      loadData={async (data) => {
        return await app.core.memberApi.query(data, {
          defaultValue: [],
          showLoading: false
        });
      }}
      columns={[
        {
          field: "name",
          header: labels.name,
          sortable: true,
          cellBoxStyle: GridDeletedCellBoxStyle
        },
        {
          field: "userRole",
          width: 180,
          header: labels.role,
          valueFormatter: ({ data }) => app.getRoleLabel(data?.userRole),
          sortable: false
        },
        {
          field: "assignedId",
          width: 150,
          header: labels.assignedId,
          sortable: true
        },
        {
          field: "creation",
          type: GridDataType.Date,
          width: 116,
          header: labels.creation,
          sortable: true,
          sortAsc: false
        },
        {
          width: DefaultUI.Widths.icon2,
          header: labels.actions,
          cellRenderer: ({
            data,
            cellProps
          }: GridCellRendererProps<MemberQueryDto, BoxProps>) => {
            if (data == null) return undefined;

            cellProps.sx = {
              paddingTop: "6px!important",
              paddingBottom: "6px!important"
            };

            return (
              <React.Fragment>
                {editPermission && data.isEditable && (
                  <IconButtonLink
                    title={labels.edit}
                    href={`./../edit/${data.id}`}
                  >
                    <EditIcon />
                  </IconButtonLink>
                )}
                <IconButtonLink
                  title={labels.view}
                  href={`./../view/${data.id}`}
                >
                  <ArticleIcon />
                </IconButtonLink>
              </React.Fragment>
            );
          }
        }
      ]}
      itemSize={[116, margin]}
      innerItemRenderer={(props) =>
        MobileListItemRenderer(props, (data) => {
          return [
            data.name,
            app.formatDate(data.creation, "d"),
            [
              editPermission && {
                label: labels.edit,
                icon: <EditIcon />,
                action: `./../edit/${data.id}`
              },
              {
                label: labels.view,
                icon: <ArticleIcon />,
                action: `./../view/${data.id}`
              }
            ],
            <React.Fragment>
              <Typography variant="body2" noWrap>
                {app.getRoleLabel(data.userRole) +
                  (data.assignedId ? ", " + data.assignedId : "")}
              </Typography>
              {data.status >= EntityStatus.Inactivated && (
                <React.Fragment>
                  <Typography variant="caption">
                    {labels.entityStatus + ": "}
                  </Typography>
                  <Typography variant="caption" color="error">
                    {app.getStatusLabel(data?.status)}
                  </Typography>
                </React.Fragment>
              )}
            </React.Fragment>
          ];
        })
      }
    />
  );
}
