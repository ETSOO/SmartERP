import {
  ResponsivePage,
  SearchField,
  IconButtonLink,
  MobileListItemRenderer
} from "@etsoo/materialui";
import ArticleIcon from "@mui/icons-material/Article";
import EditIcon from "@mui/icons-material/Edit";
import React from "react";
import { GridCellRendererProps, ScrollerListForwardRef } from "@etsoo/react";
import { useNavigate } from "react-router-dom";
import { app } from "../../../app/MyApp";
import { usePageDataEmpty } from "@etsoo/smarterp-core";
import { GroupQueryData } from "@etsoo/smarterp-crm";
import { DataTypes } from "@etsoo/shared";
import { DefaultUI } from "@etsoo/smarterp-core/components";
import { BoxProps } from "@mui/material/Box";

const template = {
  keyword: "string"
} as const satisfies DataTypes.BasicTemplate;

export default function AllGroups() {
  // Route
  const navigate = useNavigate();

  // Labels
  const labels = app.getLabels("actions", "edit", "nameB", "role", "view");

  // Refs
  const ref = React.useRef<ScrollerListForwardRef<GroupQueryData>>(undefined);

  // Load data
  const reloadData = React.useCallback(() => ref.current?.reset(), []);

  // Page data hook
  usePageDataEmpty(app);

  return (
    <ResponsivePage<GroupQueryData, typeof template>
      {...DefaultUI.pageProps({
        onRefresh: reloadData,
        fabButtons: <React.Fragment></React.Fragment>
      })}
      mRef={ref}
      quickAction={(data) => navigate(`./view/${data.id}`)}
      fieldTemplate={template}
      fields={(data) => [
        <SearchField
          label={labels.nameB}
          name="keyword"
          defaultValue={data.keyword}
        />
      ]}
      loadData={async (data) => {
        return await app.groupApi.query(data, {
          defaultValue: [],
          showLoading: false
        });
      }}
      columns={[
        {
          field: "name",
          header: labels.nameB,
          sortable: true
        },
        {
          field: "roles",
          header: labels.role,
          width: 220,
          valueFormatter: ({ data }) => app.getRoleLabel(data?.roles)
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
          }: GridCellRendererProps<GroupQueryData, BoxProps>) => {
            if (data == null) return undefined;

            return (
              <React.Fragment>
                {!data.isSystem && (
                  <IconButtonLink
                    title={labels.edit}
                    href={`./edit/${data.id}`}
                  >
                    <EditIcon />
                  </IconButtonLink>
                )}
                <IconButtonLink title={labels.view} href={`./view/${data.id}`}>
                  <ArticleIcon />
                </IconButtonLink>
              </React.Fragment>
            );
          }
        }
      ]}
      itemRenderer={(props) =>
        MobileListItemRenderer(props, (data) => {
          return [
            data.name,
            undefined,
            [
              !data.isSystem && {
                label: labels.edit,
                icon: <EditIcon />,
                action: `./edit/${data.id}`
              },
              {
                label: labels.view,
                icon: <ArticleIcon />,
                action: `./view/${data.id}`
              }
            ],
            <React.Fragment>{app.getRoleLabel(data.roles)}</React.Fragment>
          ];
        })
      }
    />
  );
}
