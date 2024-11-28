import { BaseApi, IApi, IApiPayload, IApp } from "@etsoo/appscript";

/**
 * Local app API
 */
export class AppApi extends BaseApi {
  /**
   * Constructor
   * @param app Application
   * @param api API
   */
  constructor(app: IApp, api: IApi = app.api) {
    super(app, api);
  }

  /**
   * Get user latest app
   * 获取用户最新应用
   * @param payload Payload
   * @returns Result
   */
  getUserLatestApp(payload?: IApiPayload<string>) {
    return this.api.get("App/GetUserLatestApp", undefined, payload);
  }
}
