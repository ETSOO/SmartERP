import { EntityStatus } from "@etsoo/appscript";

/**
 * All user query data
 * 所有用户查询数据
 */
export type AllUserDto = {
  /**
   * Id
   */
  id: number;

  /**
   * Name
   */
  name: string;

  /**
   * Preferred name
   * 首选名字
   */
  preferredName?: string;

  /**
   * PIN
   * 证件号码
   */
  pin?: string;

  /**
   * Organizations joined
   * 加入的组织
   */
  orgs: number;

  /**
   * Status
   */
  status: EntityStatus;

  /**
   * Creation
   */
  creation: string | Date;
};
