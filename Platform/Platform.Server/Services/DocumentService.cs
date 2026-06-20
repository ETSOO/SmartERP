using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Authentication;
using com.etsoo.CoreFramework.Models;
using com.etsoo.CoreFramework.User;
using com.etsoo.Database;
using com.etsoo.Utils;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Platform.Server.Application;
using Platform.Server.Dto.Document;
using Platform.Server.Endpoints.Document.RQ;
using PlatformShared;
using PlatformShared.Database;
using PlatformShared.Database.Models;
using PlatformShared.Dto;
using PlatformShared.Dto.Document;
using PlatformShared.Extentions;
using PlatformShared.Messages;
using PlatformShared.Services;
using RazorEngineCore;
using System.Text.Json;
using WebTemplates;

namespace Platform.Server.Services
{
    /// <summary>
    /// Document service
    /// 文档服务
    /// </summary>
    public class DocumentService : CommonUserService, IDocumentService
    {
        readonly IDbContextFactory<MyDbContext> _dbFactory;
        readonly IQueueService _queueService;
        readonly IOrgService _orgService;
        readonly ISmartERPCoordinator _erp;
        readonly IDistributedCache _cache;

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="dbFactory">Database context factory</param>
        /// <param name="app">Application</param>
        /// <param name="userAccessor">User accessor</param>
        /// <param name="logger">Logger</param>
        /// <param name="orgService">Organization service</param>
        /// <param name="queueService">Queue service</param>
        public DocumentService(IDbContextFactory<MyDbContext> dbFactory, IMyApp app, CurrentUserAccessor userAccessor, ILogger<DocumentService> logger,
            IOrgService orgService, IQueueService queueService, ISmartERPCoordinator erp, IDistributedCache cache)
            : base(app, userAccessor.UserSafe, "document", logger)
        {
            _dbFactory = dbFactory;
            _orgService = orgService;
            _queueService = queueService;
            _erp = erp;
            _cache = cache;
        }

        /// <summary>
        /// Create
        /// 创建
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> CreateAsync(DocumentCreateRQ rq, CancellationToken cancellationToken = default)
        {
            // Check org
            var orgCheck = await _orgService.FormatRQAsync(rq, UserRole.Executive, cancellationToken);
            if (!orgCheck.Ok)
            {
                return orgCheck;
            }

            var orgId = rq.OrgId ?? User.OrganizationInt;
            var now = DateTimeOffset.UtcNow;

            var document = new CoreDocument
            {
                CoreOrganizationId = orgId,
                Kind = rq.Kind.ToUpper(),
                Title = rq.Title,
                Parameters = rq.Parameters,
                Template = rq.Template,
                RefreshTime = now,
                Cultures = rq.Cultures?.ToList()
            };

            await using var _db = await _dbFactory.CreateDbContextAsync(cancellationToken);

            _db.CoreDocuments.Add(document);

            // Save changes
            await _db.SaveChangesAsync(cancellationToken);

            var id = document.Id;

            // Push message
            var message = new CreateDocumentMessage
            {
                Data = User.CreateMessageData(App.AppId, id, document.Title),
                JsonData = JsonSerializer.Serialize(rq, MyJsonSerializerContext.Default.DocumentCreateRQ)
            };
            await _queueService.PushAsync(message, PlatformSharedContext.Default.CreateDocumentMessage, cancellationToken);

            return ActionResult.Succeed(id);
        }

        /// <summary>
        /// Delete
        /// 删除
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            await using var _db = await _dbFactory.CreateDbContextAsync(cancellationToken);

            var doc = await _db.CoreDocuments.AsNoTracking()
                .Where(d => d.Id == id)
                .Select(d => new { d.CoreOrganizationId, d.Title })
                .FirstOrDefaultAsync(cancellationToken);

            if (doc == null)
            {
                return ApplicationErrors.NoId.AsResult();
            }

            var orgId = doc.CoreOrganizationId;
            var title = doc.Title;

            if (!orgId.HasValue)
            {
                // System template
                if (!_orgService.IsAdmin())
                {
                    return ApplicationErrors.AccessDenied.AsResult("Admin");
                }
            }
            else if (!(await _orgService.OwnsAsync(orgId.Value, UserRole.Executive, cancellationToken)))
            {
                return ApplicationErrors.AccessDenied.AsResult();
            }

