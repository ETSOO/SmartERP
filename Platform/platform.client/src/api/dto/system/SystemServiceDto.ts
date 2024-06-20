/**
 * System service data
 */
export type SystemServiceDto = {
  /**
   * Uid
   */
  id: string;

  /**
   * App id
   */
  appId: number;

  /**
   * Name
   */
  name: string;

  /**
   * Logo
   */
  logo?: string;

  /**
   * Web URL
   */
  webUrl: string;

  /**
   * Entity status
   */
  entityStatus: number;
};
