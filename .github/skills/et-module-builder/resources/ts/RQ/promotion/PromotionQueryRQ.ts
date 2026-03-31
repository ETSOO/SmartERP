import { PromotionListRQ } from "./PromotionListRQ";

/**
 * Promotion query request data
 * 促销查询请求数据
 */
export type PromotionQueryRQ = PromotionListRQ & {
  /**
   * Coupons applied start
   * 优惠券使用量起使
   */
  couponsAppliedStart?: number;

  /**
   * Coupons applied end
   * 优惠卷使用量结束
   */
  couponsAppliedEnd?: number;
};
