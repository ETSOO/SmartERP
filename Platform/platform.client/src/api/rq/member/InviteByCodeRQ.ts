export type InviteByCodeRQ = {
  /**
   * Target organization
   */
  organizationId: number;

  /**
   * Member role
   */
  role: number;

  /**
   * Invitation codes
   */
  codes: string[];
};
