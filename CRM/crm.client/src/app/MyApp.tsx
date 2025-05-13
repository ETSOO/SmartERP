import { IServiceAppSettings, MUGlobal } from "@etsoo/materialui";
import { DataTypes, DomUtils, ExtendUtils, Utils } from "@etsoo/shared";
import {
  AddressUtils,
  BusinessUtils,
  ExternalSettings
} from "@etsoo/appscript";
import { CoreApp, CoreCulture, ICoreServiceApp } from "@etsoo/smarterp-core";
import { CrmApp, ICrmApp, CrmCulture, CrmAppBase } from "@etsoo/smarterp-crm";

// Mixin, merge them together with the same name
interface MyApp extends ICrmApp {}
class MyApp extends CrmAppBase implements ICoreServiceApp {
  /**
   * App, for mixin
   */
  get app() {
    return this;
  }

  /**
   * Core application
   */
  readonly core = new CoreApp(this, this.coreApi);

  protected override async loadCustomResources(
    resources: DataTypes.StringRecord,
    culture: string
  ) {
    const items = await this.core.publicApi.getCustomResources(culture, {
      showLoading: false
    });
    if (items == null) return;

    BusinessUtils.mergeCustomResources(resources, items);
  }

  protected override async onUserLogin() {
    const items = await this.core.orgApi.getCustomResources(this.culture);
    if (items == null) return;

    const { resources } = this.settings.currentCulture;
    if (typeof resources === "object") {
      BusinessUtils.restoreResources(resources);
      BusinessUtils.mergeCustomResources(resources, items);
    }
  }
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
