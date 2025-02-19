import { QueryRQ } from "@etsoo/appscript";

/**
 * Audit History Request
 * 审计历史请求
 */
export type AuditHistoryRQ = QueryRQ & {
  /**
   * User id
   * 用户编号
   */
  userId?: number;

  /**
   * Organization id
   * 机构编号
   */
  orgId?: number;

  /**
   * Kind
   * 类型
   */
  kind?: string;

  /**
   * Target id
   * 目标编号
   */
  targetId?: number;

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
