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
  ScrollerListForwardRef
} from "@etsoo/react";
import { useNavigate } from "react-router-dom";
import { DataTypes } from "@etsoo/shared";
import { DefaultUI } from "@etsoo/smarterp-core/components";
import { BoxProps } from "@mui/material/Box";
import Fab from "@mui/material/Fab";
import { Permissions, PersonInfoQueryData } from "@etsoo/smarterp-crm";
import { app } from "../../app/MyApp";

const template = {
  keyword: "string"
} as const satisfies DataTypes.BasicTemplate;

export type ContactInfosProps = {
  /**
   * Person ID
   */
  personId: number;
};

export function ContactInfos(props: ContactInfosProps) {
  // Route
  const navigate = useNavigate();

  // Labels
  const labels = app.getLabels(
    "actions",
    "add",
    "creation",
    "edit",
    "keyword",
    "view"
  );

  // Refs
  const ref = React.useRef<ScrollerListForwardRef<PersonInfoQueryData>>();

  // Load data
  const reloadData = React.useCallback(() => ref.current?.reset(), []);

  const margin = MUGlobal.pagePaddings;

  return (
    <ResponsivePage<PersonInfoQueryData, typeof template>
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
          label={labels.keyword}
          name="keyword"
          defaultValue={data.keyword}
        />
      ]}
      loadData={async (data) => {
        return await app.personApi.queryInfo(data, {
          defaultValue: [],
          showLoading: false
        });
      }}
      columns={[
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
          }: GridCellRendererProps<PersonInfoQueryData, BoxProps>) => {
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
            data.identifier,
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
            <React.Fragment></React.Fragment>
          ];
        })
      }
    />
  );
}
