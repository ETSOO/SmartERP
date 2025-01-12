import { IServiceAppSettings, MUGlobal, ServiceApp } from "@etsoo/materialui";
import { MyUser } from "./MyUser";
import { DataTypes, DomUtils, Utils } from "@etsoo/shared";
import { AddressUtils, ExternalSettings } from "@etsoo/appscript";
import { CoreApp, CoreCulture, ICoreServiceApp } from "@etsoo/smarterp-core";
import { AuthApi } from "../api/AuthApi";

class MyApp extends ServiceApp<MyUser> implements ICoreServiceApp {
  /**
   * Core application
   */
  readonly core = new CoreApp(this, this.coreApi);

  /**
   * Auth APIs
   */
  readonly authApi = new AuthApi(this);
}

// Detected country or region
const { detectedCountry } = DomUtils;

// Detected culture
const { detectedCulture } = DomUtils;

// Global settings
MUGlobal.textFieldVariant = "standard";

const supportedCultures: DataTypes.CultureDefinition[] = [
  CoreCulture.zhHans(() => import("../i18n/zh-Hans.json")),
  CoreCulture.zhHant(() => import("../i18n/zh-Hant.json")),
  CoreCulture.en(() => import("../i18n/en.json"))
];
const supportedRegions = ["CN"];

// External settings
const externalSettings = ExternalSettings.create();
if (externalSettings == null) {
  throw new Error("No external settings");
}

// Settings
const settings: IServiceAppSettings = {
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

  /**
   * Current service id
   */
  appId: 1,

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
export const app = new MyApp(settings, "smarterpCore");

/**
 * Notifier provider
 */
export const NotifierProvider = MyApp.notifierProvider;