            var task1 = _db.CoreDocuments.Where(d => d.Id == id).ExecuteDeleteAsync(cancellationToken);

            // Push message
            var message = new DeleteDocumentMessage
            {
                Data = User.CreateMessageData(App.AppId, id, title),
                OrganizationId = orgId
            };
            var task2 = _queueService.PushAsync(message, PlatformSharedContext.Default.DeleteDocumentMessage, cancellationToken);

            await Task.WhenAll(task1, task2);

            return ActionResult.Succeed(id);
        }

        string GetDocumentCacheKey(long id)
        {
            return $"{nameof(DocumentService)}:{id}";
        }

        /// <summary>
        /// Generate document
        /// 输出业务文档
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<(IActionResult, string?)> GenerateAsync(DocumentGenerateRQ rq, CancellationToken cancellationToken = default)
        {
            // Validate the action
            var actionResult = await _erp.ValidateActionAsync(rq.Action, cancellationToken);
            if (!actionResult.Ok)
            {
                return (actionResult, null);
            }

            var id = rq.Id;
            var culture = rq.Culture ?? User.Language.Name;
            var targetId = rq.Action.TargetId;

            if (id < 1)
            {
                // System document
                var template = DocumentTemplateUtils.GetTemplate(id);
                if (template == null)
                {
                    return (ApplicationErrors.NoId.AsResult(), null);
                }

                var model = await template.Data(_dbFactory, targetId, rq.Data, User, cancellationToken);
                if (model == null || model is not IDocumentTemplateData tData)
                {
                    return (ApplicationErrors.NoId.AsResult(nameof(rq.Action.TargetId)), null);
                }

                var formattedPath = TemplateUtils.FormatCulture(template.Template, culture);

                var content = await TemplateUtils.BuildAsync(formattedPath, model);

                // Push message
                var title = $"{template.Subject} - {tData.TargetName}";
                var message = new GenerateDocumentMessage
                {
                    Data = User.CreateMessageData(App.AppId, id, title),
                    Culture = culture,
                    TargetId = targetId,
                    TargetName = tData.TargetName,
                    Parameters = rq.Data.Count > 0 ? JsonSerializer.Serialize(rq.Data, CommonJsonSerializerContext.Default.StringKeyDictionaryObject) : null
                };
                await _queueService.PushAsync(message, PlatformSharedContext.Default.GenerateDocumentMessage, cancellationToken);

                return (ActionResult.Success, content);
            }
            else
            {
                var orgId = User.OrganizationInt;

                await using var _db = await _dbFactory.CreateDbContextAsync(cancellationToken);

                var doc = await _db.CoreDocuments.AsNoTracking()
                    .Where(d => d.Id == id && d.CoreOrganizationId == orgId)
                    .Select(d => new { d.Kind, d.Title, d.Template })
                    .FirstOrDefaultAsync(cancellationToken);

                if (doc == null)
                {
                    return (ApplicationErrors.NoId.AsResult(), null);
                }

                var kind = doc.Kind;
                var title = doc.Title;
                var template = doc.Template;

                var model = await DocumentTemplateUtils.GetTemplateModelAsync(_dbFactory, rq.Data, kind, targetId, User, cancellationToken);
                if (model == null || model is not IDocumentTemplateData tData)
                {
                    return (ApplicationErrors.NoId.AsResult(nameof(rq.Action.TargetId)), null);
                }

                var cacheKey = GetDocumentCacheKey(id);
                if (rq.NoCache is true)
                {
                    await _cache.RemoveAsync(cacheKey, cancellationToken);
                }

                var bytes = await _cache.GetOrCreateAsync(cacheKey, async (options) =>
                {
                    // Cache 30 mins
                    // 缓存30分钟
                    options.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);

                    var razorEngine = new RazorEngine();

                    var meta = razorEngine.CompileMeta<object>(template);

                    await using var memoryStream = new MemoryStream();
                    await meta.WriteAsync(memoryStream);

                    return memoryStream.ToArray();
                }, cancellationToken);

                if (bytes == null)
                {
                    return (ApplicationErrors.NoValidData.AsResult(), null);
                }

                var compiledTemplate = await RazorEngineCompiledTemplate<object>.LoadFromStreamAsync(SharedUtils.GetStream(bytes));

                var content = await compiledTemplate.RunAsync(model);

                // Push message
                var messageTitle = $"{title} - {tData.TargetName}";
                var message = new GenerateDocumentMessage
                {
                    Data = User.CreateMessageData(App.AppId, id, messageTitle),
                    Culture = culture,
                    TargetId = targetId,
                    TargetName = tData.TargetName,
                    Parameters = rq.Data.Count > 0 ? JsonSerializer.Serialize(rq.Data, CommonJsonSerializerContext.Default.StringKeyDictionaryObject) : null
                };
                await _queueService.PushAsync(message, PlatformSharedContext.Default.GenerateDocumentMessage, cancellationToken);

                return (ActionResult.Success, content);
            }
        }

        /// <summary>
        /// List
        /// 列表
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<DocumentListData[]> ListAsync(DocumentListRQ rq, CancellationToken cancellationToken = default)
        {
            var kind = rq.Kind;

            await using var _db = await _dbFactory.CreateDbContextAsync(cancellationToken);

            var items = await _db.CoreDocuments.AsNoTracking()
                .Where(t => t.Kind == kind)
                .QueryEtsoo(rq, (d) => d.Id, null, (q) =>
                {
                    if (rq.IsSystem.HasValue)
                    {
                        if (rq.IsSystem.Value)
                        {
                            q = q.Where(d => d.CoreOrganizationId == null);
                        }
                        else
                        {
                            var orgId = User.OrganizationInt;
                            q = q.Where(d => d.CoreOrganizationId == orgId);
                        }
                    }

                    if (!string.IsNullOrEmpty(rq.Culture))
                    {
                        q = q.Where(d => d.Cultures == null || d.Cultures.Contains(rq.Culture));
                    }

                    if (rq.Keyword?.Length > 1)
                    {
                        var keyword = rq.Keyword;

                        if (keyword.IsComplexQueryKeywords())
                        {
                            q = q.QueryEtsooKeywords(keyword, DbUtils.ILikeMethod, d => d.Title);
                        }
                        else
                        {
                            q = q.Where(d => EF.Functions.ILike(d.Title, $"%{keyword}%"));
                        }
                    }

                    return q;
                })
                .Select(d => new DocumentListData
                {
                    Id = d.Id,
                    Title = d.Title,
                    Parameters = d.Parameters
                })
                .ToListAsync(cancellationToken);

            var systemItems = DocumentTemplateUtils.GetTemplates(kind, t => Properties.Resources.ResourceManager.GetString(t) ?? t);
            items.AddRange(systemItems);

            return [.. items];
        }

        /// <summary>
        /// Query document
        /// 查询文档
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<DocumentQueryData[]> QueryAsync(DocumentQueryRQ rq, CancellationToken cancellationToken = default)
        {
            // Check org
            var orgCheck = await _orgService.FormatRQAsync(rq, UserRole.Executive, cancellationToken);
            if (!orgCheck.Ok)
            {
                return [];
            }

            var orgId = rq.OrgId;

            await using var _db = await _dbFactory.CreateDbContextAsync(cancellationToken);

            return await _db.CoreDocuments.AsNoTracking()
                .QueryEtsoo(rq, (d) => d.Id, null, (q) =>
                {
                    if (orgId.HasValue)
                    {
                        q = q.Where(d => d.CoreOrganizationId == orgId);
                    }

                    if (rq.IsSystem.HasValue)
                    {
                        if (rq.IsSystem.Value)
                        {
                            q = q.Where(d => d.CoreOrganizationId == null);
                        }
                        else
                        {
                            q = q.Where(d => d.CoreOrganizationId != null);
                        }
                    }

                    if (!string.IsNullOrEmpty(rq.Kind))
                    {
                        var kind = rq.Kind.ToUpper();
                        q = q.Where(d => d.Kind == kind);
                    }

                    if (!string.IsNullOrEmpty(rq.Culture))
                    {
                        q = q.Where(d => d.Cultures == null || d.Cultures.Contains(rq.Culture));
                    }

                    if (rq.HasParameters.HasValue)
                    {
                        if (rq.HasParameters.Value)
                        {
                            q = q.Where(d => d.Parameters != null);
                        }
                        else
                        {
                            q = q.Where(d => d.Parameters == null);
                        }
                    }

                    if (rq.Keyword?.Length > 1)
                    {
                        var keyword = rq.Keyword;

                        if (keyword.IsComplexQueryKeywords())
                        {
                            q = q.QueryEtsooKeywords(keyword, DbUtils.ILikeMethod, d => d.Title);
                        }
                        else
                        {
                            q = q.Where(d => EF.Functions.ILike(d.Title, $"%{keyword}%"));
                        }
                    }

                    return q;
                })
                .Select(t => new DocumentQueryData
                {
                    Id = t.Id,
                    OrgName = t.CoreOrganization == null || orgId != null ? null : t.CoreOrganization.Name,
                    Kind = t.Kind,
                    Title = t.Title,
                    HasParameters = t.Parameters != null,
                    RefreshTime = t.RefreshTime
                })
                .ToArrayAsync(cancellationToken);
        }

        /// <summary>
        /// Read data
        /// 读取数据
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<DocumentReadData?> ReadAsync(int id, CancellationToken cancellationToken = default)
        {
            var orgId = User.OrganizationInt;

            await using var _db = await _dbFactory.CreateDbContextAsync(cancellationToken);

            return await _db.CoreDocuments.AsNoTracking()
                .Where(d => d.Id == id && (d.CoreOrganizationId == null || d.CoreOrganizationId == orgId))
                .Select(d => new DocumentReadData
                {
                    Id = d.Id,
                    OrgId = d.CoreOrganizationId,
                    Kind = d.Kind,
                    Title = d.Title,
                    Parameters = d.Parameters,
                    Template = d.Template,
                    RefreshTime = d.RefreshTime,
                    Cultures = d.Cultures
                })
                .FirstOrDefaultAsync(cancellationToken);
        }

        /// <summary>
        /// Update
        /// 更新
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> UpdateAsync(DocumentUpdateRQ rq, CancellationToken cancellationToken = default)
        {
            // Check org
            var orgCheck = await _orgService.FormatRQAsync(rq, UserRole.Executive, cancellationToken);
            if (!orgCheck.Ok)
            {
                return orgCheck;
            }

            await using var _db = await _dbFactory.CreateDbContextAsync(cancellationToken);

            var id = rq.Id;
            var document = await _db.CoreDocuments.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
            if (document == null)
            {
                return ApplicationErrors.NoId.AsResult();
            }

            // Remove cache
            var removeCache = false;

            // Check org
            var orgId = document.CoreOrganizationId;
            if (orgId == null)
            {
                // System template
                if (!_orgService.IsAdmin())
                {
                    return ApplicationErrors.AccessDenied.AsResult("Admin");
                }
            }
            else if (!(await _orgService.OwnsAsync(orgId.Value, UserRole.Executive, cancellationToken)))
            {
                return ApplicationErrors.AccessDenied.AsResult();
            }

            if (rq.IsModified(nameof(rq.OrgId)))
            {
                document.CoreOrganizationId = rq.OrgId;
            }

            if (rq.IsModified(nameof(rq.Kind)) && rq.Kind != null)
            {
                removeCache = true;
                document.Kind = rq.Kind.ToUpper();
            }

            if (rq.IsModified(nameof(rq.Title)) && rq.Title != null)
            {
                document.Title = rq.Title;
            }

            if (rq.IsModified(nameof(rq.Parameters)))
            {
                document.Parameters = rq.Parameters;
            }

            if (rq.IsModified(nameof(rq.Template)) && rq.Template != null)
            {
                removeCache = true;
                document.Template = rq.Template;
            }

            if (rq.IsModified(nameof(rq.Cultures)))
            {
                document.Cultures = rq.Cultures?.ToList();
            }

            var now = DateTimeOffset.UtcNow;
            document.RefreshTime = now;

            // Changes
            var changes = _db.ChangeTracker.Entries().GetChangedProperties();

            // Save
            var task1 = _db.SaveChangesAsync(cancellationToken);

            // Push message
            var message = new UpdateDocumentMessage
            {
                Data = User.CreateMessageData(App.AppId, rq.Id, document.Title),
                Changes = changes
            };
            var task2 = _queueService.PushAsync(message, PlatformSharedContext.Default.UpdateDocumentMessage, cancellationToken);

            var task3 = removeCache ? _cache.RemoveAsync(GetDocumentCacheKey(id), cancellationToken) : Task.CompletedTask;

            await Task.WhenAll(task1, task2, task3);

            // Return
            return ActionResult.Succeed(rq.Id);
        }
    }
}
