using com.etsoo.ImageUtils.Barcode;
using com.etsoo.Utils.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Platform.Server.Application;
using Platform.Server.Database;
using Platform.Server.Dto.Public;
using Platform.Server.Endpoints.Public.RQ;
using System.Web;

namespace Platform.Server.Services
{
    public class PublicService : CommonService, IPublicService
    {
        readonly MyDbContext _db;
        readonly IDistributedCache _cache;
        readonly IHttpContextAccessor _accessor;

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="db">Database EF</param>
        /// <param name="app">Application</param>
        /// <param name="userAccessor">User accessor</param>
        /// <param name="logger">Logger</param>
        /// <param name="cache">Cache</param>
        /// <param name="accessor">HttpContext accessor</param>
        public PublicService(MyDbContext db, IMyApp app, IMyUserAccessor userAccessor, ILogger<PublicService> logger, IDistributedCache cache, IHttpContextAccessor accessor)
            : base(app, userAccessor.User, "public", logger)
        {
            _db = db;
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
        /// Get organization public information
        /// 获取机构公开信息
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<OrgPublicInfo> OrgInfoAsync(OrgInfoRQ rq, CancellationToken cancellationToken = default)
        {
            // Organization name
            int? orgId = null;
            string? orgName = null;
            if (rq.OrgUid != null)
            {
                var data = await _db.CoreOrganizations.Where(o => o.Uid == rq.OrgUid).Select(o => new { o.Id, o.Name }).FirstOrDefaultAsync(cancellationToken);
                if (data != null)
                {
                    orgId = data.Id;
                    orgName = data.Name;
                }
            }

            string? appName = null;
            if (rq.AppId != null)
            {
                if (string.IsNullOrEmpty(rq.AppKey))
                {
                    // Get app name from root
                    appName = await _db.CoreApps.Where(a => a.Id == rq.AppId).Select(a => a.Name).FirstOrDefaultAsync(cancellationToken);
                }
                else
                {
                    appName = await _db.CoreOrganizationApps
                        .Include(oa => oa.CoreApp)
                        .Where(oa => oa.CoreAppId == rq.AppId && oa.AppKey == rq.AppKey)
                        .Select(oa => oa.LocalName ?? oa.CoreApp.Name)
                        .FirstOrDefaultAsync(cancellationToken);
                }
            }

            return new OrgPublicInfo
            {
                OrgId = orgId,
                OrgName = orgName,
                AppName = appName
            };
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
