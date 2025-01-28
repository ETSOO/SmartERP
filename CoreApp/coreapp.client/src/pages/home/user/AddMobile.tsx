import {
  CommonPage,
  CountdownButton,
  HBox,
  LoadingButton,
  TextFieldEx,
  VBox
} from "@etsoo/materialui";
import { app } from "../../../app/MyApp";
import { AuthCodeAction, usePageData } from "@etsoo/smarterp-core";
import React from "react";
import { useNavigate } from "react-router-dom";

export default function AddMobile() {
  // Navigate
  const navigate = useNavigate();

  // Labels
  const labels = app.getLabels(
    "addMobile",
    "confirmClear",
    "mobile",
    "nextStep",
    "noCodeId",
    "oneTimePin",
    "oneTimePinMobileTip",
    "resending"
  );

  // States
  const [isReady, setReady] = React.useState(false);

  // Refs
  const inputRef = React.useRef<HTMLInputElement>();
  const codeRef = React.useRef<HTMLInputElement>();
  const codeIdRef = React.useRef<string>();

  // Send verification code
  const sendCode = React.useCallback(async () => {
    const mobile = inputRef.current?.value.trim();
    if (!mobile) return 0;

    // Send verification code
    const result = await app.core.authCodeApi.sendSMS({
      deviceId: app.deviceId,
      action: AuthCodeAction.UserVerificationSMSCode,
      mobile: app.encrypt(mobile),
      region: app.region
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
      const result = await app.core.userApi.addMobile({
        deviceId: app.deviceId,
        id: codeIdRef.current,
        code: app.encrypt(code)
      });

      if (result == null) return;

      if (result.ok) {
        navigate("./../");
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

  // Page data hook
  usePageData(app, labels.addMobile, []);

  return (
    <CommonPage maxWidth="xs">
      <VBox spacing={2}>
        <TextFieldEx
          label={labels.mobile}
          inputRef={inputRef}
          autoFocus
          autoCorrect="off"
          autoCapitalize="none"
          autoComplete="mobile"
          type="tel"
          slotProps={{ input: { inputMode: "tel" } }}
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
              initState={120}
              onAction={() => sendCode()}
            >
              {labels.resending}
            </CountdownButton>
          </HBox>
        )}
        <LoadingButton variant="contained" key="next" onClick={nextClick}>
          {labels.nextStep}
        </LoadingButton>
      </VBox>
    </CommonPage>
  );
}
