import { QueryRQ } from '@etsoo/appscript';
import { MemberIdentity } from '../../dto/member/MemberIdentity';

/**
 * Product query request data
 */
export type ProductQueryRQ = QueryRQ & {
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
