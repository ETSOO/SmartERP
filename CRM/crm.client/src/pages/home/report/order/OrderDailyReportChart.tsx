import React from "react";
import {
  BarChart,
  CommonPage,
  HBox,
  NumberInputField,
  SearchField
} from "@etsoo/materialui";
import { OrderDailyReportData, usePageDataEmpty } from "@etsoo/smarterp-core";
import LinearProgress from "@mui/material/LinearProgress";
import { useTheme } from "@mui/material/styles";
import { ChartData } from "chart.js";
import { app } from "../../../../app/MyApp";
import Button from "@mui/material/Button";
import { OrderReportFieldList } from "@etsoo/smarterp-crm/components";
import { DataTypes, DateUtils, NumberUtils } from "@etsoo/shared";
import { AppActionData, BusinessUtils } from "@etsoo/appscript";

export default function OrderDailyReportChart() {
  // Labels
  const { days, refresh, startDate } = app.getLabels(
    "days",
    "refresh",
    "samePeriodLastYear",
    "startDate",
    "usage"
  );

  // Theme
  const theme = useTheme();

  // State
  const [data, setData] = React.useState<ChartData<"bar">>();

  // Ref
  const searchRef = React.useRef<{
    action?: AppActionData;
    data?: OrderDailyReportData[];
    startDate?: string;
    days?: number;
    field: string;
  }>({
    field: "orderAmount"
  });

  const transformData = React.useCallback(() => {
    let { data, field, startDate, days } = searchRef.current;

    if (data == null) {
      reloadData();
      return;
    }

    let label: string;
    let f: DataTypes.Keys<OrderDailyReportData, number>;
    switch (field) {
      case "customerCount":
        label = app.get("customerCount")!;
        f = "customers";
        break;
      case "orderCount":
        label = app.get("orderCount")!;
        f = "items";
        break;
      default:
        label = app.get("orderAmount")!;
        f = "amount";
    }

    days ??= 30;
    const now = new Date();
    const date = startDate
      ? new Date(startDate)
      : now.setDate(now.getDate() - days);

    const labels = Array.from({ length: days }, (_, i) => {
      const d = new Date(date);
      d.setDate(d.getDate() + i);

      return DateUtils.formatForInput(d);
    });

    const values = labels.map(
      (label) => data.find((d) => d.period === label)?.[f] ?? 0
    );

    setData({
      labels,
      datasets: [
        {
          label,
          data: values,
          borderColor: theme.palette.primary.main,
          backgroundColor: theme.palette.primary.main
        }
      ]
    });
  }, []);

  // Load data
  const reloadData = React.useCallback(async () => {
    let { action, startDate, days } = searchRef.current;

    if (
      action == null ||
      BusinessUtils.getSignSeconds(action.timestamp) > 600
    ) {
      action = await app.orderApi.reportAction();
      if (action == null) return;
      searchRef.current.action = action;
    }

    if (!startDate) startDate = undefined;

    const result = await app.core.reportApi.orderDailyReport({
      action,
      startDate,
      days
    });
    if (result == null) return;

    searchRef.current.data = result;

    transformData();
  }, []);

  // Page data hook
  usePageDataEmpty(app);

  return (
    <CommonPage paddings={0} onRefresh={reloadData}>
      <HBox spacing={2} sx={{ paddingY: 0.5, justifyContent: "center" }}>
        <SearchField
          label={startDate}
          name="startDate"
          type="date"
          onChange={(e) => {
            searchRef.current.startDate = e.target.value;
            reloadData();
          }}
        />
        <NumberInputField
          label={days}
          name="days"
          search
          onChange={(e) => {
            searchRef.current.days = NumberUtils.parse(e.target.value);
            reloadData();
          }}
        />
        <OrderReportFieldList
          value={searchRef.current.field}
          search
          onChange={(e) => {
            searchRef.current.field = `${e.target.value}`;
            transformData();
          }}
        />
        <Button variant="contained" onClick={reloadData}>
          {refresh}
        </Button>
      </HBox>
      {data == null ? (
        <LinearProgress />
      ) : (
        <div style={{ height: "calc(100vh - 200px)" }}>
          <BarChart data={data} options={{ maintainAspectRatio: false }} />
        </div>
      )}
    </CommonPage>
  );
}
