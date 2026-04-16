import {
  ResponsivePage,
  SearchField,
  MobileListItemRenderer,
  LinkEx,
  NumberInputField,
  IconButtonLink
} from "@etsoo/materialui";
import React from "react";
import {
  GridCellRendererProps,
  GridDeletedCellBoxStyle,
  ScrollerListForwardRef
} from "@etsoo/react";
import { useNavigate } from "react-router-dom";
import { app } from "../../../app/MyApp";
import { usePageDataEmpty } from "@etsoo/smarterp-core";
import { OrderLineQueryAllData } from "@etsoo/smarterp-crm";
import { DataTypes, DateUtils, Utils } from "@etsoo/shared";
import { DefaultUI } from "@etsoo/smarterp-core/components";
import { BoxProps } from "@mui/material/Box";
import Typography from "@mui/material/Typography";
import { CustomerList, ProductList } from "@etsoo/smarterp-crm/components";
import ArticleIcon from "@mui/icons-material/Article";
import ShoppingCartIcon from "@mui/icons-material/ShoppingCart";
import GroupsIcon from "@mui/icons-material/Groups";

const template = {
  source: "string",
  customerId: "number",
  productId: "number",
  qtyStart: "number",
  startTimeStart: "date",
  startTimeEnd: "date"
} as const satisfies DataTypes.BasicTemplate;

