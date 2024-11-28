import React from "react";
import { Button, Typography } from "@mui/material";
import { SharedLayout } from "./SharedLayout";
import { HBox, TextFieldEx, TextFieldExMethods } from "@etsoo/materialui";
import { app } from "../app/SmartApp";
import { useNavigate } from "react-router-dom";
import { CompleteRegisterRQ } from "../api/rq/auth/CompleteRegisterRQ";
import { AuthRequest } from "@etsoo/appscript";
import { Constants } from "../app/Constants";

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
  const nameRef = React.useRef<HTMLInputElement>();
  const familyNameRef = React.useRef<HTMLInputElement>();
  const givenNameRef = React.useRef<HTMLInputElement>();

  const passwordRef = React.useRef<HTMLInputElement>();
  const passwordMethodRef = React.createRef<TextFieldExMethods>();

  const repeatRef = React.useRef<HTMLInputElement>();
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
    const rq: CompleteRegisterRQ = {
      deviceId: app.deviceId,
      name: name.value,
      familyName: familyNameRef.current?.value,
      givenName: givenNameRef.current?.value,
      password: app.encrypt(app.hash(repeat.value)),
      region: app.region,
      auth
    };

    const [result, refreshToken] = await app.authApi.completeRegister(rq);
    if (result == null) return;

    if (result.ok) {
      if (refreshToken == null || result.data == null) {
        app.notifier.alert(labels.unknownError);
        return;
      }

      if (auth) {
        app.authLogin(refreshToken);
      } else {
        // User login
        app.userLogin(result.data, refreshToken);

        // Navigate to home
        app.toHome(navigate, "./../../home/");
      }
    } else {
      app.alertResult(result, () => {
        // Back to home
        navigate("./../../");
      });
    }
  };

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
            const parts = value.split(" ");
            if (parts.length > 1) {
              familyNameRef.current!.value = parts.pop()!;
              givenNameRef.current!.value = parts.join(" ");
            } else {
              familyNameRef.current!.value = value[0];
              givenNameRef.current!.value = value.substring(1);
            }
          } else {
            familyNameRef.current!.value = "";
            givenNameRef.current!.value = "";
          }
        }}
        required
        showClear
      />
      <HBox gap={1}>
        <TextFieldEx
          label={labels.familyName}
          inputRef={familyNameRef}
          autoFocus
          autoCorrect="off"
          autoCapitalize="none"
          autoComplete="familyName"
          showClear
        />
        <TextFieldEx
          label={labels.givenName}
          inputRef={givenNameRef}
          autoFocus
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
        ref={passwordMethodRef}
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
        ref={repeatMethodRef}
        onEnter={(e) => {
          completeClick();
          e.preventDefault();
        }}
      />
      <Typography variant="caption">* {labels.passwordTip}</Typography>
    </SharedLayout>
  );
}
