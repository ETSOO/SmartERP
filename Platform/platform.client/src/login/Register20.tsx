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

function Register20() {
  // Navigate
  const navigate = useNavigate();

  // Token
  const { token } = useSearchParamsEx({
    token: "string"
  });
  app.setLoginToken(token);

  // Labels
  const labels = app.getLabels(
    "nextStep",
    "mobile",
    "verifyMobileNumber",
    "resending",
    "oneTimePin",
    "oneTimePinMobileTip",
    "noCodeId",
    "confirmClear"
  );

  // States
  const [isReady, setReady] = React.useState(false);

  // Refs
  const inputRef = React.useRef<HTMLInputElement>();
  const codeRef = React.useRef<HTMLInputElement>();
  const codeIdRef = React.useRef<string>();

  // Send verification code
  const sendCode = React.useCallback(async () => {
    // Input check
    const input = inputRef.current;
    if (input == null) return 0;

    if (!input.checkValidity()) {
      input.focus();
      return 0;
    }

    const mobile = input.value.trim();

    // Send verification code
    const result = await app.authApi.sendSMS({
      deviceId: app.deviceId,
      action: 1,
      mobile: app.encrypt(mobile),
      region: app.region
    });

    if (result == null) return 0;

    if (!result.ok) {
      app.alertResult(result);
      return 0;
    }

    codeIdRef.current = result.data?.id;

    return 90;
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
      const result = await app.authApi.validateMobileRegistration({
        deviceId: app.deviceId,
        id: codeIdRef.current,
        code: app.encrypt(code)
      });

      if (result == null) return;

      if (result.ok) {
        app.setLoginToken(result.data?.token);
        navigate("./../register30");
      } else {
        app.alertResult(result);
      }
    } else {
      const result = await sendCode();
      if (result > 0) {
        setReady(true);
      }
    }
  };

  React.useEffect(() => {
    // Focus
    if (codeRef.current) codeRef.current.focus();
    else inputRef.current?.focus();
  }, [isReady]);

  return (
    <SharedLayout
      title={labels.verifyMobileNumber}
      buttons={[
        <LoadingButton variant="contained" key="next" onClick={nextClick}>
          {labels.nextStep}
        </LoadingButton>
      ]}
      liveMinutes={60}
    >
      <TextFieldEx
        label={labels.mobile}
        inputRef={inputRef}
        autoFocus
        autoCorrect="off"
        autoCapitalize="none"
        autoComplete="mobile"
        type="tel"
        inputProps={{ inputMode: "tel" }}
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
              labels.confirmClear.format(labels.mobile),
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
        <HBox gap={0.5} alignItems="flex-start">
          <TextFieldEx
            label={labels.oneTimePin}
            inputRef={codeRef}
            autoCorrect="off"
            autoCapitalize="none"
            showClear
            helperText={labels.oneTimePinMobileTip}
            onEnter={(e) => {
              nextClick();
              e.preventDefault();
            }}
          />
          <CountdownButton
            variant="outlined"
            sx={{ flexShrink: 0 }}
            initState={90}
            onAction={() => sendCode()}
          >
            {labels.resending}
          </CountdownButton>
        </HBox>
      )}
    </SharedLayout>
  );
}

export default Register20;
