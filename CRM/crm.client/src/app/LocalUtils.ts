import { PromotionCodeCalculation, PromotionItem } from "@etsoo/smarterp-crm";

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
}
