import { ButtonLink, CommonPage, VBox, ViewContainer } from "@etsoo/materialui";
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
    "permissionGroups",
    "settings",
    "updateSystemSettings"
  );

  return (
    <CommonPage paddings={0}>
      {settings == null ? (
        <LinearProgress />
      ) : (
        <VBox gap={2}>
          <Paper sx={{ paddingY: 1 }}>
            <ButtonLink href="./dept">{labels.depts}</ButtonLink>
            <ButtonLink href="./group">{labels.permissionGroups}</ButtonLink>
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
