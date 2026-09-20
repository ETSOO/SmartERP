import {
  ButtonLink,
  CommonPage,
  HBox,
  VBox,
  ViewContainer
} from "@etsoo/materialui";
import { app } from "../../../app/MyApp";
import React from "react";
import { SystemSettings } from "@etsoo/smarterp-crm";
import LinearProgress from "@mui/material/LinearProgress";
import { useNavigate } from "react-router-dom";
import Card from "@mui/material/Card";
import CardContent from "@mui/material/CardContent";
import CardActions from "@mui/material/CardActions";
import CardHeader from "@mui/material/CardHeader";
import { usePageDataEmpty } from "@etsoo/smarterp-core";
import Paper from "@mui/material/Paper";
import ArticleIcon from "@mui/icons-material/Article";
import BarChartIcon from "@mui/icons-material/BarChart";
import CalculateIcon from "@mui/icons-material/Calculate";
import Diversity3Icon from "@mui/icons-material/Diversity3";
import SupervisorAccountIcon from "@mui/icons-material/SupervisorAccount";

export default function AllSystem() {
  // Route
  const navigate = useNavigate();

  // System settings
  const userSystemSettings = app.userData?.system;

  // State
  const [settings, setSettings] = React.useState<SystemSettings>();

  // Load data
  const reloadData = React.useCallback(async () => {
    const data = await app.systemApi.readSettings();

    if (data == null) {
      return;
    }

    setSettings(data);
  }, []);

  usePageDataEmpty(app);

  React.useEffect(() => {
    if (userSystemSettings == null) {
      navigate("./updateSettings");
    } else {
      setSettings(userSystemSettings);
    }
  }, []);

  // Labels
  const labels = app.getLabels(
    "depts",
    "finance",
    "org",
    "permissionGroups",
    "reports",
    "settings",
    "updateSystemSettings"
  );

  const orgPersonId = app.userData?.system?.personId;

  return (
    <CommonPage paddings={0}>
      {settings == null ? (
        <LinearProgress />
      ) : (
        <VBox spacing={2}>
          <Paper sx={{ padding: 2 }}>
            <HBox sx={{ flexWrap: "wrap", gap: "8px" }}>
              {orgPersonId && (
                <ButtonLink
                  startIcon={<ArticleIcon />}
                  variant="outlined"
                  href={`./../contact/view/${orgPersonId}`}
                >
                  {labels.org}
                </ButtonLink>
              )}
              <ButtonLink
                startIcon={<CalculateIcon />}
                variant="outlined"
                href="./finance"
                color="success"
              >
                {labels.finance}
              </ButtonLink>
              <ButtonLink
                startIcon={<BarChartIcon />}
                variant="outlined"
                href="./../report"
              >
                {labels.reports}
              </ButtonLink>
              <ButtonLink
                startIcon={<Diversity3Icon />}
                variant="outlined"
                href="./dept"
              >
                {labels.depts}
              </ButtonLink>
              <ButtonLink
                startIcon={<SupervisorAccountIcon />}
                variant="outlined"
                href="./group"
              >
                {labels.permissionGroups}
              </ButtonLink>
            </HBox>
          </Paper>
          <Card>
            <CardHeader title={labels.settings} />
            <CardContent>
              <ViewContainer
                refresh={reloadData}
                data={settings}
                fields={[
                  {
                    data: (item) =>
                      app.system.getCustomerType(item.mainCustomerType),
                    label: "mainCustomerType"
                  },
                  {
                    data: "currencies"
                  },
                  {
                    data: "supplierCurrencies"
                  },
                  {
                    data: "cultures"
                  },
                  {
                    data: "hasInventory"
                  },
                  {
                    data: "taxRate",
                    label: "defaultTaxRate"
                  },
                  {
                    data: "orderMonthlyReportEnabled"
                  },
                  {
                    data: "orderDailyReportHour"
                  }
                ]}
              ></ViewContainer>
            </CardContent>
            <CardActions>
              <ButtonLink href="./updateSettings" size="small">
                {labels.updateSystemSettings}
              </ButtonLink>
            </CardActions>
          </Card>
        </VBox>
      )}
    </CommonPage>
  );
}
