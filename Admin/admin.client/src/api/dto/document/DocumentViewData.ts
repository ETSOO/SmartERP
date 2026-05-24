import { SystemDocumentViewData } from "@etsoo/smarterp-core";

/**
 * Document view data
 * 文档浏览数据
 */
export type DocumentViewData = SystemDocumentViewData & {
  /**
   * Organization id
   * 机构编号
   */
  orgId?: number;
};
