import { QueryRQ } from '@etsoo/appscript';
import { MemberIdentity } from '../../dto/member/MemberIdentity';

/**
 * Product query purchased request data
 */
export type ProductQueryPurchasedRQ = QueryRQ & {
  /**
   * Name
   */
  name?: string;

  /**
   * Service identity
   */
  serviceIdentity?: MemberIdentity;

  /**
   * Language
   */
  language?: string;
};
