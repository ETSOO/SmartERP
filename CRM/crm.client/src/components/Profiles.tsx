import { CommonPage } from "@etsoo/materialui";
import React from "react";

/**
 * Profiles component
 */
export type ProfilesProps = {
  /**
   * Person ID
   */
  personId: number;
};

/**
 * Profiles component
 * @param props Props
 * @returns Component
 */
export function Profiles(props: ProfilesProps) {
  // Destruct
  const { personId } = props;

  // Layout
  return <CommonPage paddings={0}></CommonPage>;
}
