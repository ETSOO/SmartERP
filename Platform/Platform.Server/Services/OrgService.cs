using com.etsoo.ApiModel;
using com.etsoo.ApiModel.Dto.SmartERP;
using com.etsoo.ApiModel.Dto.SmartERP.MessageQueue;
using com.etsoo.ApiModel.RQ.SmartERP;
using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Authentication;
using com.etsoo.CoreFramework.Business;
using com.etsoo.CoreFramework.Json;
using com.etsoo.CoreFramework.Models;
using com.etsoo.CoreFramework.User;
using com.etsoo.Database;
using com.etsoo.HtmlIO;
using com.etsoo.HTTP;
using com.etsoo.Utils;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Serialization;
using com.etsoo.Utils.Storage;
using Json.Schema;
using Microsoft.EntityFrameworkCore;
using Platform.Server.Application;
using Platform.Server.Dto.Org;
using Platform.Server.Dto.Public;
using Platform.Server.Endpoints.Org.RQ;
using Platform.Server.Schemas;
using PlatformShared;
using PlatformShared.Database;
using PlatformShared.Database.Models;
using PlatformShared.Dto;
using PlatformShared.Extentions;
using PlatformShared.Messages;
using PlatformShared.Services;
using System.Buffers;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Platform.Server.Services
{
    /// <summary>
    /// Organization service
    /// 机构服务
    /// </summary>
    public class OrgService : CommonUserService, IOrgService
    {
        const string SysResourceKeyPrefix = "etsoo";

        static readonly ConcurrentDictionary<CoreApiService, JsonSchemaCreator> apiSchemas = new()
        {
            [CoreApiService.SMTP] = CoreApiServiceSMTPSchema.Create,
            [CoreApiService.Storage] = CoreApiServiceStorageSchema.Create
        };

        static bool ValidateApiServiceSchema(CoreApiService service, string? json, [NotNullWhen(false)] out ActionResult? result)
        {
            if (apiSchemas.TryGetValue(service, out var creator))
            {
                var schema = creator();
                var sr = schema.Evaluate(JsonElement.Parse(json ?? "{}"));
                if (sr.IsValid)
                {
                    result = null;
                    return true;
                }
                else
                {
                    result = ApplicationErrors.NoValidData.AsResult("options");

                    if (sr.Errors?.Count > 0)
                    {
                        result.Detail = string.Join("; ", sr.Errors.Select(e => e.ToString()));
                    }

                    return false;
                }
            }

            result = ApplicationErrors.NoValidData.AsResult("schema");

            return false;
        }

        readonly MyDbContext _db;
        readonly IPublicService _publicService;
        readonly IStorageFactory _storageFactory;
        readonly IQueueService _queueService;
        readonly ISmartERPCoordinator _erp;

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="db">Database EF</param>
        /// <param name="app">Application</param>
        /// <param name="userAccessor">User accessor</param>
        /// <param name="logger">Logger</param>
        /// <param name="publicService">Public service</param>
        /// <param name="storage">Storage</param>
        /// <param name="queueService">Queue service</param>
        public OrgService(MyDbContext db, IMyApp app,
            CurrentUserAccessor userAccessor,
            ILogger<OrgService> logger,
            IPublicService publicService,
            IStorageFactory storageFactory,
            IQueueService queueService,
            ISmartERPCoordinator erp)
            : base(app, userAccessor.UserSafe, "org", logger)
        {
            _db = db;
            _publicService = publicService;
            _storageFactory = storageFactory;
            _queueService = queueService;
            _erp = erp;
        }

        /// <summary>
        /// Create organization
        /// 创建机构
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> CreateAsync(OrgCreateRQ rq, CancellationToken cancellationToken = default)
        {
            var (result, id) = await CreateWithIdAsync(rq, cancellationToken);
            if (id.HasValue) return ActionResult.Succeed(id.Value);
            return result;
        }

        /// <summary>
        /// Create API
        /// 创建接口
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> CreateApiAsync(OrgCreateApiRQ rq, CancellationToken cancellationToken = default)
        {
            // Format request data
            var result = await FormatRQAsync(rq, cancellationToken);
            if (!result.Ok)
            {
                return result;
            }
            else if (rq.OrgId == null)
            {
                return ApplicationErrors.NoId.AsResult(nameof(rq.OrgId));
            }

            // Validate the schema
            if (!ValidateApiServiceSchema(rq.Service, rq.Options, out var schemaResult))
            {
                return schemaResult;
            }

            // Existing
            if (await _db.CoreApis.AsNoTracking()
                .Where(a => a.CoreOrganizationId == rq.OrgId && a.Service == rq.Service)
                .AnyAsync(cancellationToken))
            {
                return ApplicationErrors.ItemExists.AsResult(nameof(rq.Service));
            }

            // Create API
            var api = new CoreApi
            {
                CoreOrganizationId = rq.OrgId.Value,
                Service = rq.Service,
                Title = rq.Title,
                Endpoint = rq.Endpoint,
                AppId = rq.AppId,
                AppSecret = EncryptAppSecret(rq.AppSecret),
                Options = rq.Options,
                RatePolicy = rq.RatePolicy,
                Enabled = rq.Enabled.GetValueOrDefault(true),
                Inheritance = rq.Inheritance.GetValueOrDefault(true)
            };

            _db.CoreApis.Add(api);
            await _db.SaveChangesAsync(cancellationToken);

            // Push message
            var message = new CreateApiMessage
            {
                Data = User.CreateMessageData(App.AppId, api.Id, rq.Title),
                OrganizationId = api.CoreOrganizationId
            };
            await _queueService.PushAsync(message, PlatformSharedContext.Default.CreateApiMessage, cancellationToken);

            return ActionResult.Succeed(api.Id);
        }

        private string EncryptAppSecret(string appSecret)
        {
            return App.EncriptData(appSecret, ServiceConstants.CoreApiAppSecretEncryptionKey);
        }

        /// <summary>
        /// Create resource
        /// 创建资源
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async ValueTask<IActionResult> CreateResourceAsync(OrgCreateResourceRQ rq, CancellationToken cancellationToken = default)
        {
            // Format request data
            var result = await FormatRQAsync(rq, cancellationToken);
            if (!result.Ok)
            {
                return result;
            }

            if (rq.Id.HasValue)
            {
                var id = rq.Id.Value;

                // Check the resource id
                var resource = await _db.FeatureCultures.AsNoTracking()
                    .Where(o => o.Id == id && (rq.OrgId == null || o.CoreOrganizationId == rq.OrgId))
                    .Select(o => new { o.Key, o.CoreOrganizationId })
                    .FirstOrDefaultAsync(cancellationToken);

                if (resource == null)
                {
                    return ApplicationErrors.NoId.AsResult();
                }

                // Remove all items
                if (rq.Items != null && !rq.Items.Any())
                {
                    await _db.FeatureCultures.Where(c => c.Key == resource.Key && c.CoreOrganizationId == resource.CoreOrganizationId)
                        .ExecuteDeleteAsync(cancellationToken);
                }
                else
                {
                    if (rq.Items != null)
                    {
                        // Load all existing cultures with tracking
                        var cultures = await _db.FeatureCultures
                            .Where(c => c.Key == resource.Key && c.CoreOrganizationId == resource.CoreOrganizationId)
                            .ToListAsync(cancellationToken);

                        foreach (var item in rq.Items)
                        {
                            var culture = cultures.Find(c => c.Culture == item.Culture);

                            if (culture == null)
                            {
                                // New culture
                                if (!string.IsNullOrEmpty(item.Title))
                                {
                                    _db.FeatureCultures.Add(new FeatureCulture
                                    {
                                        Key = rq.Key ?? resource.Key,
                                        CoreOrganizationId = rq.OrgId ?? resource.CoreOrganizationId,
                                        Culture = item.Culture,
                                        Title = item.Title,
                                        Description = item.Description,
                                        JsonData = item.JsonData
                                    });
                                }
                            }
                            else
                            {
                                if ((item.UpdatedFlag & 1) > 0 && !string.IsNullOrEmpty(item.Title))
                                {
                                    culture.Title = item.Title;
                                }

                                if ((item.UpdatedFlag & 2) > 0)
                                {
                                    culture.Description = item.Description;
                                }

                                if ((item.UpdatedFlag & 4) > 0)
                                {
                                    culture.JsonData = item.JsonData;
                                }
                            }
                        }

                        // Save changes
                        await _db.SaveChangesAsync(cancellationToken);
                    }
                    else
                    {
                        // Update the key
                        if (!string.IsNullOrEmpty(rq.Key) && rq.Key != resource.Key)
                        {
                            await _db.FeatureCultures.Where(c => c.Key == resource.Key && c.CoreOrganizationId == resource.CoreOrganizationId)
                                .ExecuteUpdateAsync(c => c.SetProperty(c => c.Key, rq.Key), cancellationToken);
                        }

                        // Update the organization id
                        if (rq.OrgId != resource.CoreOrganizationId)
                        {
                            await _db.FeatureCultures.Where(c => c.Key == resource.Key && c.CoreOrganizationId == resource.CoreOrganizationId)
                                .ExecuteUpdateAsync(c => c.SetProperty(c => c.CoreOrganizationId, rq.OrgId), cancellationToken);
                        }
                    }
                }
            }
            else
            {
                if (string.IsNullOrEmpty(rq.Key) || rq.Key.StartsWith(SysResourceKeyPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    return ApplicationErrors.NoValidData.AsResult(nameof(rq.Key));
                }

                if (rq.Items == null || rq.Items.Any(item => string.IsNullOrEmpty(item.Title)))
                {
                    return ApplicationErrors.NoValidData.AsResult(nameof(rq.Items));
                }

                // Check the resource id
                var hasKey = await _db.FeatureCultures.AsNoTracking()
                    .Where(o => o.Key == rq.Key && o.CoreOrganizationId == rq.OrgId)
                    .AnyAsync(cancellationToken);

                if (hasKey)
                {
                    return ApplicationErrors.ItemExists.AsResult(nameof(rq.Key));
                }

                _db.FeatureCultures.AddRange(rq.Items.Select(item => new FeatureCulture
                {
                    Key = rq.Key,
                    CoreOrganizationId = rq.OrgId,
                    Culture = item.Culture,
                    Title = item.Title!, // Validated above
                    Description = item.Description,
                    JsonData = item.JsonData
                }));

                // Save
                await _db.SaveChangesAsync(cancellationToken);
            }

            // Push message
            var message = new CreateResourceMessage
            {
                Data = User.CreateMessageData(App.AppId, rq.Id ?? 0),
                RequestData = JsonSerializer.Serialize(rq, MyJsonSerializerContext.Default.OrgCreateResourceRQ)
            };

            await _queueService.PushAsync(message, PlatformSharedContext.Default.CreateResourceMessage, cancellationToken);

            return ActionResult.Success;
        }

        /// <summary>
        /// Create organization with returning id
        /// 创建机构并返回编号
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<(IActionResult result, int? id)> CreateWithIdAsync(OrgCreateRQ rq, CancellationToken cancellationToken = default)
        {
            // Check exists
            var exists = await _db.CoreOrganizations.AnyAsync(o => o.Name == rq.Name && o.Pin == rq.Pin, cancellationToken);
            if (exists)
            {
                return (ApplicationErrors.OrgExists.AsResult("Name"), null);
            }

            // Default QueryKeyword
            var queryKeyword = string.IsNullOrEmpty(rq.QueryKeyword)
                ? _publicService.GetPinyin(new PinyinRQ { Input = rq.Name, Format = PinyinFormatType.Initial })
                : rq.QueryKeyword
            ;

            // Create organization
            var userId = User.IdInt;
            var org = new CoreOrganization
            {
                OwnerId = userId,
                Name = rq.Name,
                Brand = rq.Brand,
                Pin = rq.Pin,
                ParentId = rq.ParentId,
                Status = rq.Status.GetValueOrDefault(),
                QueryKeyword = queryKeyword,
                Region = rq.Region,
                Persons = [
                    new Person
                    {
                        Name = User.Name,
                        CoreUserId = userId,
                        IdentityType = IdentityTypeFlags.User,
                        UserRole = UserRole.Founder,
                        UserId = userId
                    }
                ]
            };

            // Add
            _db.CoreOrganizations.Add(org);

            // Save
            await _db.SaveChangesAsync(cancellationToken);

            // Return
            return (ActionResult.Success, org.Id);
        }

        /// <summary>
        /// Delete organization
        /// 删除机构
        /// </summary>
        /// <param name="id">Organization id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var orgExists = await _db.CoreOrganizations.AsNoTracking().AnyAsync(o => o.Id == id && o.OwnerId == User.IdInt && o.Status == EntityStatus.Deleted, cancellationToken);
            if (!orgExists)
            {
                return ApplicationErrors.NoId.AsResult();
            }

            var oid = await _db.Users(id).AsNoTracking()
                .Where(ou => ou.CoreUserId != User.IdInt)
                .Select(ou => ou.Id)
                .FirstOrDefaultAsync(cancellationToken);
            if (oid < 1)
            {
                return ApplicationErrors.DeleteReferencedData.AsResult("User");
            }

            var appExists = await _db.CoreOrganizationApps.AsNoTracking().AnyAsync(oa => oa.CoreOrganizationId == id, cancellationToken);
            if (appExists)
            {
                return ApplicationErrors.DeleteReferencedData.AsResult("App");
            }

            await _db.Persons.Where(ou => ou.Id == oid).ExecuteDeleteAsync(cancellationToken);
            await _db.CoreOrganizations.Where(o => o.Id == id).ExecuteDeleteAsync(cancellationToken);

            return ActionResult.Succeed(id);
        }

        /// <summary>
        /// Download file
        /// 下载文件
        /// </summary>
        /// <param name="kind">Kind</param>
        /// <param name="id">File id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        public async Task<IResult> DownloadFileAsync(OrgDownloadKind kind, long id, CancellationToken cancellationToken = default)
        {
            var orgId = User.OrganizationInt;
            var role = User.Role;

            FileData? data = null;
            if (kind == OrgDownloadKind.Profile)
            {
                data = await _db.PersonProfileAttachments.AsNoTracking()
                    .Where(a => a.Id == id && a.Profile.Person.OrgId == orgId && (a.Profile.UserRole == null || a.Profile.UserRole <= role))
                    .Select(a => new FileData { FileName = a.FileName, ContentType = a.ContentType, Description = a.Description })
                    .FirstOrDefaultAsync(cancellationToken);
            }

            if (data == null)
            {
                return Results.BadRequest("Invalid Id");
            }

            var storage = await _storageFactory.CreateAsync(orgId, cancellationToken);
            var stream = await storage.ReadAsync(data.FileName, cancellationToken);

            if (stream == null)
            {
                return Results.BadRequest("No Stream");
            }

            var fileName = data.Description + Path.GetExtension(data.FileName);

            return Results.File(stream, data.ContentType, fileName, enableRangeProcessing: true);
        }

        /// <summary>
        /// Format HTML content
        /// 格式化网页内容
        /// </summary>
        /// <param name="content">HTML content</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Result</returns>
        public async Task<string?> FormatHtmlContentAsync(string content, CancellationToken cancellationToken = default)
        {
            var orgId = User.OrganizationInt;

            var storage = await _storageFactory.CreateAsync(orgId, cancellationToken);

            var path = _storageFactory.GetOrgPath(orgId, "Resources");

            return await HtmlIOUtils.FormatEditorContentAsync(storage, path, content, Logger, cancellationToken);
        }

        /// <summary>
        /// Get custom resources
        /// 获取自定义资源
        /// </summary>
        /// <param name="culture">Culture</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IEnumerable<CustomResourceData>> GetCustomResourcesAsync(string culture, CancellationToken cancellationToken = default)
        {
            var orgId = User.OrganizationInt;

            return await _db.FeatureCultures.AsNoTracking()
            .Where(c => c.CoreOrganizationId == orgId && c.Culture == culture && !c.Key.StartsWith(SysResourceKeyPrefix))
            .Select(c => new CustomResourceData
            {
                Key = c.Key,
                OrgId = c.CoreOrganizationId,
                Title = c.Title,
                Description = c.Description,
                JsonData = c.JsonData
            }).ToArrayAsync(cancellationToken);
        }

        /// <summary>
        /// Get user's latest accessed organizations
        /// 获取用户最近访问的机构
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IEnumerable<OrgGetMyData>> GetMyAsync(OrgGetMyRQ rq, CancellationToken cancellationToken = default)
        {
            // Current user's latest organizations
            var ids = await _db.CoreUsers.AsNoTracking()
                .Where(u => u.Id == User.IdInt)
                .Select(u => u.LatestOrganizationIds).FirstOrDefaultAsync(cancellationToken) ?? [];

            return await _db.Persons
                .AsNoTracking()
                .Where(p => p.CoreUserId == User.IdInt
                    && p.Status <= EntityStatus.Approved
                    && (p.Expiry == null || p.Expiry >= DateTimeOffset.UtcNow)
                    && p.Organization.Status <= EntityStatus.Approved
                    && (rq.AppId == null || p.Organization.Apps.Any(a => a.CoreAppId == rq.AppId))
                )
                .Select(p => new OrgGetMyData
                {
                    Id = p.OrgId,
                    Name = p.Organization.Name,
                    Brand = p.Organization.Brand
                })
                .OrderBy(m => ids.IndexOf(m.Id))
                .Take(rq.MaxItems)
                .ToArrayAsync(cancellationToken)
            ;
        }

        /// <summary>
        /// Get user's latest accessed organizations
        /// 获取用户最近访问的机构
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="writer">Writer to hold the data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        public async Task GetMyAsync(OrgGetMyRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default)
        {
            var orgs = await GetMyAsync(rq, cancellationToken);
            await writer.SerializeAsync(orgs, MyJsonSerializerContext.Default.IEnumerableOrgGetMyData);

            /*
            var (hasContent, commandText) = await _db.CoreOrganizationUsers
                .AsNoTracking()
                .Where(ou => ou.CoreUserId == User.IdInt
                    && ou.Status <= EntityStatus.Approved
                    && (ou.Expiry == null || ou.Expiry >= DateTimeOffset.UtcNow)
                    && ou.CoreOrganization.Status <= EntityStatus.Approved)
                .OrderByDescending(ou => ou.Id)
                .Take(rq.MaxItems)
                .Select(ou => new OrgGetMyData
                {
                    Id = ou.CoreOrganizationId,
                    Name = ou.CoreOrganization.Name,
                    Brand = ou.CoreOrganization.Brand
                }).ToJsonAsync(writer, cancellationToken: cancellationToken);

            if (_db.IsSensitiveDataLoggingEnabled)
            {
                Logger.LogInformation("GetMyAsync is {hasContent} with {commandText}", hasContent, commandText);
            }
            */
        }

        /// <summary>
        /// Leave the organization
        /// 退出机构
        /// </summary>
        /// <param name="id">Organization id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> LeaveAsync(int id, CancellationToken cancellationToken = default)
        {
            // Read data
            var ou = await _db.Users(id).AsNoTracking()
                .Where(ou => ou.CoreUserId == User.IdInt)
                .Select(ou => new { ou.Id, ou.InviterId, InviterName = ou.Inviter == null ? null : ou.Inviter.Name, ou.Organization.Name })
                .FirstOrDefaultAsync(cancellationToken);

            if (ou == null)
            {
                return ApplicationErrors.NoId.AsResult();
            }

            // Check direct reports
            var hasDirectReports = await _db.Users(id).AsNoTracking()
                .AnyAsync(ou => ou.ReportTo == ou.Id, cancellationToken);

            if (hasDirectReports)
            {
                return ApplicationErrors.DeleteReferencedData.AsResult(nameof(Person.ReportTo));
            }

            // Delete the user from the organization
            var affacted = await _db.Persons.Where(p => p.Id == ou.Id).ExecuteDeleteAsync(cancellationToken);
            if (affacted == 0)
            {
                return ApplicationErrors.NoId.AsResult();
            }

            // Update user's latest organizations
            var user = await _db.CoreUsers.FindAsync([User.IdInt], cancellationToken);
            if (user != null)
            {
                var ids = user.LatestOrganizationIds;
                if (ids != null)
                {
                    ids.Remove(id);
                    user.LatestOrganizationIds = [.. ids];
                    await _db.SaveChangesAsync(cancellationToken);
                }
            }

            // Push message
            var message = new LeaveOrgMessage
            {
                Data = User.CreateMessageData(App.AppId, id),
                OrgName = ou.Name,
                InviterId = ou.InviterId,
                InviterName = ou.InviterName
            };
            await _queueService.PushAsync(message, PlatformSharedContext.Default.LeaveOrgMessage, cancellationToken);

            return ActionResult.Succeed(id);
        }

        /// <summary>
        /// List organization JSON data
        /// 机构列表JSON数据
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="writer">Writer to hold the data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task ListAsync(OrgListRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default)
        {
            var query = CreateQuery(rq);

            await query.Select(ou => new OrgListData
            {
                Id = ou.Organization.Id,
                Name = ou.Organization.Name,
                Pin = MyDbFunctions.HideData(ou.Organization.Pin, default)
            }).ToJsonAsync(writer, cancellationToken: cancellationToken);
        }

        private IQueryable<Person> CreateQuery(OrgListRQ rq, Func<IQueryable<Person>, IQueryable<Person>>? filters = null)
        {
            var query = _db.Persons
                .AsNoTracking()
                .Where(ou => ou.CoreUserId == User.IdInt)
                .QueryEtsoo(rq, (ou) => ou.OrgId, (ou) => ou.Organization.Status, (q) =>
                {
                    if (rq.ParentId.HasValue)
                    {
                        q = q.Where(ou => ou.Organization.ParentId == rq.ParentId);
                    }

                    if (rq.Keyword?.Length > 1)
                    {
                        var keyword = rq.Keyword;

                        if (keyword.IsComplexQueryKeywords())
                        {
                            q = q.QueryEtsooKeywords(keyword, DbUtils.ILikeMethod, ou => ou.Organization.Name);
                        }
                        else
                        {
                            q = q.Where(ou => ou.Organization.Brand == keyword
                            || EF.Functions.ILike(ou.Organization.Name, $"%{keyword}%")
                            || (ou.Organization.Pin != null && EF.Functions.ILike(ou.Organization.Pin, $"%{keyword}%"))
                            || (ou.Organization.QueryKeyword != null && EF.Functions.ILike(ou.Organization.QueryKeyword, $"%{keyword}%")));
                        }
                    }

                    if (filters != null)
                    {
                        q = filters(q);
                    }

                    return q;
                });

            return query;
        }

        /// <summary>
        /// Check if the user owns the organization
        /// 检查用户是否拥有机构
        /// </summary>
        /// <param name="id">Org id</param>
        /// <param name="userRole">Minimum user role</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<bool> OwnsAsync(int id, UserRole userRole = UserRole.Guest, CancellationToken cancellationToken = default)
        {
            return await _db.Users(id).AsNoTracking()
                .AnyAsync(ou => ou.CoreUserId == User.IdInt
                    && ou.Status <= EntityStatus.Approved
                    && ou.UserRole >= userRole
                    && (ou.Expiry == null || ou.Expiry >= DateTimeOffset.UtcNow), cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Query organization
        /// 查询机构
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IEnumerable<OrgQueryData>> QueryAsync(OrgQueryRQ rq, CancellationToken cancellationToken = default)
        {
            var query = CreateQuery(rq, (q) =>
            {
                if (rq.Pin?.Length > 1)
                {
                    q = q.Where(ou => ou.Organization.Pin != null && EF.Functions.ILike(ou.Organization.Pin, $"%{rq.Pin}%"));
                }

                return q;
            });

            var data = await query.Select(ou => new OrgQueryData
            {
                Id = ou.OrgId,
                Name = ou.Organization.Name,
                IsOwner = ou.Organization.OwnerId == User.IdInt,
                Brand = ou.Organization.Brand,
                Pin = MyDbFunctions.HideData(ou.Organization.Pin, default),
                ParentId = ou.Organization.ParentId,
                Status = ou.Organization.Status,
                Creation = ou.Organization.Creation,
                UserRole = ou.UserRole,
                Users = ou.Organization.Persons.Where(p => p.CoreUserId != null && p.IdentityType.HasFlag(IdentityTypeFlags.User)).Count(),
                UserStatus = ou.Status,
                IsUserExpired = ou.Expiry < DateTimeOffset.UtcNow
            }).ToListAsync(cancellationToken);

            return data;
        }

        /// <summary>
        /// Query organization JSON data
        /// 查询机构JSON数据
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="writer">Writer to hold the data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task QueryAsync(OrgQueryRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default)
        {
            var query = CreateQuery(rq, (q) =>
            {
                if (rq.Pin?.Length > 1)
                {
                    q = q.Where(ou => ou.Organization.Pin != null && EF.Functions.ILike(ou.Organization.Pin, $"%{rq.Pin}%"));
                }

                return q;
            });

            var (hasContent, commandText) = await query.Select(ou => new OrgQueryData
            {
                Id = ou.OrgId,
                Name = ou.Organization.Name,
                IsOwner = ou.Organization.OwnerId == User.IdInt,
                Brand = ou.Organization.Brand,
                Pin = MyDbFunctions.HideData(ou.Organization.Pin, default),
                ParentId = ou.Organization.ParentId,
                Status = ou.Organization.Status,
                Creation = ou.Organization.Creation,
                UserRole = ou.UserRole,
                UserStatus = ou.Status,
                Users = ou.Organization.Persons.Where(p => p.CoreUserId != null && p.IdentityType.HasFlag(IdentityTypeFlags.User)).Count(),
                IsUserExpired = ou.Expiry < DateTimeOffset.UtcNow
            }).ToJsonAsync(writer, cancellationToken: cancellationToken);

            if (_db.IsSensitiveDataLoggingEnabled && Logger.IsEnabled(LogLevel.Information))
            {
                Logger.LogInformation("QueryAsync is {hasContent} with {commandText}", hasContent, commandText);
            }
        }

        private async Task<IActionResult> FormatRQAsync(IOrgRQ rq, CancellationToken cancellationToken)
        {
            // Is admin
            var isAdmin = User.AppId == MyAppConstants.AdminAppId;

            if (!isAdmin)
            {
                if (!rq.OrgId.HasValue)
                {
                    rq.OrgId = User.OrganizationInt;
                }
                else if (!await OwnsAsync(rq.OrgId.Value, cancellationToken: cancellationToken))
                {
                    return ApplicationErrors.NoValidData.AsResult(nameof(rq.OrgId));
                }
            }

            return ActionResult.Success;
        }

        /// <summary>
        /// Query organization API JSON data
        /// 查询机构接口JSON数据
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="writer">Writer to hold the data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task QueryApiAsync(OrgQueryApiRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default)
        {
            // Format request data
            var result = await FormatRQAsync(rq, cancellationToken);
            if (!result.Ok)
            {
                return;
            }

            var (hasContent, commandText) = await _db.CoreApis
                .AsNoTracking()
                .QueryEtsoo(rq, (a) => a.Id, null, (q) =>
                {
                    if (rq.OrgId.HasValue)
                    {
                        q = q.Where(a => a.CoreOrganizationId == rq.OrgId);
                    }

                    if (rq.Service.HasValue)
                    {
                        q = q.Where(a => a.Service == rq.Service);
                    }

                    if (rq.AppId?.Length > 1)
                    {
                        q = q.Where(a => a.AppId == rq.AppId);
                    }

                    if (rq.Keyword?.Length > 1)
                    {
                        var keyword = rq.Keyword;

                        q = q.Where(a => EF.Functions.ILike(a.Title, $"%{keyword}%"));
                    }

                    return q;
                }).Select(a => new
                {
                    a.Id,
                    a.Service,
                    a.Title,
                    a.Endpoint,
                    a.AppId,
                    a.Enabled,
                    a.Inheritance,
                    a.Creation,
                    a.UpdatedAt
                }).ToJsonAsync(writer, cancellationToken: cancellationToken);

            if (_db.IsSensitiveDataLoggingEnabled)
            {
                Logger.LogInformation("{query} is {hasContent} with {commandText}", nameof(QueryApiAsync), hasContent, commandText);
            }
        }

        /// <summary>
        /// Query organization resource JSON data
        /// 查询机构资源JSON数据
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="writer">Writer to hold the data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task QueryResourceAsync(OrgQueryResourceRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default)
        {
            // Format request data
            var result = await FormatRQAsync(rq, cancellationToken);
            if (!result.Ok)
            {
                return;
            }

            var (hasContent, commandText) = await _db.FeatureCultures
                .AsNoTracking()
                .Where(c => !c.Key.StartsWith(SysResourceKeyPrefix))
                .QueryEtsoo(rq, (c) => c.Id, null, (q) =>
                {
                    if (rq.OrgId.HasValue)
                    {
                        q = q.Where(c => c.CoreOrganizationId == rq.OrgId);
                    }

                    if (!string.IsNullOrEmpty(rq.Culture))
                    {
                        q = q.Where(c => c.Culture == rq.Culture);
                    }

                    if (rq.Keyword?.Length > 1)
                    {
                        var keyword = rq.Keyword;

                        q = q.Where(c => EF.Functions.ILike(c.Key, $"%{keyword}%")
                        || EF.Functions.ILike(c.Title, $"%{keyword}%"));
                    }

                    return q;
                }).Select(c => new OrgQueryResourceData
                {
                    Id = c.Id,
                    Key = c.Key,
                    Culture = c.Culture,
                    OrgName = c.CoreOrganization == null ? null : c.CoreOrganization.Name,
                    Title = c.Title,
                    Description = c.Description
                }).ToJsonAsync(writer, cancellationToken: cancellationToken);

            if (_db.IsSensitiveDataLoggingEnabled)
            {
                Logger.LogInformation("{query} is {hasContent} with {commandText}", nameof(QueryAsSourceAsync), hasContent, commandText);
            }
        }

        /// <summary>
        /// Read organization data for view
        /// 读取用于浏览的机构数据
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="writer">Writer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task ReadAsync(int id, IBufferWriter<byte> writer, CancellationToken cancellationToken = default)
        {
            var (hasContent, commandText) = await _db.Users(id)
                .AsNoTracking()
                .Where(ou => ou.CoreUserId == User.IdInt)
                .Select(ou => new
                {
                    Id = ou.OrgId,
                    IsOwner = ou.Organization.OwnerId == User.IdInt,
                    OwnerName = MyDbFunctions.HideData(ou.Organization.Owner.Name, default),
                    ou.Organization.Name,
                    ou.Organization.Brand,
                    ou.Organization.Logo,
                    ou.Organization.Pin,
                    ParentName = (ou.Organization.Parent == null ? null : ou.Organization.Parent.Name),
                    ou.Organization.ParentId,
                    ou.Organization.Creation,
                    ou.Organization.Status,
                    ou.Organization.QueryKeyword,
                    Persons = ou.Organization.Persons.Count,
                    Users = ou.Organization.Persons.Where(p => p.CoreUserId != null && p.IdentityType.HasFlag(IdentityTypeFlags.User)).Count(),
                    UserStatus = ou.Status,
                    UserExpiry = ou.Expiry
                }).ToJsonObjectAsync(writer, cancellationToken: cancellationToken);

            if (_db.IsSensitiveDataLoggingEnabled && Logger.IsEnabled(LogLevel.Information))
            {
                Logger.LogInformation("QueryAsync is {hasContent} with {commandText}", hasContent, commandText);
            }
        }

        /// <summary>
        /// Read API schema
        /// 读取接口模式
        /// </summary>
        /// <param name="service">API service</param>
        /// <returns>Result</returns>
        public JsonSchema? ReadApiSchema(CoreApiService service)
        {
            if (apiSchemas.TryGetValue(service, out var creator))
            {
                return creator();
            }

            return null;
        }

        /// <summary>
        /// Send email
        /// 发送邮件
        /// </summary>
        /// <param name="message">Email message</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        public async Task<IActionResult> SendEmailAsync(SendEmailMessage message, CancellationToken cancellationToken = default)
        {
            var messageId = await _queueService.PushAsync(message, ApiModelJsonSerializerContext.Default.SendEmailMessage, cancellationToken);
            return ActionResult.Succeed(messageId);
        }

        /// <summary>
        /// Send SMS
        /// 发送短信
        /// </summary>
        /// <param name="message">SMS message</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        public async Task<IActionResult> SendSMSAsync(SendSMSMessage message, CancellationToken cancellationToken = default)
        {
            var messageId = await _queueService.PushAsync(message, ApiModelJsonSerializerContext.Default.SendSMSMessage, cancellationToken);
            return ActionResult.Succeed(messageId);
        }

        /// <summary>
        /// Send profile by email
        /// 用邮件发送档案
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> SendProfileEmailAsync(SendProfileEmailRQ rq, CancellationToken cancellationToken = default)
        {
            // Author
            var author = await _db.Persons.AsNoTracking()
                .Where(p => p.Id == User.Oid)
                .Select(p => p.Name)
                .FirstOrDefaultAsync(cancellationToken);

            if (string.IsNullOrEmpty(author))
            {
                return ApplicationErrors.NoUserFound.AsResult();
            }

            // Organization id
            var orgId = User.OrganizationInt;

            // Emails
            var items = await _db.QueryPersonIdentifiersAsync(orgId, CoreUserIdentifierType.Email, cancellationToken, rq.Persons);
            var emails = items[0];
            if (emails.Length == 0)
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(rq.Persons));
            }

            var profile = await _db.UserProfiles(User, rq.Id).AsNoTracking()
                .Select(p => new
                {
                    p.Title,
                    p.Comment,
                    p.Creation,
                    Attachments = p.Attachments.Select(a => new { a.Id, a.Description, a.Creation, UserName = a.User.Name }),
                    Links = p.Links.Select(l => new { l.Content, l.Creation, UserName = l.User.Name }),
                    Data = new IdentityTypeData
                    {
                        Name = p.Person.Name,
                        IdentityType = p.Person.IdentityType,
                        Owner = p.Person.ContactOwners.Select(o => new IdentityTypeDataBase
                        {
                            Name = o.Person.Name,
                            IdentityType = o.Person.IdentityType
                        }).FirstOrDefault()
                    }
                })
                .FirstOrDefaultAsync(cancellationToken);
            if (profile == null)
            {
                return ApplicationErrors.NoId.AsResult();
            }

            var subject = $"[{Resources.Profile}] {profile.Title}";
            var relatedTarget = profile.Data.GetRelatedTarget();

            var attachments = new StringBuilder();
            var attachmentCount = profile.Attachments.Count();
            if (attachmentCount > 0 && rq.IncludeAttachments is true)
            {
                var expiry = DateTime.UtcNow.AddHours(72);

                attachments.Append($$"""<div class="title">{{Resources.Attachments}} ({{attachmentCount}}), <span class="small">{{Resources.Valid72Hours}}</span></div>""");
                attachments.Append("""<hr class="line" />""");
                attachments.Append($$"""<ol>""");
                var timestamp = SharedUtils.UTCToJsMiliseconds(expiry).ToString();
                foreach (var attachment in profile.Attachments)
                {
                    var key = await App.HashPasswordAsync(timestamp + attachment.Id);
                    attachments.Append($$"""
                        <li>
                          <a href="{{App.Configuration.ApiUrl}}/Storage/ProfileAttachment/{{attachment.Id}}?timestamp={{timestamp}}&key={{WebUtility.UrlEncode(key)}}">{{attachment.Description}} ({{attachment.UserName}}, {{attachment.Creation:yyyy-MM-dd}})</a>
                        </li>
                        """);
                }
                attachments.Append($$"""</ol>""");
            }

            var comments = new StringBuilder();
            var links = profile.Links.ToArray();
            var commentCount = links.Length;
            if (commentCount > 0 && rq.IncludeComments is true)
            {
                comments.Append($$"""<div class="title">{{Resources.Comments}} ({{commentCount}})</div>""");
                comments.Append("""<hr class="line" />""");

                for (var c = 0; c < commentCount; c++)
                {
                    var link = links[c];
                    comments.Append($$"""
                        <div>
                        {{link.Content}}
                        </div>
                        <div class="auth">{{c + 1}}. <b>{{link.UserName}}</b>, {{link.Creation:yyyy-MM-dd}}</div>
                        """);
                }
            }

            var body = $$"""
                <!DOCTYPE html>
                <html>
                <head>
                  <meta charset="UTF-8">
                  <title>{{subject}}</title>
                  <link rel="stylesheet" href="{{App.Configuration.ApiUrl}}/Storage/EditorStyles"/>
                  <style>
                    .field-label {
                      width: 100px;
                    }

                    .field-value {
                      font-weight: bold;
                    }

                    .title {
                        padding: 8px 0px;
                    }

                    .line {
                        width: 100%;
                        background: none;
                        border: none;
                        border-top: 1px solid #d5d5d5;
                        height: 1px;
                        margin: 2px;
                    }

                    .small {
                        font-size: 12px;
                    }

                    .auth {
                        padding: 4px;
                        margin-bottom: 6px;
                        background-color:#f5f5f5;
                        border-bottom: 1px solid #e5e5e5;
                        border-radius: 4px;
                        font-size: 12px;
                    }
                  </style>
                </head>
                <body>

                <table width="100%" cellpadding="4" cellspacing="0" style="background-color:#f5f5f5; border-radius: 4px; font-size: 12px">
                    <tr>
                        <td class="field-label">{{Resources.RelatedTarget}}:</td>
                        <td class="field-value">{{relatedTarget}}</td>
                    </tr>
                    <tr>
                        <td class="field-label">{{Resources.Creation}}:</td>
                        <td class="field-value">{{profile.Creation:yyyy-MM-dd}}, #{{rq.Id}}</td>
                    </tr>
                    <tr>
                        <td class="field-label">{{Resources.Sender}}:</td>
                        <td class="field-value">{{author}}</td>
                    </tr>
                    <tr>
                        <td class="field-label">{{Resources.Message}}:</td>
                        <td class="field-value">{{rq.Message}}</td>
                    </tr>
                </table>

                {{profile.Comment}}

                {{attachments}}

                {{comments}}

                </body>
                </html>
                """;

            var message = new SendEmailMessage
            {
                Subject = subject,
                Body = body,
                To = emails,

                OrgId = orgId > 1 ? orgId : null,
            };

            try
            {
                return await SendEmailAsync(message, cancellationToken);
            }
            catch (Exception ex)
            {
                return LogException(ex);
            }
        }

        /// <summary>
        /// Update organization
        /// 更新机构
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> UpdateAsync(OrgUpdateRQ rq, CancellationToken cancellationToken = default)
        {
            var org = await _db.CoreOrganizations.FirstOrDefaultAsync(o => o.Id == rq.Id && o.OwnerId == User.IdInt, cancellationToken);
            if (org == null)
            {
                return ApplicationErrors.NoId.AsResult();
            }

            // QueryKeyword
            var isQueryKeywordModified = rq.IsModified(nameof(rq.QueryKeyword));

            // Update organization
            if (rq.IsModified(nameof(rq.Name)) && !string.IsNullOrEmpty(rq.Name))
            {
                org.Name = rq.Name;
                if (!isQueryKeywordModified)
                {
                    org.QueryKeyword = _publicService.GetPinyin(new PinyinRQ { Input = rq.Name, Format = PinyinFormatType.Initial });
                }
            }

            if (rq.IsModified(nameof(rq.Brand)))
            {
                org.Brand = rq.Brand;
            }

            if (rq.IsModified(nameof(rq.Pin)))
            {
                org.Pin = rq.Pin;
            }

            if (rq.IsModified(nameof(rq.ParentId)))
            {
                org.ParentId = rq.ParentId;
            }

            if (rq.IsModified(nameof(rq.Status)) && rq.Status.HasValue)
            {
                org.Status = rq.Status.Value;
            }

            if (isQueryKeywordModified)
            {
                org.QueryKeyword = rq.QueryKeyword;
            }

            // Changes
            var changes = _db.ChangeTracker.Entries().GetChangedProperties();

            // Save
            await _db.SaveChangesAsync(cancellationToken);

            // Push message
            var message = new UpdateOrgMessage
            {
                Data = User.CreateMessageData(App.AppId, rq.Id, org.Name),
                Changes = changes
            };
            await _queueService.PushAsync(message, PlatformSharedContext.Default.UpdateOrgMessage, cancellationToken);

            // Return
            return ActionResult.Succeed(rq.Id);
        }

        /// <summary>
        /// Update API
        /// 更新接口
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> UpdateApiAsync(OrgUpdateApiRQ rq, CancellationToken cancellationToken = default)
        {
            // Format request data
            var result = await FormatRQAsync(rq, cancellationToken);
            if (!result.Ok)
            {
                return result;
            }

            var api = await _db.CoreApis
                .FirstOrDefaultAsync(o => o.Id == rq.Id && (rq.OrgId == null || o.CoreOrganizationId == rq.OrgId), cancellationToken);
            if (api == null)
            {
                return ApplicationErrors.NoId.AsResult();
            }

            // Update API
            if (rq.IsModified(nameof(rq.Service)) && rq.Service.HasValue)
            {
                api.Service = rq.Service.Value;
            }

            if (rq.IsModified(nameof(rq.Title)) && !string.IsNullOrEmpty(rq.Title))
            {
                api.Title = rq.Title;
            }

            if (rq.IsModified(nameof(rq.Endpoint)) && rq.Endpoint != null)
            {
                api.Endpoint = rq.Endpoint;
            }

            if (rq.IsModified(nameof(rq.AppId)) && !string.IsNullOrEmpty(rq.AppId))
            {
                api.AppId = rq.AppId;
            }

            if (rq.IsModified(nameof(rq.AppSecret)) && !string.IsNullOrEmpty(rq.AppSecret))
            {
                api.AppSecret = EncryptAppSecret(rq.AppSecret);
            }

            if (rq.IsModified(nameof(rq.Options)))
            {
                // Validate the schema
                if (!ValidateApiServiceSchema(api.Service, rq.Options, out var schemaResult))
                {
                    return schemaResult;
                }

                api.Options = rq.Options;
            }

            if (rq.IsModified(nameof(rq.RatePolicy)))
            {
                api.RatePolicy = rq.RatePolicy;
            }

            if (rq.IsModified(nameof(rq.Enabled)) && rq.Enabled.HasValue)
            {
                api.Enabled = rq.Enabled.Value;
            }

            if (rq.IsModified(nameof(rq.Inheritance)) && rq.Inheritance.HasValue)
            {
                api.Inheritance = rq.Inheritance.Value;
            }

            api.UpdatedAt = DateTimeOffset.UtcNow;

            // Changes
            var changes = _db.ChangeTracker.Entries().GetChangedProperties();

            // Save
            await _db.SaveChangesAsync(cancellationToken);

            // Push message
            var message = new UpdateApiMessage
            {
                Data = User.CreateMessageData(App.AppId, rq.Id, api.Title),
                Changes = changes,
                OrganizationId = api.CoreOrganizationId
            };
            await _queueService.PushAsync(message, PlatformSharedContext.Default.UpdateApiMessage, cancellationToken);

            // Return
            return ActionResult.Succeed(rq.Id);
        }

        /// <summary>
        /// Update avatar
        /// 更新头像
        /// </summary>
        /// <param name="id">Organization id</param>
        /// <param name="avatarStream">Avatar stream</param>
        /// <param name="contentType">Cotent type</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>New URL</returns>
        public async Task<IActionResult> UpdateAvatarAsync(int id, Stream avatarStream, string contentType, CancellationToken cancellationToken = default)
        {
            // Check the stream
            if (avatarStream.Length is not (> 10240 and < 102400000))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(avatarStream));
            }

            // Check the organization id
            var org = await _db.CoreOrganizations.AsNoTracking()
                .Where(o => o.Id == id && o.OwnerId == User.IdInt)
                .Select(o => new { o.Logo, o.Name })
                .FirstOrDefaultAsync(cancellationToken);
            if (org == null)
            {
                return ApplicationErrors.NoId.AsResult();
            }

            var extension = MimeTypeMap.TryGetExtension(contentType);
            if (string.IsNullOrEmpty(extension))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(contentType));
            }

            // File path
            var path = "/OrgAvatar/" + DateTime.UtcNow.ToString("yyyyMM") + "/" + Path.GetRandomFileName() + extension;

            // Storage
            var storage = await _storageFactory.CreateAsync(null, cancellationToken);

            // Save the stream to file directly
            var saveResult = await storage.WriteAsync(path, avatarStream, WriteCase.CreateNew, cancellationToken: cancellationToken);

            if (saveResult)
            {
                // New avatar URL
                var url = storage.GetUrl(path);

                // Update
                await _db.CoreOrganizations.Where(o => o.Id == id).ExecuteUpdateAsync(o => o.SetProperty(o => o.Logo, url), cancellationToken);

                // Remove current avatar
                if (!string.IsNullOrEmpty(org.Logo))
                    await storage.DeleteUrlAsync(org.Logo, cancellationToken);

                // Push message
                var message = new UpdateOrgAvatarMessage
                {
                    Data = User.CreateMessageData(App.AppId, id, org.Name)
                };
                await _queueService.PushAsync(message, PlatformSharedContext.Default.UpdateOrgAvatarMessage, cancellationToken);

                // Return
                return ActionResult.Succeed(url);
            }
            else
            {
                Logger.LogError("Avatar write path is {path}", path);
                return ApplicationErrors.DataProcessingFailed.AsResult();
            }
        }

        /// <summary>
        /// Read organization data for update
        /// 读取用于更新的机构数据
        /// </summary>
        /// <param name="id">Organization id</param>
        /// <param name="writer">Writer to hold the data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task UpdateReadAsync(int id, IBufferWriter<byte> writer, CancellationToken cancellationToken = default)
        {
            var query = _db.CoreOrganizations
                .AsNoTracking()
                .Where(o => o.Id == id && o.OwnerId == User.IdInt);

            await query.Select(o => new
            {
                o.Id,
                o.Name,
                o.Brand,
                o.Pin,
                o.ParentId,
                o.Status,
                o.QueryKeyword
            }).ToJsonObjectAsync(writer, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Read organization API data for update
        /// 读取用于机构接口更新的数据
        /// </summary>
        /// <param name="id">API id</param>
        /// <param name="writer">Writer to hold the data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task UpdateApiReadAsync(int id, IBufferWriter<byte> writer, CancellationToken cancellationToken = default)
        {
            var query = _db.CoreApis
                .AsNoTracking()
                .Where(o => o.Id == id && o.CoreOrganizationId == User.OrganizationInt);

            await query.Select(a => new
            {
                a.Id,
                a.Service,
                a.Title,
                a.Endpoint,
                a.AppId,
                AppSecret = "******",
                a.Options,
                a.RatePolicy,
                a.Enabled,
                a.Inheritance
            }).ToJsonObjectAsync(writer, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Read resource data for update
        /// 读取用于更新的资源数据
        /// </summary>
        /// <param name="id">Resource id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<OrgUpdateResourceReadData?> UpdateResourceReadAsync(int id, CancellationToken cancellationToken = default)
        {
            // Is admin
            var isAdmin = User.AppId == CurrentUser.ScopeToAppId("admin");

            // Check the resource id
            var resource = await _db.FeatureCultures.AsNoTracking()
                .Where(o => o.Id == id)
                .Select(o => new { o.Key, o.CoreOrganizationId })
                .FirstOrDefaultAsync(cancellationToken);

            if (resource == null)
            {
                return null;
            }

            if (!isAdmin && (resource.CoreOrganizationId == null || !await OwnsAsync(resource.CoreOrganizationId.Value, cancellationToken: cancellationToken)))
            {
                return null;
            }

            var items = await _db.FeatureCultures.AsNoTracking()
                .Where(o => o.Key == resource.Key && o.CoreOrganizationId == resource.CoreOrganizationId)
                .Select(o => new OrgResourceItem
                {
                    Culture = o.Culture,
                    Title = o.Title,
                    Description = o.Description,
                    JsonData = o.JsonData
                })
                .ToArrayAsync(cancellationToken);

            return new OrgUpdateResourceReadData
            {
                Id = id,
                Key = resource.Key,
                OrgId = resource.CoreOrganizationId,
                Items = items
            };
        }

        /// <summary>
        /// Async upload profile attachment files
        /// 异步上传档案附件
        /// </summary>
        /// <param name="id">Profile id</param>
        /// <param name="files">Attachment files</param>
        /// <param name="action">Signed action</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        public async Task<IActionResult> UploadProfileFilesAsync(long id, IEnumerable<IFormFile> files, string action, CancellationToken cancellationToken = default)
        {
            // Validate the action
            var actionResult = await _erp.ValidateActionAsync(action, ServiceConstants.ActionUploadProfileFiles, id, cancellationToken);
            if (!actionResult.Ok)
            {
                return actionResult;
            }

            var oid = User.Oid;
            if (oid < 1)
            {
                return ApplicationErrors.NoId.AsResult(nameof(oid));
            }

            // Validate the profile id
            var orgId = User.OrganizationInt;
            var exists = await _db.UserProfiles(User, id).AsNoTracking()
               .AnyAsync(cancellationToken);

            if (!exists)
            {
                return ApplicationErrors.NoId.AsResult();
            }

            var exceptions = new ConcurrentQueue<Exception>();

            // Storage
            var storage = await _storageFactory.CreateAsync(orgId, cancellationToken);

            // File path
            var path = _storageFactory.GetOrgPath(orgId, "Profiles");

            await Parallel.ForEachAsync(files, cancellationToken, async (file, cancellationToken) =>
            {
                try
                {
                    var filePath = path + Path.GetRandomFileName() + Path.GetExtension(file.FileName);

                    var saveResult = await storage.WriteAsync(filePath, file.OpenReadStream(), WriteCase.CreateNew, cancellationToken: cancellationToken);

                    if (saveResult)
                    {
                        _db.PersonProfileAttachments.Add(new PersonProfileAttachment
                        {
                            ProfileId = id,
                            FileName = filePath,
                            FileSize = file.Length,
                            ContentType = file.ContentType,
                            Description = Path.GetFileNameWithoutExtension(file.FileName),
                            UserId = oid
                        });
                        await _db.SaveChangesAsync(cancellationToken);
                    }
                    else
                    {
                        exceptions.Enqueue(new Exception($"Failed to save file {file.FileName}"));
                    }
                }
                catch (Exception ex)
                {
                    exceptions.Enqueue(ex);
                }
            });

            if (!exceptions.IsEmpty)
            {
                return LogException(new AggregateException(exceptions));
            }

            return ActionResult.Success;
        }
    }
}
