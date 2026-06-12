using com.etsoo.CoreFramework.User;
using com.etsoo.Utils.String;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using PlatformShared.Database;
using PlatformShared.Database.Models;
using PlatformShared.Dto.Document.Order;
using PlatformShared.Extentions;
using PlatformShared.Services;
using System.Collections.Frozen;

namespace PlatformShared.Dto.Document
{
    /// <summary>
    /// Document template utilities
    /// 文档模板工具类
    /// </summary>
    public static class DocumentTemplateUtils
    {
        private const int OrgCacheMinutes = 5;
        private static readonly MemoryCache cache = new(new MemoryCacheOptions());

        private static readonly FrozenDictionary<int, SystemTemplateItem> systemTemplates = new Dictionary<int, SystemTemplateItem>
        {
            [-1] = new SystemTemplateItem
            {
                Kind = DocumentKind.CmsOrderData,
                Subject = "OrderStandardContract",
                Template = "Order/StandardContract_{culture}",
                Data = (dbFactory, id, dic, user, cancellationToken) => CreateOrderViewAsync(dbFactory, id, dic, user, cancellationToken)
            },
            [-2] = new SystemTemplateItem
            {
                Kind = DocumentKind.CmsOrderData,
                Subject = "OrderProductList",
                Template = "Order/ProductList_{culture}",
                Data = (dbFactory, id, dic, user, cancellationToken) => CreateOrderViewAsync(dbFactory, id, dic, user, cancellationToken)
            }
        }.ToFrozenDictionary();

        /// <summary>
        /// Create organization view data
        /// 创建组织视图数据
        /// </summary>
        /// <param name="dbFactory">Database context factory</param>
        /// <param name="user">Current user</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public static Task<OrgViewData?> CreateOrgDataAsync(IDbContextFactory<MyDbContext> dbFactory, CurrentUser user, CancellationToken cancellationToken = default)
        {
            return CreateOrgDataAsync(dbFactory, user.OrganizationInt, user.Pid, cancellationToken);
        }

        /// <summary>
        /// Create organization cultures
        /// 创建机构标签信息
        /// </summary>
        /// <param name="dbFactory">Database context factory</param>
        /// <param name="orgId">Organization id</param>
        /// <param name="culture">Culture</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public static Task<CustomResourceData[]?> CreateOrgCulturesAsync(IDbContextFactory<MyDbContext> dbFactory, int orgId, string culture, CancellationToken cancellationToken = default)
        {
            var cacheKey = $"{nameof(CreateOrgCulturesAsync)}:{orgId}:{culture}";
            return cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(OrgCacheMinutes);

                await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);

                var allItems = await db.FeatureCultures.AsNoTracking()
                    .Where(c => (c.CoreOrganizationId == null || c.CoreOrganizationId == orgId) && c.Culture == culture && !c.Key.StartsWith(ServiceConstants.SysResourceKeyPrefix))
                    .Select(c => new CustomResourceData
                    {
                        Key = c.Key,
                        OrgId = c.CoreOrganizationId,
                        Title = c.Title,
                        Description = c.Description,
                        JsonData = c.JsonData
                    })
                    .ToArrayAsync(cancellationToken);

