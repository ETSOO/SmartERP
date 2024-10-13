import React from "react";
import { Button, Typography } from "@mui/material";
import { SharedLayout } from "./SharedLayout";
import { TextFieldEx, TextFieldExMethods } from "@etsoo/materialui";
import { app } from "../app/SmartApp";
import { useNavigate } from "react-router-dom";
import { CompleteRegisterRQ } from "../api/rq/auth/CompleteRegisterRQ";

function RegisterPassword() {
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
    "unknownError"
  );

  // Refs
  const nameRef = React.useRef<HTMLInputElement>();

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

    // Complete the registration
    const rq: CompleteRegisterRQ = {
      deviceId: app.deviceId,
      name: name.value,
      password: app.encrypt(app.hash(repeat.value)),
      region: app.region
    };

    const [result, refreshToken] = await app.authApi.completeRegister(rq);
    if (result == null) return;

    if (result.ok) {
      if (refreshToken == null || result.data == null) {
        app.notifier.alert(labels.unknownError);
        return;
      }

      // User login
      app.userLogin(result.data, refreshToken, true);
      navigate("./../../home/");
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
        required
        showClear
      />
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

export default RegisterPassword;
