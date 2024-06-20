/**
 * Register request data
 */
export type RegisterRQ = {
  /**
   * Device id
   */
  deviceId: string;

  /**
   * Email or mobile
   */
  id: string;

  /**
   * Verification code id
   */
  codeId: string;

  /**
   * Password
   */
  password: string;

  /**
   * Name
   */
  name: string;

  /**
   * Country or region
   */
  region: string;
};
