import { EntityStatus } from "@etsoo/appscript";

/**
 * Read user data
 */
export type ReadUserDto = {
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
   * Family name
   * 姓
   */
  familyName?: string;

  /**
   * Given name
   * 名
   */
  givenName?: string;

  /**
   * Latin family name
   * 拉丁姓
   */
  latinFamilyName?: string;

  /**
   * Latin given name
   * 拉丁名
   */
  latinGivenName?: string;

  /**
   * Avatar
   */
  avatar?: string;

  /**
   * PIN
   * 证件号码
   */
  pin?: string;

  /**
   * Status
   */
  status: EntityStatus;

  /**
   * Creation
   */
  creation: string | Date;
};
