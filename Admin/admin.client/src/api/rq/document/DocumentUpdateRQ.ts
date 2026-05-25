import { DataTypes } from "@etsoo/shared";
import { DocumentCreateRQ } from "./DocumentCreateRQ";

/**
 * Document update request data
 * 文档更新请求数据
 */
export type DocumentUpdateRQ = DataTypes.EditType<DocumentCreateRQ>;
