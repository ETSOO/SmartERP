import { IApiPayload, AuthApi as AuthApiBase, LoginRQ } from "@etsoo/appscript";
import { SmartERPLoginResult } from "@etsoo/materialui";
import { TokenResultPayload } from "./dto/auth/TokenResultPayload";
import { RegisterUserData } from "./dto/auth/RegisterUserData";
import { CompleteRegisterRQ } from "./rq/auth/CompleteRegisterRQ";
import { ValidateRQ } from "@etsoo/smarterp-core";

/**
 * Authentication API
 */
export class AuthApi extends AuthApiBase {
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
      ? this.app.getResponseToken(
          payload.response,
          AuthApiBase.HeaderTokenField
        )
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
   * Login
   * @param rq Request data
   * @param payload Payload
   * @returns Result
   */
  login(rq: LoginRQ, payload?: IApiPayload<SmartERPLoginResult>) {
    return this.loginBase(rq, payload);
  }

  /**
   * Validate email callback password code
   * @param rq Request data
   * @param payload Payload
   * @returns Result
   */
  validateEmailCallback(rq: ValidateRQ, payload?: TokenResultPayload) {
    return this.api.put("Auth/ValidateEmailCallback", rq, payload);
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
   * Validate mobile callback password code
   * @param rq Request data
   * @param payload Payload
   * @returns Result
   */
  validateMobileCallback(rq: ValidateRQ, payload?: TokenResultPayload) {
    return this.api.put("Auth/ValidateMobileCallback", rq, payload);
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
