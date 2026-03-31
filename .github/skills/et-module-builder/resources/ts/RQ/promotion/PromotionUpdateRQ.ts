import { DataTypes } from "@etsoo/shared";
import { PromotionCreateRQ } from "./PromotionCreateRQ";

/**
 * Promotion update request data
 * 促销更新请求数据
 */
export type PromotionUpdateRQ = DataTypes.EditType<PromotionCreateRQ>;
