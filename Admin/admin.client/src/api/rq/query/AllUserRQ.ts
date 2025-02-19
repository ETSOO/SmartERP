import { QueryRQ } from "@etsoo/appscript";

/**
 * Query all users request data
 * 查询所有用户请求数据
 */
export type AllUserRQ = QueryRQ & {
  /**
   * Organization id
   * 机构编号
   */
  orgId?: number;

  /**
   * Inviter id
   * 邀请人编号
   */
  inviterId?: number;

  /**
   * Is frozen or not
   * 是否冻结
   */
  isFrozen?: boolean;

  /**
   * Identifier
   * 识别号
   */
  identifier?: string;

  /**
   * Creation start
   * 登记开始时间
   */
  creationStart?: Date;

  /**
   * Creation end
   * 登记结束时间
   */
  creationEnd?: Date;
};
