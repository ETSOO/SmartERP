import React from "react";
import { SharedLayout } from "./SharedLayout";
import { HBox, TextFieldEx, TextFieldExMethods } from "@etsoo/materialui";
import { app } from "../app/SmartApp";
import { useNavigate } from "react-router-dom";
import { AuthRequest } from "@etsoo/appscript";
import { Constants } from "../app/Constants";
import Button from "@mui/material/Button";
import Typography from "@mui/material/Typography";
import { Utils } from "@etsoo/shared";

export default function RegisterPassword() {
  // Router
  const navigate = useNavigate();

  // Labels
  const labels = app.getLabels(
    "passwordTip",
    "passwordRepeatError",
    "createPassword",
    "completeRegistration",
    "yourPassword",
    "repeatPassword",
    "yourname",
    "familyName",
    "givenName",
    "unknownError"
  );

  // Refs
  const nameRef = React.useRef<HTMLInputElement>(null);
  const familyNameRef = React.useRef<HTMLInputElement>(null);
  const givenNameRef = React.useRef<HTMLInputElement>(null);

  const passwordRef = React.useRef<HTMLInputElement>(null);
  const passwordMethodRef = React.createRef<TextFieldExMethods>();

  const repeatRef = React.useRef<HTMLInputElement>(null);
  const repeatMethodRef = React.createRef<TextFieldExMethods>();

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

  // Complete
  const completeClick = async () => {
    const name = nameRef.current;
    if (name == null || name.value === "") {
      name?.focus();
      return;
    }

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

    const auth = app.storage.getData<AuthRequest>(Constants.AuthRequestField);

    // Complete the registration
    const [result, refreshToken] = await app.authApi.completeRegister({
      name: name.value,
      familyName: familyNameRef.current?.value,
      givenName: givenNameRef.current?.value,
      password: app.encrypt(app.hash(repeat.value)),
      auth
    });
    if (result == null) return;

    if (result.ok) {
      if (refreshToken == null || result.data == null) {
        app.notifier.alert(labels.unknownError);
        return;
      }

      // Login success
      app.loginComplete(auth, result.data, refreshToken);
    } else {
      app.alertResult(result, () => {
        // Back to home
        navigate("./../../");
      });
    }
  };

  React.useEffect(() => {
    // Check authorized
    if (!app.registrationAuthorized) {
      navigate("./../../");
    }
  }, [app.registrationAuthorized]);

  return (
    <SharedLayout
      title={labels.createPassword}
      buttons={[
        <Button variant="contained" key="next" onClick={completeClick}>
          {labels.completeRegistration}
        </Button>
      ]}
    >
      <TextFieldEx
        label={labels.yourname}
        inputRef={nameRef}
        autoFocus
        autoCorrect="off"
        autoCapitalize="none"
        autoComplete="name"
        onChange={(event) => {
          const value = event.target.value.trim();
          if (value) {
            const nd = Utils.parseName(value);
            familyNameRef.current!.value = nd.familyName ?? "";
            givenNameRef.current!.value = nd.givenName ?? "";
          } else {
            familyNameRef.current!.value = "";
            givenNameRef.current!.value = "";
          }
        }}
        required
        showClear
      />
      <HBox spacing={1}>
        <TextFieldEx
          label={labels.familyName}
          inputRef={familyNameRef}
          autoCorrect="off"
          autoCapitalize="none"
          autoComplete="familyName"
          showClear
        />
        <TextFieldEx
          label={labels.givenName}
          inputRef={givenNameRef}
          autoCorrect="off"
          autoCapitalize="none"
          autoComplete="givenName"
          showClear
        />
      </HBox>
      <TextFieldEx
        label={labels.yourPassword}
        showPassword
        autoComplete="new-password"
        required
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
        required
        inputRef={repeatRef}
        mRef={repeatMethodRef}
        onEnter={(e) => {
          completeClick();
          e.preventDefault();
        }}
      />
      <Typography variant="caption">* {labels.passwordTip}</Typography>
    </SharedLayout>
  );
}
