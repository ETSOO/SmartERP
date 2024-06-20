import { CountdownButton, HBox, TextFieldEx } from '@etsoo/materialui';
import React from 'react';
import { SendSMSRQ } from '../../api/rq/authcode/SendSMSRQ';
import { app } from '../../app/SmartApp';

export function UserMobileAdd() {
  // Labels
  const labels = app.getLabels(
    'mobile',
    'enterCode',
    'resending',
    'send',
    'itemExists'
  );

  // Ref
  const inputRef = React.useRef<HTMLInputElement>();
  const codeRef = React.useRef<HTMLInputElement>();

  // State
  const [resending, setResending] = React.useState(false);
  const [codeId, setCodeId] = React.useState<string>();
  const [exists, setExists] = React.useState(false);

  // Send email
  const send = async () => {
    // Validate
    const mobile = inputRef.current?.value;
    if (mobile == null || !mobile.isDigits(7)) {
      inputRef.current?.focus();
      return 0;
    }

    const data: SendSMSRQ = {
      deviceId: app.deviceId,
      region: app.region,
      mobile: app.encrypt(mobile),
      action: 5
    };

    const result = await app.authCodeApi.sendSMS(data, { showLoading: false });

    // Error, back to normal
    if (result == null) return 0;

    if (!result.ok) {
      // Popup
      app.alertResult(result);
      return 0;
    }

    codeRef.current?.focus();

    setCodeId(result.data?.id);
    if (!resending) setResending(true);

    return 90;
  };

  return (
    <React.Fragment>
      <input type="hidden" name="codeId" value={codeId ?? ''} />
      <TextFieldEx
        name="mobile"
        label={labels.mobile}
        autoCorrect="off"
        autoCapitalize="none"
        inputProps={{ inputMode: 'tel' }}
        inputRef={inputRef}
        showClear
        required
        changeDelay={500}
        onChange={async (event) => {
          const id = event.target.value;
          if (id != null && id.isDigits(7)) {
            const result = await app.authApi.loginId(id, {
              showLoading: false
            });
            if (result == null) return;

            setExists(result.ok);
          } else {
            setExists(false);
          }
        }}
        helperText={exists ? labels.itemExists.format(labels.mobile) : ''}
      />
      <HBox gap={1} marginTop={1}>
        <TextFieldEx
          name="code"
          label={labels.enterCode}
          autoCorrect="off"
          autoCapitalize="none"
          inputProps={{ inputMode: 'numeric' }}
          inputRef={codeRef}
          showClear
          required
        />
        <CountdownButton
          variant="outlined"
          key="resending"
          sx={{ minWidth: 120 }}
          onAction={send}
        >
          {resending ? labels.resending : labels.send}
        </CountdownButton>
      </HBox>
    </React.Fragment>
  );
}
