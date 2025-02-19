import { EntityStatus } from "@etsoo/appscript";
import { IdentityType } from "@etsoo/smarterp-core";

/**
 * Application query data
 * 应用查询数据
 */
export type AllAppDto = {
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
   * Local name
   * 本地名称
   */
  localName?: string;

  /**
   * Identity type
   * 身份类型
   */
  identityType: IdentityType;

  /**
   * Organization name
   * 机构名称
   */
  orgName: string;

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
