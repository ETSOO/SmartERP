import { QueryRQ, UserRole } from '@etsoo/appscript';

/**
 * Member query request data
 */
export type MemberQueryRQ = QueryRQ & {
  /**
   * Name
   */
  name?: string;

  /**
   * External id
   */
  externalId?: string;

  /**
   * Organization id
   */
  organizationId?: number;

  /**
   * Role
   */
  role?: UserRole;
};
