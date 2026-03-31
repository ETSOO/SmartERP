/**
 * Promotion code data
 * 促销代码数据
 */
export type PromotionCodeData = {
  /**
   * Related product ids
   * 关联的产品编号
   */
  productIds?: number[];

  /**
   * Related product category ids
   * 关联的产品类目编号
   */
  productCategoryIds?: number[];

  /**
   * Related person (customer) ids
   * 关联的人员（客户）编号
   */
  personIds?: number[];

  /**
   * Related person category ids
   * 关联的人员类目编号
   */
  personCategoryIds?: number[];

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
};
