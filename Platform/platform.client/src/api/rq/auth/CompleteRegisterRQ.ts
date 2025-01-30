import { AuthRequest } from "@etsoo/appscript";

/**
 * Complete register input request data
 */
export type CompleteRegisterInputRQ = {
  /**
   * Password
   */
  password: string;

  /**
   * Name
   */
  name: string;

  /**
   * Family name
   */
  familyName?: string;

  /**
   * Given name
   */
  givenName?: string;

  /**
   * Authorization request data
   */
  auth?: AuthRequest;
};

/**
 * Complete register request data
 */
export type CompleteRegisterRQ = CompleteRegisterInputRQ & {
  /**
   * Device id
   */
  deviceId: string;

  /**
   * Country or region
   */
  region: string;

  /**
   * Timezone
   */
  timezone: string;
};
