import { EntityStatus } from "@etsoo/appscript";
import { PromotionCodeData } from "./PromotionCodeData";

/**
 * Promotion update read data
 * 更新促销读取数据
 */
export type PromotionUpdateReadData = {
  /**
   * Id
   * 编号
   */
  id: number;

  /**
   * Code
   * 代码
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
  status: EntityStatus;

  /**
   * Order index
   * 排序数
   */
  orderIndex: number;
} & PromotionCodeData;
