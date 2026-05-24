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
   * Has parameters or not
   * 是否有参数
   */
  hasParameters?: boolean;
};
