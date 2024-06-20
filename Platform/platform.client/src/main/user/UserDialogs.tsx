import { DomUtils } from '@etsoo/shared';
import { app } from '../../app/SmartApp';
import { UserEmailAdd } from './UserEmailAdd';
import { UserMobileAdd } from './UserMobileAdd';

/**
 * User dialogs
 */
export namespace UserDialogs {
  /**
   * Add email address
   */
  export function addEmail(callback?: () => void) {
    // Labels
    const labels = app.getLabels('add', 'noCodeId');

    app.showInputDialog({
      title: labels.add,
      message: '',
      fullScreen: app.smDown,
      callback: async (form) => {
        // Cancelled
        if (form == null) {
          return;
        }

        // Form data
        const data = DomUtils.dataAs(new FormData(form), {
          email: 'string',
          code: 'string',
          codeId: 'string'
        });

        if (data.email == null) {
          DomUtils.setFocus('email', form);
          return false;
        }

        if (data.codeId == null) {
          app.notifier.alert(labels.noCodeId);
          return false;
        }

        if (data.code == null || !data.code.isDigits(4)) {
          DomUtils.setFocus('code', form);
          return false;
        }

        // Submit
        const result = await app.userApi.verifyEmail(data.codeId, data.code, {
          showLoading: false
        });
        if (result == null) return;

        if (result.ok) {
          if (callback) callback();
          return;
        }

        app.alertResult(result);
        return false;
      },
      inputs: <UserEmailAdd />
    });
  }

  export function addMobile(callback?: () => void) {
    // Labels
    const labels = app.getLabels('add', 'noCodeId');

    app.showInputDialog({
      title: labels.add,
      message: '',
      fullScreen: app.smDown,
      callback: async (form) => {
        // Cancelled
        if (form == null) {
          return;
        }

        // Form data
        const data = DomUtils.dataAs(new FormData(form), {
          mobile: 'string',
          code: 'string',
          codeId: 'string'
        });

        if (data.mobile == null) {
          DomUtils.setFocus('mobile', form);
          return false;
        }

        if (data.codeId == null) {
          app.notifier.alert(labels.noCodeId);
          return false;
        }

        if (data.code == null || !data.code.isDigits(4)) {
          DomUtils.setFocus('code', form);
          return false;
        }

        // Submit
        const result = await app.userApi.verifyMobile(data.codeId, data.code, {
          showLoading: false
        });
        if (result == null) return;

        if (result.ok) {
          if (callback) callback();
          return;
        }

        app.alertResult(result);
        return false;
      },
      inputs: <UserMobileAdd />
    });
  }
}
