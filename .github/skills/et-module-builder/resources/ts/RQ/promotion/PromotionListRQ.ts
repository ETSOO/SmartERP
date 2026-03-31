import { QueryRQ } from "@etsoo/appscript";

/**
 * Promotion list request data
 * 促销列表请求数据
 */
export type PromotionListRQ = QueryRQ & {
  /**
   * Code
   * 代码
   */
  code?: number;

  /**
   * Currency
   * 币种
   */
  currency?: string;

  /**
   * Is valid
   * 是否有效
   */
  isValid?: boolean;

  /**
   * Person (customer) id
   * 人员（客户）编号
   */
  personId?: number;

  /**
   * Product included
   * 包含的产品
   */
  productId?: number;

  /**
   * Stackable
   * 是否可叠加
   */
  stackable?: boolean;
};
