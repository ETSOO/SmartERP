import { ButtonLink, CommonPage, ViewContainer } from "@etsoo/materialui";
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

export default function AllSystem() {
  // Route
  const navigate = useNavigate();

  // Labels
  const labels = app.getLabels("settings", "updateSystemSettings");

  // State
  const [settings, setSettings] = React.useState<SystemSettings>();

  // Load data
  const reloadData = React.useCallback(async () => {
    const data = await app.systemApi.readSettings();

    if (data == null) {
      navigate("./updateSettings");
      return;
    }

    setSettings(data);
  }, []);

  usePageDataEmpty(app);

  return (
    <CommonPage paddings={0} onRefresh={reloadData}>
      {settings == null ? (
        <LinearProgress />
      ) : (
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
      )}
    </CommonPage>
  );
}
