import React from 'react';
import { TextFieldEx } from '@etsoo/materialui';
import { Button, Typography } from '@mui/material';
import { SharedLayout } from './SharedLayout';
import { Constants } from '../app/Constants';
import { RegisterRQ } from '../api/rq/auth/RegisterRQ';
import { app } from '../app/SmartApp';
import { Navigate, useNavigate, useParams } from 'react-router-dom';

function RegisterComplete() {
  // Router
  const navigate = useNavigate();
  const { username } = useParams<{ username: string }>();

  // Labels
  var labels = app.getLabels('completeRegistration', 'submit', 'yourname');

  // Name field
  const nameRef = React.useRef<HTMLInputElement>();

  // Password
  const passwordEncrypted = app.storage.getData<string>(
    Constants.FieldRegisterPassword
  );

  // Code id
  const codeId = app.storage.getData<string>(Constants.CodeFieldRegister);

  React.useEffect(() => {
    return () => {
      // Clear the password
      app.storage.setData(Constants.FieldRegisterPassword, undefined);
    };
  });

  if (
    username == null ||
    passwordEncrypted == null ||
    passwordEncrypted === '' ||
    codeId == null ||
    codeId === ''
  ) {
    return <Navigate to={'./../../register'} replace />;
  }

  // Decode
  const usernameDecoded = decodeURIComponent(username);
  const id = app.decrypt(usernameDecoded);

  if (id == null || id === '') {
    return <Navigate to={'./../../register'} replace />;
  }

  // Submit button click
  const submitClick = async () => {
    // Input check
    const input = nameRef.current!;
    const name = input.value.trim();
    if (name === '') {
      input.focus();
      return;
    }

    // Submit data
    const data: RegisterRQ = {
      deviceId: app.deviceId,
      id: usernameDecoded,
      codeId,
      password: passwordEncrypted,
      name,
      region: app.region
    };

    const result = await app.authApi.register(data);
    if (result == null) return;

    if (result.ok) {
      // Remove code ids
      app.storage.setData(Constants.CodeFieldRegister, undefined);

      // Back to the login page
      navigate(`./../../password/${encodeURIComponent(username)}`);
      return;
    }

    app.alertResult(result);
  };

  return (
    <SharedLayout
      title={labels.completeRegistration}
      subTitle={<Typography variant="subtitle2">{id}</Typography>}
      buttons={[
        <Button variant="contained" key="next" onClick={submitClick}>
          {labels.submit}
        </Button>
      ]}
    >
      <TextFieldEx
        label={labels.yourname}
        inputRef={nameRef}
        autoFocus
        autoCorrect="off"
        autoCapitalize="none"
        showClear
        onEnter={(e) => {
          submitClick();
          e.preventDefault();
        }}
      />
    </SharedLayout>
  );
}

export default RegisterComplete;
