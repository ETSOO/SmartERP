import React from "react";
import { LoadingButton, TextFieldEx } from "@etsoo/materialui";
import { Button, Grid, SvgIcon, Typography } from "@mui/material";
import { SharedLayout } from "./SharedLayout";
import { app } from "../app/SmartApp";
import { Link, useNavigate, useParams } from "react-router-dom";
import { AppUtils } from "../app/AppUtils";

function Register20() {
  // Navigate
  const navigate = useNavigate();

  // Labels
  const labels = app.getLabels("nextStep", "mobile", "verifyMobileNumber");

  // Input field
  const inputRef = React.useRef<HTMLInputElement>();

  // Next button click
  const nextClick = async () => {};

  React.useEffect(() => {
    // Focus
    inputRef.current?.focus();
  }, []);

  return (
    <SharedLayout
      title={labels.verifyMobileNumber}
      buttons={[
        <Button variant="contained" key="next" onClick={nextClick}>
          {labels.nextStep}
        </Button>
      ]}
    >
      <TextFieldEx
        label={labels.mobile}
        inputRef={inputRef}
        autoFocus
        autoCorrect="off"
        autoCapitalize="none"
        type="tel"
        inputProps={{ inputMode: "tel" }}
        showClear
        onEnter={(e) => {
          nextClick();
          e.preventDefault();
        }}
      />
    </SharedLayout>
  );
}

export default Register20;
