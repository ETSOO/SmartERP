/**
 * Send SMS request data
 */
export type SendSMSRQ = {
  /**
   * Device id
   */
  deviceId: string;

  /**
   * Template/action id
   */
  action: number;

  /**
   * Mobile number
   */
  mobile: string;

  /**
   * Country or region
   */
  region?: string;
};
