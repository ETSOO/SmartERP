using com.etsoo.WeiXin;
using com.etsoo.WeiXin.Dto;
using System.Web;

namespace Platform.Server.OAuth2
{
    /// <summary>
    /// Wechat OAuth2
    /// 微信 OAuth2
    /// </summary>
    public static class WechatOAuth2
    {
        public static RouteGroupBuilder MapWechat(this RouteGroupBuilder builder)
        {
            var g = builder.MapGroup("Wechat");

            // https://developers.weixin.qq.com/doc/offiaccount/Basic_Information/Access_Overview.html
            g.MapGet("Listen", async ([AsParameters] WXCheckSignatureInput data, IWXClient client) =>
            {
                var success = await client.CheckSignatureAsync(data);
                return success ? data.Echostr : "Failed";
            }).WithDescription("接口配置确认，验证消息的确来自微信服务器");

            // https://developers.weixin.qq.com/doc/oplatform/Website_App/WeChat_Login/Wechat_Login.html
            g.MapGet("GetAuthUrl", (IWXClient client, HttpRequest request) =>
            {
                var state = request.HttpContext.Connection.Id;
                return $"https://open.weixin.qq.com/connect/qrconnect?appid={"wx818fea374dc58e11"}&redirect_uri={HttpUtility.UrlEncode("http://etsoo.v7.idcfengye.com/api/OAuth2/Wechat/")}&response_type=code&scope=snsapi_login&state={state}#wechat_redirect";
            }).WithDescription("获取网站应用授权登录网址");

            return builder;
        }
    }
}
