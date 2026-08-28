import React from "react";
import { HBox, LoadingButton } from "@etsoo/materialui";
import { SharedLayout } from "./SharedLayout";
import { app } from "../app/SmartApp";
import { useNavigate, useParams } from "react-router-dom";
import { AppUtils } from "../app/AppUtils";
import { useSearchParamsEx } from "@etsoo/react";
import Button from "@mui/material/Button";
import Typography from "@mui/material/Typography";
import Grid from "@mui/material/Grid";
import SvgIcon from "@mui/material/SvgIcon";
import { Constants } from "../app/Constants";

export function Register() {
  // Navigate
  const navigate = useNavigate();

  const { auth: authType } = useSearchParamsEx({
    auth: "string"
  });

  let { username } = useParams<{ username: string }>();
  if (username) username = app.decrypt(decodeURIComponent(username));

  // Labels
  const labels = app.getLabels(
    "register",
    "back",
    "signUpWith",
    "directRegistration"
  );

  // Auth clients
  const authClients =
    app.storage.getPersistedData<string[]>(Constants.AuthClients) ?? [];

  // Do auth
  const doAuth = React.useCallback(async (ac: string) => {
    const url = await app.authApi.getAuthSignUpUrl(ac);
    if (url) {
      globalThis.location.href = url;
    }
  }, []);

  React.useEffect(() => {
    if (authType) {
      doAuth(authType);
    }
  }, [authType]);

  return (
    <SharedLayout title={labels.register} homeUrl="./../../">
      <HBox>
        <Button
          variant="contained"
          fullWidth
          onClick={() => navigate("./../register10")}
        >
          {labels.directRegistration}
        </Button>
      </HBox>
      {authClients.length > 0 && (
        <React.Fragment>
          <Typography variant="caption">{labels.signUpWith}</Typography>
          <Grid container spacing={0.5}>
            {authClients.map((ac) => (
              <Grid size={{ xs: 6 }} key={ac}>
                <LoadingButton
                  variant="outlined"
                  fullWidth
                  startIcon={
                    <SvgIcon
                      component={AppUtils.getBrandIcon(ac)}
                      inheritViewBox
                    />
                  }
                  onClick={() => doAuth(ac)}
                >
                  {app.get(`brand${ac}`)}
                </LoadingButton>
              </Grid>
            ))}
          </Grid>
        </React.Fragment>
      )}
    </SharedLayout>
  );
}
