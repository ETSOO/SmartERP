import { BaseApi, IApiPayload } from '@etsoo/appscript';
import { DashboardView } from './dto/system/DashboardView';

/**
 * System API
 */
export class SystemApi extends BaseApi {
  /**
   * Get dashboard data
   * @param payload Payload
   * @returns Result
   */
  dashboard(payload?: IApiPayload<DashboardView>) {
    return this.api.get('System/Dashboard', undefined, payload);
  }
}
