import { IServiceAppSettings, MUGlobal, ServiceApp } from "@etsoo/materialui";
import { MyUser } from "./MyUser";
import { DataTypes, DomUtils, Utils } from "@etsoo/shared";
import {
  AddressUtils,
  en,
  ExternalSettings,
  zhHans,
  zhHant
} from "@etsoo/appscript";
import enSys from "../../../../Platform/platform.client/src/i18n/en.sys.json";
import zhHansSys from "../../../../Platform/platform.client/src/i18n/zh-Hans.sys.json";
import zhHantSys from "../../../../Platform/platform.client/src/i18n/zh-Hant.sys.json";
import { UserApi } from "../api/UserApi";

class MyApp extends ServiceApp<MyUser> {
  /**
   * User APIs
   */
  readonly userApi = new UserApi(this);
}

// Detected country or region
const { detectedCountry } = DomUtils;

// Detected culture
const { detectedCulture } = DomUtils;

// Global settings
MUGlobal.textFieldVariant = "standard";

const supportedCultures: DataTypes.CultureDefinition[] = [
  zhHans(zhHansSys, () => import("../i18n/zh-Hans.json")),
  zhHant(zhHantSys, () => import("../i18n/zh-Hant.json")),
  en(enSys, () => import("../i18n/en.json"))
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
