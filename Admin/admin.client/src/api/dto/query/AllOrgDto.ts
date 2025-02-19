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
