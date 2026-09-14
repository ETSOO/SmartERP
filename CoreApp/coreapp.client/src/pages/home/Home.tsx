import { ButtonLink, CommonPage, HBox, SVGUtils } from "@etsoo/materialui";
import Paper from "@mui/material/Paper";
import React from "react";
import { app } from "../../app/MyApp";
import { AppData, AppSwitchCall } from "@etsoo/smarterp-core";
import { IdentityType } from "@etsoo/appscript";
import Typography from "@mui/material/Typography";
import Stack from "@mui/material/Stack";
import Button from "@mui/material/Button";

export default function Home() {
  // User context / state
  const { state } = React.useContext(app.userState.context);
  const { organization, organizationName } = state;

  // Current app
  const currentApp = app.settings.appId;

  const [apps, setApps] = React.useState<AppData[]>([]);

  // Labels
  const labels = app.getLabels(
    "currentOrg",
    "homeGuide",
    "homeNoOrgGuide",
    "homeOrgGuide",
    "homeUserGuide"
  );

  React.useEffect(() => {
    app.core.appApi
      .getMy(
        { maxItems: 16, identityType: IdentityType.User },
        { showLoading: false }
      )
      .then((apps) => {
        if (apps == null) return;
        setApps(apps);
      });
  }, []);

  return (
    <CommonPage paddings={0}>
      <Paper sx={{ padding: 1 }}>
        <HBox sx={{ alignItems: "center", flexWrap: "wrap" }}>
          <Typography variant="body2">{labels.currentOrg}:</Typography>
          {organization ? (
            <ButtonLink href={`./org/my/${organization}`}>
              {organizationName}
            </ButtonLink>
          ) : (
            <ButtonLink href={`./app`}>{labels.homeNoOrgGuide}</ButtonLink>
          )}
        </HBox>
        <Typography variant="caption">{labels.homeGuide}</Typography>
      </Paper>

      {apps.length > 0 && (
        <Paper sx={{ padding: 1, marginTop: 2 }}>
          <Stack direction="row" sx={{ flexWrap: "wrap", gap: "8px" }}>
            {apps.map((a) => (
              <Button
                key={a.id}
                onClick={async (e) => {
                  e.currentTarget.disabled = true;
                  await AppSwitchCall(app, a);
                  e.currentTarget.disabled = false;
                }}
                variant="outlined"
                disabled={a.id === currentApp}
                sx={{ flex: "0 0 auto" }}
                startIcon={SVGUtils.createIcon(a.logo)}
              >
                {app.core.getAppName(a)}
              </Button>
            ))}
          </Stack>
        </Paper>
      )}

      <Paper sx={{ padding: 1, marginTop: 2 }}>
        <Typography component="div" variant="caption">
          1. {labels.homeUserGuide}
        </Typography>
        <Typography component="div" variant="caption">
          2. {labels.homeOrgGuide}
        </Typography>
      </Paper>
    </CommonPage>
  );
}
