using Admin.Server.Dto.Document;
using Admin.Server.RQ.Document;
using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Models;
using com.etsoo.CoreFramework.User;
using com.etsoo.Database;
using com.etsoo.ServiceApp.SmartERP;
using com.etsoo.Utils.Actions;
using Microsoft.EntityFrameworkCore;
using PlatformShared;
using PlatformShared.Database;
using PlatformShared.Database.Models;
using PlatformShared.Extentions;
using PlatformShared.Messages;

namespace Admin.Server.Services
{
    /// <summary>
    /// Document service
    /// 文档服务
    /// </summary>
    public class DocumentService : SEUserService, IDocumentService
    {
        readonly MyDbContext _db;
        readonly IQueueService _queueService;

        public DocumentService(
            MyDbContext db,
            ISEServiceApp app,
            CurrentUserAccessor userAccessor,
            ILogger<DocumentService> logger,
            IQueueService queueService
        )
            : base(app, userAccessor.UserSafe, "document", logger)
        {
            _db = db;
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
            var orgId = rq.OrgId;

            if (orgId.HasValue)
            {
                var hasOrg = await _db.CoreOrganizations.AnyAsync(o => o.Id == orgId.Value, cancellationToken);
                if (!hasOrg)
                {
                    return ApplicationErrors.NoId.AsResult(nameof(rq.OrgId));
                }
            }

            var now = DateTimeOffset.UtcNow;

            var document = new CoreDocument
            {
                CoreOrganizationId = orgId,
                Kind = rq.Kind,
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
            var message = new AdminCreateDocumentMessage
            {
                Data = User.CreateMessageData(App.AppId, document.Id, document.Title)
            };
            await _queueService.PushAsync(message, PlatformSharedContext.Default.AdminCreateDocumentMessage, cancellationToken);

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
            var result = await _db.CoreDocuments.Where(d => d.Id == id).ExecuteDeleteAsync(cancellationToken);

            if (result == 0)
            {
                return ApplicationErrors.NoId.AsResult();
            }

            return ActionResult.Succeed(id);
        }

        /// <summary>
        /// Query document
        /// 查询文档
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public Task<DocumentQueryData[]> QueryAsync(DocumentQueryRQ rq, CancellationToken cancellationToken = default)
        {
            return _db.CoreDocuments.AsNoTracking()
                .QueryEtsoo(rq, (d) => d.Id, null, (q) =>
                {
                    if (rq.OrgId.HasValue)
                    {
                        var orgId = rq.OrgId.Value;
                        if (orgId < 1)
                        {
                            q = q.Where(d => d.CoreOrganizationId == null);
                        }
                        else
                        {
                            q = q.Where(d => d.CoreOrganizationId == orgId);
                        }
                    }

                    if (rq.SystemTemplate.HasValue)
                    {
                        if (rq.SystemTemplate.Value)
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
                    OrgName = t.CoreOrganization != null ? t.CoreOrganization.Name : null,
                    Kind = t.Kind,
                    Title = t.Title,
                    HasParameters = t.Parameters != null,
                    RefreshTime = t.RefreshTime
                })
                .ToArrayAsync(cancellationToken);
        }

        /// <summary>
        /// Read data for view
        /// 读取用于浏览的数据
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public Task<DocumentViewData?> ReadAsync(int id, CancellationToken cancellationToken = default)
        {
            return _db.CoreDocuments.AsNoTracking()
                .Where(d => d.Id == id)
                .Select(d => new DocumentViewData
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
            var document = await _db.CoreDocuments.FirstOrDefaultAsync(d => d.Id == rq.Id, cancellationToken);
            if (document == null)
            {
                return ApplicationErrors.NoId.AsResult();
            }

            if (rq.IsModified(nameof(rq.OrgId)))
            {
                var orgId = rq.OrgId;
                if (orgId.HasValue)
                {
                    var hasOrg = await _db.CoreOrganizations.AnyAsync(o => o.Id == orgId.Value, cancellationToken);
                    if (!hasOrg)
                    {
                        return ApplicationErrors.NoId.AsResult(nameof(rq.OrgId));
                    }
                }

                document.CoreOrganizationId = orgId;
            }

            if (rq.IsModified(nameof(rq.Kind)) && rq.Kind != null)
            {
                document.Kind = rq.Kind;
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
