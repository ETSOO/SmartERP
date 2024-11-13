import {
  AddressUtils,
  zhHant,
  en,
  ExternalSettings,
  BridgeUtils,
  ApiService,
  zhHans
} from "@etsoo/appscript";
import { ISmartSettings } from "./SmartSettings";
import { DataTypes, DomUtils, Utils } from "@etsoo/shared";
import { Constants } from "./Constants";
import { ISmartPageData } from "./SmartPageData";
import { CommonApp, ISmartERPUser, MUGlobal } from "@etsoo/materialui";
import { PublicApi } from "../api/PublicApi";
import { AuthApi } from "../api/AuthApi";
import { MemberApi } from "../api/MemberApi";
import { StorageApi } from "../api/StorageApi";
import { SystemApi } from "../api/SystemApi";
import { UserApi } from "../api/UserApi";
import { MemberIdentity } from "../api/dto/member/MemberIdentity";
import { ProductApi } from "../api/ProductApi";
import { OrgApi } from "../api/OrgApi";
import { ApiServiceApi } from "../api/ApiServiceApi";
import { NavigateFunction } from "react-router-dom";
import { AppApi } from "../api/AppApi";
import { AppData } from "./AppData";
import enSys from "../i18n/en.sys.json";
import zhHansSys from "../i18n/zh-Hans.sys.json";
import zhHantSys from "../i18n/zh-Hant.sys.json";

/**
 * SmartERP App
 */
class SmartApp extends CommonApp<
  ISmartERPUser,
  ISmartPageData,
  ISmartSettings
> {
  /**
   * Api service API
   */
  readonly apiServiceApi = new ApiServiceApi(this);

  /**
   * App API
   */
  readonly appApi = new AppApi(this);

  /**
   * Public API
   */
  readonly publicApi = new PublicApi(this);

  /**
   * Organization API
   */
  readonly orgApi = new OrgApi(this);

  /**
   * Authorization API
   */
  readonly authApi = new AuthApi(this);

  /**
   * Member API
   */
  readonly memberApi = new MemberApi(this);

  /**
   * Product API
   */
  readonly productApi = new ProductApi(this);

  /**
   * Storage API
   */
  readonly storageApi = new StorageApi(this);

  /**
   * System API
   */
  readonly systemApi = new SystemApi(this);

  /**
   * System API
   */
  readonly userApi = new UserApi(this);

  private _apps: AppData[] = [];
  /**
   * User applications
   */
  public get apps() {
    return this._apps;
  }
  protected set apps(value) {
    this._apps = value;
    this._origins = value.map((app) => new URL(app.webUrl).origin);
  }

  private _origins: string[] = [];
  /**
   * Origins
   */
  public get origins() {
    return this._origins;
  }

  /**
   * Authorization login
   * @param url Authorization URL
   */
  authLogin(url: string) {
    // Remove the auth request cache
    this.storage.setData(Constants.AuthRequestField, null);

    // Redirect to authorization request
    window.location.replace(url);
  }

  /**
   * Get Api services
   * @returns List
   */
  getApiServices() {
    return this.getEnumList(ApiService, "apiService");
  }

  /**
   * Get identities
   * @returns List
   */
  getIdentities() {
    return this.getEnumList(MemberIdentity, "id");
  }

  /**
   * Get cached URL
   * @param baseUrl Base URL
   * @returns Result
   */
  getCachedUrl(baseUrl?: string) {
    /*
    const url = this.storage.getData<string>(Constants.RedirectUrlCache);
    if (url) {
      if (!url.includes("://")) return url;

      baseUrl ??= `${globalThis.location.protocol}//${globalThis.location.host}`;
      if (url.startsWith(baseUrl)) return url.substring(baseUrl.length);
    }
    this.storage.setData(Constants.RedirectUrlCache, null);
    */
    return baseUrl;
  }

  /**
   * Get service Url
   * @param serviceUrl Service Url
   * @param serviceToken Service token
   * @param redirectUrl Redirect URL
   * @returns Formated URL
   */
  getServiceUrl(
    serviceUrl: string,
    serviceToken: string,
    redirectUrl?: string
  ) {
    return (
      serviceUrl +
      `/api/?provider=SmartERP&culture=${
        this.culture
      }&token=${encodeURIComponent(serviceToken)}&url=${
        redirectUrl ? encodeURIComponent(redirectUrl) : ""
      }`
    );
  }

  /**
   * Set login token
   * @param token Login token
   */
  setLoginToken(token?: string) {
    if (token) {
      app.api.authorize(Constants.RegistrationTokenScheme, token);
    }
  }

  /**
   * To home URL
   * @param navigate Navigate
   * @param home Default home URL
   */
  toHome(navigate: NavigateFunction, home: string) {
    navigate(this.getCachedUrl() ?? home);
  }

  /**
   * Navigate to the service Url
   * @param appId App id
   * @param serviceUrl Service Url
   * @param serviceToken Service token
   * @param newWindow Open new window
   */
  toServiceUrl(
    appId: number,
    serviceUrl: string,
    serviceToken: string,
    newWindow: boolean = false
  ) {
    // Persist data
    this.persist();

    const redirectUrl = this.getCachedUrl(serviceUrl);

    // Is bridge service
    const host = BridgeUtils.host;
    if (host) {
      // Get service Url
      const url = app.getServiceUrl("", serviceToken, redirectUrl);

      host.loadApp(`s${appId}`, url);
    } else {
      // Get service Url
      const url = app.getServiceUrl(serviceUrl, serviceToken, redirectUrl);

      // Clear cached data
      this.storage.setData(Constants.CurentService, undefined);

      // Replace current loation
      // window.location.replace(url);
      // Open new window
      if (newWindow) window.open(url, `App${appId}`);
      else window.location.replace(url);
    }
  }

  /**
   * On authorized or not callback
   * @param success Success or not
   */
  protected override onAuthorized(success: boolean) {
    // Call parent
    super.onAuthorized(success);

    // Get user apps
    if (success) {
      this.appApi.getApps({ showLoading: false }).then((apps) => {
        if (apps == null) return;
        this.apps = apps;
      });
    } else {
      this.apps = [];
    }
  }
}

// Detected country or region
const { detectedCountry } = DomUtils;

// Detected culture
const { detectedCulture } = DomUtils;

// Global settings
MUGlobal.textFieldVariant = "standard";

// Supported cultures
const supportedCultures: DataTypes.CultureDefinition[] = [
  zhHans(zhHansSys, () => import("./../i18n/zh-Hans.json")),
  zhHant(zhHantSys, () => import("./../i18n/zh-Hant.json")),
  en(enSys, () => import("./../i18n/en.json"))
];

// Supported regions
const supportedRegions = ["CN"];

// External settings
const externalSettings = ExternalSettings.create<ISmartSettings>();
if (externalSettings == null) {
  throw new Error("No external settings");
}

// Settings
const settings: ISmartSettings = {
  // Merge external configs first
  ...externalSettings,

  // Detected culture
  detectedCulture,

  // Supported cultures
  cultures: supportedCultures,

  // Supported regions
  regions: supportedRegions,

  // Browser's time zone
  timeZone: Utils.getTimeZone(),

  // Current country or region
  currentRegion: AddressUtils.getRegion(
    supportedRegions,
    detectedCountry,
    detectedCulture
  ),

  // Current culture
  currentCulture: DomUtils.getCulture(supportedCultures, detectedCulture)[0]!
};

/**
 * Application
 */
export const app = new SmartApp(settings, "smartERP", import.meta.env.DEV);

/**
 * Notifier provider
 */
export const NotifierProvider = SmartApp.notifierProvider;
