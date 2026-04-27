using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using PlatformShared.Database;
using PlatformShared.Database.Models;
using PlatformShared.Dto;

namespace CRM.Server.Dto
{
    /// <summary>
    /// Promotion summary
    /// </summary>
    public class PromotionSummary
    {
        /// <summary>
        /// Summary data, key is id, value is times / coupons_applied
        /// </summary>
        private readonly Dictionary<int, int> Data = [];

        /// <summary>
        /// Add promotions
        /// </summary>
        /// <param name="promotions">Promotions</param>
        public void Add(IEnumerable<PromotionSaleItem>? promotions)
        {
            if (promotions == null) return;

            foreach (var p in promotions)
            {
                if (Data.ContainsKey(p.Id))
                {
                    Data[p.Id] += p.Times;
                }
                else
                {
                    Data[p.Id] = p.Times;
                }
            }
        }

        /// <summary>
        /// Update db.Promotions
        /// </summary>
        /// <param name="db">Database context</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        public async Task UpdateAsync(MyDbContext db, CancellationToken cancellationToken = default)
        {
            if (Data.Count == 0) return;

            var ids = Data.Keys.ToArray();
            var times = Data.Values.ToArray();

            await db.Database.ExecuteSqlAsync($"""
                UPDATE "promotion" AS p
                    SET "coupons_applied" = p."coupons_applied" + v.times
                FROM UNNEST({ids}::int[], {times}::int[]) AS v(id, times)
                    WHERE p."id" = v.id
            """, cancellationToken);
        }
    }
}
