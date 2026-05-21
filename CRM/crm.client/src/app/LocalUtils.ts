import {
  PromotionCodeCalculation,
  PromotionItem,
  QueryForPurchaseRQ,
  QueryForSaleRQ
} from "@etsoo/smarterp-crm";
import { app } from "./MyApp";

/**
 * Local utilities
 */
export namespace LocalUtils {
  /**
   * Data key for order customer data
   */
  export const ORDER_CUSTOMER_DATA_KEY = "etsoo-order-customer-data";

  /**
   * Data key for order lines data
   */
  export const ORDER_LINES_DATA_KEY = "etsoo-order-lines-data";

  /**
   * Data key for order promotions data
   */
  export const ORDER_PROMOTIONS_DATA_KEY = "etsoo-order-promotions-data";

  /**
   * Data key for PO supplier data
   */
  export const PO_SUPPLIER_DATA_KEY = "etsoo-po-supplier-data";

  /**
   * Data key for PO lines data
   */
  export const PO_LINES_DATA_KEY = "etsoo-po-lines-data";

  /**
   * Data key for PO promotions data
   */
  export const PO_PROMOTIONS_DATA_KEY = "etsoo-po-promotions-data";

  /**
   * Data key for current location
   */
  export const CURRENT_LOCATION_KEY = "etsoo-current-location";

  /**
   * Data key for PO stock
   */
  export const STOCK_PO_DATA_KEY = "etsoo-stock-po-data";

  /**
   * Data key for order stock
   */
  export const STOCK_ORDER_DATA_KEY = "etsoo-stock-order-data";

  /**
   * Customer query data, used for order creation
   */
  export type CustomerQueryData = Pick<
    QueryForSaleRQ,
    "customerId" | "currency" | "culture"
  >;

  /**
   * Supplier query data, used for order creation
   */
  export type SupplierQueryData = Pick<
    QueryForPurchaseRQ,
    "supplierId" | "currency" | "culture"
  >;

  /**
   * Promotion item with amount and formatted title
   */
  export type PromotionItemWithAmount = PromotionItem & {
    amount?: number;
    formattedTitle?: string;
  };

  /**
   * Order line item
   */
  export type OrderLine = {
    /**
     * Random ID
     */
    id: string;

    /**
     * Product ID
     */
    productId: number;

    /**
     * Title, default is product name
     */
    title: string;

    /**
     * Description of the line
     */
    description?: string;

    /**
     * Original price of the product, before discount
     */
    originalPrice: number;

    /**
     * Actual price
     */
    price: number;

    /**
     * Qty
     */
    qty: number;

    /**
     * Amount, exclude discount
     */
    amount: number;

    /**
     * Discount
     */
    discount: number;

    /**
     * Line level promotions
     */
    promotions?: PromotionCodeCalculation[];

    /**
     * Additional data, includes modifiers
     */
    data?: Record<string, unknown>;
  };

  /**
   * Clear order data from storage
   */
  export function clearOrderData(all: boolean = true) {
    if (all) {
      app.storage.setPersistedData(LocalUtils.ORDER_CUSTOMER_DATA_KEY, null);
    }
    app.storage.setPersistedData(LocalUtils.ORDER_LINES_DATA_KEY, null);
    app.storage.setPersistedData(LocalUtils.ORDER_PROMOTIONS_DATA_KEY, null);
  }

  /**
   * Clear PO data from storage
   */
  export function clearPOData(all: boolean = true) {
    if (all) {
      app.storage.setPersistedData(LocalUtils.PO_SUPPLIER_DATA_KEY, null);
    }
    app.storage.setPersistedData(LocalUtils.PO_LINES_DATA_KEY, null);
    app.storage.setPersistedData(LocalUtils.PO_PROMOTIONS_DATA_KEY, null);
  }

  /**
   * Get current location ID from storage
   */
  export function getCurrentLocationId() {
    return app.storage.getPersistedData<number>(
      LocalUtils.CURRENT_LOCATION_KEY
    );
  }

  /**
   * Set current location ID to storage
   * @param locationId New location id
   */
  export function setCurrentLocationId(locationId: number | null | undefined) {
    app.storage.setPersistedData(LocalUtils.CURRENT_LOCATION_KEY, locationId);
  }
}
