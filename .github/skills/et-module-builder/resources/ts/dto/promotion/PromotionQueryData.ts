import { EntityStatus } from "@etsoo/appscript";

/**
 * Promotion query data
 * 促销查询数据
 */
export type PromotionQueryData = {
  /**
   * Id
   * 编号
   */
  id: number;

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
   * Minimum spending amount
   * 最低消费金额
   */
  minAmount: number;

  /**
   * Discount percentage, like 10 for 10%
   * 折扣百分比，如 10 代表 10%
   */
  discount: number;

  /**
   * Number of coupons
   * 优惠券数量
   */
  coupons?: number;

  /**
   * Number of coupons applied
   * 已使用优惠券数量
   */
  couponsApplied: number;

  /**
   * Status
   * 状况
   */
  status: EntityStatus;

  /**
   * Creation
   * 登记时间
   */
  creation: string | Date;
};
