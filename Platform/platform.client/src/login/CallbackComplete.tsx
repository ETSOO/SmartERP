import React from "react";
import { SharedLayout } from "./SharedLayout";
import { TextFieldEx, TextFieldExMethods } from "@etsoo/materialui";
import { Constants } from "../app/Constants";
import { app } from "../app/SmartApp";
import { Navigate, useNavigate, useParams } from "react-router-dom";
import Typography from "@mui/material/Typography";
import Button from "@mui/material/Button";

const homeUrl = "./../../../";
function NavigateHome() {
  return <Navigate to={homeUrl} replace />;
}

export default function CallbackComplete() {
  // Router
  const navigate = useNavigate();
  const { username } = useParams<{ username: string }>();

  // Labels
  const labels = app.getLabels(
    "createPassword",
    "passwordTip",
    "passwordRepeatError",
    "repeatPassword",
    "submit",
    "yourPassword"
  );

  // Refs
  const passwordRef = React.useRef<HTMLInputElement>(null);
  const passwordMethodRef = React.createRef<TextFieldExMethods>();

  const repeatRef = React.useRef<HTMLInputElement>(null);
  const repeatMethodRef = React.createRef<TextFieldExMethods>();

  if (!username) {
    return <NavigateHome />;
  }

  // Decode
  const usernameDecoded = decodeURIComponent(username);
  const id = app.decrypt(usernameDecoded);

  if (!id) {
    return <NavigateHome />;
  }

  // Repeat step
  const repeatStep = (check: boolean = false) => {
    const password = passwordRef.current!;
    if (password.value === "") {
      password.focus();
      return false;
    }

    if (!app.isValidPassword(password.value)) {
      passwordMethodRef.current?.setError(labels.passwordTip);
      password.focus();
      return false;
    }

    if (!check) repeatRef.current?.focus();

    return true;
  };

  // Submit
  const submitClick = async () => {
    if (!repeatStep(true)) {
      return;
    }

    const repeat = repeatRef.current!;
    if (repeat.value === "") {
      repeat.focus();
      return;
    }

    if (repeat.value !== passwordRef.current?.value) {
      repeatMethodRef.current?.setError(labels.passwordRepeatError);
      return;
    }

    const result = await app.authApi.resetPassword({
      id: usernameDecoded,
      password: app.encrypt(app.hash(repeat.value))
    });
    if (result == null) return;

    if (result.ok) {
      // Clear the code
      app.storage.setData(Constants.CodeFieldCallback, undefined);

      // Back to the login page
      navigate(`./../../password/${encodeURIComponent(username)}`);
      return;
    }

    app.alertResult(result);
  };

  return (
    <SharedLayout
      title={labels.createPassword}
      subTitle={<Typography variant="subtitle2">{id}</Typography>}
      homeUrl={"./../../../"}
      buttons={[
        <Button variant="contained" key="next" onClick={submitClick}>
          {labels.submit}
        </Button>
      ]}
    >
      <TextFieldEx
        label={labels.yourPassword}
        showPassword
        autoComplete="new-password"
        autoFocus
        inputRef={passwordRef}
        mRef={passwordMethodRef}
        onEnter={(e) => {
          repeatStep();
          e.preventDefault();
        }}
      />
      <TextFieldEx
        label={labels.repeatPassword}
        showPassword
        inputRef={repeatRef}
        mRef={repeatMethodRef}
        onEnter={(e) => {
          submitClick();
          e.preventDefault();
        }}
      />
      <Typography variant="body2">* {labels.passwordTip}</Typography>
    </SharedLayout>
  );
}
