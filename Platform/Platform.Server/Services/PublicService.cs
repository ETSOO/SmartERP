using com.etsoo.Address.Validators;
using com.etsoo.AliAmapApi;
using com.etsoo.ApiModel.Dto.Maps;
using com.etsoo.ApiModel.Dto.SmartERP;
using com.etsoo.ApiModel.RQ.Maps;
using com.etsoo.ApiModel.RQ.SmartERP;
using com.etsoo.ApiProxy.Defs;
using com.etsoo.BaiduApi.Maps;
using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Business;
using com.etsoo.CoreFramework.User;
using com.etsoo.Database.Converters;
using com.etsoo.ImageUtils.Barcode;
using com.etsoo.Localization;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Crypto;
using com.etsoo.Utils.Serialization;
using com.etsoo.Utils.Serialization.Country;
using com.etsoo.Utils.String;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Platform.Server.Application;
using Platform.Server.Dto.Member;
using Platform.Server.Dto.Public;
using Platform.Server.Endpoints.Public.RQ;
using PlatformShared;
using PlatformShared.Database;
using PlatformShared.Database.Models;
using PlatformShared.Dto;
using PlatformShared.Extentions;
using PlatformShared.Messages;
using System.Globalization;
using System.Text.Json;
using System.Web;
using static Google.Cloud.Iam.V1.AuditConfigDelta.Types;

