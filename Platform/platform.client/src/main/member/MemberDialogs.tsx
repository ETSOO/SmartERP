import { IActionResult, UserRole } from '@etsoo/appscript';
import {
  ComboBox,
  EmailInput,
  HBox,
  TabBox,
  TooltipClick,
  VBox
} from '@etsoo/materialui';
import { DomUtils } from '@etsoo/shared';
import { Button, TextField, Typography } from '@mui/material';
import React from 'react';
import { NavigateFunction } from 'react-router-dom';
import { app } from '../../app/SmartApp';
import { InviteRQ } from '../../api/rq/member/InviteRQ';
import { MyInvitationCodeRQ } from '../../api/rq/member/MyInvitationCodeRQ';
import { AppCache } from '../../app/AppCache';

/**
 * Member dialogs
 */
export namespace MemberDialogs {
  /**
   * Accept invitation
   * @param inviteId Invitation id
   * @param navigate Navigate function
   */
  export function accept(inviteId: string, navigate: NavigateFunction) {
    // Labels
    const labels = app.getLabels(
      'acceptInvitation',
      'acceptInvitationMessage',
      'inviter',
      'organization',
      'preferredName'
    );

    app.authApi.invite(inviteId).then((data) => {
      if (data == null) return;

      app.showInputDialog({
        title: labels.acceptInvitation,
        message: labels.acceptInvitationMessage,
        fullScreen: app.smDown,
        callback: async (form) => {
          // Cancelled
          if (form == null) {
            // Clear the id
            app.setPageData({ inviteId: undefined });
            return;
          }

          const localNameInput = form.elements.namedItem(
            'localName'
          ) as HTMLInputElement;

          const localName = localNameInput.value;
          if (localName === '') {
            localNameInput.focus();
            return false;
          }

          var result = await app.memberApi.acceptInvitation(
            inviteId,
            localName,
            {
              showLoading: false
            }
          );

          if (result == null) return;

          if (result.ok && result.data?.id != null) {
            // Clear the id
            app.setPageData({ inviteId: undefined });

            if (result.data?.id === app.userData?.organization) {
              // Refresh token
              if (await app.refreshToken()) {
                navigate(app.addRootUrl('/home/organization/all'));
              }
            } else if (await app.orgApi.switch(result.data?.id)) {
              // Refresh token
              if (await app.refreshToken()) {
                navigate(app.addRootUrl('/home/organization/all'));
              }
            }

            return;
          }

          app.alertResult(result);
        },
        inputs: (
          <VBox gap={1} width="100%">
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
            <TextField
              name="localName"
              margin="dense"
              variant="standard"
              label={labels.preferredName}
              defaultValue={app.userData?.name}
              required
              inputProps={{ maxLength: 128 }}
            />
          </VBox>
        )
      });
    });
  }

