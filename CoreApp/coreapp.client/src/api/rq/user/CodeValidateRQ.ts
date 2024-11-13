/**
 * Code validate request data
 */
export type CodeValidateRQ = {
  /**
   * Device id
   */
  deviceId: string;

  /**
   * Code id
   */
  id: string;

  /**
   * Code encrypted
   */
  code: string;
};
