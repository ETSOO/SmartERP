import {
  AddressUtils,
  zhHant,
  zhHans,
  en,
  ExternalSettings,
  BridgeUtils,
  ApiService
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
import { CoreConstants } from "@etsoo/react";

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

  /**
   * Do login
   * @param userData User data
   * @param refreshToken Refresh token
   * @param keep Keep login
   */
  doLogin(
    userData: ISmartERPUser,
    refreshToken: string,
    keep: boolean = false
  ) {
    // Service token
    const serviceToken = userData.serviceToken;

    // Clear the token
    if (serviceToken) Reflect.set(userData, "serviceToken", undefined);

    // User login
    app.userLogin(userData, refreshToken, keep);

    // Keep
    app.storage.setData(CoreConstants.FieldLoginKeep, keep);

    return serviceToken;
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
   * Override doing user login
   * @param data User data
   * @param refreshToken Refresh token
   * @param keep Keep login
   * @returns Success data
   */
  protected override doUserLogin(
    data: ISmartERPUser,
    refreshToken: string,
    keep: boolean
  ): string | undefined {
    // Service token
    const serviceToken = data.serviceToken;

    // User login
    // Service login, token will be null and should not trigger user state change
    this.userLogin(data, refreshToken, keep, serviceToken == null);

    return serviceToken;
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
  zhHans(() => import("../i18n/zh-Hans.json")),
  zhHant(() => import("../i18n/zh-Hant.json")),
  en(() => import("../i18n/en.json"))
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
