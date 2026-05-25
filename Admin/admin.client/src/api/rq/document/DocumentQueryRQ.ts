import { SystemDocumentListRQ } from "@etsoo/smarterp-core";

/**
 * Document query request data
 * 文档查询请求数据
 */
export type DocumentQueryRQ = SystemDocumentListRQ & {
  /**
   * Organizaton id
   * 机构编号
   */
  orgId?: number;

  /**
   * System template or not
   * 系统模板与否
   */
  systemTemplate?: boolean;

  /**
   * Has parameters or not
   * 是否有参数
   */
  hasParameters?: boolean;
};
