import { IServiceAppSettings, MUGlobal, ServiceApp } from "@etsoo/materialui";
import { MyUser } from "./MyUser";
import { DataTypes, DomUtils, ExtendUtils, Utils } from "@etsoo/shared";
import { AddressUtils, ExternalSettings } from "@etsoo/appscript";
import { CoreApp, CoreCulture, ICoreServiceApp } from "@etsoo/smarterp-core";
import { CrmApp, ICrmApp, CrmCulture } from "@etsoo/smarterp-crm";
import { AppModule } from "./AppModule";

// Mixin, merge them together with the same name
interface MyApp extends ICrmApp {}
class MyApp extends ServiceApp<MyUser> implements ICoreServiceApp {
  /**
   * Core name
   */
  override get coreName(): string {
    return "platform";
  }

  /**
   * App, for mixin
   */
  get app() {
    return this;
  }

  /**
   * Modules permission
   */
  readonly module: Record<AppModule, boolean> = {
    [AppModule.Organization]: true,
    [AppModule.User]: true,
    [AppModule.Customer]: true,
    [AppModule.Supplier]: true,
    [AppModule.Product]: true,
    [AppModule.Order]: true,
    [AppModule.PO]: true,
    [AppModule.Inventory]: true,
    [AppModule.Finance]: true
  };

  /**
   * Core application
   */
  readonly core = new CoreApp(this, this.coreApi);
}
ExtendUtils.applyMixins(MyApp, [CrmApp]);

// Detected country or region
const { detectedCountry } = DomUtils;

// Detected culture
const { detectedCulture } = DomUtils;

// Global settings
MUGlobal.textFieldVariant = "standard";

const supportedCultures: DataTypes.CultureDefinition[] = [
  CoreCulture.zhHans(CrmCulture.zhHans, () => import("../i18n/zh-Hans.json")),
  CoreCulture.zhHant(CrmCulture.zhHant, () => import("../i18n/zh-Hant.json")),
  CoreCulture.en(CrmCulture.en, () => import("../i18n/en.json"))
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
  appId: 3,

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
export const app = new MyApp(settings, "smarterpCRM");

/**
 * Notifier provider
 */
export const NotifierProvider = MyApp.notifierProvider;
