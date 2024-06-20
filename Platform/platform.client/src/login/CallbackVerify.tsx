import React from 'react';
import { Button, Typography } from '@mui/material';
import { SharedLayout } from './SharedLayout';
import {
  CountdownButton,
  TextFieldEx,
  TextFieldExMethods
} from '@etsoo/materialui';
import { IdActionResult } from '@etsoo/appscript';
import { Constants } from '../app/Constants';
import { app } from '../app/SmartApp';
import { Navigate, useNavigate, useParams } from 'react-router-dom';

const homeUrl = './../../../';
function NavigateHome() {
  return <Navigate to={homeUrl} replace />;
}

function RegisterVerify() {
  // Router
  const navigate = useNavigate();
  const { username } = useParams<{ username: string }>();

  // Labels
  const labels = app.getLabels(
    'enterCodeTip',
    'verification',
    'resending',
    'enterCode',
    'submit'
  );

  // Refs
  const inputRef = React.useRef<HTMLInputElement>();
  const mRef = React.createRef<TextFieldExMethods>();

  if (username == null) {
    return <NavigateHome />;
  }

  // Decode
  const usernameDecoded = decodeURIComponent(username);
  const id = app.decrypt(usernameDecoded);

  if (id == null || id === '') {
    return <NavigateHome />;
  }

  // Code id
  let codeId = app.storage.getData<string>(Constants.CodeFieldCallback);

  // Tip
  const enterCodeTip = labels.enterCodeTip.format(id.hideEmail());

  // Resending
  const resending = async () => {
    let result: IdActionResult<string> | undefined;
    if (id.indexOf('@') === -1) {
      result = await app.authCodeApi.sendSMS({
        deviceId: app.deviceId,
        region: app.region,
        mobile: usernameDecoded,
        action: 3
      });
    } else {
      result = await app.authCodeApi.sendEmail({
        deviceId: app.deviceId,
        region: app.region,
        email: usernameDecoded,
        action: 4,
        timezone: app.getTimeZone()
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
      inputRef.current.value = '';
      inputRef.current.focus();
    }

    return 90;
  };

  // Submit
  const submit = async () => {
    const input = inputRef.current!;
    if (input.value === '' || codeId == null) {
      input.focus();
      return;
    }

    const result = await app.authCodeApi.validate({
      deviceId: app.deviceId,
      id: codeId,
      code: app.encrypt(input.value)
    });

    if (result == null) return;

    if (!result.ok) {
      mRef.current?.setError(result.title);
      return;
    }

    navigate(`./../../callbackcomplete/${username}`);
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
            if (codeId == null || codeId === '') instance?.click();
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
        inputProps={{ inputMode: 'numeric' }}
        ref={mRef}
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

export default RegisterVerify;
