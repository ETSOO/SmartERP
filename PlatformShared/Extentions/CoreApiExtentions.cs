using Microsoft.EntityFrameworkCore;
using Npgsql;
using PlatformShared.Database;
using PlatformShared.Database.Models;
using PlatformShared.Dto;

namespace PlatformShared.Extentions
{
    /// <summary>
    /// Core API extentions
    /// 核心接口扩展
    /// </summary>
    public static class CoreApiExtentions
    {
        /// <summary>
        /// Get API
        /// 获取接口
        /// </summary>
        /// <param name="db">Database context</param>
        /// <param name="orgId">Organization id</param>
        /// <param name="service">API service</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public static async Task<ApiItem?> GetApiAsync(this MyDbContext db, int orgId, CoreApiService service, CancellationToken cancellationToken = default)
        {
            var orgIdSP = new NpgsqlParameter<int>("p_org_id", orgId);
            var serviceSP = new NpgsqlParameter<short>("p_service", (short)service);

            var data = (await db.Database.SqlQuery<ApiItem>($"SELECT * FROM get_core_api({orgIdSP}, {serviceSP})")
                .ToListAsync(cancellationToken)).FirstOrDefault();

            return data;
        }

        /// <summary>
        /// Get API
        /// 获取接口
        /// </summary>
        /// <typeparam name="T">Generic options type</typeparam>
        /// <param name="db">Database context</param>
        /// <param name="orgId">Organization id</param>
        /// <param name="service">API service</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public static async Task<ApiItem<T>?> GetApiAsync<T>(this MyDbContext db, int orgId, CoreApiService service, CancellationToken cancellationToken = default)
            where T : class
        {
            var item = await db.GetApiAsync(orgId, service, cancellationToken);
            if (item is null)
                return null;

            T options = default!;

            return new ApiItem<T>(item, options);
        }
    }
}
