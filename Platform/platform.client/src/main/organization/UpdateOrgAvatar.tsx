import {
  CommonPage,
  UserAvatarEditor,
  UserAvatarEditorToBlob
} from '@etsoo/materialui';
import { useParamsEx } from '@etsoo/react';
import { Stack } from '@mui/material';
import React from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { app } from '../../app/SmartApp';

function UpdateOrgAvatar() {
  // Route
  const navigate = useNavigate();
  const { id = 0 } = useParamsEx({ id: 'number' });

  const location = useLocation();
  const avatar: string | undefined = location.state;

  // Labels
  const labels = app.getLabels('logo');

  const handleDone = async (
    canvas: HTMLCanvasElement,
    toBlob: UserAvatarEditorToBlob,
    type: string
  ) => {
    // Photo blob
    const blob = await toBlob(canvas, type, 1);

    // Form data
    const form = new FormData();
    form.append('avatar', blob);

    var result = await app.orgApi.uploadAvatar(id, form);
    if (result == null) return;

    // Refresh token to get the updated avatar
    navigate(`./../../view/${id}`);
  };

  React.useEffect(() => {
    // Page title
    app.setPageKey('logo');
  }, []);

  return (
    <CommonPage sx={{ width: 'fit-content' }}>
      <Stack direction={{ xs: 'column', sm: 'column', md: 'row' }} spacing={1}>
        {avatar == null ? (
          <React.Fragment />
        ) : (
          <img
            src={avatar}
            alt={labels.logo}
            style={{
              width: '320px',
              height: '160px',
              border: '1px solid #666'
            }}
          />
        )}
        <UserAvatarEditor
          width={320}
          height={160}
          maxWidth={640}
          onDone={handleDone}
        />
      </Stack>
    </CommonPage>
  );
}

export default UpdateOrgAvatar;
