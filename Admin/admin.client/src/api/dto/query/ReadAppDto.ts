import { EntityStatus } from "@etsoo/appscript";
import { AppUrl, IdentityType } from "@etsoo/smarterp-core";

/**
 * Read app data
 */
export type ReadAppDto = {
  /**
   * Id
   */
  id: number;

  /**
   * App global name
   */
  name: string;

  /**
   * Global app id
   */
  appId: number;

  /**
   * URLs
   */
  urls: AppUrl[];

  /**
   * App key
   */
  appKey?: string;

  /**
   * Local name
   */
  localName?: string;

  /**
   * Local URLs
   */
  localUrls?: AppUrl[];

  /**
   * Expiry
   */
  expiry?: Date | string;

  /**
   * Expiry days
   */
  expiryDays?: number;

  /**
   * Identity type
   * 身份类型
   */
  identityType: IdentityType;

  /**
   * Status
   */
  status: EntityStatus;

  /**
   * Creation
   * 创建时间
   */
  creation: string | Date;
};
