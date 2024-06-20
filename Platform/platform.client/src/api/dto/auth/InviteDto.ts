/**
 * Invitation data
 */
export type InviteDto = {
  /**
   * Identifier
   */
  identifier: string;

  /**
   * Inviter's name
   */
  inviterName: string;

  /**
   * Organization name
   */
  organizationName: string;

  /**
   * Is expired or not
   */
  isExpired: boolean;

  /**
   * Is inviated or not
   */
  isInvited: boolean;
};
