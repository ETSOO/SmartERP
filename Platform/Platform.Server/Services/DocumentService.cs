using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Authentication;
using com.etsoo.CoreFramework.Models;
using com.etsoo.CoreFramework.User;
using com.etsoo.Database;
using com.etsoo.Utils.Actions;
using Microsoft.EntityFrameworkCore;
using Platform.Server.Application;
using Platform.Server.Dto.Document;
using Platform.Server.Endpoints.Document.RQ;
using PlatformShared;
using PlatformShared.Database;
using PlatformShared.Database.Models;
using PlatformShared.Dto;
using PlatformShared.Extentions;
using PlatformShared.Messages;

namespace Platform.Server.Services
{
    /// <summary>
    /// Document service
    /// 文档服务
    /// </summary>
    public class DocumentService : CommonUserService, IDocumentService
    {
        readonly MyDbContext _db;
        readonly IQueueService _queueService;
        readonly IOrgService _orgService;

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="db">Database EF</param>
        /// <param name="app">Application</param>
        /// <param name="userAccessor">User accessor</param>
        /// <param name="logger">Logger</param>
        /// <param name="orgService">Organization service</param>
        /// <param name="queueService">Queue service</param>
        public DocumentService(MyDbContext db, IMyApp app, CurrentUserAccessor userAccessor, ILogger<DocumentService> logger,
            IOrgService orgService, IQueueService queueService)
            : base(app, userAccessor.UserSafe, "document", logger)
        {
            _db = db;
            _orgService = orgService;
            _queueService = queueService;
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

            _db.CoreDocuments.Add(document);

            // Save changes
            await _db.SaveChangesAsync(cancellationToken);

            // Push message
            var message = new CreateDocumentMessage
            {
                Data = User.CreateMessageData(App.AppId, document.Id, document.Title)
            };
            await _queueService.PushAsync(message, PlatformSharedContext.Default.CreateDocumentMessage, cancellationToken);

            return ActionResult.Succeed(document.Id);
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
            var orgId = await _db.CoreDocuments.Where(d => d.Id == id).Select(d => (int?)d.CoreOrganizationId).FirstOrDefaultAsync(cancellationToken);

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

            var result = await _db.CoreDocuments.Where(d => d.Id == id).ExecuteDeleteAsync(cancellationToken);

            if (result == 0)
            {
                return ApplicationErrors.NoId.AsResult();
            }

            return ActionResult.Succeed(id);
        }

        /// <summary>
        /// List
        /// 列表
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public Task<DocumentListData[]> ListAsync(DocumentListRQ rq, CancellationToken cancellationToken = default)
        {
            return _db.CoreDocuments.AsNoTracking()
                .Where(t => t.Kind == rq.Kind)
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
                    Title = d.Title
                })
                .ToArrayAsync(cancellationToken);
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
        public Task<DocumentReadData?> ReadAsync(int id, CancellationToken cancellationToken = default)
        {
            var orgId = User.OrganizationInt;

            return _db.CoreDocuments.AsNoTracking()
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

            var document = await _db.CoreDocuments.FirstOrDefaultAsync(d => d.Id == rq.Id, cancellationToken);
            if (document == null)
            {
                return ApplicationErrors.NoId.AsResult();
            }

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
                document.Template = rq.Template;
            }

            if (rq.IsModified(nameof(rq.Cultures)))
            {
                document.Cultures = rq.Cultures?.ToList();
            }

            var now = DateTimeOffset.UtcNow;
            document.RefreshTime = now;

            // Save
            await _db.SaveChangesAsync(cancellationToken);

            // Return
            return ActionResult.Succeed(rq.Id);
        }
    }
}
