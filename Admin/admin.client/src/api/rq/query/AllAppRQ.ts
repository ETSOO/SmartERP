import { IdentityType, QueryRQ } from "@etsoo/appscript";

/**
 * All app request data
 * 所有应用请求数据
 */
export type AllAppRQ = QueryRQ & {
  /**
   * Identity type
   * 身份类型
   */
  identityType?: IdentityType;

  /**
   * App ID
   * 应用编号
   */
  appId?: number;

  /**
   * Organization ID
   * 机构编号
   */
  orgId?: number;

  /**
   * Expiry
   * 到期时间
   */
  expiry?: string | Date;

  /**
   * Expiry days
   * 到期天数
   */
  expiryDays?: number;

  /**
   * Creation start
   * 登记开始时间
   */
  creationStart?: string | Date;

  /**
   * Creation end
   * 登记结束时间
   */
  creationEnd?: string | Date;
};
