import { CommonPage, HBox, IconButtonLink, LinkEx } from "@etsoo/materialui";
import { usePageDataEmpty } from "@etsoo/smarterp-core";
import { app } from "../../../app/MyApp";
import Card from "@mui/material/Card";
import CardHeader from "@mui/material/CardHeader";
import CardContent from "@mui/material/CardContent";
import Grid from "@mui/material/Grid";
import BarChartIcon from "@mui/icons-material/BarChart";

export default function AllReports() {
  // Labels
  const labels = app.getLabels(
    "chart",
    "order",
    "orderDailyReport",
    "orderMonthlyReport"
  );

  usePageDataEmpty(app);

  return (
    <CommonPage paddings={0}>
      <Grid container spacing={2}>
        <Grid size={{ xs: 12, sm: 6, md: 4 }}>
          <Card>
            <CardHeader title={labels.order} />
            <CardContent sx={{ display: "flex", flexDirection: "column" }}>
              {app.userData?.system?.orderDailyReportHour != null && (
                <HBox
                  sx={{ justifyContent: "space-between", alignItems: "center" }}
                >
                  <LinkEx to="./order/daily">{labels.orderDailyReport}</LinkEx>
                  <IconButtonLink
                    href="./order/dailychart"
                    title={labels.chart}
                  >
                    <BarChartIcon />
                  </IconButtonLink>
                </HBox>
              )}
              <HBox
                sx={{ justifyContent: "space-between", alignItems: "center" }}
              >
                <LinkEx to="./order/monthly">
                  {labels.orderMonthlyReport}
                </LinkEx>
                <IconButtonLink
                  href="./order/monthlychart"
                  title={labels.chart}
                >
                  <BarChartIcon />
                </IconButtonLink>
              </HBox>
            </CardContent>
          </Card>
        </Grid>
      </Grid>
    </CommonPage>
  );
}
