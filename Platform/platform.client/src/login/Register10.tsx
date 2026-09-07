import React from "react";
import {
  CountdownButton,
  HBox,
  LoadingButton,
  TextFieldEx
} from "@etsoo/materialui";
import { SharedLayout } from "./SharedLayout";
import { app } from "../app/SmartApp";
import { useNavigate } from "react-router-dom";
import { useSearchParamsEx } from "@etsoo/react";
import { AuthCodeAction } from "@etsoo/smarterp-core";
import Button from "@mui/material/Button";

export default function Register20() {
  // Navigate
  const navigate = useNavigate();

  // Token
  const { openid, token } = useSearchParamsEx({
    openid: "string",
    token: "string"
  });
  app.setLoginToken(token);

  // Labels
  const labels = app.getLabels(
    "back",
    "confirmClear",
    "email",
    "nextStep",
    "noCodeId",
    "oneTimePin",
    "oneTimePinEmailTip",
    "resending",
    "useMobile",
    "verifyEmail"
  );

  // States
  const [isReady, setReady] = React.useState(false);

  // Refs
  const inputRef = React.useRef<HTMLInputElement>(null);
  const codeRef = React.useRef<HTMLInputElement>(null);
  const codeIdRef = React.useRef<string>(undefined);

  // Send verification code
  const sendCode = React.useCallback(async () => {
    const email = inputRef.current?.value.trim();
    if (!email) {
      return 0;
    }

    // Send verification code
    const result = await app.authCodeApi.sendEmail({
      action: AuthCodeAction.UserRegistrationEmailCode,
      email: await app.encrypt(email)
    });

    if (result == null) return 0;

    if (!result.ok) {
      app.alertResult(result);
      return 0;
    }

    codeIdRef.current = result.data?.id;

    if (codeRef.current) {
      codeRef.current.value = "";
      codeRef.current.focus();
    }

    return 120;
  }, []);

  // Next button click
  const nextClick = async () => {
    if (isReady && codeRef.current) {
      // Verify code
      const code = codeRef.current.value.trim();
      if (code == null || code === "") {
        codeRef.current.focus();
        return;
      }

      if (!codeIdRef.current) {
        app.notifier.alert(labels.noCodeId);
        return;
      }

      // Verify
      const result = await app.authApi.validateEmailRegistration({
        deviceId: app.deviceId,
        id: codeIdRef.current,
        code: await app.encrypt(code)
      });

      if (result == null) return;

      if (result.ok) {
        app.setLoginToken(result.data?.token);
        const nextUrl = result.data?.simpleRegistration
          ? "./../register30"
          : "./../register20";
        navigate(nextUrl);
      } else {
        app.alertResult(result);
      }
    } else {
      // Input check
      const input = inputRef.current;
      if (input == null) return;

      if (!input.checkValidity()) {
        input.focus();
        return;
      }

      const result = await sendCode();
      if (result > 0) {
        setReady(true);
      }
    }
  };

  React.useEffect(() => {
    if (inputRef.current) {
      inputRef.current.value = openid ?? "";
    }
  }, [openid]);

  React.useEffect(() => {
    // Focus
    if (codeRef.current) codeRef.current.focus();
    else inputRef.current?.focus();
  }, [isReady]);

  return (
    <SharedLayout
      title={labels.verifyEmail}
      homeUrl="./../../"
      buttons={[
        <Button variant="outlined" key="back" onClick={() => navigate(-1)}>
          {labels.back}
        </Button>,
        <LoadingButton variant="contained" key="next" onClick={nextClick}>
          {labels.nextStep}
        </LoadingButton>
      ]}
      liveMinutes={60}
    >
      <TextFieldEx
        label={labels.email}
        inputRef={inputRef}
        autoFocus
        autoCorrect="off"
        autoCapitalize="none"
        autoComplete="email"
        type="email"
        slotProps={{ input: { inputMode: "email" } }}
        required
        showClear
        onChange={() => setReady(false)}
        onEnter={(e) => {
          nextClick();
          e.preventDefault();
        }}
        readOnly={isReady}
        onClear={(doClear) => {
          if (isReady) {
            app.notifier.confirm(
              labels.confirmClear.format(labels.email),
              undefined,
              (result) => {
                if (result) {
                  doClear();
                }
              }
            );
          } else {
            doClear();
          }
        }}
      />
      {isReady && (
        <HBox spacing={0.5} sx={{ alignItems: "flex-start" }}>
          <TextFieldEx
            label={labels.oneTimePin}
            inputRef={codeRef}
            autoCorrect="off"
            autoCapitalize="none"
            showClear
            helperText={labels.oneTimePinEmailTip}
            onEnter={(e) => {
              nextClick();
              e.preventDefault();
            }}
          />
          <CountdownButton
            variant="outlined"
            sx={{ flexShrink: 0 }}
            initState={120}
            onAction={() => sendCode()}
          >
            {labels.resending}
          </CountdownButton>
        </HBox>
      )}
      <HBox sx={{ justifyContent: "center", width: "100%", paddingTop: 2 }}>
        <Button onClick={() => navigate("./../register20")}>
          {labels.useMobile}
        </Button>
      </HBox>
    </SharedLayout>
  );
}
