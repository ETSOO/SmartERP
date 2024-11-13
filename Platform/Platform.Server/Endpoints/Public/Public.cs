using com.etsoo.ApiModel.RQ.Maps;
using com.etsoo.CoreFramework.Models;
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

            g.MapPost("CreateBarcode", (IPublicService service, BarcodeOptions rq, CancellationToken cancellationToken) =>
            {
                return service.CreateBarcodeAsync(rq, cancellationToken);
            }).WithDescription("Create barcode image Base64 string / 创建条形码图片的Base64字符串").WithTags("Public");

            g.MapPost("GetCurrencies", (IPublicService service, [FromBody] IEnumerable<string>? ids, CancellationToken cancellationToken) =>
            {
                return service.GetCurrenciesAsync(ids, cancellationToken);
            }).WithDescription("Get currencies / 获取货币列表").WithTags("Public");

            g.MapPost("GetRegions", (IPublicService service, [FromBody] IEnumerable<string>? ids, CancellationToken cancellationToken) =>
            {
                return service.GetRegionsAsync(ids, cancellationToken);
            }).WithDescription("Get regions / 获取地区列表").WithTags("Public");

            g.MapPost("GetPinyin", (IPublicService service, PinyinRQ rq) =>
            {
                return service.GetPinyin(rq);
            }).WithDescription("Get Chinese Pinyin / 获取汉字拼音").WithTags("Public");

            g.MapPost("MobileQRCode", (IPublicService service, MobileQRCodeRQ rq, CancellationToken cancellationToken) =>
            {
                return service.MobileQRCodeAsync(rq, cancellationToken);
            }).WithDescription("Get mobile QRCode image Base64 string / 获取移动端QRCode图片的Base64字符串").WithTags("Public");

            g.MapPost("OrgInfo", (IPublicService service, IHttpContextAccessor accessor, OrgInfoRQ rq, CancellationToken cancellationToken) =>
            {
                // Check device
                if (!service.CheckDevice(accessor.UserAgent(), rq.DeviceId, out var checkResult, out _))
                {
                    return null;
                }

                return service.OrgInfoAsync(rq, cancellationToken);
            }).WithDescription("Get organization public information / 获取机构公开信息").WithTags("Public");

            g.MapPost("QueryPlace", (IPublicService service, PlaceQueryRQ rq, CancellationToken cancellationToken) =>
            {
                return service.QueryPlaceAsync(rq, cancellationToken);
            }).WithDescription("Query place / 查询地点").WithTags("Public");

            return builder;
        }
    }
}
