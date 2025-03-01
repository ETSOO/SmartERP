import { StatusQueryRQ } from "@etsoo/appscript";

/**
 * User list request data
 */
export type UserListRQ = StatusQueryRQ & {
  /**
   * Organization id
   */
  orgId?: number;

  /**
   * Exclude self  or not
   */
  excludeSelf?: boolean;
};
