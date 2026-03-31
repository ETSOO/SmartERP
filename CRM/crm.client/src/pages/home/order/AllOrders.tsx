import {
  ResponsivePage,
  SearchField,
  IconButtonLink,
  MobileListItemRenderer,
  ButtonLink,
  SelectBool
} from "@etsoo/materialui";
import AddIcon from "@mui/icons-material/Add";
import EditIcon from "@mui/icons-material/Edit";
import ArticleIcon from "@mui/icons-material/Article";
import PaymentsIcon from "@mui/icons-material/Payments";
import LocalShippingIcon from "@mui/icons-material/LocalShipping";
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

export default function AllDepts() {
  // Route
  const navigate = useNavigate();

  // Labels
  const labels = app.getLabels(
    "actions",
    "add",
    "amount",
    "approvedDiscount",
    "confirmAction",
    "creation",
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
            <Fab
              title={labels.add}
              size="medium"
              color="primary"
              onClick={() => navigate("./add")}
            >
              <AddIcon />
            </Fab>
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
          cellRenderer: ({
            data,
            cellProps
          }: GridCellRendererProps<OrderQueryData, BoxProps>) => {
            if (data == null) return undefined;

            cellProps.sx = {
              paddingTop: "6px!important",
              paddingBottom: "6px!important"
            };

            return (
              <React.Fragment>
                <Typography sx={GridDeletedCellBoxStyle(data)}>
                  {data.title}
                </Typography>
                <Typography variant="caption" color="text.secondary">
                  {data.customerName}
                  {data.source ? ` / ${data.source}` : ""}
                </Typography>
              </React.Fragment>
            );
          }
        },
        {
          width: 108,
          header: labels.orderLines,
          align: "right",
          cellRenderer: ({
            data,
            cellProps
          }: GridCellRendererProps<OrderQueryData, BoxProps>) => {
            if (data == null) return undefined;

            cellProps.sx = {
              paddingTop: "6px!important",
              paddingBottom: "6px!important"
            };

            return (
              <React.Fragment>
                <Typography>{app.formatNumber(data.lines)}</Typography>
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
          cellRenderer: ({
            data,
            cellProps
          }: GridCellRendererProps<OrderQueryData, BoxProps>) => {
            if (data == null) return undefined;

            cellProps.sx = {
              paddingTop: "6px!important",
              paddingBottom: "6px!important"
            };

            const discount = data.discount + data.lineDiscount;

            return (
              <React.Fragment>
                <Typography>
                  {app.formatMoney(data.amount, undefined, {
                    currency: data.currency
                  })}
                </Typography>
                <Typography variant="caption" color="text.secondary">
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
          cellRenderer: ({
            data,
            cellProps
          }: GridCellRendererProps<OrderQueryData, BoxProps>) => {
            if (data == null) return undefined;

            cellProps.sx = {
              paddingTop: "6px!important",
              paddingBottom: "6px!important"
            };

            return (
              <React.Fragment>
                <Typography>
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
          cellRenderer: ({
            data,
            cellProps
          }: GridCellRendererProps<OrderQueryData, BoxProps>) => {
            if (data == null) return undefined;

            cellProps.sx = {
              paddingTop: "6px!important",
              paddingBottom: "6px!important"
            };

            return (
              <React.Fragment>
                <Typography>{app.formatDate(data.startDate) ?? "-"}</Typography>
                <Typography variant="caption" color="text.secondary">
                  {app.formatDate(data.creation)}
                </Typography>
              </React.Fragment>
            );
          }
        },
        {
          width: DefaultUI.Widths.icon2,
          header: labels.actions,
          cellRenderer: ({
            data,
            cellProps
          }: GridCellRendererProps<OrderQueryData, BoxProps>) => {
            if (data == null) return undefined;

            cellProps.sx = {
              paddingTop: "6px!important",
              paddingBottom: "6px!important"
            };

            return (
              <React.Fragment>
                <IconButtonLink title={labels.edit} href={`./edit/${data.id}`}>
                  <EditIcon />
                </IconButtonLink>
                <IconButtonLink title={labels.view} href={`./view/${data.id}`}>
                  <ArticleIcon />
                </IconButtonLink>
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
              {
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
            <React.Fragment>
              <Typography variant="body2" sx={GridDeletedCellBoxStyle(data)}>
                {data.customerName}
                {data.source ? ` / ${data.source}` : ""}
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
                {labels.promotion})
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
