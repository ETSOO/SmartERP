using com.etsoo.CoreFramework.Models;
using com.etsoo.CoreFramework.User;
using com.etsoo.Database;
using CRM.Server.Application;
using CRM.Server.Dto.Tag;
using CRM.Server.RQ.Tag;
using Microsoft.EntityFrameworkCore;
using PlatformShared.Database;
using PlatformShared.Database.Models;
using System.Buffers;

namespace CRM.Server.Services
{
    /// <summary>
    /// Feature tag service
    /// 特征标签服务
    /// </summary>
    public class TagService : MyUserService, ITagService
    {
        readonly MyDbContext _db;
        readonly ICommonService _commonService;

        public TagService(
            MyDbContext db,
            IMyApp app,
            CurrentUserAccessor userAccessor,
            ILogger<TagService> logger,
            ICommonService commonService
        )
            : base(app, userAccessor.UserSafe, "tag", logger)
        {
            _db = db;
            _commonService = commonService;
        }

        private IQueryable<FeatureTag> CreateQuery(QueryIntRQ rq, Func<IQueryable<FeatureTag>, IQueryable<FeatureTag>>? filters = null)
        {
            var query = _db.FeatureTags.AsNoTracking()
                .Where(t => t.CoreOrganizationId == User.OrganizationInt)
                .QueryEtsoo(rq, (d) => d.Id, null, (q) =>
                {
                    if (rq.Keyword?.Length > 1)
                    {
                        q = q.Where(t => t.Tag == rq.Keyword);
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
        /// List
        /// 列表
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<string[]> ListAsync(TagListRQ rq, CancellationToken cancellationToken = default)
        {
            return await CreateQuery(rq, (q) =>
            {
                q = q.Where(t => rq.Kind.HasFlag(t.Kind));

                return q;
            })
            .OrderByDescending(t => t.Total).ThenBy(t => t.Tag)
            .Select(d => d.Tag)
            .ToArrayAsync(cancellationToken);
        }

        /// <summary>
        /// Query tags JSON data
        /// 查询标签JSON数据
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="writer">Writer to hold the data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task QueryAsync(TagQueryRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default)
        {
            // Permission check
            if (!await _commonService.HasPermissionAsync((short)Permissions.Org.Manage, cancellationToken))
            {
                return;
            }

            await CreateQuery(rq, (q) =>
            {
                if (rq.Kind.HasValue)
                {
                    q = q.Where(t => rq.Kind.Value.HasFlag(t.Kind));
                }

                return q;
            })
            .OrderBy(t => t.Tag)
            .Select(t => new TagQueryData
            {
                Id = t.Id,
                Kind = t.Kind,
                Tag = t.Tag,
                Total = t.Total
            }).ToJsonAsync(writer, cancellationToken: cancellationToken);
        }
    }
}