  /**
   * Invite member
   * @param organizationId Organization id
   * @param navigate Navigate function
   * @param callback Callback
   */
  export function invite(
    organizationId: number,
    navigate: NavigateFunction,
    callback?: () => void
  ) {
    // Labels
    const labels = app.getLabels(
      'inviteMember',
      'inviteMemberMessage',
      'inviteMemberMessageCode',
      'inviteResult',
      'email',
      'message',
      'role',
      'invitationCode'
    );

    // Roles, UserRole.User and items below it
    const roles = app.getRoles(2 * UserRole.User - 1);

    app.showInputDialog({
      title: labels.inviteMember,
      message: '',
      fullScreen: app.smDown,
      callback: async (form) => {
        // Cancelled
        if (form == null) {
          return;
        }

        // Form data
        const data = DomUtils.dataAs(new FormData(form), {
          kind: 'number',
          role: 'number',
          email: 'string[]',
          code: 'string[]',
          message: 'string'
        });

        // Role
        const role = data.role;
        if (role == null) {
          DomUtils.setFocus('roleInput', form);
          return false;
        }

        if (data.kind === 1) {
          // Invitation codes
          const codes = data.code?.filter(
            (code, index, array) =>
              code.length >= 30 && array.indexOf(code) === index
          );

          if (codes == null || codes.length === 0) {
            // No valid code provided
            DomUtils.setFocus('code', form);
            return false;
          }

          // Submit
          const result = await app.memberApi.inviteByCode(
            {
              organizationId,
              role,
              codes
            },
            {
              showLoading: false
            }
          );
          if (result == null) return;

          if (result.ok) {
            AppCache.removeMemberCache();
            if (callback) callback();
            else navigate(app.addRootUrl('/home/member/all')); // Not sure where call it
            return;
          }

          // Get all errors
          const detail = result.detail;
          if (detail) {
            const errorItems: React.ReactNode[] = [];
            const errors: IActionResult[] = JSON.parse(detail);
            errors.forEach((error, index) => {
              errorItems.push(
                <Typography key={error.type ?? index}>
                  {app.formatResult(error)}
                </Typography>
              );
            });
            app.notifier.alert(<VBox>{errorItems}</VBox>);
          } else {
            app.alertResult(result);
          }

          return false;
        } else {
          // Emails
          const emails = data.email?.filter(
            (email, index, array) =>
              email.includes('@') && array.indexOf(email) === index
          );

          if (emails == null || emails.length === 0) {
            // No valid email provided
            DomUtils.setFocus('email', form);
            return false;
          }

          // Request data
          const rq: InviteRQ = {
            organizationId,
            role,
            emails,
            message: data.message,
            timezone: app.getTimeZone()
          };

          // Submit
          const result = await app.memberApi.invite(rq, { showLoading: false });
          if (result == null) return;

          if (result.ok) {
            app.notifier.succeed(
              labels.inviteResult.format(result.data?.emails.join(', ') ?? ''),
              undefined,
              () => {
                AppCache.removeMemberCache();
                if (callback) callback();
                else navigate(app.addRootUrl('/home/member/all'));
              }
            );
            return;
          }

          app.alertResult(result);
          return false;
        }
      },
      inputs: (
        <React.Fragment>
          <ComboBox
            options={roles}
            name="role"
            label={labels.role}
            idValue={8}
            inputVariant="standard"
            inputMargin="dense"
            inputRequired
          />
          <TabBox
            inputName="kind"
            tabs={[
              {
                label: labels.email,
                children: (
                  <React.Fragment>
                    {' '}
                    <EmailInput
                      autoFocus
                      name="email"
                      label={labels.email}
                      required
                      variant="standard"
                      margin="dense"
                    />
                    <EmailInput
                      name="email"
                      label={labels.email}
                      variant="standard"
                      margin="dense"
                    />
                    <TextField
                      autoFocus
                      margin="dense"
                      name="message"
                      label={labels.message}
                      fullWidth
                      variant="standard"
                      inputProps={{ maxLength: 256 }}
                    />
                    <Typography variant="caption">
                      {labels.inviteMemberMessage}
                    </Typography>
                  </React.Fragment>
                )
              },
              {
                label: labels.invitationCode,
                children: (
                  <React.Fragment>
                    <TextField
                      autoFocus
                      margin="dense"
                      name="code"
                      label={labels.invitationCode}
                      required
                      fullWidth
                      variant="standard"
                      inputProps={{ maxLength: 128 }}
                    />
                    <TextField
                      autoFocus
                      margin="dense"
                      name="code"
                      label={labels.invitationCode}
                      fullWidth
                      variant="standard"
                      inputProps={{ maxLength: 128 }}
                    />
                    <TextField
                      autoFocus
                      margin="dense"
                      name="code"
                      label={labels.invitationCode}
                      fullWidth
                      variant="standard"
                      inputProps={{ maxLength: 128 }}
                    />
                    <Typography variant="caption">
                      {labels.inviteMemberMessageCode}
                    </Typography>
                  </React.Fragment>
                )
              }
            ]}
          />
        </React.Fragment>
      )
    });
  }

  /**
   * My invitation code
   */
  export function myInvitationCode() {
    // Labels
    const labels = app.getLabels(
      'myInvitationCode',
      'myInvitationCodeTip',
      'organizationName',
      'copy',
      'completeTip',
      'preferredName'
    );

    app.showInputDialog({
      title: labels.myInvitationCode,
      message: labels.myInvitationCodeTip,
      fullScreen: app.smDown,
      callback: async (form) => {
        // Cancelled
        if (form == null) {
          return;
        }

        // Form data
        const data = DomUtils.dataAs(new FormData(form), {
          organization: 'string',
          localName: 'string'
        });

        // Organization
        if (data.organization == null) {
          DomUtils.setFocus('organization', form);
          return false;
        }

        if (data.localName == null) {
          DomUtils.setFocus('localName', form);
          return false;
        }

        // Request data
        const rq: MyInvitationCodeRQ = {
          deviceId: app.deviceId,
          organization: data.organization,
          localName: data.localName
        };

        // Submit
        const result = await app.memberApi.myInvitationCode(rq, {
          showLoading: false
        });
        if (result == null) return;

        if (result.ok) {
          const codeEncrypted = result.data!.code;
          const code = app.decrypt(codeEncrypted);

          app.notifier.succeed(
            labels.myInvitationCode,
            undefined,
            undefined,
            120,
            {
              inputs: (
                <HBox gap={1} alignItems="center">
                  <Typography>{code}</Typography>
                  <TooltipClick title={labels.completeTip.format(labels.copy)}>
                    {(openTooltip) => (
                      <Button
                        variant="outlined"
                        size="small"
                        onClick={() => {
                          navigator.clipboard?.writeText(result.data!.code);
                          openTooltip();
                        }}
                      >
                        {labels.copy}
                      </Button>
                    )}
                  </TooltipClick>
                </HBox>
              )
            }
          );
          return;
        }

        app.alertResult(result);
      },
      inputs: (
        <React.Fragment>
          <TextField
            autoFocus
            margin="dense"
            name="organization"
            required
            label={labels.organizationName}
            fullWidth
            variant="standard"
            inputProps={{ maxLength: 128 }}
          />
          <TextField
            name="localName"
            margin="dense"
            variant="standard"
            label={labels.preferredName}
            defaultValue={app.userData?.name}
            required
            fullWidth
            inputProps={{ maxLength: 128 }}
          />
        </React.Fragment>
      )
    });
  }
}
