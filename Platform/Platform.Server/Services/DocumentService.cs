using com.etsoo.CoreFramework.Models;
using com.etsoo.CoreFramework.User;
using com.etsoo.Database;
using Microsoft.EntityFrameworkCore;
using Platform.Server.Application;
using PlatformShared.Database;
using PlatformShared.Dto;
using PlatformShared.RQ;

namespace Platform.Server.Services
{
    /// <summary>
    /// Sytem Document service
    /// 系统文档服务
    /// </summary>
    public class DocumentService : CommonUserService, IDocumentService
    {
        readonly MyDbContext _db;

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="db">Database EF</param>
        /// <param name="app">Application</param>
        /// <param name="userAccessor">User accessor</param>
        /// <param name="logger">Logger</param>
        /// 
        public DocumentService(MyDbContext db, IMyApp app, CurrentUserAccessor userAccessor, ILogger<DocumentService> logger)
            : base(app, userAccessor.UserSafe, "document", logger)
        {
            _db = db;
        }

        /// <summary>
        /// List
        /// 列表
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public Task<SystemDocumentListData[]> ListAsync(SystemDocumentListRQ rq, CancellationToken cancellationToken = default)
        {
            return _db.CoreDocuments.AsNoTracking()
                .Where(t => t.CoreOrganizationId == null)
                .QueryEtsoo(rq, (d) => d.Id, null, (q) =>
                {
                    if (!string.IsNullOrEmpty(rq.Kind))
                    {
                        var kind = rq.Kind.ToUpper();
                        q = q.Where(d => d.Kind == kind);
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
                .Select(d => new SystemDocumentListData
                {
                    Id = d.Id,
                    Title = d.Title
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
        public Task<SystemDocumentViewData?> ReadAsync(int id, CancellationToken cancellationToken = default)
        {
            return _db.CoreDocuments.AsNoTracking()
                .Where(d => d.Id == id && d.CoreOrganizationId == null)
                .Select(d => new SystemDocumentViewData
                {
                    Id = d.Id,
                    Kind = d.Kind,
                    Title = d.Title,
                    Parameters = d.Parameters,
                    Template = d.Template,
                    RefreshTime = d.RefreshTime
                })
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
