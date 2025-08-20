import {
  CommonPage,
  CountdownButton,
  HBox,
  LoadingButton,
  TextFieldEx,
  VBox
} from "@etsoo/materialui";
import { app } from "../../../app/MyApp";
import { AuthCodeAction, usePageDataEmpty } from "@etsoo/smarterp-core";
import React from "react";
import { useNavigate } from "react-router-dom";

export default function AddEmail() {
  // Navigate
  const navigate = useNavigate();

  // Labels
  const labels = app.getLabels(
    "confirmClear",
    "email",
    "nextStep",
    "noCodeId",
    "oneTimePin",
    "oneTimePinEmailTip",
    "resending"
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
    const result = await app.core.authCodeApi.sendEmail({
      action: AuthCodeAction.UserVerificationEmailCode,
      email: app.encrypt(email)
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
      const result = await app.core.userApi.addEmail({
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
  usePageDataEmpty(app);

  return (
    <CommonPage maxWidth="xs">
      <VBox spacing={2}>
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
          <HBox gap={0.5} alignItems="flex-start">
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
        <LoadingButton variant="contained" key="next" onClick={nextClick}>
          {labels.nextStep}
        </LoadingButton>
      </VBox>
    </CommonPage>
  );
}
