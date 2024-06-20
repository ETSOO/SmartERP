/**
 * Send email request data
 */
export type SendEmailRQ = {
  /**
   * Device id
   */
  deviceId: string;

  /**
   * Template/action id
   */
  action: number;

  /**
   * Email address
   */
  email: string;

  /**
   * Country or region
   */
  region?: string;

  /**
   * Timezone
   */
  timezone?: string;
};
