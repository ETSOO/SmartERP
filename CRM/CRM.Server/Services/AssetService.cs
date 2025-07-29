using com.etsoo.CoreFramework.Models;
using com.etsoo.CoreFramework.User;
using com.etsoo.Database;
using com.etsoo.ServiceApp.SmartERP;
using CRM.Server.Dto.Asset;
using CRM.Server.RQ.Asset;
using Microsoft.EntityFrameworkCore;
using PlatformShared.Database;
using PlatformShared.Database.Models;
using System.Buffers;

namespace CRM.Server.Services
{
    /// <summary>
    /// Asset service
    /// 资产服务
    /// </summary>
    public class AssetService : SEUserService, IAssetService
    {
        readonly MyDbContext _db;

        public AssetService(
            MyDbContext db,
            ISEServiceApp app,
            CurrentUserAccessor userAccessor,
            ILogger<AssetService> logger
        )
            : base(app, userAccessor.UserSafe, "asset", logger)
        {
            _db = db;
        }

        private IQueryable<PersonAsset> CreateQuery(AssetListRQ rq, Func<IQueryable<PersonAsset>, IQueryable<PersonAsset>>? filters = null)
        {
            var orgId = User.OrganizationInt;
            var query = _db.PersonAssets.AsNoTracking()
                .Where(a => a.OrgId == orgId)
                .QueryEtsoo(rq, (a) => a.Id, (a) => a.Status, (q) =>
                {
                    if (rq.Keyword?.Length > 1)
                    {
                        var keyword = rq.Keyword;

                        if (keyword.IsComplexQueryKeywords())
                        {
                            q = q.QueryEtsooKeywords(keyword, DbUtils.ILikeMethod, a => a.Title, a => a.Description);
                        }
                        else
                        {
                            q = q.Where(ou => EF.Functions.ILike(ou.Title, $"%{keyword}%")
                            || EF.Functions.ILike(ou.Sn, $"%{keyword}%")
                            || (ou.Description != null && EF.Functions.ILike(ou.Description, $"%{keyword}%"))
                            );
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
        /// List asset JSON data
        /// 资产列表JSON数据
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="writer">Writer to hold the data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task ListAsync(AssetListRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default)
        {
            await CreateQuery(rq)
                .Select(a => new AssetListData
                {
                    Id = a.Id,
                    Title = a.Title
                }).ToJsonAsync(writer, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Query asset JSON data
        /// 查询资产JSON数据
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="writer">Writer to hold the data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task QueryAsync(AssetQueryRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default)
        {
            await CreateQuery(rq)
                .Select(a => new AssetQueryData
                {
                    Id = a.Id,
                    Title = a.Title
                }).ToJsonAsync(writer, cancellationToken: cancellationToken);
        }
    }
}