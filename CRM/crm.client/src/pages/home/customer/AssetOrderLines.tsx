import {
  IconButtonLink,
  MobileListItemRenderer,
  ResponsivePage
} from "@etsoo/materialui";
import {
  GridCellRendererProps,
  GridDataType,
  GridDeletedCellBoxStyle,
  ScrollerListForwardRef
} from "@etsoo/react";
import { DataTypes } from "@etsoo/shared";
import { DefaultUI } from "@etsoo/smarterp-core/components";
import { OrderLineQueryAllData, OrderLineQueryData } from "@etsoo/smarterp-crm";
import { app } from "../../../app/MyApp";
import { BoxProps } from "@mui/material/Box";
import React from "react";
import { useNavigate } from "react-router-dom";
import ArticleIcon from "@mui/icons-material/Article";
import AddIcon from "@mui/icons-material/Add";
import EditIcon from "@mui/icons-material/Edit";
import { Permissions } from "@etsoo/smarterp-crm";
import Typography from "@mui/material/Typography";
import Fab from "@mui/material/Fab";

const template = {} as const satisfies DataTypes.BasicTemplate;

export type AssetOrderLinesProps = {
  assetId: number;
};

export function AssetOrderLines(props: AssetOrderLinesProps) {
  // Route
  const navigate = useNavigate();

  // Destruct
  const { assetId } = props;

  // Labels
  const labels = app.getLabels(
    "actions",
    "add",
    "amount",
    "discount",
    "edit",
    "keywords",
    "orderLineStartTime",
    "price",
    "qty",
    "qtyStart",
    "title",
    "view"
  );

  // Refs
  const ref =
    React.useRef<ScrollerListForwardRef<OrderLineQueryAllData>>(undefined);

  // Load data
  const reloadData = React.useCallback(() => ref.current?.reset(), []);

  return (
    <ResponsivePage<OrderLineQueryAllData, typeof template>
      {...DefaultUI.pageProps({
        onRefresh: reloadData,
        fabButtons: <React.Fragment></React.Fragment>
      })}
      mRef={ref}
      quickAction={(data) =>
        navigate(`./../../../../order/viewline/${data.id}`)
      }
      fieldTemplate={template}
      fields={(data) => []}
      loadData={(data) =>
        app.orderLineApi.queryAll(
          { assetId, ...data },
          {
            defaultValue: [],
            showLoading: false
          }
        )
      }
      columns={[
        {
          field: "title",
          header: labels.title,
          sortable: true,
          cellBoxStyle: GridDeletedCellBoxStyle
        },
        {
          field: "price",
          header: labels.price,
          type: GridDataType.Money,
          width: 116
        },
        {
          field: "qty",
          header: labels.qty,
          type: GridDataType.Number,
          width: 88
        },
        {
          field: "discount",
          header: labels.discount,
          type: GridDataType.Money,
          valueFormatter: ({ data }) =>
            data?.discount === 0 ? undefined : data?.discount,
          width: 116
        },
        {
          field: "amount",
          header: labels.amount,
          type: GridDataType.Money,
          width: 116
        },
        {
          field: "startTime",
          type: GridDataType.DateTime,
          width: 128,
          header: labels.orderLineStartTime,
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
          }: GridCellRendererProps<OrderLineQueryData, BoxProps>) => {
            if (data == null) return undefined;

            return (
              <React.Fragment>
                <IconButtonLink
                  title={labels.view}
                  href={`./../../../../order/viewline/${data.id}`}
                >
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
            data.title,
            app.formatDate(data.startTime, "ds"),
            [
              {
                label: labels.view,
                icon: <ArticleIcon />,
                action: `./../../../../order/viewline/${data.id}`
              }
            ],
            <React.Fragment>
              <Typography variant="body2">
                {labels.amount}: {app.formatMoney(data.amount)}
              </Typography>
              <Typography component="div" variant="caption">
                {app.formatNumber(data.price)} x {app.formatNumber(data.qty)}
                {data.discount == 0
                  ? ""
                  : ` - ${app.formatNumber(data.discount)}`}
              </Typography>
              {data.startTime && (
                <Typography variant="body2">
                  {labels.orderLineStartTime}:{" "}
                  {app.formatDate(data.startTime, "ds")}
                </Typography>
              )}
            </React.Fragment>
          ];
        })
      }
    />
  );
}
