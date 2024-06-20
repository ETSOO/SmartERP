import { VBox } from '@etsoo/materialui';
import { Button, CircularProgress, TextField } from '@mui/material';
import React from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { app } from '../app/SmartApp';
import { InviteDto } from '../api/dto/auth/InviteDto';
import { SharedLayout } from './SharedLayout';

function Invite() {
  // Router
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();

  // Labels
  const labels = app.getLabels(
    'acceptInvitation',
    'email',
    'organization',
    'inviter',
    'loading',
    'login',
    'register'
  );

  // State
  const [status, setStatus] = React.useState(0);
  const [data, setData] = React.useState<InviteDto>();
  const [subTitle, setSubTitle] = React.useState<string>();

  // Mount
  const isMounted = React.useRef(true);

  // Is loading
  const isLoading = status === 0;

  // Button label
  const buttonLabel =
    status === 0
      ? labels.loading
      : status === 1
      ? labels.register
      : labels.login;

  // Button click handler
  const buttonHandler =
    data == null || isLoading
      ? undefined
      : () => {
          const url =
            (status === 1
              ? './../../login/registerpassword/'
              : './../../?loginid=') +
            encodeURIComponent(app.encrypt(data.identifier, app.name));
          navigate(url);
        };

  // Ready
  React.useEffect(() => {
    if (id == null) return;

    // Labels
    const { inviteMemberExpired, inviteMemberDone, inviteMemberExist } =
      app.getLabels(
        'inviteMemberExpired',
        'inviteMemberDone',
        'inviteMemberExist'
      );

    // Query data
    app.authApi.invite(id).then((result) => {
      // No data or unmounted
      if (result == null || !isMounted.current) return;

      // Decrypt
      result.identifier = app.decrypt(result.identifier, result.inviterName)!;

      // Update data
      setData(result);

      // Validate
      if (result.isExpired) {
        setSubTitle(inviteMemberExpired);
        return;
      }

      if (result.isInvited) {
        setSubTitle(inviteMemberDone);
        return;
      }

      // Cache the id to page data
      app.setPageData({ inviteId: id });

      // Is a current user?
      app.authApi.loginId(result.identifier).then((idResult) => {
        if (idResult == null) return;

        if (idResult.ok) setSubTitle(inviteMemberExist);

        setStatus(idResult.ok ? 2 : 1);
      });
    });
  }, [id]);

  React.useEffect(() => {
    return () => {
      isMounted.current = false;
    };
  }, []);

  return (
    <SharedLayout
      title={labels.acceptInvitation}
      subTitle={subTitle}
      buttons={[
        <Button
          key="submit"
          onClick={buttonHandler}
          variant="outlined"
          disabled={isLoading}
          endIcon={isLoading ? <CircularProgress size={12} /> : undefined}
        >
          {buttonLabel}
        </Button>
      ]}
    >
      {data && (
        <VBox gap={1} width="100%">
          <TextField
            margin="dense"
            variant="standard"
            label={labels.email}
            value={data.identifier.hideEmail()}
            disabled
          />
          <TextField
            margin="dense"
            variant="standard"
            label={labels.inviter}
            value={data.inviterName}
            disabled
          />
          <TextField
            margin="dense"
            variant="standard"
            label={labels.organization}
            value={data.organizationName}
            disabled
          />
        </VBox>
      )}
    </SharedLayout>
  );
}

export default Invite;
