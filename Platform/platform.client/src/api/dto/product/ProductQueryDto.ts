import { ProductUnit, PublicProductDto } from '@etsoo/appscript';
import { MemberIdentity } from '../member/MemberIdentity';

export type ProductQueryDto = PublicProductDto & {
  /**
   * Identity
   */
  serviceIdentity: MemberIdentity;

  /**
   * Help URL
   */
  helpUrl?: string;

  /**
   * Creation
   */
  creation: Date;

  /**
   * Price
   */
  price: number;

  /**
   * Product unit
   */
  productUnit: ProductUnit;

  /**
   * Entity status
   */
  entityStatus: number;
};
