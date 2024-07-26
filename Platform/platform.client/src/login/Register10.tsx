import React from "react";
import { LoadingButton, TextFieldEx } from "@etsoo/materialui";
import { Button, Grid, SvgIcon, Typography } from "@mui/material";
import { SharedLayout } from "./SharedLayout";
import { app } from "../app/SmartApp";
import { Link, useNavigate, useParams } from "react-router-dom";
import { AppUtils } from "../app/AppUtils";

function Register10() {
  // Navigate
  const navigate = useNavigate();

  let { username } = useParams<{ username: string }>();
  if (username) username = app.decrypt(decodeURIComponent(username));

  // Labels
  const labels = app.getLabels(
    "userFound",
    "register",
    "back",
    "nextStep",
    "loginId",
    "signUpWith"
  );

  // Login id field
  const loginRef = React.useRef<HTMLInputElement>();

  // Next button click
  const nextClick = async () => {
    // Input check
    const input = loginRef.current!;
    const id = input.value.trim();
    if (id == null || id === "") {
      input.focus();
      return;
    }

    // Encrypted id
    const idEncrypted = app.encrypt(id);

    const result = await app.authApi.loginId(id);

    if (result != null) {
      if (result.ok) {
        // Account registered
        app.notifier.confirm(labels.userFound, undefined, (value) => {
          if (value) {
            navigate("./../password/" + encodeURIComponent(idEncrypted));
          } else {
            input.focus();
          }
        });
      } else {
        // Continue
        navigate("./../registerpassword/" + encodeURIComponent(idEncrypted));
      }
    }
  };

  // Do auth
  const doAuth = React.useCallback(async (ac: string) => {
    const url = await app.authApi.getSignUpUrl(ac);
    if (url) {
      globalThis.location.href = url;
    }
  }, []);

  return (
    <SharedLayout
      title={labels.register}
      buttons={[
        <Button variant="outlined" component={Link} key="back" to={"./../../"}>
          {labels.back}
        </Button>,
        <Button variant="contained" key="next" onClick={nextClick}>
          {labels.nextStep}
        </Button>
      ]}
    >
      <TextFieldEx
        label={labels.loginId}
        inputRef={loginRef}
        autoFocus
        autoCorrect="off"
        autoCapitalize="none"
        inputProps={{ inputMode: "email" }}
        showClear
        defaultValue={username}
        onEnter={(e) => {
          nextClick();
          e.preventDefault();
        }}
      />
      <Typography variant="caption">{labels.signUpWith}</Typography>
      {app.settings.authClients.length > 0 && (
        <Grid container>
          {app.settings.authClients.map((ac) => (
            <Grid item padding={0.5} xs={6} key={ac}>
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

export default Register10;
