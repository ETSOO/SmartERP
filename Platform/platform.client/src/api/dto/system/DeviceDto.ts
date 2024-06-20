/**
 * Device data
 */
export type DeviceDto = {
  /**
   * Device id
   */
  id: number;

  /**
   * Name
   */
  name: string;

  /**
   * Enabled
   */
  enabled: boolean;

  /**
   * Last login time
   */
  lastLogin?: Date;
};
