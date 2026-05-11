import {
  IconButtonLink,
  MobileListItemRenderer,
  NumberInputField,
  ResponsivePage,
  SearchField
} from "@etsoo/materialui";
import {
  GridCellRendererProps,
  GridDataType,
  GridDeletedCellBoxStyle,
  ScrollerListForwardRef
} from "@etsoo/react";
import { DataTypes } from "@etsoo/shared";
import { DefaultUI } from "@etsoo/smarterp-core/components";
import { app } from "../../../app/MyApp";
import { BoxProps } from "@mui/material/Box";
import React from "react";
import { ProductList } from "@etsoo/smarterp-crm/components";
import { useNavigate } from "react-router-dom";
import ArticleIcon from "@mui/icons-material/Article";
import AddIcon from "@mui/icons-material/Add";
import EditIcon from "@mui/icons-material/Edit";
import {
  OrderLineQueryData,
  Permissions,
  ProductScope
} from "@etsoo/smarterp-crm";
import Typography from "@mui/material/Typography";
import Fab from "@mui/material/Fab";
import { EntityStatus } from "@etsoo/appscript";
import { POUIUtils } from "./POUIUtils";

const template = {
  keyword: "string",
  productId: "number",
  qtyStart: "number"
} as const satisfies DataTypes.BasicTemplate;

export type AllPOLinesProps = {
  poId: number;
  poStatus: EntityStatus;
  currency: string;
  supplierId: number;
  refresh: () => Promise<void>;
};

export function POLines(props: AllPOLinesProps) {
  // Route
  const navigate = useNavigate();

  // Destruct
  const { poId, poStatus, currency, supplierId, refresh } = props;

  // Labels
  const labels = app.getLabels(
    "actions",
    "addPOLine",
    "amount",
    "discount",
    "edit",
    "keywords",
    "poLineStartTime",
    "price",
    "qty",
    "qtyStart",
    "title",
    "view"
  );

  // Refs
  const ref =
    React.useRef<ScrollerListForwardRef<OrderLineQueryData>>(undefined);

  // Load data
  const reloadData = React.useCallback(() => ref.current?.reset(), []);

  return (
    <ResponsivePage<OrderLineQueryData, typeof template>
      {...DefaultUI.pageProps({
        onRefresh: reloadData,
        fabButtons: (
          <React.Fragment>
            {app.owns(Permissions.PO.Edit) && (
              <Fab
                title={labels.addPOLine}
                size="medium"
                color="primary"
                onClick={() =>
                  POUIUtils.addPOLine({ poId, currency, supplierId }, () => {
                    reloadData();
                    refresh();
                  })
                }
              >
                <AddIcon />
              </Fab>
            )}
          </React.Fragment>
        )
      })}
      mRef={ref}
      quickAction={(data) => navigate(`./../../viewline/${data.id}`)}
      fieldTemplate={template}
      fields={(data) => [
        <ProductList
          search
          idValue={data.productId}
          rq={{ scope: ProductScope.Purchase }}
        />,
        <SearchField
          label={labels.keywords}
          name="keyword"
          defaultValue={data.keyword}
          minChars={2}
        />,
        <NumberInputField search name="qtyStart" label={labels.qtyStart} />
      ]}
      loadData={(data) =>
        app.poLineApi.query(
          {
            poId,
            ...data,
            queryPaging: {
              batchSize: 20,
              orderBy: [{ field: "id", desc: false, unique: true }]
            }
          },
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
          cellBoxStyle: (data) => ({
            ...GridDeletedCellBoxStyle(data),
            paddingLeft: data?.bomId ? "32px!important" : undefined
          })
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
          renderProps: { currency },
          width: 116
        },
        {
          field: "startTime",
          type: GridDataType.DateTime,
          width: 128,
          header: labels.poLineStartTime,
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
          }: GridCellRendererProps<OrderLineQueryData, BoxProps>) => {
            if (data == null) return undefined;

            return (
              <React.Fragment>
                {app.owns(Permissions.PO.Edit) &&
                  poStatus < EntityStatus.Inactivated && (
                    <IconButtonLink
                      title={labels.edit}
                      href={`./../../editline/${data.id}`}
                    >
                      <EditIcon />
                    </IconButtonLink>
                  )}
                <IconButtonLink
                  title={labels.view}
                  href={`./../../viewline/${data.id}`}
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
            data.title + (data.bomId ? " ***" : ""),
            app.formatDate(data.startTime, "ds"),
            [
              {
                label: labels.edit,
                icon: <EditIcon />,
                action: `./../../editline/${data.id}`
              },
              {
                label: labels.view,
                icon: <ArticleIcon />,
                action: `./../../viewline/${data.id}`
              }
            ],
            <React.Fragment>
              <Typography variant="body2">
                {labels.amount}:{" "}
                {app.formatMoney(data.amount, undefined, {
                  currency
                })}
              </Typography>
              <Typography component="div" variant="caption">
                {app.formatNumber(data.price)} x {app.formatNumber(data.qty)}
                {data.discount == 0
                  ? ""
                  : ` - ${app.formatNumber(data.discount)}`}
              </Typography>
              {data.startTime && (
                <Typography variant="body2">
                  {labels.poLineStartTime}:{" "}
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
