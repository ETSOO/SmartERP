import {
  IApiPayload,
  AuthApi as AuthApiBase,
  LoginRQ,
  StringIdResultPayload,
  AuthRequest
} from "@etsoo/appscript";
import { SmartERPLoginResult } from "@etsoo/materialui";
import { InviteDto } from "./dto/auth/InviteDto";
import { SendEmailRQ } from "./rq/auth/SendEmailRQ";
import { SendSMSRQ } from "./rq/auth/SendSMSRQ";
import { ValidateRQ } from "./rq/auth/ValidateRQ";
import { TokenResultPayload } from "./dto/auth/TokenResultPayload";
import { RegisterUserData } from "./dto/auth/RegisterUserData";
import { CompleteRegisterRQ } from "./rq/auth/CompleteRegisterRQ";

/**
 * Authentication API
 */
export class AuthApi extends AuthApiBase {
  /**
   * Authorization request
   * @param auth Authorization request data
   * @param payload Payload
   */
  authRequest(auth: AuthRequest, payload?: IApiPayload<string>) {
    return this.api.post("Auth/AuthRequest", auth, payload);
  }

  /**
   * Complete register
   * @param rq Request data
   * @param payload Payload
   * @returns Result
   */
  async completeRegister(
    rq: CompleteRegisterRQ,
    payload?: IApiPayload<SmartERPLoginResult>
  ): Promise<[SmartERPLoginResult | undefined, string | null]> {
    payload ??= {};
    const result = await this.api.put("Auth/CompleteRegister", rq, payload);
    const refreshToken = result?.ok
      ? this.app.getResponseToken(payload.response)
      : null;
    return [result, refreshToken];
  }

  /**
   * Get log in URL
   * @param ac Auth client
   * @param payload Payload
   * @returns Result
   */
  getAuthLogInUrl(ac: string, payload?: IApiPayload<string>) {
    return this.api.get(
      `OAuth2/${ac}/GetLogInUrl?region=${
        this.app.region
      }&device=${encodeURIComponent(this.app.deviceId)}`,
      undefined,
      payload
    );
  }

  /**
   * Get sign up URL
   * @param ac Auth client
   * @param payload Payload
   * @returns Result
   */
  getAuthSignUpUrl(ac: string, payload?: IApiPayload<string>) {
    return this.api.get(
      `OAuth2/${ac}/GetSignUpUrl?region=${
        this.app.region
      }&device=${encodeURIComponent(this.app.deviceId)}`,
      undefined,
      payload
    );
  }

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
   * Send email
   * @param rq Request data
   * @param payload Payload
   * @returns Result
   */
  sendEmail(rq: SendEmailRQ, payload?: StringIdResultPayload) {
    return this.api.put("Auth/SendEmail", rq, payload);
  }

  /**
   * Send email
   * @param rq Request data
   * @param payload Payload
   * @returns Result
   */
  sendSMS(rq: SendSMSRQ, payload?: StringIdResultPayload) {
    return this.api.put("Auth/SendSMS", rq, payload);
  }

  /**
   * Validate email registration code
   * @param rq Request data
   * @param payload Payload
   * @returns Result
   */
  validateEmailRegistration(rq: ValidateRQ, payload?: TokenResultPayload) {
    return this.api.put("Auth/ValidateEmailRegistration", rq, payload);
  }

  /**
   * Validate mobile registration code
   * @param rq Request data
   * @param payload Payload
   * @returns Result
   */
  validateMobileRegistration(rq: ValidateRQ, payload?: TokenResultPayload) {
    return this.api.put("Auth/ValidateMobileRegistration", rq, payload);
  }

  /**
   * View register data
   * @param payload Payload
   * @returns Result
   */
  viewRegisterData(payload?: IApiPayload<RegisterUserData>) {
    return this.api.get("Auth/ViewRegisterData", undefined, payload);
  }
}
