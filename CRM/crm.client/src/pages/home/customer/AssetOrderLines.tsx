import {
  IconButtonLink,
  MobileListItemRenderer,
  ResponsivePage,
  SearchField
} from "@etsoo/materialui";
import {
  GridCellRendererProps,
  GridDataType,
  GridDeletedCellBoxStyle,
  ScrollerListForwardRef
} from "@etsoo/react";
import { DataTypes, DateUtils } from "@etsoo/shared";
import { DefaultUI, UserTiplist } from "@etsoo/smarterp-core/components";
import { OrderKind, OrderLineQueryAssetData } from "@etsoo/smarterp-crm";
import { app } from "../../../app/MyApp";
import { BoxProps } from "@mui/material/Box";
import React from "react";
import { useNavigate } from "react-router-dom";
import ArticleIcon from "@mui/icons-material/Article";
import Typography from "@mui/material/Typography";

const template = {
  userId: "number",
  creationStart: "date",
  creationEnd: "date"
} as const satisfies DataTypes.BasicTemplate;

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
    "assetQty",
    "costPrice",
    "creation",
    "keywords",
    "price",
    "supplier",
    "title",
    "view"
  );

  // Refs
  const ref =
    React.useRef<ScrollerListForwardRef<OrderLineQueryAssetData>>(undefined);

  // Load data
  const reloadData = React.useCallback(() => ref.current?.reset(), []);

  return (
    <ResponsivePage<OrderLineQueryAssetData, typeof template>
      {...DefaultUI.pageProps({
        onRefresh: reloadData,
        fabButtons: <React.Fragment></React.Fragment>
      })}
      mRef={ref}
      quickAction={(data) =>
        navigate(`./../../../../order/viewline/${data.id}`)
      }
      fieldTemplate={template}
      fields={(data) => [
        <UserTiplist search idValue={data.userId} />,
        <SearchField
          label={labels.creation}
          name="creationStart"
          type="date"
          defaultValue={DateUtils.formatForInput(data.creationStart)}
        />,
        <SearchField
          label=""
          name="creationEnd"
          type="date"
          defaultValue={DateUtils.formatForInput(data.creationEnd)}
        />
      ]}
      loadData={(data) =>
        app.orderLineApi.queryAsset(
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
          field: "supplierName",
          header: labels.supplier
        },
        {
          field: "assetQty",
          header: labels.assetQty,
          valueFormatter: ({ data }) =>
            data == null
              ? undefined
              : `${app.formatNumber(data.qty)} x ${app.formatNumber(data.assetQty)}`,
          width: 118
        },
        {
          field: "costPrice",
          header: labels.costPrice,
          type: GridDataType.Money,
          width: 116
        },
        {
          field: "price",
          header: labels.price,
          type: GridDataType.Money,
          width: 116
        },
        {
          field: "creation",
          header: labels.creation,
          type: GridDataType.Date,
          width: 116,
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
          }: GridCellRendererProps<OrderLineQueryAssetData, BoxProps>) => {
            if (data == null) return undefined;

            return (
              <React.Fragment>
                <IconButtonLink
                  title={labels.view}
                  href={`./../../../../${data.kind == OrderKind.Order ? "order" : "po"}/viewline/${data.id}`}
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
            app.formatDate(data.creation, "d"),
            [
              {
                label: labels.view,
                icon: <ArticleIcon />,
                action: `./../../../../${data.kind == OrderKind.Order ? "order" : "po"}/viewline/${data.id}`
              }
            ],
            <React.Fragment>
              <Typography variant="body2">
                {labels.assetQty}: {app.formatNumber(data.qty)} x{" "}
                {app.formatNumber(data.assetQty)}
              </Typography>
              <Typography component="div" variant="caption">
                {app.formatNumber(data.price)},{" "}
                {app.formatNumber(data.costPrice)}
              </Typography>
            </React.Fragment>
          ];
        })
      }
    />
  );
}
