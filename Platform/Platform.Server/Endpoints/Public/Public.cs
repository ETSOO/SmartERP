using com.etsoo.ApiModel.RQ.Maps;
using com.etsoo.ApiModel.RQ.SmartERP;
using com.etsoo.ImageUtils.Barcode;
using com.etsoo.Web;
using com.etsoo.WebUtils;
using Microsoft.AspNetCore.Mvc;
using Platform.Server.Endpoints.Public.RQ;
using Platform.Server.Services;

namespace Platform.Server.Endpoints.Public
{
    /// <summary>
    /// Public service APIs
    /// 公共服务API
    /// </summary>
    public static class Public
    {
        public static RouteGroupBuilder MapPublic(this RouteGroupBuilder builder)
        {
            var g = builder.MapGroup("Public").AllowAnonymous();

            g.MapPost("AcceptInvitation", (IPublicService service, AcceptInvitationRQ rq, CancellationToken cancellationToken) => service.AcceptInvitationAsync(rq, cancellationToken))
                .WithDescription("Accept member invitation / 接受成员邀请").WithTags("Public");

            g.MapPost("CreateBarcode", (IPublicService service, BarcodeOptions rq, CancellationToken cancellationToken) => service.CreateBarcodeAsync(rq, cancellationToken))
                .WithDescription("Create barcode image Base64 string / 创建条形码图片的Base64字符串").WithTags("Public");

            g.MapPost("CreateBarcodeSimple", (IPublicService service, BarcodeSimpleOptions rq, CancellationToken cancellationToken) => service.CreateBarcodeAsync(new BarcodeOptions
            {
                BackgroundText = rq.Background.Name,
                ForegroundText = rq.Foreground.Name,
                Type = rq.Type,
                Content = rq.Content,
                Width = rq.Width,
                Height = rq.Height,
                PureBarcode = rq.PureBarcode,
                Margin = rq.Margin
            }, cancellationToken))
                .WithDescription("Create barcode image Base64 string with simple options / 使用简单选项创建条形码图片的Base64字符串").WithTags("Public");

            g.MapPost("GetCurrencies", (IPublicService service, [FromBody] IEnumerable<string>? ids, CancellationToken cancellationToken) => service.GetCurrenciesAsync(ids, cancellationToken))
                .WithDescription("Get currencies / 获取货币列表").WithTags("Public");

            g.MapPost("GetRegions", (IPublicService service, [FromBody] IEnumerable<string>? ids, CancellationToken cancellationToken) => service.GetRegionsAsync(ids, cancellationToken))
                .WithDescription("Get regions / 获取地区列表").WithTags("Public");

            g.MapPost("GetPinyin", (IPublicService service, PinyinRQ rq) => service.GetPinyin(rq))
                .WithDescription("Get Chinese Pinyin / 获取汉字拼音").WithTags("Public");

            g.MapPost("MobileQRCode", (IPublicService service, MobileQRCodeRQ rq, CancellationToken cancellationToken) => service.MobileQRCodeAsync(rq, cancellationToken))
                .WithDescription("Get mobile QRCode image Base64 string / 获取移动端QRCode图片的Base64字符串").WithTags("Public");

            g.MapPost("OrgInfo", async (IPublicService service, IHttpContextAccessor accessor, OrgInfoRQ rq, CancellationToken cancellationToken) =>
            {
                // Check device
                if (!service.CheckDevice(accessor.UserAgent(), rq.DeviceId, out var checkResult, out _))
                {
                    return null;
                }

                return await service.OrgInfoAsync(rq, cancellationToken);
            }).WithDescription("Get organization public information / 获取机构公开信息").WithTags("Public");

            g.MapPost("QueryPlace", (IPublicService service, PlaceQueryRQ rq, CancellationToken cancellationToken) => service.QueryPlaceAsync(rq, cancellationToken))
                .WithDescription("Query place / 查询地点").WithTags("Public");

            g.MapGet("ParseChinaPin/{pin}", (IPublicService service, string pin) => service.ParseChinaPin(pin))
                .WithDescription("Parse China PIN / 解析中国身份证").WithTags("Public");

            g.MapGet("ReadInvitation/{id:guid}", (IPublicService service, Guid id, CancellationToken cancellationToken) => service.ReadInvitationAsync(id, cancellationToken))
                .WithDescription("Read member invitation / 读取成员邀请").WithTags("Public");

            return builder;
        }
    }
}
