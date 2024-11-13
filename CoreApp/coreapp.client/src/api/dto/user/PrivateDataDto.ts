import { IdLabelPrimaryDto } from "@etsoo/appscript";

/**
 * Private data view
 */
export type PrivateDataDto = {
  /**
   * Email addresses
   */
  emails?: IdLabelPrimaryDto[];

  /**
   * Mobile phones
   */
  mobiles?: IdLabelPrimaryDto[];
};
