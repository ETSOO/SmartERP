import {
  ResponsivePage,
  SearchField,
  MobileListItemRenderer
} from "@etsoo/materialui";
import React from "react";
import { GridDataType, ScrollerListForwardRef } from "@etsoo/react";
import {
  OrderDailyReportQueryData,
  usePageDataEmpty
} from "@etsoo/smarterp-core";
import { DefaultUI } from "@etsoo/smarterp-core/components";
import { app } from "../../../../app/MyApp";
import { DataTypes } from "@etsoo/shared";
import { AppActionData, BusinessUtils } from "@etsoo/appscript";
import { Typography } from "@mui/material";

const template = {
  startDate: "string",
  endDate: "string"
} as const satisfies DataTypes.BasicTemplate;

export default function OrderDailyReport() {
  // Labels
  const labels = app.getLabels(
    "amountPaid",
    "customers",
    "discount",
    "endDate",
    "period",
    "orderAmount",
    "orderCount",
    "orderLineDiscount",
    "qty",
    "startDate"
  );

  // Refs
  const ref =
    React.useRef<ScrollerListForwardRef<OrderDailyReportQueryData>>(undefined);

  const actionRef = React.useRef<AppActionData>(undefined);

  // Load data
  const reloadData = React.useCallback(() => ref.current?.reset(), []);

  // Page data hook
  usePageDataEmpty(app);

  return (
    <ResponsivePage<OrderDailyReportQueryData, typeof template>
      {...DefaultUI.pageProps({
        onRefresh: reloadData
      })}
      mRef={ref}
      fieldTemplate={template}
      fields={(data) => [
        <SearchField
          label={labels.startDate}
          name="startDate"
          type="date"
          defaultValue={data.startDate}
        />,
        <SearchField
          label={labels.endDate}
          name="endDate"
          type="date"
          defaultValue={data.endDate}
        />
      ]}
      loadData={async (data) => {
        let action = actionRef.current;
        if (
          action == null ||
          BusinessUtils.getSignSeconds(action.timestamp) > 600
        ) {
          action = await app.orderApi.reportAction();
          if (action == null) return [];
          actionRef.current = action;
        }

        return await app.core.reportApi.orderDailyReportQuery(
          { action, ...data },
          {
            defaultValue: [],
            showLoading: false
          }
        );
      }}
      columns={[
        {
          field: "period",
          header: labels.period,
          width: 108
        },
        {
          header: labels.amountPaid + " / " + labels.orderAmount,
          valueFormatter: ({ data }) =>
            data == null
              ? ""
              : `${app.formatMoney(data.paidAmount)} / ${app.formatMoney(data.amount)}`
        },
        {
          header: labels.discount + " / " + labels.orderLineDiscount,
          valueFormatter: ({ data }) =>
            data == null
              ? ""
              : `${app.formatMoney(data.discount)} / ${app.formatMoney(data.lineDiscount)}`
        },
        {
          header: labels.orderCount + " / " + labels.customers,
          valueFormatter: ({ data }) =>
            data == null ? "" : `${data.items} / ${data.customers}`
        },
        {
          field: "qty",
          header: labels.qty,
          type: GridDataType.Number,
          width: 88
        }
      ]}
      rowHeight={250}
      itemRenderer={(props) =>
        MobileListItemRenderer(props, (data) => {
          return [
            data.period,
            app.formatMoney(data.amount) + " / " + app.formatNumber(data.qty),
            [],
            <React.Fragment>
              <Typography variant="body2">
                {labels.amountPaid} / {labels.orderAmount}:{" "}
                {app.formatMoney(data.paidAmount)} /{" "}
                {app.formatMoney(data.amount)}
              </Typography>
              <Typography variant="body2">
                {labels.discount} / {labels.orderLineDiscount}:{" "}
                {app.formatMoney(data.discount)} /{" "}
                {app.formatMoney(data.lineDiscount)}
              </Typography>
              <Typography variant="body2">
                {labels.orderCount} / {labels.customers}: {data.items} /{" "}
                {data.customers}
              </Typography>
            </React.Fragment>
          ];
        })
      }
    />
  );
}
