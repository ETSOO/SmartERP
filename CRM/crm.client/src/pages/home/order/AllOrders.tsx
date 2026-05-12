import {
  ResponsivePage,
  SearchField,
  IconButtonLink,
  MobileListItemRenderer,
  ButtonLink,
  SelectBool,
  LinkEx
} from "@etsoo/materialui";
import AddIcon from "@mui/icons-material/Add";
import EditIcon from "@mui/icons-material/Edit";
import ArticleIcon from "@mui/icons-material/Article";
import PaymentsIcon from "@mui/icons-material/Payments";
import LocalShippingIcon from "@mui/icons-material/LocalShipping";
import ListAltIcon from "@mui/icons-material/ListAlt";
import React from "react";
import {
  GridCellRendererProps,
  GridDeletedCellBoxStyle,
  ScrollerListForwardRef
} from "@etsoo/react";
import { useNavigate } from "react-router-dom";
import { app } from "../../../app/MyApp";
import { usePageDataEmpty } from "@etsoo/smarterp-core";
import { OrderQueryData } from "@etsoo/smarterp-crm";
import { DataTypes, DateUtils } from "@etsoo/shared";
import { DefaultUI, StatusList } from "@etsoo/smarterp-core/components";
import { BoxProps } from "@mui/material/Box";
import Fab from "@mui/material/Fab";
import { Permissions } from "@etsoo/smarterp-crm";
import { Typography } from "@mui/material";
import { CustomerList, ProductList } from "@etsoo/smarterp-crm/components";

const template = {
  keyword: "string",
  customerId: "number",
  source: "string",
  sourceId: "string",
  hasPromotion: "boolean",
  productId: "number",
  creationStart: "date",
  creationEnd: "date",
  status: "number"
} as const satisfies DataTypes.BasicTemplate;

