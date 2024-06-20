import React from 'react';
import { Button, Typography } from '@mui/material';
import { SharedLayout } from './SharedLayout';
import { TextFieldEx, TextFieldExMethods } from '@etsoo/materialui';
import { Constants } from '../app/Constants';
import { app } from '../app/SmartApp';
import { Navigate, useNavigate, useParams } from 'react-router-dom';
import { ResetPasswordRQ } from '@etsoo/appscript';

const homeUrl = './../../../';
function NavigateHome() {
  return <Navigate to={homeUrl} replace />;
}

function CallbackComplete() {
  // Router
  const navigate = useNavigate();
  const { username } = useParams<{ username: string }>();

  // Labels
  const labels = app.getLabels(
    'passwordTip',
    'passwordRepeatError',
    'createPassword',
    'yourPassword',
    'repeatPassword',
    'submit'
  );

  // Refs
  const passwordRef = React.useRef<HTMLInputElement>();
  const passwordMethodRef = React.createRef<TextFieldExMethods>();

  const repeatRef = React.useRef<HTMLInputElement>();
  const repeatMethodRef = React.createRef<TextFieldExMethods>();

  const codeId = app.storage.getData<string>(Constants.CodeFieldCallback);

  if (username == null || codeId == null || codeId === '') {
    return <NavigateHome />;
  }

  // Decode
  const usernameDecoded = decodeURIComponent(username);
  const id = app.decrypt(usernameDecoded);

  if (id == null || id === '') {
    return <NavigateHome />;
  }

  // Repeat step
  const repeatStep = (check: boolean = false) => {
    const password = passwordRef.current!;
    if (password.value === '') {
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

  // Submit
  const submitClick = async () => {
    if (!repeatStep(true)) {
      return;
    }

    const repeat = repeatRef.current!;
    if (repeat.value === '') {
      repeat.focus();
      return;
    }

    if (repeat.value !== passwordRef.current?.value) {
      repeatMethodRef.current?.setError(labels.passwordRepeatError);
      return;
    }

    // Submit data
    const data: ResetPasswordRQ = {
      id: usernameDecoded,
      deviceId: app.deviceId,
      codeId,
      password: app.encrypt(app.hash(repeat.value)),
      region: app.region
    };

    const result = await app.authApi.resetPassword(data);
    if (result == null) return;

    if (result.ok) {
      // Clear the code
      app.storage.setData(Constants.CodeFieldCallback, undefined);

      // Back to the login page
      navigate(`./../../password/${username}`);
      return;
    }

    app.alertResult(result);
  };

  return (
    <SharedLayout
      title={labels.createPassword}
      subTitle={<Typography variant="subtitle2">{id}</Typography>}
      buttons={[
        <Button variant="contained" key="next" onClick={submitClick}>
          {labels.submit}
        </Button>
      ]}
    >
      <TextFieldEx
        label={labels.yourPassword}
        showPassword
        autoComplete="new-password"
        autoFocus
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
        inputRef={repeatRef}
        ref={repeatMethodRef}
        onEnter={(e) => {
          submitClick();
          e.preventDefault();
        }}
      />
      <Typography variant="body2">* {labels.passwordTip}</Typography>
    </SharedLayout>
  );
}

export default CallbackComplete;
