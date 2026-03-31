/**
 * Promotion item
 * 促销项目
 */
export type PromotionItem = {
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
   * Minimum amount
   * 最小金额
   */
  minAmount: number;

  /**
   * Discount
   * 折扣
   */
  discount: number;

  /**
   * Stackable
   * 是否可叠加
   */
  stackable: boolean;
};
