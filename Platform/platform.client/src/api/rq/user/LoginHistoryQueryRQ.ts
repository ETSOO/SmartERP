import { QueryRQ } from '@etsoo/appscript';

/**
 * Login history query request data
 */
export type LoginHistoryQueryRQ = QueryRQ & {
  /**
   * Device id
   */
  deviceId?: number;

  /**
   * Creation start
   */
  creationStart?: Date | string;

  /**
   * Creation end
   */
  creationEnd?: Date | string;

  /**
   * Success or not
   */
  success?: boolean;
};
