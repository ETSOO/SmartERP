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
import {
  AuthCodeAction,
  AuthCodeSendResult,
  ValidateRQ
} from "@etsoo/smarterp-core";
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
    "codeSent",
    "enterCodeTip",
    "verification",
    "resending",
    "enterCode",
    "submit"
  );

  // State
  const [feedback, setFeedback] = React.useState<string>();

  // Refs
  const inputRef = React.useRef<HTMLInputElement>(null);
  const mRef = React.createRef<TextFieldExMethods>();

  if (!username) {
    return <NavigateHome />;
  }

  // Decode
  const usernameDecoded = decodeURIComponent(username);

  // Button ref
  const countdownRef = React.useRef<HTMLButtonElement | null>(null);

  // Id
  const [id, setId] = React.useState("");

  // Callback way
  const isEmail = id.includes("@");

  // Code id
  let codeId = app.storage.getData<string>(Constants.CodeFieldCallback);

  // Tip
  const enterCodeTip = labels.enterCodeTip.format(id.hideEmail());

  // Resending
  const resending = async () => {
    let result: AuthCodeSendResult | undefined;
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

    const recipient = result.data.recipient;
    setFeedback(labels.codeSent.format(recipient.hideEmail()));

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

    const code = await app.encrypt(input.value);

    const rq: ValidateRQ = {
      deviceId: app.deviceId,
      id: codeId,
      code
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

  React.useEffect(() => {
    app.decrypt(usernameDecoded).then((id) => {
      if (!id) {
        navigate(homeUrl);
        return;
      }

      setId(id);
    });
  }, [usernameDecoded]);

  // Only check the original codeId, no dependency
  React.useEffect(() => {
    if (!!id && !codeId) {
      countdownRef.current?.click();
    }
  }, [id]);

  return (
    <SharedLayout
      title={labels.verification}
      subTitle={<Typography variant="subtitle2">{enterCodeTip}</Typography>}
      homeUrl={"./../../../"}
      buttons={[
        <CountdownButton
          variant="outlined"
          key="resending"
          ref={countdownRef}
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
        helperText={feedback}
      />
    </SharedLayout>
  );
}
