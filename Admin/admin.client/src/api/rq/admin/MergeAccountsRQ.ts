/**
 * Merge accounts request data
 * 合并账户请求数据
 */
export type MergeAccountsRQ = {
  /**
   * Retained user id
   * 保留的用户编号
   */
  id: number;

  /**
   * Merged user id
   * 合并的用户编号
   */
  mergedUserId: number;

  /**
   * Description
   * 描述
   */
  description: string;
};
