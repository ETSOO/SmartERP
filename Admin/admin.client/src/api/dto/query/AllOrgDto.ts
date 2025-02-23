import { EntityStatus } from "@etsoo/appscript";

/**
 * Organization query data
 * 机构查询数据
 */
export type AllOrgDto = {
  /**
   * Id
   * 编号
   */
  id: number;

  /**
   * Name
   * 名称
   */
  name: string;

  /**
   * Apps purchased
   * 已购买应用数
   */
  apps: number;

  /**
   * Users
   * 用户数
   */
  users: number;

  /**
   * Brand
   * 品牌
   */
  brand?: string;

  /**
   * PIN
   * 编号
   */
  pin?: string;

  /**
   * Owner
   * 所有人
   */
  owner: string;

  /**
   * Status
   * 状态
   */
  status: EntityStatus;

  /**
   * Creation
   * 创建时间
   */
  creation: string | Date;
};
