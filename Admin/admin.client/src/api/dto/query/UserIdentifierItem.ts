import { UserIdentifierType } from "@etsoo/appscript";

/**
 * User Identifier Item
 * 用户标识项
 */
export type UserIdentifierItem = {
  /**
   * Id
   * 编号
   */
  id: number;

  /**
   * Type
   * 类型
   */
  type: UserIdentifierType;

  /**
   * Value
   * 值
   */
  value: string;
};
