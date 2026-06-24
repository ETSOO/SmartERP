import React from "react";
import { BarChart, CommonPage, HBox } from "@etsoo/materialui";
import {
  CoreUtils,
  OrderMonthlyReportData,
  usePageDataEmpty
} from "@etsoo/smarterp-core";
import LinearProgress from "@mui/material/LinearProgress";
import { alpha, useTheme } from "@mui/material/styles";
import { ChartData } from "chart.js";
import { app } from "../../../../app/MyApp";
import {
  LatestYearList,
  OrderReportFieldList
} from "@etsoo/smarterp-crm/components";
import { Button } from "@mui/material";
import { DataTypes, NumberUtils } from "@etsoo/shared";
import { AppActionData, BusinessUtils } from "@etsoo/appscript";

export default function OrderMonthlyReportChart() {
  // Labels
  const { refresh, samePeriodLastYear } = app.getLabels(
    "refresh",
    "samePeriodLastYear"
  );

  // Theme
  const theme = useTheme();

  // State
  const [data, setData] = React.useState<ChartData<"bar">>();

  // Ref
  const searchRef = React.useRef<{
    action?: AppActionData;
    data?: OrderMonthlyReportData[];
    year?: number;
    field: string;
  }>({
    field: "orderAmount"
  });

  const transformData = React.useCallback(() => {
    const { data, field } = searchRef.current;

    if (data == null) {
      reloadData();
      return;
    }

    let label: string;
    let f: DataTypes.Keys<OrderMonthlyReportData, number>;
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

    var { labels, currentYearData, lastYearData } =
      CoreUtils.transformReportDataBase(app, data, f);

    // Datasets
    const datasets = [
      {
        label: samePeriodLastYear,
        data: lastYearData,
        borderColor: alpha(theme.palette.secondary.main, 0.1),
        backgroundColor: alpha(theme.palette.secondary.main, 0.1)
      },
      {
        label,
        data: currentYearData,
        borderColor: theme.palette.primary.main,
        backgroundColor: theme.palette.primary.main
      }
    ];

    setData({
      labels,
      datasets
    });
  }, []);

  // Load data
  const reloadData = React.useCallback(async () => {
    let { action, year } = searchRef.current;

    if (
      action == null ||
      BusinessUtils.getSignSeconds(action.timestamp) > 600
    ) {
      action = await app.orderApi.reportAction();
      if (action == null) return;
      searchRef.current.action = action;
    }

    const result = await app.core.reportApi.orderMonthlyReport({
      action,
      year
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
        <LatestYearList
          value={0}
          search
          onChange={(e) => {
            searchRef.current.year = NumberUtils.parse(e.target.value);
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
