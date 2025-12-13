import React from "react";
import { SharedLayout } from "./SharedLayout";
import {
  CountdownButton,
  TextFieldEx,
  TextFieldExMethods
} from "@etsoo/materialui";
import { Constants } from "../app/Constants";
import { app } from "../app/SmartApp";
import { Navigate, useNavigate, useParams } from "react-router-dom";
import { IdActionResult } from "@etsoo/shared";
import { AuthCodeAction, ValidateRQ } from "@etsoo/smarterp-core";
import Typography from "@mui/material/Typography";
import Button from "@mui/material/Button";

const homeUrl = "./../../../";
function NavigateHome() {
  return <Navigate to={homeUrl} replace />;
}

export default function RegisterVerify() {
  // Router
  const navigate = useNavigate();
  const { username } = useParams<{ username: string }>();

  // Labels
  const labels = app.getLabels(
    "enterCodeTip",
    "verification",
    "resending",
    "enterCode",
    "submit"
  );

  // Refs
  const inputRef = React.useRef<HTMLInputElement>(null);
  const mRef = React.createRef<TextFieldExMethods>();

  if (!username) {
    return <NavigateHome />;
  }

  // Decode
  const usernameDecoded = decodeURIComponent(username);
  const id = app.decrypt(usernameDecoded);

  if (!id) {
    return <NavigateHome />;
  }

  // Callback way
  const isEmail = id.includes("@");

  // Code id
  let codeId = app.storage.getData<string>(Constants.CodeFieldCallback);

  // Tip
  const enterCodeTip = labels.enterCodeTip.format(id.hideEmail());

  // Resending
  const resending = async () => {
    let result: IdActionResult<string> | undefined;
    if (isEmail) {
      result = await app.authCodeApi.sendEmail({
        email: usernameDecoded,
        action: AuthCodeAction.UserCallbackEmailCode
      });
    } else {
      result = await app.authCodeApi.sendSMS({
        mobile: usernameDecoded,
        action: AuthCodeAction.UserCallbackSMSCode
      });
    }

    // Error, back to normal
    if (result == null) return 0;

    if (!result.ok || result.data?.id == null) {
      app.alertResult(result);
      return 180;
    }

    codeId = result.data.id;
    app.storage.setData(Constants.CodeFieldCallback, codeId);

    mRef.current?.setError(undefined);
    if (inputRef.current) {
      inputRef.current.value = "";
      inputRef.current.focus();
    }

    return 90;
  };

  // Submit
  const submit = async () => {
    const input = inputRef.current!;
    if (input.value === "" || codeId == null) {
      input.focus();
      return;
    }

    const rq: ValidateRQ = {
      deviceId: app.deviceId,
      id: codeId,
      code: app.encrypt(input.value)
    };

    const result = isEmail
      ? await app.authApi.validateEmailCallback(rq)
      : await app.authApi.validateMobileCallback(rq);

    if (result == null) return;

    if (result.ok) {
      app.setLoginToken(result.data?.token);
      navigate(`./../../callbackcomplete/${encodeURIComponent(username)}`);
    } else {
      app.alertResult(result);
    }
  };

  return (
    <SharedLayout
      title={labels.verification}
      subTitle={<Typography variant="subtitle2">{enterCodeTip}</Typography>}
      buttons={[
        <CountdownButton
          variant="outlined"
          key="resending"
          ref={(instance: HTMLButtonElement | null) => {
            if (!codeId) instance?.click();
          }}
          onAction={resending}
        >
          {labels.resending}
        </CountdownButton>,
        <Button variant="contained" key="submit" onClick={submit}>
          {labels.submit}
        </Button>
      ]}
    >
      <TextFieldEx
        label={labels.enterCode}
        autoCorrect="off"
        autoCapitalize="none"
        slotProps={{ htmlInput: { inputMode: "numeric" } }}
        mRef={mRef}
        inputRef={inputRef}
        showClear
        onEnter={(e) => {
          submit();
          e.preventDefault();
        }}
      />
    </SharedLayout>
  );
}
