import {
  EntityApi,
  IApiPayload,
  ResultPayload,
  TiplistRQ
} from '@etsoo/appscript';
import { ReactAppType } from '@etsoo/materialui';
import { ListType } from '@etsoo/shared';
import { LoginHistoryDto } from './dto/user/LoginHistoryDto';
import { PrivateDataDto } from './dto/user/PrivateDataDto';
import { CodeValidateRQ } from './rq/user/CodeValidateRQ';
import { LoginHistoryQueryRQ } from './rq/user/LoginHistoryQueryRQ';

/**
 * User API
 */
export class UserApi extends EntityApi {
  /**
   * Constructor
   * @param app Application
   */
  constructor(app: ReactAppType) {
    super('User', app);
  }

  /**
   * Change password
   * @param oldPassword Ole password
   * @param password New password
   * @param payload Payload
   * @returns Result
   */
  async changePassword(
    oldPassword: string,
    password: string,
    payload?: ResultPayload
  ) {
    const rq = {
      deviceId: this.app.deviceId,
      oldPassword: this.app.encrypt(this.app.hash(oldPassword)),
      password: this.app.encrypt(this.app.hash(password))
    };
    return await this.api.put('User/ChangePassword', rq, payload);
  }

  /**
   * Delete email
   * @param id Entity id
   * @param payload Payload
   * @returns Result
   */
  deleteEmail(id: number, payload?: ResultPayload) {
    return this.api.delete(`User/DeleteEmail/${id}`, undefined, payload);
  }

  /**
   * Delete mobile
   * @param id Entity id
   * @param payload Payload
   * @returns Result
   */
  deleteMobile(id: number, payload?: ResultPayload) {
    return this.api.delete(`User/DeleteMobile/${id}`, undefined, payload);
  }

  /**
   * Device list
   * @param rq Request data
   * @param payload Payload
   * @returns Result
   */
  deviceList(rq: TiplistRQ, payload?: IApiPayload<ListType[]>) {
    return this.api.post('User/DeviceList', rq, payload);
  }

  /**
   * Set email as default
   * @param id Email entity id
   * @param payload Payload
   * @returns Result
   */
  emailSetAsDefault(id: number, payload?: ResultPayload) {
    return this.api.put(`User/EmailSetAsDefault/${id}`, undefined, payload);
  }

  /**
   * Set mobile as default
   * @param id Mobile entity id
   * @param payload Payload
   * @returns Result
   */
  mobileSetAsDefault(id: number, payload?: ResultPayload) {
    return this.api.put(`User/MobileSetAsDefault/${id}`, undefined, payload);
  }

  /**
   * Get current user's private data
   * @param payload Payload
   * @returns Result
   */
  getPrivateData(payload?: IApiPayload<PrivateDataDto>) {
    return this.api.get('User/GetPrivateData', undefined, payload);
  }

  /**
   * Login history
   * @param rq Request data
   * @param payload Payload
   * @returns Result
   */
  loginHistory(
    rq: LoginHistoryQueryRQ,
    payload?: IApiPayload<LoginHistoryDto[]>
  ) {
    return this.api.post('User/LoginHistory', rq, payload);
  }

  /**
   * Upload avatar
   * @param data Avatar form data
   * @param payload Payload
   * @returns Result
   */
  uploadAvatar(data: FormData, payload?: IApiPayload<string>) {
    return this.api.put('User/UploadAvatar', data, payload);
  }

  /**
   * Verify email
   * @param id Id
   * @param code Code
   * @param payload Payload
   * @returns Result
   */
  async verifyEmail(id: string, code: string, payload?: ResultPayload) {
    const data: CodeValidateRQ = {
      deviceId: this.app.deviceId,
      id,
      code: this.app.encrypt(code)
    };
    return await this.api.put('User/VerifyEmail', data, payload);
  }

  /**
   * Verify mobile
   * @param id Id
   * @param code Code
   * @param payload Payload
   * @returns Result
   */
  async verifyMobile(id: string, code: string, payload?: ResultPayload) {
    const data: CodeValidateRQ = {
      deviceId: this.app.deviceId,
      id,
      code: this.app.encrypt(code)
    };
    return await this.api.put('User/VerifyMobile', data, payload);
  }
}
