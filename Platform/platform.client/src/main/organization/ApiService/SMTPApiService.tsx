import { InputField, OptionBool } from '@etsoo/materialui';
import { Grid } from '@mui/material';
import React from 'react';
import { app } from '../../../app/SmartApp';
import { ApiServiceProps } from './ApiServiceProps';

export function SMTPApiService(props: ApiServiceProps) {
  // Destruct
  const { data, handleChange } = props;

  // Labels
  const labels = app.getLabels(
    'apiServiceSMTPAppId',
    'apiServiceSMTPAppSecret',
    'apiServiceSMTPHost',
    'apiServiceSMTPPort',
    'apiServiceSMTPSenderEmail',
    'apiServiceSMTPSenderName',
    'apiServiceSMTPUseSsl'
  );

  return (
    <React.Fragment>
      <Grid item xs={12} sm={9}>
        <InputField
          fullWidth
          required
          label={labels.apiServiceSMTPHost}
          name="settings.host"
          inputProps={{ maxLength: 256 }}
          value={data.settings?.host ?? ''}
          onChange={handleChange}
        />
      </Grid>
      <Grid item xs={12} sm={3}>
        <InputField
          fullWidth
          required
          label={labels.apiServiceSMTPPort}
          name="settings.port"
          inputProps={{ type: 'number' }}
          value={data.settings?.port ?? ''}
          onChange={handleChange}
        />
      </Grid>
      <Grid item xs={12} sm={6}>
        <InputField
          fullWidth
          required
          label={labels.apiServiceSMTPAppId}
          name="appId"
          inputProps={{ maxLength: 50 }}
          value={data.appId ?? ''}
          onChange={handleChange}
        />
      </Grid>
      <Grid item xs={12} sm={6}>
        <InputField
          fullWidth
          required
          label={labels.apiServiceSMTPAppSecret}
          name="appSecret"
          inputProps={{ type: 'password', maxLength: 50 }}
          value={data.appSecret ?? ''}
          onChange={handleChange}
        />
      </Grid>
      <Grid item xs={12} sm={9}>
        <InputField
          fullWidth
          required
          label={labels.apiServiceSMTPSenderEmail}
          name="settings.senderEmail"
          inputProps={{ type: 'email', maxLength: 256 }}
          value={data.settings?.senderEmail ?? ''}
          onChange={handleChange}
        />
      </Grid>
      <Grid item xs={12} sm={3}>
        <InputField
          fullWidth
          required
          label={labels.apiServiceSMTPSenderName}
          name="settings.senderName"
          inputProps={{ maxLength: 128 }}
          value={data.settings?.senderName ?? ''}
          onChange={handleChange}
        />
      </Grid>
      <Grid item xs={6} sm={3}>
        <OptionBool
          name="settings.useSsl"
          label={labels.apiServiceSMTPUseSsl}
          variant="outlined"
          fullWidth
          defaultValue={data.settings?.useSsl ?? ''}
          onChange={handleChange}
        />
      </Grid>
    </React.Fragment>
  );
}
