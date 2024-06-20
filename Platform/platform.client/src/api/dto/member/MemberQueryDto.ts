export type MemberQueryDto = {
  /**
   * Id
   */
  id: string;

  /**
   * Name
   */
  name: string;

  /**
   * Role
   */
  entityRole: number;

  /**
   * Entity status
   */
  entityStatus: number;

  /**
   * External id
   */
  externalId?: string;

  /**
   * Is myself
   */
  isSelf: boolean;

  /**
   * Creation
   */
  creation: Date;
};
