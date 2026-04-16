import {
  ResponsivePage,
  SearchField,
  MobileListItemRenderer,
  IconButtonLink,
  HBox
} from "@etsoo/materialui";
import AddIcon from "@mui/icons-material/Add";
import EditIcon from "@mui/icons-material/Edit";
import ShareIcon from "@mui/icons-material/Share";
import React from "react";
import { useNavigate } from "react-router-dom";
import {
  GridCellRendererProps,
  GridDataType,
  ScrollerListForwardRef,
  useParamsEx
} from "@etsoo/react";
import { app } from "../../../app/MyApp";
import { OrgQueryApiData, usePageData } from "@etsoo/smarterp-core";
import { DataTypes } from "@etsoo/shared";
import { DefaultUI } from "@etsoo/smarterp-core/components";
import Fab from "@mui/material/Fab";
import { BoxProps } from "@mui/material/Box";
import { BusinessUtils } from "@etsoo/appscript";

const template = {
  keyword: "string",
  service: "number",
  appId: "string"
} as const satisfies DataTypes.BasicTemplate;

export default function AllApis() {
  // Route
  const navigate = useNavigate();
  const { id = 0 } = useParamsEx({ id: "number" });

  // Labels
  const labels = app.getLabels(
    "actions",
    "add",
    "apiService",
    "appApiUrl",
    "appId",
    "edit",
    "org",
    "title",
    "updatedAt"
  );

  // Refs
  const ref = React.useRef<ScrollerListForwardRef<OrgQueryApiData>>(undefined);

  // Load data
  const reloadData = React.useCallback(() => ref.current?.reset(), []);

  usePageData(
    app,
    {
      breadcrumbs: (bc) => {
        bc.splice(bc.length - 1, 0, {
          title: labels.org,
          path: `./../my/${id}`
        });
        return bc;
      }
    },
    []
  );

  return (
    <ResponsivePage<OrgQueryApiData, typeof template>
      {...DefaultUI.pageProps({
        onRefresh: reloadData,
        fabButtons: (
          <Fab
            title={labels.add}
            size="medium"
            color="primary"
            onClick={() => navigate(`./../../addapi?orgId=${id}`)}
          >
            <AddIcon />
          </Fab>
        )
      })}
      mRef={ref}
      fieldTemplate={template}
      fields={(data) => [
        <SearchField
          label={labels.title}
          name="keyword"
          defaultValue={data.keyword}
        />,
        <SearchField
          label={labels.appId}
          name="appId"
          minChars={2}
          defaultValue={data.appId}
        />
      ]}
      loadData={(data, lastItem) =>
        app.core.orgApi.queryApi(
          {
            ...BusinessUtils.setupPagingKeysets(data, lastItem, "id"),
            orgId: id
          },
          {
            defaultValue: [],
            showLoading: false
          }
        )
      }
      columns={[
        {
          field: "service",
          header: labels.apiService,
          valueFormatter: ({ data }) => app.core.getApiService(data?.service),
          width: 120
        },
        {
          field: "title",
          header: labels.title,
          cellRenderer: ({
            data,
            cellProps
          }: GridCellRendererProps<OrgQueryApiData, BoxProps>) => {
            if (data == null) return undefined;
            cellProps.sx = {
              textDecoration: data.enabled ? "none" : "line-through"
            };
            return (
              <HBox gap={1}>
                {data.title}{" "}
                {data.inheritance ? <ShareIcon fontSize="small" /> : undefined}
              </HBox>
            );
          }
        },
        {
          field: "endpoint",
          header: labels.appApiUrl
        },
        {
          field: "appId",
          header: labels.appId,
          width: 220
        },
        {
          field: "updatedAt",
          type: GridDataType.Date,
          width: 116,
          header: labels.updatedAt,
          sortable: true,
          sortAsc: false
        },
        {
          width: DefaultUI.Widths.icon1,
          header: labels.actions,
          cellBoxStyle: {
            paddingTop: "6px!important",
            paddingBottom: "6px!important"
          },
          cellRenderer: ({
            data
          }: GridCellRendererProps<OrgQueryApiData, BoxProps>) => {
            if (data == null) return undefined;

            return (
              <React.Fragment>
                <IconButtonLink
                  title={labels.edit}
                  href={`./../../editapi/${data.id}?orgId=${id}`}
                >
                  <EditIcon />
                </IconButtonLink>
              </React.Fragment>
            );
          }
        }
      ]}
      rowHeight={172}
      itemRenderer={(props) =>
        MobileListItemRenderer(props, (data) => {
          return [
            data.title,
            app.formatDate(data.updatedAt, "d"),
            [
              {
                label: labels.edit,
                icon: <EditIcon />,
                action: `./../../editapi/${data.id}?orgId=${id}`
              }
            ],
            <React.Fragment>
              [{app.core.getApiService(data?.service)}], {data.endpoint},{" "}
              {data.appId}
            </React.Fragment>
          ];
        })
      }
    />
  );
}
