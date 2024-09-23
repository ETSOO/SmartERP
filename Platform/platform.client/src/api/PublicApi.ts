import { IApiPayload, PublicApi as PublicApiBase } from "@etsoo/appscript";
import { OrgInfo } from "./dto/org/OrgInfo";
import { OrgInfoRQ } from "./rq/public/OrgInfoRQ";

/**
 * Public API
 */
export class PublicApi extends PublicApiBase {
  /**
   * Get organization public information
   * 获取机构公开信息
   * @param rq Request data
   * @param payload Payload
   * @returns Result
   */
  orgInfo(rq: OrgInfoRQ, payload?: IApiPayload<OrgInfo>) {
    return this.api.post("Public/OrgInfo", rq, payload);
  }
}