export default function AllOrders() {
  // Route
  const navigate = useNavigate();

  // Labels
  const labels = app.getLabels(
    "actions",
    "add",
    "allOrderLines",
    "amount",
    "approvedDiscount",
    "confirmAction",
    "creation",
    "discount",
    "keywords",
    "edit",
    "orderDeliveries",
    "orderLines",
    "orderPayments",
    "orderSource",
    "orderSourceId",
    "promotion",
    "startDate",
    "taxAmount",
    "title",
    "view"
  );

  // Refs
  const ref = React.useRef<ScrollerListForwardRef<OrderQueryData>>(undefined);

  // Load data
  const reloadData = React.useCallback(() => ref.current?.reset(), []);

  // Page data hook
  usePageDataEmpty(app);

  return (
    <ResponsivePage<OrderQueryData, typeof template>
      {...DefaultUI.pageProps({
        onRefresh: reloadData,
        fabButtons: (
          <React.Fragment>
            {app.owns(Permissions.Order.Manage) && (
              <React.Fragment>
                <ButtonLink
                  href={`./delivery`}
                  size="small"
                  variant="outlined"
                  startIcon={<PaymentsIcon />}
                >
                  {labels.orderDeliveries}
                </ButtonLink>
                <ButtonLink
                  href={`./payment`}
                  size="small"
                  variant="outlined"
                  startIcon={<LocalShippingIcon />}
                >
                  {labels.orderPayments}
                </ButtonLink>
              </React.Fragment>
            )}
            <ButtonLink
              href={`./lines`}
              size="small"
              variant="outlined"
              startIcon={<ListAltIcon />}
            >
              {labels.allOrderLines}
            </ButtonLink>
            {app.owns(Permissions.Order.Add) && (
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
          label={labels.keywords}
          name="keyword"
          defaultValue={data.keyword}
          minChars={2}
        />,
        <CustomerList search idValue={data.customerId} />,
        <SearchField
          label={labels.orderSource}
          name="source"
          defaultValue={data.source}
          minChars={2}
        />,
        <SearchField
          label={labels.orderSourceId}
          name="sourceId"
          defaultValue={data.sourceId}
          minChars={2}
        />,
        <SelectBool
          search
          name="hasPromotion"
          label={labels.promotion}
          value={data.hasPromotion}
        />,
        <ProductList search idValue={data.productId} />,
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
        />,
        <StatusList search idValue={data.status} />
      ]}
      loadData={(data) =>
        app.orderApi.query(data, {
          defaultValue: [],
          showLoading: false
        })
      }
      columns={[
        {
          header: labels.title,
          cellBoxStyle: {
            paddingTop: "10px!important",
            paddingBottom: "10px!important"
          },
          cellRenderer: ({
            data
          }: GridCellRendererProps<OrderQueryData, BoxProps>) => {
            if (data == null) return undefined;

            return (
              <React.Fragment>
                <Typography variant="body2" sx={GridDeletedCellBoxStyle(data)}>
                  {data.title}
                </Typography>
                <Typography variant="caption" color="text.secondary">
                  {data.customerName}
                  {data.source ? ` / ${data.source}` : ""}{" "}
                  <LinkEx to={`./../contact/view/${data.customerId}`}>
                    {labels.view}
                  </LinkEx>
                </Typography>
              </React.Fragment>
            );
          }
        },
        {
          width: 108,
          header: labels.orderLines,
          align: "right",
          cellBoxStyle: {
            paddingTop: "10px!important",
            paddingBottom: "10px!important"
          },
          cellRenderer: ({
            data
          }: GridCellRendererProps<OrderQueryData, BoxProps>) => {
            if (data == null) return undefined;

            return (
              <React.Fragment>
                <Typography variant="body2">
                  {app.formatNumber(data.lines)}
                </Typography>
                <Typography variant="caption" color="text.secondary">
                  {app.formatNumber(data.items)}
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
          }: GridCellRendererProps<OrderQueryData, BoxProps>) => {
            if (data == null) return undefined;

            const discount = data.discount + data.lineDiscount;

            return (
              <React.Fragment>
                <Typography variant="body2">
                  {app.formatMoney(data.amount, undefined, {
                    currency: data.currency
                  })}
                </Typography>
                <Typography
                  variant="caption"
                  color="text.secondary"
                  title={labels.discount}
                >
                  {app.formatNumber(discount)}
                </Typography>
              </React.Fragment>
            );
          }
        },
        {
          width: 116,
          header: labels.taxAmount,
          align: "right",
          cellBoxStyle: {
            paddingTop: "10px!important",
            paddingBottom: "10px!important"
          },
          cellRenderer: ({
            data
          }: GridCellRendererProps<OrderQueryData, BoxProps>) => {
            if (data == null) return undefined;

            return (
              <React.Fragment>
                <Typography variant="body2">
                  {app.formatMoney(data.taxAmount, undefined, {
                    currency: data.currency
                  })}
                </Typography>
                <Typography variant="caption" color="text.secondary">
                  {data.approvedDiscount == 0
                    ? ""
                    : `(${app.formatNumber(-data.approvedDiscount)})`}
                </Typography>
              </React.Fragment>
            );
          }
        },
        {
          width: 116,
          header: labels.startDate,
          cellBoxStyle: {
            paddingTop: "10px!important",
            paddingBottom: "10px!important"
          },
          cellRenderer: ({
            data
          }: GridCellRendererProps<OrderQueryData, BoxProps>) => {
            if (data == null) return undefined;

            return (
              <React.Fragment>
                <Typography variant="body2">
                  {app.formatDate(data.startDate) ?? "-"}
                </Typography>
                <Typography
                  variant="caption"
                  color="text.secondary"
                  title={labels.creation}
                >
                  {app.formatDate(data.creation)}
                </Typography>
              </React.Fragment>
            );
          }
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
          }: GridCellRendererProps<OrderQueryData, BoxProps>) => {
            if (data == null) return undefined;

            return (
              <React.Fragment>
                {app.owns(Permissions.Order.Edit) && (
                  <IconButtonLink
                    title={labels.edit}
                    href={`./edit/${data.id}`}
                  >
                    <EditIcon />
                  </IconButtonLink>
                )}
                {app.owns(Permissions.Order.View) && (
                  <IconButtonLink
                    title={labels.view}
                    href={`./view/${data.id}`}
                  >
                    <ArticleIcon />
                  </IconButtonLink>
                )}
              </React.Fragment>
            );
          }
        }
      ]}
      rowHeight={[64, 220]}
      itemRenderer={(props) =>
        MobileListItemRenderer(props, (data) => {
          return [
            data.title,
            app.formatDate(data.creation, "d"),
            [
              app.owns(Permissions.Order.Edit) && {
                label: labels.edit,
                icon: <EditIcon />,
                action: `./edit/${data.id}`
              },
              app.owns(Permissions.Order.View) && {
                label: labels.view,
                icon: <ArticleIcon />,
                action: `./view/${data.id}`
              }
            ],
            <React.Fragment>
              <Typography variant="body2" sx={GridDeletedCellBoxStyle(data)}>
                {data.customerName}
                {data.source ? ` / ${data.source}` : ""}{" "}
                <LinkEx to={`./../contact/view/${data.customerId}`}>
                  {labels.view}
                </LinkEx>
              </Typography>
              <Typography variant="body2">
                {labels.orderLines}: {app.formatNumber(data.lines)} /{" "}
                {app.formatNumber(data.items)}
              </Typography>
              <Typography variant="body2">
                {labels.amount}:{" "}
                {app.formatMoney(data.amount, undefined, {
                  currency: data.currency
                })}{" "}
                / {app.formatNumber(data.discount + data.lineDiscount)} (
                {labels.discount})
              </Typography>
              <Typography variant="body2">
                {labels.startDate}: {app.formatDate(data.startDate) ?? "-"} /{" "}
                {app.formatDate(data.creation)}
              </Typography>
            </React.Fragment>
          ];
        })
      }
    />
  );
}
