/**
 * Complete register request data
 */
export type CompleteRegisterRQ = {
  /**
   * Device id
   */
  deviceId: string;

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
