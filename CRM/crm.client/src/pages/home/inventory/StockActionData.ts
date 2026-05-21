import { StockOrderItem } from "@etsoo/smarterp-crm";

/**
 * Stock action data
 */
export type StockActionData = {
  personId: number;
  personName: string;
  locationFromId: number;
  locationToId: number;
  locationFromName: string;
  locationToName: string;
  orders: number[];
  trackingNumber?: string;
  title: string;
  description?: string;

  lines: StockOrderItem[];
};
