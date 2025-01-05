import { AddressUtils, ExternalSettings } from "@etsoo/appscript";
import { ISmartSettings } from "./SmartSettings";
import { DataTypes, DomUtils, Utils } from "@etsoo/shared";
import { Constants } from "./Constants";
import { ISmartPageData } from "./SmartPageData";
import { CommonApp, ISmartERPUser, MUGlobal } from "@etsoo/materialui";
import { AppApi, CoreCulture, UserApi } from "@etsoo/smarterp-core";
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
   * App API
   */
  readonly appApi = new AppApi(this);

  /**
   * Public API
   */
  readonly publicApi = new PublicApi(this);

  /**
   * User API
   */
  readonly userApi = new UserApi(this);

  /**
   * Authorization login
   * @param url Authorization URL
   */
  authLogin(url: string) {
    // Remove the auth request cache
    this.storage.setData(Constants.AuthRequestField, null);

    // Redirect to authorization request
    window.location.replace(this.addCultureParam(url));
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

  private addCultureParam(url: string) {
    return url.addUrlParam(DomUtils.CultureField, app.culture);
  }

  /**
   * To main URL
   * @param navigate Navigate
   * @param home Default home URL
   */
  toMain() {
    // Get user's latest app
    this.userApi.getLatestApp().then((url) => {
      if (url) {
        // Go to the app
        globalThis.location.href = this.addCultureParam(url);
      } else {
        // Go to the home page
        this.navigate("/");
      }
    });
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
