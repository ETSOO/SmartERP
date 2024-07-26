import { Button, Stack, SvgIcon, Typography } from "@mui/material";
import { Link } from "react-router-dom";
import { app } from "../app/SmartApp";
import { SharedLayout } from "./SharedLayout";
import { AppUtils } from "../app/AppUtils";
import { useSearchParamsEx } from "@etsoo/react";
import React from "react";

function AuthFail() {
  // Labels
  const labels = app.getLabels(
    "authFail",
    "back",
    "continueRegistration",
    "authFailContinue",
    "authFailTip"
  );

  const { type, error, errorType, errorField } = useSearchParamsEx({
    type: "string",
    error: "string",
    errorType: "string",
    errorField: "string"
  });

  const brand = app.get(`brand${type}`) ?? type ?? "";
  let content: React.ReactNode;
  if (error) {
    content = (
      <React.Fragment>
        <Typography>{labels.authFailTip.format(brand)}</Typography>
        <Typography variant="caption">
          {[error, errorType, errorField]}
        </Typography>
      </React.Fragment>
    );
  } else {
    content = <Typography>{labels.authFailContinue.format(brand)}</Typography>;
  }

  return (
    <SharedLayout
      title={
        <Stack direction="row" gap={1} alignItems="center">
          {<SvgIcon component={AppUtils.getBrandIcon(type)} inheritViewBox />}
          {labels.authFail}
        </Stack>
      }
      buttons={
        error ? (
          <Button variant="outlined" component={Link} to="./../../">
            {labels.back}
          </Button>
        ) : (
          [
            <Button
              variant="outlined"
              key="back"
              component={Link}
              to="./../../"
            >
              {labels.back}
            </Button>,
            <Button
              variant="contained"
              key="continue"
              component={Link}
              to={`./../register?auth=${type}`}
            >
              {labels.continueRegistration}
            </Button>
          ]
        )
      }
    >
      {content}
    </SharedLayout>
  );
}

export default AuthFail;
