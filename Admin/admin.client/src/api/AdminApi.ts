import { BaseApi, ResultPayload } from "@etsoo/appscript";
import { AppRenewRQ } from "./rq/admin/AppRenewRQ";
import { MergeAccountsRQ } from "./rq/admin/MergeAccountsRQ";

/**
 * Admin API
 */
export class AdminApi extends BaseApi {
  /**
   * Application renew
   * 应用续费
   * @param rq Request data
   * @returns Result
   */
  appRenew(rq: AppRenewRQ, payload?: ResultPayload) {
    return this.api.post("Admin/AppRenew", rq, payload);
  }

  /**
   * Clear user frozen time
   * 清除用户冻结时间
   * @param userId User ID
   * @param payload Payload
   * @returns Result
   */
  clearUserFrozen(userId: number, payload?: ResultPayload) {
    return this.api.post(`Admin/ClearUserFrozen/${userId}`, undefined, payload);
  }

  /**
   * Merge accounts
   * 合并账户
   * @param rq Request data
   * @param payload Payload
   * @returns Result
   */
  mergeAccounts(rq: MergeAccountsRQ, payload?: ResultPayload) {
    return this.api.post("Admin/MergeAccounts", rq, payload);
  }
}
