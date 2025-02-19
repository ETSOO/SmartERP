import { QueryRQ } from "@etsoo/appscript";

/**
 * Query all organizations request data
 * 查询所有机构请求数据
 */
export type AllOrgRQ = QueryRQ & {
  /**
   * Parent org. ID
   * 父机构编号
   */
  parentId?: number;

  /**
   * Owner id
   * 所有人编号
   */
  ownerId?: number;

  /**
   * PIN
   * 唯一编号
   */
  pin?: string;

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
