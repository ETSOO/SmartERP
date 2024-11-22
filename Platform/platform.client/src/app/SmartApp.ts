import { AddressUtils, ExternalSettings, BridgeUtils } from "@etsoo/appscript";
import { ISmartSettings } from "./SmartSettings";
import { DataTypes, DomUtils, Utils } from "@etsoo/shared";
import { Constants } from "./Constants";
import { ISmartPageData } from "./SmartPageData";
import { CommonApp, ISmartERPUser, MUGlobal } from "@etsoo/materialui";
import { NavigateFunction } from "react-router-dom";
import { CoreCulture } from "@etsoo/smarterp-core";
import { AuthApi } from "../api/AuthApi";
import { PublicApi } from "../api/PublicApi";

/**
 * SmartERP App
 */
class SmartApp extends CommonApp<
  ISmartERPUser,
  ISmartPageData,
  ISmartSettings
> {
  /**
   * Authorization API
   */
  readonly authApi = new AuthApi(this);

  /**
   * Public API
   */
  readonly publicApi = new PublicApi(this);

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
}

// Detected country or region
const { detectedCountry } = DomUtils;

// Detected culture
const { detectedCulture } = DomUtils;

// Global settings
MUGlobal.textFieldVariant = "standard";

// Supported cultures
const supportedCultures: DataTypes.CultureDefinition[] = [
  CoreCulture.zhHans(() => import("./../i18n/zh-Hans.json")),
  CoreCulture.zhHant(() => import("./../i18n/zh-Hant.json")),
  CoreCulture.en(() => import("./../i18n/en.json"))
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
