import { IServiceAppSettings, MUGlobal, ServiceApp } from "@etsoo/materialui";
import { MyUser } from "./MyUser";
import { DataTypes, DomUtils, Utils } from "@etsoo/shared";
import {
  AddressUtils,
  BusinessUtils,
  ExternalSettings
} from "@etsoo/appscript";
import { CoreApp, CoreCulture, ICoreServiceApp } from "@etsoo/smarterp-core";
import { AdminApi } from "../api/AdminApi";
import { QueryApi } from "../api/QueryApi";

class MyApp extends ServiceApp<MyUser> implements ICoreServiceApp {
  /**
   * Core name
   */
  override get coreName(): string {
    return "platform";
  }

  override async loadCustomResources(
    resources: DataTypes.StringRecord,
    culture: string
  ) {
    const items = await this.core.publicApi.getCustomResources(culture, {
      showLoading: false
    });
    if (items == null) return;

    BusinessUtils.mergeCustomResources(resources, items);
  }

  /**
   * Admin APIs
   */
  readonly adminApi = new AdminApi(this);

  /**
   * Core application
   */
  readonly core = new CoreApp(this, this.coreApi);

  /**
   * Query APIs
   */
  readonly queryApi = new QueryApi(this);
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

// Settings
const settings: IServiceAppSettings = {
  // Merge external configs first
  ...ExternalSettings.create(undefined, import.meta.env.VITE_APP_HOST_NAME),

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
  appId: 2,

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
export const app = new MyApp(settings, "smarterpAdmin");

/**
 * Notifier provider
 */
export const NotifierProvider = MyApp.notifierProvider;
