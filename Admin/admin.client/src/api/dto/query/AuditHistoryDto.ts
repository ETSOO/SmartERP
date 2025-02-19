/**
 * Audit history data
 * 操作历史数据
 */
export type AuditHistoryDto = {
  /**
   * Id
   */
  id: number;

  /**
   * Kind
   */
  kind: string;

  /**
   * Culture
   */
  culture: string;

  /**
   * Title
   */
  title: string;

  /**
   * IP
   */
  ip: string;

  /**
   * User id
   */
  userId: number;

  /**
   * Organization id
   */
  organizationId?: number;

  /**
   * Target id
   */
  targetId?: number;

  /**
   * JSON data
   */
  data?: string;

  /**
   * Creation
   */
  creation: Date | string;
};
