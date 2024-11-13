/**
 * Login history data
 */
export type LoginHistoryDto = {
  /**
   * Id
   */
  id: number;

  /**
   * Device id
   */
  deviceId: number;

  /**
   * Device name
   */
  deviceName: string;

  /**
   * Json data
   */
  jsonData: string;

  /**
   * Language
   */
  language: string;

  /**
   * Region
   */
  region: string;

  /**
   * Time zone
   */
  timezone: string;

  /**
   * Login organization
   */
  organizationId: number;

  /**
   * Service id
   */
  serviceId: string;

  /**
   * Login success
   */
  success: boolean;

  /**
   * Creation
   */
  creation: Date;

  /**
   * Fail reason
   */
  reason: string;
};
