import {
  ResponsivePage,
  SearchField,
  MobileListItemRenderer,
  IconButtonLink
} from "@etsoo/materialui";
import AddIcon from "@mui/icons-material/Add";
import EditIcon from "@mui/icons-material/Edit";
import React from "react";
import { useNavigate } from "react-router-dom";
import {
  GridCellRendererProps,
  GridDataType,
  ScrollerListForwardRef,
  useParamsEx
} from "@etsoo/react";
import { DocumentQueryData, usePageData } from "@etsoo/smarterp-core";
import { DataTypes } from "@etsoo/shared";
import { DefaultUI, DocumentKindList } from "@etsoo/smarterp-core/components";
import Fab from "@mui/material/Fab";
import { BoxProps } from "@mui/material/Box";
import { BusinessUtils } from "@etsoo/appscript";
import { Typography } from "@mui/material";
import { app } from "../../../app/MyApp";

const template = {
  keyword: "string",
  kind: "string"
} as const satisfies DataTypes.BasicTemplate;

export default function AllDocument() {
  // Route
  const navigate = useNavigate();
  const { id = 0 } = useParamsEx({ id: "number" });

  // Labels
  const labels = app.getLabels(
    "actions",
    "add",
    "edit",
    "keywords",
    "org",
    "parameters",
    "refreshTime",
    "systemTemplate",
    "title",
    "type"
  );

  // Refs
  const ref =
    React.useRef<ScrollerListForwardRef<DocumentQueryData>>(undefined);

  // Load data
  const reloadData = React.useCallback(() => ref.current?.reset(), []);

  const isAdmin = app.isAdminUser();

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
    <ResponsivePage<DocumentQueryData, typeof template>
      {...DefaultUI.pageProps({
        onRefresh: reloadData,
        fabButtons: (
          <Fab
            title={labels.add}
            size="medium"
            color="primary"
            onClick={() => navigate(`./../../adddocument?orgId=${id}`)}
          >
            <AddIcon />
          </Fab>
        )
      })}
      mRef={ref}
      fieldTemplate={template}
      fields={(data) => [
        <SearchField
          label={`${labels.keywords}`}
          name="keyword"
          minChars={2}
          defaultValue={data.keyword}
        />,
        <DocumentKindList
          name="kind"
          sx={{ width: 160 }}
          search
          value={data.kind}
        />
      ]}
      loadData={(data, lastItem) =>
        app.core.documentApi.query(
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
          field: "refreshTime",
          type: GridDataType.Date,
          width: 104,
          header: labels.refreshTime,
          renderProps: app.getDateFormatProps()
        },
        {
          field: "kind",
          header: labels.type,
          width: 200,
          valueFormatter: ({ data }) => app.core.getDocumentKind(data?.kind)
        },
        {
          field: "title",
          header: labels.title
        },
        {
          field: "hasParameters",
          header: labels.parameters,
          width: 80
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
          }: GridCellRendererProps<DocumentQueryData, BoxProps>) => {
            if (data == null) return undefined;

            return (
              <React.Fragment>
                {isAdmin && (
                  <IconButtonLink
                    title={labels.edit}
                    href={`./../../editdocument/${data.id}?orgId=${id}`}
                  >
                    <EditIcon />
                  </IconButtonLink>
                )}
              </React.Fragment>
            );
          }
        }
      ]}
      rowHeight={160}
      itemRenderer={(props) =>
        MobileListItemRenderer(props, (data) => {
          return [
            `${data.title}`,
            app.formatDate(data.refreshTime),
            [
              isAdmin && {
                label: labels.edit,
                icon: <EditIcon />,
                action: `./../../editdocument/${data.id}?orgId=${id}`
              }
            ],
            <React.Fragment>
              {data.orgName && (
                <Typography variant="body2">{data.orgName}</Typography>
              )}
              <Typography variant="body2">{data.kind}</Typography>
            </React.Fragment>
          ];
        })
      }
    />
  );
}
