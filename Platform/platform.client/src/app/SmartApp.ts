import {
  AddressUtils,
  AuthRequest,
  ExternalSettings,
  LoginInputAuthResult
} from "@etsoo/appscript";
import { ISmartSettings } from "./SmartSettings";
import { DataTypes, DomUtils, Utils } from "@etsoo/shared";
import { Constants } from "./Constants";
import { CommonApp, ISmartERPUser, MUGlobal } from "@etsoo/materialui";
import {
  AppApi,
  AuthCodeApi,
  CoreCulture,
  UserApi
} from "@etsoo/smarterp-core";
import { AuthApi } from "../api/AuthApi";
import { PublicApi } from "../api/PublicApi";

/**
 * SmartERP App
 */
class SmartApp extends CommonApp<ISmartERPUser, ISmartSettings> {
  /**
   * Authorization API
   */
  readonly authApi = new AuthApi(this);

  /**
   * Authorization code API
   */
  readonly authCodeApi = new AuthCodeApi(this);

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

  private _registrationAuthorized: boolean = false;
  /**
   * Registration authorized
   */
  get registrationAuthorized() {
    return this._registrationAuthorized;
  }
  private set registrationAuthorized(value: boolean) {
    this._registrationAuthorized = value;
  }

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
   * Login complete
   * @param auth Auth request
   * @param data User data
   * @param refreshToken Refresh token
   */
  async loginComplete(
    auth: AuthRequest | undefined,
    data?: ISmartERPUser | LoginInputAuthResult,
    refreshToken?: string
  ) {
    if (auth) {
      if (refreshToken && data && "uri" in data) {
        this.saveCacheToken(refreshToken);
        this.authLogin(data.uri);
      } else {
        const url = await this.authApi.authRequest(auth);
        if (!url) return;
        this.authLogin(url);
      }
    } else {
      if (refreshToken && data && !("uri" in data)) {
        // User login
        this.userLogin(data, refreshToken, false);

        // Accept invitation
        const [id, code] =
          this.storage.getData<[string, string]>(Constants.MemberInvitation) ??
          [];

        if (id && code) {
          const result = await this.publicApi.acceptInvitation({ id, code });
          if (result == null) return;

          if (result.ok) {
            this.storage.setData(Constants.MemberInvitation, null);
          } else {
            this.alertResult(result, () => this.toMain());
            return;
          }
        }
      }

      // Navigate to main URL
      this.toMain();
    }
  }

  /**
   * Set login token
   * @param token Login token
   */
  setLoginToken(token?: string) {
    if (token) {
      this.api.authorize(Constants.RegistrationTokenScheme, token);
      this.registrationAuthorized = true;
    }
  }

  private addCultureParam(url: string) {
    return url.addUrlParam(DomUtils.CultureField, this.culture);
  }

  /**
   * To main URL
   * @param navigate Navigate
   * @param home Default home URL
   */
  toMain() {
    // Get user's latest app
    this.userApi.getLatestApp().then((appData) => {
      if (appData) {
        // Error message
        const error = this.get("networkFailure");

        // Call the api
        const tasks = appData.urls.map((u) =>
          app.authApi.getLogInUrl(
            "APP",
            { showLoading: false, onError: () => false },
            u.api
          )
        );

        Promise.allSettled(tasks)
          .then((result) => {
            const url = result.find(
              (r) => r.status === "fulfilled" && r.value != null
            ) as PromiseFulfilledResult<string> | undefined;

            if (url) {
              this.loadUrl(url.value);
            } else {
              this.notifier.alert(error, () => {
                // Navigate to home
                this.navigate("/");
              });
            }
          })
          .catch((error) => {
            // Navigate to home
            this.navigate("/");
            console.error("SmartApp.toMain", error);
          });
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

// Settings
const settings: ISmartSettings = {
  // Merge external configs first
  ...ExternalSettings.create<ISmartSettings>(),

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
 * import.meta.env.DEV
 */
export const app = new SmartApp(settings, "smartERP");

/**
 * Notifier provider
 */
export const NotifierProvider = SmartApp.notifierProvider;
