import {
  EntityApi,
  IApi,
  IApiPayload,
  IApp,
  IdResultPayload
} from "@etsoo/appscript";
import { PromotionCreateRQ } from "./rq/promotion/PromotionCreateRQ";
import { PromotionListData } from "./dto/promotion/PromotionListData";
import { PromotionQueryData } from "./dto/promotion/PromotionQueryData";
import { PromotionUpdateRQ } from "./rq/promotion/PromotionUpdateRQ";
import { PromotionListRQ } from "./rq/promotion/PromotionListRQ";
import { PromotionQueryRQ } from "./rq/promotion/PromotionQueryRQ";
import { PromotionUpdateReadData } from "./dto/promotion/PromotionUpdateReadData";

/**
 * Promotion API
 */
export class PromotionApi extends EntityApi {
  /**
   * Constructor
   * @param app Application
   * @param api API
   */
  constructor(app: IApp, api: IApi = app.api) {
    super("Promotion", app, api);
  }

  /**
   * Create
   * @param rq Request data
   * @param payload Payload
   * @returns Result
   */
  create(rq: PromotionCreateRQ, payload?: IdResultPayload) {
    return this.createBase(rq, payload);
  }

  /**
   * List
   * @param rq Request data
   * @param payload Payload
   * @returns Result
   */
  list(rq: PromotionListRQ, payload?: IApiPayload<PromotionListData[]>) {
    return this.listBase(rq, payload);
  }

  /**
   * Query
   * @param rq Request data
   * @param payload Payload
   * @returns Result
   */
  query(rq: PromotionQueryRQ, payload?: IApiPayload<PromotionQueryData[]>) {
    return this.queryBase(rq, payload);
  }

  /**
   * Sort promotions
   * @param items Items to sort
   * @param payload Payload
   * @returns Result
   */
  sort(items: PromotionListData[], payload?: IApiPayload<number>) {
    return this.sortBase(items, payload);
  }

  /**
   * Update
   * @param rq Request data
   * @param payload Payload
   * @returns Result
   */
  update(rq: PromotionUpdateRQ, payload?: IdResultPayload) {
    return this.updateBase(rq, payload);
  }

  /**
   * Update read
   * @param id Id
   * @param payload Payload
   * @returns Result
   */
  updateRead(id: number, payload?: IApiPayload<PromotionUpdateReadData>) {
    return this.updateReadBase(id, payload);
  }
}
