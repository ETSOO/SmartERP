import React from 'react';
import { Button, Typography } from '@mui/material';
import { SharedLayout } from './SharedLayout';
import { TextFieldEx, TextFieldExMethods } from '@etsoo/materialui';
import { Constants } from '../app/Constants';
import { app } from '../app/SmartApp';
import { Link, Navigate, useNavigate, useParams } from 'react-router-dom';

function NavigateHome() {
  return <Navigate to="./../../../" replace />;
}

function RegisterPassword() {
  // Router
  const navigate = useNavigate();
  const { username } = useParams<{ username: string }>();

  // Labels
  const labels = app.getLabels(
    'passwordTip',
    'passwordRepeatError',
    'createPassword',
    'back',
    'nextStep',
    'yourPassword',
    'repeatPassword'
  );

  // Refs
  const passwordRef = React.useRef<HTMLInputElement>();
  const passwordMethodRef = React.createRef<TextFieldExMethods>();

  const repeatRef = React.useRef<HTMLInputElement>();
  const repeatMethodRef = React.createRef<TextFieldExMethods>();

  if (username == null) {
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

  // Next
  const nextClick = () => {
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

    // Hold the password
    app.storage.setData(
      Constants.FieldRegisterPassword,
      app.encrypt(app.hash(repeat.value))
    );

    // Continue
    navigate(`./../../registerverify/${encodeURIComponent(username)}`);
  };

  return (
    <SharedLayout
      title={labels.createPassword}
      subTitle={id.hideEmail()}
      buttons={[
        <Button
          variant="outlined"
          component={Link}
          key="back"
          to="./../../register"
        >
          {labels.back}
        </Button>,
        <Button variant="contained" key="next" onClick={nextClick}>
          {labels.nextStep}
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
          nextClick();
          e.preventDefault();
        }}
      />
      <Typography variant="caption">* {labels.passwordTip}</Typography>
    </SharedLayout>
  );
}

export default RegisterPassword;
