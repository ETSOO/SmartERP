import React from "react";
import { HBox, LoadingButton } from "@etsoo/materialui";
import { SharedLayout } from "./SharedLayout";
import { app } from "../app/SmartApp";
import { Link, useNavigate, useParams } from "react-router-dom";
import { AppUtils } from "../app/AppUtils";
import { useSearchParamsEx } from "@etsoo/react";
import Button from "@mui/material/Button";
import Typography from "@mui/material/Typography";
import Grid from "@mui/material/Grid";
import SvgIcon from "@mui/material/SvgIcon";

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
    <SharedLayout
      title={labels.register}
      buttons={[
        <Button variant="outlined" component={Link} key="back" to={"./../../"}>
          {labels.back}
        </Button>
      ]}
    >
      <HBox>
        <Button
          variant="contained"
          fullWidth
          onClick={() => navigate("./../register10")}
        >
          {labels.directRegistration}
        </Button>
      </HBox>
      <Typography variant="caption">{labels.signUpWith}</Typography>
      {app.settings.authClients.length > 0 && (
        <Grid container spacing={0.5}>
          {app.settings.authClients.map((ac) => (
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
      )}
    </SharedLayout>
  );
}
