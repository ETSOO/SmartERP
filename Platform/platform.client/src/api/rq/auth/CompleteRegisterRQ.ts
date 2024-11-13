import { AuthRequest } from "@etsoo/appscript";

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
   * Family name
   */
  familyName?: string;

  /**
   * Given name
   */
  givenName?: string;

  /**
   * Country or region
   */
  region: string;

  /**
   * Authorization request data
   */
  auth?: AuthRequest;
};
