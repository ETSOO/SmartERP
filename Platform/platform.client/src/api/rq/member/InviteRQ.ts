export type InviteRQ = {
  /**
   * Target organization
   */
  organizationId: number;

  /**
   * Member role
   */
  role: number;

  /**
   * Invited emails
   */
  emails: string[];

  /**
   * Additional message
   */
  message?: string;

  /**
   * Timezone
   */
  timezone?: string;
};
