using com.etsoo.ImageUtils.Barcode;
using com.etsoo.Utils.Serialization;
using Microsoft.Extensions.Caching.Distributed;
using Platform.Server.Application;
using Platform.Server.Endpoints.Public.RQ;
using System.Web;

namespace Platform.Server.Services
{
    public class PublicService : CommonService, IPublicService
    {
        readonly IDistributedCache _cache;
        readonly IHttpContextAccessor _accessor;

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="app">Application</param>
        /// <param name="userAccessor">User accessor</param>
        /// <param name="logger">Logger</param>
        /// <param name="accessor">HttpContext accessor</param>
        public PublicService(IMyApp app, IMyUserAccessor userAccessor, ILogger<PublicService> logger, IDistributedCache cache, IHttpContextAccessor accessor)
            : base(app, userAccessor.User, "public", logger)
        {
            _cache = cache;
            _accessor = accessor;
        }

        /// <summary>
        /// Get mobile QRCode image Base64 string
        /// 获取移动端QRCode图片的Base64字符串
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <returns>Base64 string</returns>
        public async ValueTask<string> MobileQRCodeAsync(MobileQRCodeRQ rq, CancellationToken cancellationToken = default)
        {
            if (rq.Host == null)
            {
                var header = _accessor.HttpContext?.Request.GetTypedHeaders();
                var uriReferer = header?.Referer;
                if (uriReferer == null)
                {
                    return string.Empty;
                }
                rq.Host = uriReferer.AbsoluteUri;
            }

            var baseUrl = $"{rq.Host}?loginid={HttpUtility.UrlEncode(rq.Id)}";

            var options = new BarcodeOptions
            {
                Type = "QRCode",
                Content = baseUrl,
                ForegroundText = "#3f51b5",
                Width = 180,
                Height = 180
            };

            return await QRCodeAsync(options, cancellationToken);
        }

        /// <summary>
        /// Get QRCode image Base64 string
        /// 获取QRCode图片的Base64字符串
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <returns>Base64 string</returns>
        public async Task<string> QRCodeAsync(BarcodeOptions rq, CancellationToken cancellationToken = default)
        {
            return await CacheFactory.DoStringAsync(
                _cache,
                App.Configuration.CacheHours,
                () => $"{nameof(PublicService)}.{nameof(QRCodeAsync)}.{rq}",
                () => Task.Run(() => BarcodeUtils.Create(rq)),
                cancellationToken: cancellationToken);
        }
    }
}
