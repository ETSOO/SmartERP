import { BaseApi, IApiPayload } from "@etsoo/appscript";
import { AuditHistoryRQ } from "./rq/query/AuditHistoryRQ";
import { AuditHistoryDto } from "./dto/query/AuditHistoryDto";
import { AllAppRQ } from "./rq/query/AllAppRQ";
import { AllAppDto } from "./dto/query/AllAppDto";
import { AllOrgRQ } from "./rq/query/AllOrgRQ";
import { AllOrgDto } from "./dto/query/AllOrgDto";
import { AllUserRQ } from "./rq/query/AllUserRQ";
import { AllUserDto } from "./dto/query/AllUserDto";
import { DataTypes } from "@etsoo/shared";
import { OrgListRQ } from "./rq/query/OrgListRQ";
import { AppListRQ } from "./rq/query/AppListRQ";
import { UserListRQ } from "./rq/query/UserListRQ";
import { OrgListDto } from "./dto/query/OrgListDto";
import { ReadOrgData } from "./dto/query/ReadOrgDto";
import { ReadAppDto } from "./dto/query/ReadAppDto";

/**
 * Query API
 */
export class QueryApi extends BaseApi {
  /**
   * Query all applications
   * @param rq Request data
   * @param payload Payload
   * @returns Result
   */
  allApps(rq: AllAppRQ, payload?: IApiPayload<AllAppDto[]>) {
    return this.api.post("Query/AllApps", rq, payload);
  }

  /**
   * Query all organizations
   * @param rq Request data
   * @param payload Payload
   * @returns Result
   */
  allOrgs(rq: AllOrgRQ, payload?: IApiPayload<AllOrgDto[]>) {
    return this.api.post("Query/AllOrgs", rq, payload);
  }

  /**
   * Query all users
   * @param rq Request data
   * @param payload Payload
   * @returns Result
   */
  allUsers(rq: AllUserRQ, payload?: IApiPayload<AllUserDto[]>) {
    return this.api.post("Query/AllUsers", rq, payload);
  }

  /**
   * App list
   * @param rq Request data
   * @param payload Payload
   * @returns Result
   */
  appList(rq: AppListRQ, payload?: IApiPayload<DataTypes.IdNameItem[]>) {
    return this.api.post("Query/AppList", rq, payload);
  }

  /**
   * Audit history
   * @param rq Request data
   * @param payload Payload
   * @returns Result
   */
  auditHistory(rq: AuditHistoryRQ, payload?: IApiPayload<AuditHistoryDto[]>) {
    return this.api.post("Query/AuditHistory", rq, payload);
  }

  /**
   * Organization list
   * @param rq Request data
   * @param payload Payload
   * @returns Result
   */
  orgList(rq: OrgListRQ, payload?: IApiPayload<OrgListDto[]>) {
    return this.api.post("Query/OrgList", rq, payload);
  }

  /**
   * Read application data
   * @param id Id
   * @param payload Payload
   * @returns Result
   */
  readApp(id: number, payload?: IApiPayload<ReadAppDto>) {
    return this.api.get(`Query/ReadApp/${id}`, undefined, payload);
  }

  /**
   * Read organization data
   * @param id Id
   * @param payload Payload
   * @returns Result
   */
  readOrg(id: number, payload?: IApiPayload<ReadOrgData>) {
    return this.api.get(`Query/ReadOrg/${id}`, undefined, payload);
  }

  /**
   * Read user data
   * @param id Id
   * @param payload Payload
   * @returns Result
   */
  readUser(id: number, payload?: IApiPayload<AllUserDto>) {
    return this.api.get(`Query/ReadUser/${id}`, undefined, payload);
  }

  /**
   * User list
   * @param rq Request data
   * @param payload Payload
   * @returns Result
   */
  userList(rq: UserListRQ, payload?: IApiPayload<DataTypes.IdNameItem[]>) {
    return this.api.post("Query/UserList", rq, payload);
  }
}
