import { EntityStatus } from "@etsoo/appscript";

/**
 * Read organization data
 */
export type ReadOrgData = {
  /**
   * Id
   */
  id: number;

  /**
   * Owner id
   * 所有人编号
   */
  ownerId: number;

  /**
   * Owner name
   */
  ownerName: string;

  /**
   * Name
   */
  name: string;

  /**
   * Brand
   */
  brand?: string;

  /**
   * Logo
   */
  logo?: string;

  /**
   * Region
   * 地区
   */
  region: string;

  /**
   * PIN
   */
  pin?: string;

  /**
   * Parent id
   */
  parentId?: number;

  /**
   * Parent name
   */
  parentName?: string;

  /**
   * Creation
   */
  creation: string | Date;

  /**
   * Status
   */
  status: EntityStatus;

  /**
   * Query Keyword
   */
  queryKeyword?: string;

  /**
   * All apps purchased
   */
  apps: number;

  /**
   * All users
   */
  users: number;

  /**
   * All persons
   */
  persons: number;

  /**
   * All orders
   */
  orders: number;
};
