import {
  IApiPayload,
  ResultPayload,
  AuthApi as AuthApiBase,
  LoginRQ
} from '@etsoo/appscript';
import { SmartERPLoginResult } from '@etsoo/materialui';
import { InviteDto } from './dto/auth/InviteDto';
import { RegisterRQ } from './rq/auth/RegisterRQ';

/**
 * Authentication API
 */
export class AuthApi extends AuthApiBase {
  /**
   * Invite
   * @param id Id
   * @param payload Payload
   * @returns Result
   */
  invite(id: string, payload?: IApiPayload<InviteDto>) {
    return this.api.get<InviteDto>(`Auth/Invite/${id}`, undefined, payload);
  }

  /**
   * Login
   * @param rq Request data
   * @param payload Payload
   * @returns Result
   */
  login(rq: LoginRQ, payload?: IApiPayload<SmartERPLoginResult>) {
    return this.loginBase(rq, payload);
  }

  /**
   * Register
   * @param rq Request data
   * @param payload Payload
   * @returns Result
   */
  register(rq: RegisterRQ, payload?: ResultPayload) {
    return this.api.post('Auth/Register', rq, payload);
  }
}
