import {
  EntityApi,
  IApi,
  IApiPayload,
  IApp,
  IdResultPayload
} from "@etsoo/appscript";
import { DocumentCreateRQ } from "./rq/document/DocumentCreateRQ";
import { DocumentQueryRQ } from "./rq/document/DocumentQueryRQ";
import { DocumentQueryData } from "./dto/document/DocumentQueryData";
import { DocumentUpdateRQ } from "./rq/document/DocumentUpdateRQ";

/**
 * Document API
 */
export class DocumentApi extends EntityApi {
  /**
   * Constructor
   * @param app Application
   * @param api API
   */
  constructor(app: IApp, api: IApi = app.api) {
    super("Document", app, api);
  }

  /**
   * Create
   * @param rq Request data
   * @param payload Payload
   * @returns Result
   */
  create(rq: DocumentCreateRQ, payload?: IdResultPayload) {
    return this.createBase(rq, payload);
  }

  /**
   * Delete
   * @param id Id
   * @param payload Payload
   * @returns Result
   */
  delete(id: number, payload?: IdResultPayload) {
    return this.deleteBase(id, payload);
  }

  /**
   * Query
   * @param rq Request data
   * @param payload Payload
   * @returns Result
   */
  query(rq: DocumentQueryRQ, payload?: IApiPayload<DocumentQueryData[]>) {
    return this.queryBase(rq, payload);
  }

  /**
   * Update
   * @param rq Request data
   * @param payload Payload
   * @returns Result
   */
  update(rq: DocumentUpdateRQ, payload?: IdResultPayload) {
    return this.updateBase(rq, payload);
  }
}
