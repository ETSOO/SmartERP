/**
 * Application renew request data
 * 应用续费请求数据
 */
export type AppRenewRQ = {
  /**
   * Organization application ID
   * 机构应用编号
   */
  id: number;

  /**
   * Months to review
   * 续费月数
   */
  months: number;

  /**
   * Requester
   * 请求人
   */
  requester: number;

  /**
   * Approver
   * 批准人
   */
  approver: number;

  /**
   * Comment
   * 备注
   */
  comment: string;
};
