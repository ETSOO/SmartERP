import { DataTypes } from "@etsoo/shared";

/**
 * Organization list data
 */
export type OrgListDto = DataTypes.IdNameItem & {
  pin?: string;
};
