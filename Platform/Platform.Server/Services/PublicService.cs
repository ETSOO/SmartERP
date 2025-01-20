using com.etsoo.ApiModel.Dto.Maps;
using com.etsoo.ApiModel.RQ.Maps;
using com.etsoo.ApiProxy.Defs;
using com.etsoo.BaiduApi.Maps;
using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Business;
using com.etsoo.CoreFramework.Models;
using com.etsoo.CoreFramework.User;
using com.etsoo.ImageUtils.Barcode;
using com.etsoo.Localization;
using com.etsoo.Localization.Country;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Serialization;
using com.etsoo.Utils.String;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Platform.Server.Application;
using Platform.Server.Dto.Member;
using Platform.Server.Dto.Public;
using Platform.Server.Endpoints.Public.RQ;
using PlatformShared.Database;
using PlatformShared.Database.Models;
using System.Globalization;
using System.Web;

namespace Platform.Server.Services
{
    /// <summary>
    /// Public service
    /// 公共服务
    /// </summary>
    public class PublicService : CommonService, IPublicService
    {
        readonly MyDbContext _db;
        readonly IDistributedCache _cache;
        readonly IHttpContextAccessor _accessor;
        readonly IMapPlaceService _baidu;
        readonly IBridgeProxy _proxy;
        readonly IAuthCodeService _authCodeService;

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
        /// <param name="baidu">Baidu Map API</param>
        /// <param name="proxy">Proxy API</param>
        public PublicService(MyDbContext db,
            IMyApp app,
            CurrentUserAccessor userAccessor,
            ILogger<PublicService> logger,
            IDistributedCache cache,
            IHttpContextAccessor accessor,
            IMapPlaceService baidu,
            IBridgeProxy proxy,
            IAuthCodeService authCodeService)
            : base(app, userAccessor.User, "public", logger)
        {
            _db = db;
            _cache = cache;
            _accessor = accessor;
            _baidu = baidu;
            _proxy = proxy;
            _authCodeService = authCodeService;
        }

        /// <summary>
        /// Accept member invitation
        /// 接受成员邀请
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> AcceptInvitationAsync(AcceptInvitationRQ rq, CancellationToken cancellationToken = default)
        {
            if (User == null)
            {
                return ApplicationErrors.AccessDenied.AsResult();
            }

            var code = await _authCodeService.ReadAsync(rq.Id, AuthCodeAction.MemberInvitationEmailCode, cancellationToken);
            if (code == null)
            {
                return ApplicationErrors.NoId.AsResult();
            }

            if (code.Expiry < DateTime.UtcNow)
            {
                return ApplicationErrors.CodeExpired.AsResult();
            }

            var data = code.DeserializeData(MyJsonSerializerContext.Default.AuthCodeMemberInvitationData);
            if (data == null)
            {
                return ApplicationErrors.NoValidData.AsResult();
            }

            var orgId = data.UserData.OrganizationId;
            var userId = User.IdInt;
            var exists = await _db.CoreOrganizationUsers.AnyAsync(ou => ou.CoreOrganizationId == orgId && ou.CoreUserId == userId, cancellationToken);
            if (!exists)
            {
                _db.CoreOrganizationUsers.Add(new CoreOrganizationUser
                {
                    CoreOrganizationId = orgId,
                    CoreUserId = userId,
                    UserRole = data.UserRole,
                    IdentityType = IdentityTypeFlags.User
                });

                await _db.SaveChangesAsync(cancellationToken);
            }

            // Delete the code
            await _db.CoreAuthCodes.Where(c => c.Id == rq.Id).ExecuteDeleteAsync(cancellationToken);

            return ActionResult.Success;
        }

        /// <summary>
        /// Create barcode image Base64 string
        /// 创建条形码图片的Base64字符串
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <returns>Base64 string</returns>
        public Task<string> CreateBarcodeAsync(BarcodeOptions rq, CancellationToken cancellationToken = default)
        {
            return Task.Run(() => BarcodeUtils.Create(rq), cancellationToken);
        }

        /// <summary>
        /// Get Chinese Pinyin
        /// 获取汉字拼音
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <returns>Result</returns>
        public string GetPinyin(PinyinRQ rq)
        {
            var py = ChineseUtils.GetPinyin(rq.Input, rq.IsName.GetValueOrDefault());
            return rq.Format switch
            {
                PinyinFormatType.Tone => py.ToPinyin(true),
                PinyinFormatType.Initial => py.ToInitials(),
                _ => py.ToPinyin(false)
            };
        }

