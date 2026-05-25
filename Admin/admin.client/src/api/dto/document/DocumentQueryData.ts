/**
 * Document query data
 * 文档查询数据
 */
export type DocumentQueryData = {
  /**
   * Id
   * 编号
   */
  id: number;

  /**
   * Organization name
   * 机构名称
   */
  orgName?: string;

  /**
   * Kind
   * 类型
   */
  kind: string;

  /**
   * Title
   * 标题
   */
  title: string;

  /**
   * Has parameters or not
   * 是否有参数
   */
  hasParameters: boolean;

  /**
   * Refresh time
   * 刷新时间
   */
  refreshTime: Date | string;
};
