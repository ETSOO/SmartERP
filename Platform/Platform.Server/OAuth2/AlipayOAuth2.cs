using com.etsoo.WeiXin;
using System.Web;

namespace Platform.Server.OAuth2
{
    /// <summary>
    /// Alipay OAuth2
    /// 支付宝 OAuth2
    /// </summary>
    public static class AlipayOAuth2
    {
        public static RouteGroupBuilder MapAlipay(this RouteGroupBuilder builder)
        {
            var g = builder.MapGroup("Alipay");

            // https://opendocs.alipay.com/support/04y56c
            g.MapGet("GetAuthUrl", (IWXClient client, HttpRequest request) =>
            {
                var state = request.HttpContext.Connection.Id;
                return $"https://openauth.alipay.com/oauth2/publicAppAuthorize.htm?app_id={"9021000138623309"}&redirect_uri={HttpUtility.UrlEncode("http://etsoo.v7.idcfengye.com/api/OAuth2/Alipay/")}&scope=auth_user&state={state}";
            }).WithDescription("获取网站应用授权登录网址");

            return builder;
        }
    }
}