        /// <summary>
        /// Get currencies
        /// 获取货币定义
        /// </summary>
        /// <param name="ids">Ids to include and sort by</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IEnumerable<CurrencyItem>> GetCurrenciesAsync(IEnumerable<string>? ids = null, CancellationToken cancellationToken = default)
        {
            var currencies = await CacheFactory.DoAsync(
                _cache,
                App.Configuration.CacheHours,
                () => $"{nameof(PublicService)}.{nameof(GetCurrenciesAsync)}.{CultureInfo.CurrentCulture.LCID}",
                (typeInfo) => Task.Run(() => LocalizationUtils.GetAllRegions().GetCurrencies()),
                MyJsonSerializerContext.Default.IEnumerableCurrencyItem,
                null, cancellationToken);

            if (ids != null)
            {
                var sortIds = ids.ToList();
                currencies = currencies.Where(c => sortIds.Contains(c.Id)).OrderBy(c => sortIds.IndexOf(c.Id));
            }

            return currencies;
        }

        /// <summary>
        /// Get regions
        /// 获取地区
        /// </summary>
        /// <param name="ids">Ids to include and sort by</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IEnumerable<RegionData>> GetRegionsAsync(IEnumerable<string>? ids = null, CancellationToken cancellationToken = default)
        {
            var regions = await CacheFactory.DoAsync(
                _cache,
                App.Configuration.CacheHours,
                () => $"{nameof(PublicService)}.{nameof(GetRegionsAsync)}.{CultureInfo.CurrentCulture.LCID}",
                (typeInfo) => Task.Run(() => LocalizationUtils.GetAllRegions().Values.Select(r => new RegionData
                {
                    Id = r.Id,
                    Id3 = r.Id3,
                    Name = r.Name,
                    EnglishName = r.EnglishName,
                    Currency = r.Currency.Id,
                    Cultures = r.Cultures.Select(c => c.Id)
                })),
                MyJsonSerializerContext.Default.IEnumerableRegionData,
                null, cancellationToken);

            if (ids != null)
            {
                var sortIds = ids.ToList();
                regions = regions.Where(r => sortIds.Contains(r.Id)).OrderBy(r => sortIds.IndexOf(r.Id));
            }

            return regions;
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

            return await CreateBarcodeAsync(options, cancellationToken);
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
                        .Where(oa => oa.CoreAppId == rq.AppId && oa.AppKey == rq.AppKey)
                        .Select(oa => oa.LocalName ?? oa.CoreApp.Name)
                        .FirstOrDefaultAsync(cancellationToken);
                }
            }

            return new OrgPublicInfo
            {
                OrgId = orgId,
                OrgName = orgName,
                AppId = rq.AppId,
                AppName = appName
            };
        }

        /// <summary>
        /// Query place
        /// 查询地点
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <returns>Result</returns>
        public async Task<IEnumerable<PlaceCommon>?> QueryPlaceAsync(PlaceQueryRQ rq, CancellationToken cancellationToken = default)
        {
            if (rq.Provider == ApiProvider.Baidu || (rq.Provider == null && rq.Region?.Equals("CN") is true))
            {
                // Baidu
                return await _baidu.SearchCommonPlaceAsync(com.etsoo.BaiduApi.Maps.Place.RQ.SearchPlaceRQ.CreateFrom(rq), cancellationToken);
            }
            else
            {
                return await _proxy.SearchCommonPlaceAsync(com.etsoo.GoogleApi.Maps.Place.RQ.SearchPlaceRQ.CreateFrom(rq), cancellationToken);
            }
        }

        /// <summary>
        /// Read invitation data
        /// 读取邀请数据
        /// </summary>
        /// <param name="id">Code id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<MemberInvitationData?> ReadInvitationAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var code = await _authCodeService.ReadAsync(id, AuthCodeAction.MemberInvitationEmailCode, cancellationToken);
            if (code == null) return null;

            var data = code.DeserializeData(MyJsonSerializerContext.Default.AuthCodeMemberInvitationData);
            if (data == null) return null;

            var userExists = await _db.CoreUserIdentifiers.AnyAsync(ui => ui.Type == CoreUserIdentifierType.Email && ui.Value == code.OpenId, cancellationToken);

            return new MemberInvitationData
            {
                Email = EncryptWeb(code.OpenId, id.ToString()[..4]),
                Inviter = StringUtils.HideData(data.UserData.Name),
                OrgName = data.UserData.OrganizationName,
                IsExpired = code.Expiry < DateTime.UtcNow,
                UserExists = userExists
            };
        }
    }
}