                // When OrgId is not null, remove the same key item with OrgId equals to null
                var orgSpecificKeys = allItems.Where(item => item.OrgId != null).Select(item => item.Key).ToHashSet();
                return allItems.Where(item => item.OrgId != null || !orgSpecificKeys.Contains(item.Key)).ToArray();
            });
        }

        /// <summary>
        /// Create organization view data
        /// 创建组织视图数据
        /// </summary>
        /// <param name="dbFactory">Database context factory</param>
        /// <param name="orgId">Global organization id</param>
        /// <param name="personId">Person id for the organization</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public static Task<OrgViewData?> CreateOrgDataAsync(IDbContextFactory<MyDbContext> dbFactory, int orgId, long personId, CancellationToken cancellationToken = default)
        {
            var cacheKey = $"{nameof(CreateOrgDataAsync)}:{orgId}";

            return cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(OrgCacheMinutes);

                if (personId < 1)
                {
                    var id = await GetOrgPersonIdAsync(dbFactory, orgId, cancellationToken);
                    if (id == null) return null;

                    personId = id.Value;
                }

                await using var db1 = await dbFactory.CreateDbContextAsync(cancellationToken);
                var task1 = db1.CoreOrganizations.AsNoTracking()
                    .Where(o => o.Id == orgId)
                    .Select(o => new
                    {
                        o.Name,
                        o.Brand,
                        o.Logo,
                        o.Pin,
                        o.Uid,
                        o.Region,
                        o.Slogan
                    }).FirstAsync(cancellationToken);

                await using var db2 = await dbFactory.CreateDbContextAsync(cancellationToken);
                var task2 = db2.PersonInfos.AsNoTracking()
                    .Where(i => i.PersonId == personId && (i.Kind == PersonInfoKind.Email || i.Kind == PersonInfoKind.Phone || i.Kind == PersonInfoKind.Pin || i.Kind == PersonInfoKind.TaxId || i.Kind == PersonInfoKind.Website))
                    .OrderBy(i => i.Kind).ThenByDescending(i => i.IsDefault)
                    .Select(i => new { i.Kind, i.Identifier })
                    .ToArrayAsync(cancellationToken);

                await using var db3 = await dbFactory.CreateDbContextAsync(cancellationToken);
                var task3 = db3.SettingCrms.AsNoTracking()
                    .Where(s => s.Id == orgId)
                    .Select(s => new { s.Cultures, s.Currencies, s.MainCustomerType, s.HasInventory, s.TaxRate })
                    .FirstOrDefaultAsync(cancellationToken);

                await Task.WhenAll(task1, task2, task3);

                var orgData = task1.Result;
                var settings = task3.Result;
                var personInfos = task2.Result.GroupBy(i => i.Kind)
                    .ToDictionary(g => g.Key, g => g.First().Identifier);

                var pin = personInfos.GetValueOrDefault(PersonInfoKind.Pin) ?? orgData.Pin;

                return new OrgViewData
                {
                    Id = orgId,
                    PersonId = personId,
                    Uid = orgData.Uid,
                    Name = orgData.Name,
                    Brand = orgData.Brand,
                    Slogan = orgData.Slogan,
                    Logo = orgData.Logo,
                    Pin = pin,
                    Region = orgData.Region,
                    Cultures = settings?.Cultures,
                    Currencies = settings?.Currencies,
                    MainCustomerType = settings?.MainCustomerType,
                    HasInventory = settings?.HasInventory ?? false,
                    TaxRate = settings?.TaxRate ?? 0,
                    Email = personInfos.GetValueOrDefault(PersonInfoKind.Email),
                    Phone = personInfos.GetValueOrDefault(PersonInfoKind.Phone),
                    TaxId = personInfos.GetValueOrDefault(PersonInfoKind.TaxId),
                    Website = personInfos.GetValueOrDefault(PersonInfoKind.Website)
                };
            });
        }

        /// <summary>
        /// Create order view data
        /// 创建订单视图数据
        /// </summary>
        /// <param name="dbFactory">Database context factory</param>
        /// <param name="id">Order id</param>
        /// <param name="dic">Dictionary object</param>
        /// <param name="user">Current user</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public static async Task<object?> CreateOrderViewAsync(IDbContextFactory<MyDbContext> dbFactory, long id, StringKeyDictionaryObject dic, CurrentUser user, CancellationToken cancellationToken = default)
        {
            var org = await CreateOrgDataAsync(dbFactory, user, cancellationToken);
            if (org == null) return null;

            return new OrderTemplateData
            {
                Subject = dic.Get(nameof(OrderTemplateData.Subject)),
                Org = org
            };
        }

        /// <summary>
        /// Get organization person id
        /// 获取机构人员编号
        /// </summary>
        /// <param name="dbFactory">Database context factory</param>
        /// <param name="orgId">Organization ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public static Task<long?> GetOrgPersonIdAsync(IDbContextFactory<MyDbContext> dbFactory, long orgId, CancellationToken cancellationToken = default)
        {
            var cacheKey = $"{nameof(GetOrgPersonIdAsync)}:{orgId}";

            return cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(OrgCacheMinutes);

                await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);

                return await db.Persons.AsNoTracking()
                    .Where(p => p.OrgId == orgId && p.CoreOrganizationId == orgId)
                    .Select(p => (long?)p.Id)
                    .FirstOrDefaultAsync(cancellationToken);
            });
        }

        /// <summary>
        /// Get person report to id
        /// 获取人员上级编号
        /// </summary>
        /// <param name="dbFactory">Database context factory</param>
        /// <param name="personId">Person ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public static Task<long?> GetPersonReportToIdAsync(IDbContextFactory<MyDbContext> dbFactory, long personId, CancellationToken cancellationToken = default)
        {
            var cacheKey = $"{nameof(GetPersonReportToIdAsync)}:{personId}";

            return cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(OrgCacheMinutes);

                await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);

                var reportTo = await db.Persons.AsNoTracking()
                    .Where(p => p.Id == personId)
                    .Select(p => p.ReportTo)
                    .FirstOrDefaultAsync(cancellationToken);

                return reportTo;
            });
        }

        /// <summary>
        /// Get person and line identifiers by type
        /// 获取人员和线路标识符按类型
        /// </summary>
        /// <param name="dbFactory">Database context factory</param>
        /// <param name="orgId">Organization ID</param>
        /// <param name="personId">Person ID</param>
        /// <param name="type">Identifier type</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public static Task<string[]?> GetPersonAndLineIdentifiersAsync(IDbContextFactory<MyDbContext> dbFactory, int orgId, long personId, CoreUserIdentifierType type, CancellationToken cancellationToken = default)
        {
            var cacheKey = $"{nameof(GetPersonAndLineIdentifiersAsync)}:{personId}:{type}";

            return cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(OrgCacheMinutes);
                
                var reportTo = await GetPersonReportToIdAsync(dbFactory, personId, cancellationToken);

                var ids = new List<long> { personId };
                if (reportTo.HasValue)
                {
                    ids.Add(reportTo.Value);
                }

                await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);
                var items = await db.QueryPersonIdentifiersAsync(orgId, type, cancellationToken, ids);

                return items[0];
            });
        }

        /// <summary>
        /// Get templates by kind
        /// 从类型获取模板
        /// </summary>
        /// <param name="kind">Kind</param>
        /// <param name="getLabel">Function to get label</param>
        /// <returns>Result</returns>
        public static IEnumerable<DocumentListData> GetTemplates(string kind, Func<string, string> getLabel)
        {
            return systemTemplates.Where(t => t.Value.Kind == kind).Select(t => new DocumentListData
            {
                Id = t.Key,
                Title = getLabel(t.Value.Subject),
                Parameters = t.Value.Parameters
            });
        }
    }
}
