import { EntityStatus, UserRole } from "@etsoo/appscript";
import {
  MUGlobal,
  ResponsivePage,
  SearchField,
  ComboBox,
  IconButtonLink,
  MobileListItemRenderer,
  Switch
} from "@etsoo/materialui";
import { BoxProps, Fab, IconButton, Typography } from "@mui/material";
import AddIcon from "@mui/icons-material/Add";
import EditIcon from "@mui/icons-material/Edit";
import PersonRemoveIcon from "@mui/icons-material/PersonRemove";
import React from "react";
import {
  GridCellRendererProps,
  GridDataType,
  GridDeletedCellBoxStyle,
  ScrollerListForwardRef
} from "@etsoo/react";
import { useNavigate } from "react-router-dom";
import { app } from "../../../app/MyApp";
import { MemberQueryDto } from "@etsoo/smarterp-core";
import { DataTypes } from "@etsoo/shared";
import { DefaultUI } from "@etsoo/smarterp-core/components";

const template = {
  name: "string",
  userRole: "number",
  assignedId: "string",
  enabled: "boolean"
} as const satisfies DataTypes.BasicTemplate;

export default function AllMembers() {
  // Route
  const navigate = useNavigate();

  // Roles
  const roles = app.getRoles(UserRole.Founder * 2 - 1);

  const getRoleLabel = (data?: MemberQueryDto) => {
    if (data == null) return "";
    return app
      .getRoles(data.userRole)
      .map((item) => item.label)
      .join(", ");
  };

  const deleteMember = (id: number) => {
    app.notifier.confirm(
      labels.confirmAction.format(labels.remove),
      undefined,
      async (confirmed) => {
        if (!confirmed) return;
        const result = await app.core.memberApi.delete(id);
        if (result == null) return;

        if (result.ok) {
          reloadData();
          return;
        }

        app.alertResult(result);
      }
    );
  };

  // Edit permission
  const editPermission = app.isHRUser();

  // Labels
  const labels = app.getLabels(
    "id",
    "name",
    "organization",
    "creation",
    "actions",
    "role",
    "assignedId",
    "inviteMember",
    "edit",
    "remove",
    "inactivated",
    "entityStatus",
    "confirmAction",
    "statusNormal"
  );

  // Current organization
  const organization = app.userData?.organization;

  // Refs
  const ref = React.useRef<ScrollerListForwardRef<MemberQueryDto>>();

  // Load data
  const reloadData = () => ref.current?.reset();

  const margin = MUGlobal.pagePaddings;

  React.useEffect(() => {
    // Page title
    app.setPageKey("members");
  }, []);

  return (
    <ResponsivePage<MemberQueryDto, typeof template>
      {...DefaultUI.createProps({
        onRefresh: reloadData,
        fabButtons: (
          <React.Fragment>
            {editPermission && (
              <Fab title={labels.inviteMember} size="medium" color="primary">
                <AddIcon />
              </Fab>
            )}
          </React.Fragment>
        )
      })}
      mRef={ref}
      defaultOrderBy={[{ field: "creation", desc: true }]}
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
          valueFormatter: ({ data }) => getRoleLabel(data),
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
          width: 120,
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
                {editPermission && (
                  <React.Fragment>
                    <IconButtonLink
                      title={labels.edit}
                      href={`./../edit/${data.id}`}
                    >
                      <EditIcon />
                    </IconButtonLink>
                    {!data.isSelf &&
                      data.userRole < UserRole.Founder &&
                      data.status < EntityStatus.Inactivated && (
                        <IconButton
                          title={labels.remove}
                          onClick={() => deleteMember(data.id)}
                        >
                          <PersonRemoveIcon />
                        </IconButton>
                      )}
                  </React.Fragment>
                )}
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
              !data.isSelf &&
                data.userRole < UserRole.Founder && {
                  label: labels.remove,
                  icon: <PersonRemoveIcon />,
                  action: () => deleteMember(data.id)
                }
            ],
            <React.Fragment>
              <Typography variant="body2" noWrap>
                {getRoleLabel(data) +
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
