import {
  ResponsivePage,
  SearchField,
  IconButtonLink,
  MobileListItemRenderer
} from "@etsoo/materialui";
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
import { usePageDataEmpty } from "@etsoo/smarterp-core";
import { UserQueryData } from "@etsoo/smarterp-crm";
import { DataTypes } from "@etsoo/shared";
import { DefaultUI } from "@etsoo/smarterp-core/components";
import { BoxProps } from "@mui/material/Box";
import { DeptTiplist, GroupTiplist } from "@etsoo/smarterp-crm/components";
import Typography from "@mui/material/Typography";

const template = {
  keyword: "string",
  deptId: "number",
  groupId: "number"
} as const satisfies DataTypes.BasicTemplate;

export default function AllUsers() {
  // Route
  const navigate = useNavigate();

  // Labels
  const labels = app.getLabels(
    "actions",
    "confirmAction",
    "creation",
    "dept",
    "depts",
    "edit",
    "entityStatus",
    "permissionGroup",
    "name",
    "role",
    "view"
  );

  // Refs
  const ref = React.useRef<ScrollerListForwardRef<UserQueryData>>(undefined);

  // Load data
  const reloadData = React.useCallback(() => ref.current?.reset(), []);

  // Page data hook
  usePageDataEmpty(app);

  return (
    <ResponsivePage<UserQueryData, typeof template>
      {...DefaultUI.pageProps({
        onRefresh: reloadData,
        fabButtons: <React.Fragment></React.Fragment>
      })}
      mRef={ref}
      defaultOrderBy={[{ field: "creation", desc: true }]}
      quickAction={(data) => navigate(`./../contact/view/${data.id}`)}
      fieldTemplate={template}
      fields={(data) => [
        <SearchField
          label={labels.name}
          name="keyword"
          defaultValue={data.keyword}
        />,
        <DeptTiplist label={labels.dept} search idValue={data.deptId} />,
        <GroupTiplist
          label={labels.permissionGroup}
          search
          idValue={data.groupId}
        />
      ]}
      loadData={async (data) => {
        return await app.userApi.query(data, {
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
          field: "depts",
          width: 120,
          header: labels.depts,
          sortable: true
        },
        {
          field: "userRole",
          width: 142,
          header: labels.role,
          valueFormatter: ({ data }) => app.getRoleLabel(data?.userRole)
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
          cellBoxStyle: {
            paddingTop: "6px!important",
            paddingBottom: "6px!important"
          },
          cellRenderer: ({
            data
          }: GridCellRendererProps<UserQueryData, BoxProps>) => {
            if (data == null) return undefined;

            return (
              <React.Fragment>
                {data.editable && (
                  <IconButtonLink
                    title={labels.edit}
                    href={`./edit/${data.id}`}
                  >
                    <EditIcon />
                  </IconButtonLink>
                )}
                <IconButtonLink
                  title={labels.view}
                  href={`./../contact/view/${data.id}`}
                >
                  <ArticleIcon />
                </IconButtonLink>
              </React.Fragment>
            );
          }
        }
      ]}
      rowHeight={160}
      itemRenderer={(props) =>
        MobileListItemRenderer(props, (data) => [
          data.name,
          app.formatDate(data.creation, "d"),
          [
            data.editable && {
              label: labels.edit,
              icon: <EditIcon />,
              action: `./edit/${data.id}`
            },
            {
              label: labels.view,
              icon: <ArticleIcon />,
              action: `./../contact/view/${data.id}`
            }
          ],
          <React.Fragment>
            {data.userRole && (
              <Typography variant="body2" noWrap>
                {labels.role}: {app.getRoleLabel(data.userRole)}
              </Typography>
            )}
            {data.depts && (
              <Typography variant="body2" noWrap>
                {data.depts.join(", ")}
              </Typography>
            )}
          </React.Fragment>
        ])
      }
    />
  );
}
