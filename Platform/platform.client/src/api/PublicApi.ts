import { IApiPayload } from "@etsoo/appscript";
import { PublicApi as PublicApiBase } from "@etsoo/smarterp-core";
import { PublicOrgInfoRQ } from "./rq/public/PublicOrgInfoRQ";
import { PublicOrgInfo } from "./dto/public/PublicOrgInfo";

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
  orgInfo(rq: PublicOrgInfoRQ, payload?: IApiPayload<PublicOrgInfo>) {
    return this.api.post("Public/OrgInfo", rq, payload);
  }
}
