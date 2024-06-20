import { ProductUnit } from '@etsoo/appscript';

export type ProductPurchasedDto = {
  /**
   * Id
   */
  id: string;

  /**
   * Identity
   */
  serviceIdentity: number;

  /**
   * Custom name
   */
  customName?: string;

  /**
   * Name
   */
  name: string;

  /**
   * Price
   */
  price: number;

  /**
   * Product unit
   */
  productUnit: ProductUnit;

  /**
   * Help URL
   */
  helpUrl?: string;

  /**
   * Creation
   */
  creation: Date;

  /**
   * Expiry
   */
  expiry: Date;
};
