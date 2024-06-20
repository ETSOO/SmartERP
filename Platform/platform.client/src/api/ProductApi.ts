import {
  EntityApi,
  IApiPayload,
  ResultPayload,
  StringIdResultPayload
} from '@etsoo/appscript';
import { ReactAppType } from '@etsoo/materialui';
import { ProductPurchasedDto } from './dto/product/ProductPurchasedDto';
import { ProductQueryDto } from './dto/product/ProductQueryDto';
import { ProductBuyNewRQ } from './rq/product/ProductBuyNewRQ';
import { ProductQueryPurchasedRQ } from './rq/product/ProductQueryPurchasedRQ';
import { ProductQueryRQ } from './rq/product/ProductQueryRQ';
import { ProductRenewRQ } from './rq/product/ProductRenewRQ';

/**
 * Product API
 */
export class ProductApi extends EntityApi {
  /**
   * Constructor
   * @param app Application
   */
  constructor(app: ReactAppType) {
    super('Product', app);
  }

  /**
   * Buy product or service to new organization
   * @param id Id
   * @param organization Organization
   * @param payload Payload
   * @returns Result
   */
  buy(id: number, organization: number, payload?: ResultPayload) {
    return this.api.post('Product/Buy', { id, organization }, payload);
  }

  /**
   * Buy product or service for current organization
   * @param rq Request data
   * @param payload Payload
   * @returns Result
   */
  buyNew(rq: ProductBuyNewRQ, payload?: ResultPayload) {
    return this.api.post('Product/BuyNew', rq, payload);
  }

  /**
   * Create API key
   * @param id Bought service id
   * @param payload Payload
   * @returns Result
   */
  createApiKey(id: string, payload?: StringIdResultPayload) {
    return this.api.put(`Product/CreateApiKey/${id}`, undefined, payload);
  }

  /**
   * Query
   * @param rq Request data
   * @param payload Payload
   * @returns Result
   */
  query(rq: ProductQueryRQ, payload?: IApiPayload<ProductQueryDto[]>) {
    return this.queryBase(rq, payload);
  }

  /**
   * Query purchased products
   * @param rq Request data
   * @param payload Payload
   * @returns Result
   */
  queryPurchased(
    rq: ProductQueryPurchasedRQ,
    payload?: IApiPayload<ProductPurchasedDto[]>
  ) {
    return this.api.post('Product/QueryPurchased', rq, payload);
  }

  /**
   * Renew purchased product
   * @param rq Request data
   * @param payload Payload
   * @returns Result
   */
  renew(rq: ProductRenewRQ, payload?: ResultPayload) {
    return this.api.put('Product/Renew', rq, payload);
  }

  /**
   * Set custom name for purchased product
   * @param id Product id
   * @param name Custom name for the product
   * @param payload Payload
   * @returns Result
   */
  setCustomName(id: string, name: string, payload?: ResultPayload) {
    return this.api.post('Product/SetCustomName', { id, name }, payload);
  }
}
