/**
 * Organization query public information request
 * 获取机构公开信息请求
 */
export type OrgInfoRQ = {
  /**
   * Application ID
   * 程序编号
   */
  appId?: number;

  /**
   * Application key
   * 程序键名
   */
  appKey?: string;

  /**
   * Organization unique identifier, manually activated
   * 机构全局唯一标识，手动激活
   */
  orgUid?: string;
};
