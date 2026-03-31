import { EntityStatus } from "@etsoo/appscript";
import { PromotionCodeData } from "../../dto/promotion/PromotionCodeData";

/**
 * Create promotion request data
 * 创建促销请求数据
 */
export type PromotionCreateRQ = PromotionCodeData & {
  /**
   * Promotion code
   * 促销代码
   */
  code: number;

  /**
   * Title
   * 标题
   */
  title: string;

  /**
   * Currency
   * 币种
   */
  currency: string;

  /**
   * Valid start date
   * 有效开始时间
   */
  validStart: string | Date;

  /**
   * Valid start end
   * 有效结束时间
   */
  validEnd: string | Date;

  /**
   * Max coupons
   * 最大优惠券
   */
  coupons?: number;

  /**
   * Stackable
   * 促销是否可叠加
   */
  stackable?: boolean;

  /**
   * Status
   * 状态
   */
  status?: EntityStatus;

  /**
   * Order index
   * 排序数
   */
  orderIndex?: number;
};
