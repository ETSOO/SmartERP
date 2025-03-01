import { EntityStatus } from "@etsoo/appscript";
import { DataTypes } from "@etsoo/shared";
import { UserIdentifierItem } from "./UserIdentifierItem";

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

  /**
   * Frozen time
   */
  frozenTime?: string | Date;

  /**
   * Organization count
   */
  orgs: number;

  /**
   * Organization list
   */
  orgList: DataTypes.IdNameItem[];

  /**
   * Device count
   */
  devices: number;

  /**
   * Device list
   */
  deviceList: DataTypes.IdNameItem[];

  /**
   * Identifier list
   */
  identifierList: UserIdentifierItem[];
};
