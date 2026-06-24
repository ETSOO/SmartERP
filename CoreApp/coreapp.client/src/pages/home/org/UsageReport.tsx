import { BarChart, CommonPage } from "@etsoo/materialui";
import { usePageDataEmpty } from "@etsoo/smarterp-core";
import { app } from "../../../app/MyApp";
import React from "react";
import { ChartData } from "chart.js";
import LinearProgress from "@mui/material/LinearProgress";
import { useParamsEx } from "@etsoo/react";
import { alpha, useTheme } from "@mui/material/styles";

export default function UsageReport() {
  const { id } = useParamsEx({
    id: "number"
  });

  // Labels
  const { samePeriodLastYear, usage } = app.getLabels(
    "samePeriodLastYear",
    "usage"
  );

  // Theme
  const theme = useTheme();

  // State
  const [data, setData] = React.useState<ChartData<"bar">>();

  // Load data
  const reloadData = React.useCallback(async () => {
    const result = await app.core.reportApi.usageReport({ orgId: id });
    if (result == null) return;

    const { labels, currentYearData, lastYearData } =
      app.core.transformReportData(result);

    // Datasets
    const datasets = [
      {
        label: samePeriodLastYear,
        data: lastYearData,
        borderColor: alpha(theme.palette.secondary.main, 0.1),
        backgroundColor: alpha(theme.palette.secondary.main, 0.1)
      },
      {
        label: usage,
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

  // Page data hook
  usePageDataEmpty(app);

  return (
    <CommonPage paddings={0} onRefresh={reloadData}>
      {data == null ? <LinearProgress /> : <BarChart data={data} />}
    </CommonPage>
  );
}
