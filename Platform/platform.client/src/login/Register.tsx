import React from 'react';
import { TextFieldEx } from '@etsoo/materialui';
import { Button } from '@mui/material';
import { SharedLayout } from './SharedLayout';
import { app } from '../app/SmartApp';
import { Link, useNavigate, useParams } from 'react-router-dom';

function Register() {
  // Navigate
  const navigate = useNavigate();

  let { username } = useParams<{ username: string }>();
  if (username) username = app.decrypt(decodeURIComponent(username));

  // Labels
  const labels = app.getLabels(
    'userFound',
    'register',
    'back',
    'nextStep',
    'loginId'
  );

  // Login id field
  const loginRef = React.useRef<HTMLInputElement>();

  // Next button click
  const nextClick = async () => {
    // Input check
    const input = loginRef.current!;
    const id = input.value.trim();
    if (id == null || id === '') {
      input.focus();
      return;
    }

    // Encrypted id
    const idEncrypted = app.encrypt(id);

    const result = await app.authApi.loginId(id);

    if (result != null) {
      if (result.ok) {
        // Account registered
        app.notifier.confirm(labels.userFound, undefined, (value) => {
          if (value) {
            navigate('./../password/' + encodeURIComponent(idEncrypted));
          } else {
            input.focus();
          }
        });
      } else {
        // Continue
        navigate('./../registerpassword/' + encodeURIComponent(idEncrypted));
      }
    }
  };

  return (
    <SharedLayout
      title={labels.register}
      buttons={[
        <Button variant="outlined" component={Link} key="back" to={'./../../'}>
          {labels.back}
        </Button>,
        <Button variant="contained" key="next" onClick={nextClick}>
          {labels.nextStep}
        </Button>
      ]}
    >
      <TextFieldEx
        label={labels.loginId}
        inputRef={loginRef}
        autoFocus
        autoCorrect="off"
        autoCapitalize="none"
        inputProps={{ inputMode: 'email' }}
        showClear
        defaultValue={username}
        onEnter={(e) => {
          nextClick();
          e.preventDefault();
        }}
      />
    </SharedLayout>
  );
}

export default Register;
