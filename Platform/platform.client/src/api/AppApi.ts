import { EntityApi, IApiPayload } from "@etsoo/appscript";
import { ReactAppType } from "@etsoo/materialui";
import { AppData } from "../app/AppData";

/**
 * Application API
 */
export class AppApi extends EntityApi {
  /**
   * Constructor
   * @param app Application
   */
  constructor(app: ReactAppType) {
    super("App", app);
  }

  /**
   * Get user applications
   * @param payload Payload
   * @returns Result
   */
  getApps(payload?: IApiPayload<AppData[]>) {
    return this.api.get("App/GetApps", undefined, payload);
  }
}
