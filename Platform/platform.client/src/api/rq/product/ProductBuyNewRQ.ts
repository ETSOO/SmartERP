/**
 * Buy product or service to new organization request data
 */
export type ProductBuyNewRQ = {
  /**
   * Product id
   */
  id: number;

  /**
   * New organization region
   */
  region: string;

  /**
   * New organization name
   */
  name: string;

  /**
   * New organization identifier
   */
  identifier?: string;

  /**
   * Client device id
   */
  deviceId: string;
};
