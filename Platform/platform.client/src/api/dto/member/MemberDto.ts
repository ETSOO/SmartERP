export type MemberDto = {
  /**
   * Id
   */
  id: string;

  /**
   * Local name
   */
  localName?: string;

  /**
   * Role
   */
  entityRole?: number;

  /**
   * External id
   */
  externalId?: string;

  /**
   * Enabled or not
   */
  enabled?: boolean;
};
