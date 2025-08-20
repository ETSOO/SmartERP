import {
  MUGlobal,
  ResponsivePage,
  SearchField,
  IconButtonLink,
  MobileListItemRenderer
} from "@etsoo/materialui";
import EditIcon from "@mui/icons-material/Edit";
import ArticleIcon from "@mui/icons-material/Article";
import AddIcon from "@mui/icons-material/Add";
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
import { DeptQueryData } from "@etsoo/smarterp-crm";
import { DataTypes } from "@etsoo/shared";
import { DefaultUI } from "@etsoo/smarterp-core/components";
import { BoxProps } from "@mui/material/Box";
import { UserTiplist } from "@etsoo/smarterp-crm/components";
import Fab from "@mui/material/Fab";
import { Permissions } from "@etsoo/smarterp-crm";

const template = {
  keyword: "string",
  leaderId: "number"
} as const satisfies DataTypes.BasicTemplate;

export default function AllDepts() {
  // Route
  const navigate = useNavigate();

  // Labels
  const labels = app.getLabels(
    "actions",
    "add",
    "creation",
    "edit",
    "leader",
    "nameB",
    "staff",
    "view"
  );

  // Refs
  const ref = React.useRef<ScrollerListForwardRef<DeptQueryData>>(undefined);

  // Load data
  const reloadData = React.useCallback(() => ref.current?.reset(), []);

  const margin = MUGlobal.pagePaddings;

  // Page data hook
  usePageDataEmpty(app);

  return (
    <ResponsivePage<DeptQueryData, typeof template>
      {...DefaultUI.pageProps({
        onRefresh: reloadData,
        fabButtons: (
          <React.Fragment>
            {app.owns(Permissions.Dept.Add) && (
              <Fab
                title={labels.add}
                size="medium"
                color="primary"
                onClick={() => navigate("./add")}
              >
                <AddIcon />
              </Fab>
            )}
          </React.Fragment>
        )
      })}
      mRef={ref}
      defaultOrderBy={[{ field: "creation", desc: true }]}
      quickAction={(data) => navigate(`./view/${data.id}`)}
      fieldTemplate={template}
      fields={(data) => [
        <SearchField
          label={labels.nameB}
          name="keyword"
          defaultValue={data.keyword}
        />,
        <UserTiplist label={labels.leader} name="leaderId" search />
      ]}
      loadData={async (data) => {
        return await app.deptApi.query(data, {
          defaultValue: [],
          showLoading: false
        });
      }}
      columns={[
        {
          field: "name",
          header: labels.nameB,
          sortable: true,
          cellBoxStyle: GridDeletedCellBoxStyle
        },
        {
          field: "staff",
          width: 80,
          header: labels.staff,
          type: GridDataType.Number
        },
        {
          field: "leader",
          header: labels.leader,
          width: 120
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
          }: GridCellRendererProps<DeptQueryData, BoxProps>) => {
            if (data == null) return undefined;

            cellProps.sx = {
              paddingTop: "6px!important",
              paddingBottom: "6px!important"
            };

            return (
              <React.Fragment>
                {app.owns(Permissions.Dept.Edit) && (
                  <IconButtonLink
                    title={labels.edit}
                    href={`./edit/${data.id}`}
                  >
                    <EditIcon />
                  </IconButtonLink>
                )}
                {app.owns(Permissions.Dept.View) && (
                  <IconButtonLink
                    title={labels.view}
                    href={`./../../contact/view/${data.id}`}
                  >
                    <ArticleIcon />
                  </IconButtonLink>
                )}
              </React.Fragment>
            );
          }
        }
      ]}
      itemSize={[116, margin, "1px"]}
      innerItemRenderer={(props) =>
        MobileListItemRenderer(props, (data) => {
          return [
            data.name,
            app.formatDate(data.creation, "d"),
            [
              app.owns(Permissions.Dept.Edit) && {
                label: labels.edit,
                icon: <EditIcon />,
                action: `./edit/${data.id}`
              },
              app.owns(Permissions.Dept.View) && {
                label: labels.view,
                icon: <ArticleIcon />,
                action: `./../../contact/view/${data.id}`
              }
            ],
            <React.Fragment>
              {labels.leader}: {data.leader}
              <br />
              {labels.staff}: {data.staff}
            </React.Fragment>
          ];
        })
      }
    />
  );
}
