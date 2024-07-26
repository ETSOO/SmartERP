import {
  AlipayIcon,
  GoogleIcon,
  MicrosoftIcon,
  WechatIcon
} from "./../images/SVGIcons";

/**
 * Application utilities
 */
export namespace AppUtils {
  /**
   * Get brand icon
   * @param ac Auth client name
   * @returns Icon
   */
  export function getBrandIcon(ac?: string) {
    switch (ac) {
      case "Alipay":
        return AlipayIcon;
      case "Google":
        return GoogleIcon;
      case "Microsoft":
        return MicrosoftIcon;
      default:
        return WechatIcon;
    }
  }
}