export default function AllOrderLines() {
  // Route
  const navigate = useNavigate();

  // Labels
  const labels = app.getLabels(
    "actions",
    "amount",
    "creation",
    "customer",
    "keywords",
    "orderLineStartTime",
    "orderSource",
    "qtyStart",
    "status",
    "title",
    "view",
    "viewCustomer",
    "viewOrder"
  );

  // Refs
  const ref =
    React.useRef<ScrollerListForwardRef<OrderLineQueryAllData>>(undefined);

  // Load data
  const reloadData = React.useCallback(() => ref.current?.reset(), []);

  // Page data hook
  usePageDataEmpty(app);

  return (
    <ResponsivePage<OrderLineQueryAllData, typeof template>
      {...DefaultUI.pageProps({
        onRefresh: reloadData,
        fabButtons: <React.Fragment></React.Fragment>
      })}
      mRef={ref}
      quickAction={(data) => navigate(`./../viewline/${data.id}`)}
      defaultOrderBy={[{ field: "creation", desc: true }]}
      fieldTemplate={template}
      fields={(data) => [
        <ProductList search idValue={data.productId} />,
        <NumberInputField search name="qtyStart" label={labels.qtyStart} />,
        <SearchField
          label={labels.orderSource}
          name="source"
          defaultValue={data.source}
          minChars={2}
        />,
        <CustomerList search idValue={data.customerId} />,
        <SearchField
          label={labels.orderLineStartTime}
          name="startTimeStart"
          type="date"
          defaultValue={DateUtils.formatForInput(data.startTimeStart)}
        />,
        <SearchField
          label=""
          name="startTimeEnd"
          type="date"
          defaultValue={DateUtils.formatForInput(data.startTimeEnd)}
        />
      ]}
      loadData={async (data) => {
        return await app.orderLineApi.queryAll(data, {
          defaultValue: [],
          showLoading: false
        });
      }}
      columns={[
        {
          header: labels.title,
          cellBoxStyle: {
            paddingTop: "10px!important",
            paddingBottom: "10px!important"
          },
          cellRenderer: ({
            data
          }: GridCellRendererProps<OrderLineQueryAllData, BoxProps>) => {
            if (data == null) return undefined;

            return (
              <React.Fragment>
                <Typography variant="body2" sx={GridDeletedCellBoxStyle(data)}>
                  {data.title}
                </Typography>
                <Typography variant="caption" color="text.secondary">
                  {data.source ? `[${data.source}] ` : ""}
                  {data.description}
                </Typography>
              </React.Fragment>
            );
          }
        },
        {
          width: 116,
          header: labels.amount,
          align: "right",
          cellBoxStyle: {
            paddingTop: "10px!important",
            paddingBottom: "10px!important"
          },
          cellRenderer: ({
            data
          }: GridCellRendererProps<OrderLineQueryAllData, BoxProps>) => {
            if (data == null) return undefined;

            return (
              <React.Fragment>
                <Typography variant="body2">
                  {app.formatMoney(data.amount, undefined, {
                    currency: data.currency
                  })}
                </Typography>
                <Typography variant="caption" color="text.secondary">
                  {app.formatNumber(data.price)} x {app.formatNumber(data.qty)}
                </Typography>
              </React.Fragment>
            );
          }
        },
        {
          width: 280,
          header: labels.customer,
          cellBoxStyle: {
            paddingTop: "10px!important",
            paddingBottom: "10px!important"
          },
          cellRenderer: ({
            data
          }: GridCellRendererProps<OrderLineQueryAllData, BoxProps>) => {
            if (data == null) return undefined;

            return (
              <React.Fragment>
                <Typography variant="body2">{data.customer}</Typography>
                <Typography
                  variant="caption"
                  color="text.secondary"
                  sx={{ display: "flex", gap: 1 }}
                >
                  <LinkEx to={`./../../contact/view/${data.customerId}`}>
                    {labels.viewCustomer}
                  </LinkEx>
                  <LinkEx to={`./../../order/view/${data.orderId}`}>
                    {labels.viewOrder}
                  </LinkEx>
                </Typography>
              </React.Fragment>
            );
          }
        },
        {
          width: 148,
          header: labels.creation + " / " + labels.status,
          cellBoxStyle: {
            paddingTop: "10px!important",
            paddingBottom: "10px!important"
          },
          cellRenderer: ({
            data
          }: GridCellRendererProps<OrderLineQueryAllData, BoxProps>) => {
            if (data == null) return undefined;

            const startTimeStr = app.formatDate(data.startTime, "dm");
            const endTimeStr = app.formatDate(data.endTime, "dm");
            let endTimeDisplay = undefined;
            if (startTimeStr && endTimeStr) {
              endTimeDisplay =
                " - " +
                endTimeStr.substring(
                  Utils.commonPrefixFrom(startTimeStr, endTimeStr).length
                );
            }

            return (
              <React.Fragment>
                <Typography variant="body2">
                  {app.formatDate(data.creation)} /{" "}
                  {app.getStatusLabel(data.status)}
                </Typography>
                <Typography variant="caption" color="text.secondary">
                  {startTimeStr}
                  {endTimeDisplay}
                </Typography>
              </React.Fragment>
            );
          }
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
          }: GridCellRendererProps<OrderLineQueryAllData, BoxProps>) => {
            if (data == null) return undefined;

            return (
              <React.Fragment>
                <IconButtonLink
                  title={labels.view}
                  href={`./../viewline/${data.id}`}
                >
                  <ArticleIcon />
                </IconButtonLink>
              </React.Fragment>
            );
          }
        }
      ]}
      rowHeight={200}
      itemRenderer={(props) =>
        MobileListItemRenderer(props, (data) => {
          return [
            `${data.source ? `[${data.source}] ` : ""}${data.title}`,
            app.formatDate(data.creation, "d") +
              " / " +
              app.getStatusLabel(data.status),
            [
              {
                label: labels.view,
                icon: <ArticleIcon />,
                action: `./../viewline/${data.id}`
              },
              {
                label: labels.viewCustomer,
                icon: <GroupsIcon />,
                action: `./../../contact/view/${data.customerId}`
              },
              {
                label: labels.viewOrder,
                icon: <ShoppingCartIcon />,
                action: `./../../order/view/${data.orderId}`
              }
            ],
            <React.Fragment>
              <Typography variant="body2">
                {app.formatNumber(data.price)} x {app.formatNumber(data.qty)} ={" "}
                {app.formatMoney(data.amount, undefined, {
                  currency: data.currency
                })}
              </Typography>
              <Typography variant="body2">{data.customer}</Typography>
              <Typography variant="caption" color="text.secondary">
                {data.description}
              </Typography>
            </React.Fragment>
          ];
        })
      }
    />
  );
}