namespace Platform.Server.Services
{
    /// <summary>
    /// Public service
    /// 公共服务
    /// </summary>
    public class PublicService : CommonService, IPublicService
    {
        readonly IDbContextFactory<MyDbContext> _dbFactory;
        readonly IDistributedCache _cache;
        readonly IHttpContextAccessor _accessor;
        readonly IMapPlaceService _baidu;
        readonly IAmapService _amap;
        readonly IBridgeProxy _proxy;
        readonly IAuthCodeService _authCodeService;
        readonly IQueueService _queueService;

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="app">Application</param>
        /// <param name="userAccessor">User accessor</param>
        /// <param name="logger">Logger</param>
        /// <param name="cache">Cache</param>
        /// <param name="accessor">HttpContext accessor</param>
        /// <param name="baidu">Baidu Map API</param>
        /// <param name="amap">Amap API</param>
        /// <param name="proxy">Proxy API</param>
        /// <param name="authCodeService">Authcode service</param>
        /// <param name="queueService">Queue service</param>
        public PublicService(
            IDbContextFactory<MyDbContext> dbFactory,
            IMyApp app,
            CurrentUserAccessor userAccessor,
            ILogger<PublicService> logger,
            IDistributedCache cache,
            IHttpContextAccessor accessor,
            IMapPlaceService baidu,
            IAmapService amap,
            IBridgeProxy proxy,
            IAuthCodeService authCodeService,
            IQueueService queueService)
            : base(app, userAccessor.User, "public", logger)
        {
            _dbFactory = dbFactory;
            _cache = cache;
            _accessor = accessor;
            _baidu = baidu;
            _amap = amap;
            _proxy = proxy;
            _authCodeService = authCodeService;
            _queueService = queueService;
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
            if (code == null || !code.UserId.HasValue)
            {
                return ApplicationErrors.NoId.AsResult();
            }

            if (code.Expiry < DateTime.UtcNow)
            {
                return ApplicationErrors.CodeExpired.AsResult();
            }

            var data = code.DeserializeData(PlatformSharedContext.Default.AuthCodeMemberInvitationData);
            if (data == null)
            {
                return ApplicationErrors.NoValidData.AsResult();
            }

            var tasks = new List<Task>();

            var orgId = data.UserData.OrganizationId;
            var userId = User.IdInt;
            var inviterId = code.UserId.Value;

            await using var _db = await _dbFactory.CreateDbContextAsync(cancellationToken);

            var personIds = await _db.CheckUserPersonIdsAsync(orgId, [userId, inviterId], cancellationToken);
            var userPersonId = personIds[0];
            var inviterPersonId = personIds[1];

            if (inviterPersonId == null)
            {
                return ApplicationErrors.NoValidData.AsResult("InviterPersonId");
            }

            if (userPersonId == null)
            {
                _db.Persons.Add(new Person
                {
                    OrgId = orgId,
                    CoreUserId = userId,
                    Name = User.Name,
                    FamilyName = User.FamilyName,
                    GivenName = User.GivenName,
                    UserRole = data.UserRole,
                    IdentityType = IdentityTypeFlags.User,
                    QueryKeyword = ChineseUtils.GetPinyin(User.Name, true).ToInitials(),
                    InviterId = inviterId,
                    UserId = inviterPersonId.Value // User.Oid is a user's person id in a specific organization
                });

                var user = await _db.CoreUsers.Where(u => u.Id == userId)
                    .Select(u => new CoreUser { Id = u.Id, LatestOrganizationIds = u.LatestOrganizationIds })
                    .FirstOrDefaultAsync(cancellationToken);
                if (user != null)
                {
                    _db.Attach(user);

                    if (user.LatestOrganizationIds == null)
                    {
                        user.LatestOrganizationIds = [orgId];
                    }
                    else
                    {
                        user.LatestOrganizationIds.Remove(orgId);
                        user.LatestOrganizationIds = [orgId, .. user.LatestOrganizationIds.Take(9)];
                    }
                }

                var task1 = _db.SaveChangesAsync(cancellationToken);

                // Log
                var message = new AcceptInvitationMessage
                {
                    Data = User.CreateMessageData(App.AppId, inviterId),
                    UserData = data.UserData
                };
                var task2 = _queueService.PushAsync(message, PlatformSharedContext.Default.AcceptInvitationMessage, cancellationToken);

                tasks.AddRange(task1, task2);
            }

            // Delete the code
            await using var codeDb = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var task3 = codeDb.CoreAuthCodes.Where(c => c.Id == rq.Id).ExecuteDeleteAsync(cancellationToken);
            tasks.Add(task3);

            await Task.WhenAll(tasks);

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
        /// Get cultures
        /// 获取语言文化
        /// </summary>
        /// <param name="ids">Ids to include and sort by</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IEnumerable<CultureItem>> GetCulturesAsync(IEnumerable<string> ids, CancellationToken cancellationToken = default)
        {
            return await Task.Run(() => LocalizationUtils.GetCultures(ids), cancellationToken);
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
            var key = $"{nameof(PublicService)}.{nameof(GetCurrenciesAsync)}.{CultureInfo.CurrentCulture.LCID}";
            var currencies = await _cache.GetOrCreateAsync(key, async (options) =>
            {
                options.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(App.Configuration.CacheHours);
                return await Task.Run(() => LocalizationUtils.GetAllRegions().GetCurrencies(), cancellationToken);
            }, CommonJsonSerializerContext.Default.IEnumerableCurrencyItem, cancellationToken);

            if (currencies == null)
            {
                return [];
            }

            if (ids != null)
            {
                var sortIds = ids.ToList();
                return currencies.Where(c => sortIds.Contains(c.Id)).OrderBy(c => sortIds.IndexOf(c.Id));
            }

            return currencies;
        }

        /// <summary>
        /// Get custom resources
        /// 获取自定义资源
        /// </summary>
        /// <param name="culture">Culture</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public ValueTask<CustomResourceData[]?> GetCustomResourcesAsync(string culture, CancellationToken cancellationToken = default)
        {
            var key = $"{nameof(PublicService)}.{nameof(GetCustomResourcesAsync)}.{culture}";
            return _cache.GetOrCreateAsync(key, async (options) =>
            {
                options.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(App.Configuration.CacheHours);

                await using var _db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                return await _db.FeatureCultures.AsNoTracking()
                    .Where(c => c.Culture == culture && c.CoreOrganizationId == null)
                    .Select(c => new CustomResourceData
                    {
                        Key = c.Key,
                        Title = c.Title,
                        Description = c.Description,
                        JsonData = c.JsonData
                    }).ToArrayAsync(cancellationToken);
            }, PlatformSharedContext.Default.CustomResourceDataArray, cancellationToken);
        }

        /// <summary>
        /// Get regions
        /// 获取地区
        /// </summary>
        /// <param name="ids">Ids to include and sort by</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IEnumerable<RegionItem>> GetRegionsAsync(IEnumerable<string>? ids = null, CancellationToken cancellationToken = default)
        {
            var key = $"{nameof(PublicService)}.{nameof(GetRegionsAsync)}.{CultureInfo.CurrentCulture.LCID}";

            var regions = await _cache.GetOrCreateAsync(key, async (options) =>
            {
                options.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(App.Configuration.CacheHours);
                return await Task.Run(() => LocalizationUtils.GetAllRegions().GetRegions(), cancellationToken);
            }, CommonJsonSerializerContext.Default.IEnumerableRegionItem, cancellationToken);

            if (regions == null)
            {
                return [];
            }

            if (ids != null)
            {
                var sortIds = ids.ToList();
                return regions.Where(r => sortIds.Contains(r.Id)).OrderBy(r => sortIds.IndexOf(r.Id));
            }

            return regions;
        }

        /// <summary>
        /// Get time zones
        /// 获取时区
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IEnumerable<TimeZoneItem>> GetTimeZonesAsync(TimeZoneRQ rq, CancellationToken cancellationToken = default)
        {
            var culture = rq.Culture;
            if (string.IsNullOrEmpty(culture))
            {
                culture = CultureInfo.CurrentCulture.IsNeutralCulture ? CultureInfo.CurrentCulture.Name : CultureInfo.CurrentCulture.Parent.Name;
            }
            else
            {
                LocalizationUtils.SetCulture(culture);
            }

            var all = rq.All ?? false;
            var key = $"{nameof(PublicService)}.{nameof(GetTimeZonesAsync)}.{culture}";
            if (all) key += ".All";

            var timeZones = await _cache.GetOrCreateAsync(key, async (options) =>
            {
                options.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(App.Configuration.CacheHours);

                if (all)
                {
                    return await Task.Run(() => TimeZoneInfo.GetSystemTimeZones().Select(s => TimeZoneUtils.CreateFrom(s)), cancellationToken);
                }

                await using var _db = await _dbFactory.CreateDbContextAsync(cancellationToken);

                var jsonData = await _db.FeatureCultures.AsNoTracking()
                    .Where(c => c.Culture == culture && c.CoreOrganizationId == null && c.Key == MyAppConstants.TimeZoneResourceKey)
                    .Select(c => c.JsonData)
                    .FirstOrDefaultAsync(cancellationToken);

                TimeZoneResourceItem[]? items = null;
                if (!string.IsNullOrEmpty(jsonData))
                {
                    try
                    {
                        items = JsonSerializer.Deserialize(jsonData, MyJsonSerializerContext.Default.IEnumerableTimeZoneResourceItem)?.ToArray();
                    }
                    catch (Exception ex)
                    {
                        LogException(ex);
                    }
                }

                var timeZones = new List<TimeZoneItem>();

                if (items != null)
                {
                    foreach (var item in items)
                    {
                        var tz = TimeZoneUtils.CreateFrom(item.Id);
                        if (tz == null) continue;

                        if (!string.IsNullOrEmpty(item.Label))
                        {
                            tz.DisplayName = item.Label;
                        }

                        timeZones.Add(tz);
                    }
                }

                return [.. timeZones];
            }, CommonJsonSerializerContext.Default.IEnumerableTimeZoneItem, cancellationToken);

            if (timeZones == null || !timeZones.Any())
            {
                timeZones = [TimeZoneUtils.CreateFrom(TimeZoneInfo.Local)];
            }

            if (rq.Id != null)
            {
                timeZones = timeZones.Where(tz => tz.Id == rq.Id);
            }

            if (rq.Ids != null)
            {
                var sortIds = rq.Ids.ToList();
                timeZones = timeZones.Where(tz => sortIds.Contains(tz.Id)).OrderBy(tz => sortIds.IndexOf(tz.Id));
            }

            if (rq.ExcludedIds != null)
            {
                timeZones = timeZones.Where(tz => !rq.ExcludedIds.Contains(tz.Id));
            }

            var keyword = rq.Keyword;
            if (!string.IsNullOrEmpty(keyword))
            {
                if (int.TryParse(keyword, out var offset))
                {
                    timeZones = timeZones.Where(tz => Math.Abs(tz.UtcOffset.TotalHours) == offset);
                }
                else
                {
                    timeZones = timeZones.Where(tz => tz.DisplayName.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                        || tz.StandardName.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                        || tz.Id.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                    );
                }
            }

            var page = rq.QueryPaging?.CurrentPage ?? 1;
            var batchSize = rq.QueryPaging?.BatchSize ?? 16;
            
            var skip = Convert.ToInt32((page - 1) * batchSize);
            if (skip < 0) skip = 0;

            return timeZones.Skip(skip).Take(batchSize);
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
                await using var _db = await _dbFactory.CreateDbContextAsync(cancellationToken);

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
                await using var _db = await _dbFactory.CreateDbContextAsync(cancellationToken);

                if (string.IsNullOrEmpty(rq.AppKey))
                {
                    // Get app name from root
                    appName = await _db.CoreApps.AsNoTracking()
                        .Where(a => a.Id == rq.AppId)
                        .Select(a => a.Name)
                        .FirstOrDefaultAsync(cancellationToken);
                }
                else
                {
                    appName = await _db.CoreOrganizationApps.AsNoTracking()
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
        /// Parse China Pin
        /// 解析中国身份证
        /// </summary>
        /// <param name="pin">PIN</param>
        /// <returns>Result</returns>
        public ChinaPinData? ParseChinaPin(string pin)
        {
            var validator = new ChinaPinValidator(pin);
            if (validator.Valid)
            {
                return new ChinaPinData
                {
                    StateNum = validator.StateNum,
                    CityNum = validator.CityNum,
                    DistrictNum = validator.DistrictNum,
                    Birthday = validator.Birthday.Value,
                    IsFemale = validator.IsFemale.Value
                };
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Parse name
        /// 解析姓名
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <returns>Result</returns>
        public NameData ParseName(ParseNameRQ rq)
        {
            return ParseName(rq.Name, rq.FamilyName, rq.GivenName);
        }

        /// <summary>
        /// Parse name
        /// 解析姓名
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="familyName">Family name</param>
        /// <param name="givenName">Given name</param>
        /// <returns>Result</returns>
        public NameData ParseName(string name, string? familyName, string? givenName)
        {
            return LocalizationUtils.ParseName(name, familyName, givenName);
        }

        /// <summary>
        /// Query place
        /// 查询地点
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <returns>Result</returns>
        public async Task<IEnumerable<PlaceCommon>?> QueryPlaceAsync(PlaceQueryRQ rq, CancellationToken cancellationToken = default)
        {
            if (rq.Provider == ApiProvider.Amap)
            {
                // 高德地图
                return await _amap.SearchCommonPlaceAsync(com.etsoo.AliAmapApi.RQ.SearchPlaceRQ.CreateFrom(rq), cancellationToken);
            }
            else if (rq.Provider == ApiProvider.Baidu)
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
            var action = AuthCodeAction.MemberInvitationEmailCode;

            var actionItem = AuthCodeActionItem.Actions.FirstOrDefault(a => a.Id == action);
            if (actionItem == null) return null;

            var auth = await _authCodeService.ReadAsync(id, action, cancellationToken);
            if (auth == null) return null;

            var data = auth.DeserializeData(PlatformSharedContext.Default.AuthCodeMemberInvitationData);
            if (data == null) return null;

            await using var _db = await _dbFactory.CreateDbContextAsync(cancellationToken);

            var userId = await _db.CoreUserIdentifiers.Where(ui => ui.Type == CoreUserIdentifierType.Email
                && ui.Value == auth.OpenId
                && ui.CoreUser.Step == 0)
            .Select(ui => ui.CoreUserId).FirstOrDefaultAsync(cancellationToken);

            var isAccepted = false;
            var userExists = false;
            if (userId > 0)
            {
                userExists = true;
                isAccepted = await _db.Users(data.UserData.OrganizationId).AnyAsync(ou => ou.CoreUserId == userId, cancellationToken);
            }

            return new MemberInvitationData
            {
                Email = EncryptWeb(auth.OpenId, id.ToString()[..4]),
                Inviter = StringUtils.HideData(data.UserData.Name),
                OrgName = data.UserData.OrganizationName,
                IsExpired = auth.Expiry < DateTime.UtcNow,
                IsAccepted = isAccepted,
                UserExists = userExists
            };
        }
    }
}
