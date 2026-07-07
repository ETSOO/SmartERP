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
        private const int CacheMinutes = 2;
        private static readonly MemoryCache cache = new(new MemoryCacheOptions());

        private static readonly FrozenDictionary<int, SystemTemplateItem> systemTemplates = new Dictionary<int, SystemTemplateItem>
        {
            [-1] = new SystemTemplateItem
            {
                Kind = DocumentKind.CmsOrderData,
                Subject = "OrderStandardContract",
                Template = "Order/StandardContract_{culture}",
                Data = (dbFactory, id, dic, user, cancellationToken) => CreateOrderViewObjectAsync(dbFactory, id, dic, user, cancellationToken)
            },
            [-2] = new SystemTemplateItem
            {
                Kind = DocumentKind.CmsOrderData,
                Subject = "OrderProductList",
                Template = "Order/ProductList_{culture}",
                Data = (dbFactory, id, dic, user, cancellationToken) => CreateOrderViewObjectAsync(dbFactory, id, dic, user, cancellationToken)
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
                entry.SetAbsoluteExpiration(TimeSpan.FromMinutes(CacheMinutes));

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
        /// Create order view data
        /// 创建订单视图数据
        /// </summary>
        /// <param name="dbFactory">Database context factory</param>
        /// <param name="orderId">Order id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public static Task<OrderViewData?> CreateOrderDataAsync(IDbContextFactory<MyDbContext> dbFactory, long orderId, CurrentUser user, CancellationToken cancellationToken = default)
        {
            var cacheKey = $"{nameof(CreateOrderDataAsync)}:{orderId}";

            return cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.SetAbsoluteExpiration(TimeSpan.FromMinutes(CacheMinutes));

                var orgId = user.OrganizationInt;

                var orderDb = await dbFactory.CreateDbContextAsync(cancellationToken);
                var order = await orderDb.Orders(orgId).AsNoTracking()
                    .Where(o => o.Id == orderId)
                    .Select(o => new OrderViewData
                    {
                        Id = orderId,
                        Source = o.Source,
                        SourceId = o.SourceId,
                        AssignedId = o.AssignedId,
                        Title = o.Title,
                        Description = o.Description,
                        StartDate = o.StartDate,
                        EndDate = o.EndDate,
                        Currency = o.Currency,
                        Amount = o.Amount,
                        PaidAmount = o.PaidAmount,
                        Discount = o.Discount,
                        LineDiscount = o.LineDiscount,
                        ApprovedDiscount = o.ApprovedDiscount,
                        TaxAmount = o.TaxAmount,
                        Lines = o.Lines,
                        Items = o.Items,
                        Promotions = o.Promotions == null ? Array.Empty<PromotionSaleItem>() : o.Promotions.ToArray(),
                        Culture = o.Culture,
                        Payment = o.Payment == null ? null : o.Payment.Title,
                        PaymentKind = o.Payment == null ? null : o.Payment.Kind,
                        PaymentDescription = o.Payment == null ? null : o.Payment.Description,
                        PaymentInstruction = o.PaymentInstruction,
                        Delivery = o.Delivery == null ? null : o.Delivery.Title,
                        DeliveryKind = o.Delivery == null ? null : o.Delivery.Kind,
                        DeliveryDescription = o.Delivery == null ? null : o.Delivery.Description,
                        DeliveryInstruction = o.DeliveryInstruction,
                        AddressFormatted = o.AddressFormatted,
                        Contact = o.Contact == null ? null : o.Contact.Name,
                        ContactId = o.ContactId,
                        User = o.User.Name,
                        UserId = o.UserId,
                        Creation = o.Creation,
                        Status = o.Status,
                        Tags = o.Tags == null ? null : orderDb.FeatureTags.Where(k => k.CoreOrganizationId == orgId && o.Tags.Contains(k.Id)).OrderByDescending(t => t.Total).ThenBy(t => t.Tag).Select(k => k.Tag).ToList(),

                        Customer = new OrderCustomerData
                        {
                            Id = o.BuyerId,
                            IsLegalPerson = o.Buyer.IsLegalPerson,
                            Name = o.Buyer.Name,
                            PreferredName = o.Buyer.PreferredName,
                            AssignedId = o.Buyer.AssignedId,
                            Description = o.Buyer.Description,
                            Birthday = o.Buyer.Birthday,
                            Categories = o.Buyer.CategoryIds,
                            Infos = o.Buyer.Infos
                                .Select(i => new PersonInfoViewItem
                                {
                                    Kind = i.Kind,
                                    Identifier = i.Identifier,
                                    IsDefault = i.IsDefault,
                                    IsVerified = i.IsVerified ?? false
                                })
                                .ToList()
                        },

                        OrderLines = o.OrderLines.Select(l => new OrderLineViewData
                        {
                            Id = l.Id,
                            ProductId = l.ProductId,
                            ProductName = l.Product.Name,
                            ProductAssignedId = l.Product.AssignedId,
                            ProductDescription = l.Product.Description,
                            ProductLogo = l.Product.Logo,
                            ProductModifiers = l.Product.Modifiers,
                            UnitName = l.Product.Unit.Name,
                            BaseUnit = l.Product.Unit.BaseUnit,
                            Title = l.Title,
                            Description = l.Description,
                            OriginalPrice = l.OriginalPrice,
                            CostPrice = l.CostPrice,
                            Price = l.Price,
                            Qty = l.Qty,
                            QtyDelivered = l.QtyDelivered ?? 0,
                            AssetQty = l.AssetQty,
                            Amount = l.Amount,
                            Discount = l.Discount,
                            Promotions = l.Promotions == null ? Array.Empty<PromotionSaleItem>() : l.Promotions.ToArray(),
                            StartTime = l.StartTime,
                            EndTime = l.EndTime,
                            Data = l.Data,
                            AssetId = l.AssetId,
                            AssetSn = l.Asset == null ? null : l.Asset.Sn,
                            Status = l.Status,
                            Creation = l.Creation,
                            BomId = l.BomId,
                            BomTitle = l.Bom == null ? null : l.Bom.Title
                        }).ToArray()
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                return order;
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
                entry.SetAbsoluteExpiration(TimeSpan.FromMinutes(CacheMinutes));

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
                        o.Slogan,
                        o.CompanySeal
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
                    CompanySeal = orgData.CompanySeal,
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
        public static async Task<OrderTemplateData?> CreateOrderViewAsync(IDbContextFactory<MyDbContext> dbFactory, long id, StringKeyDictionaryObject dic, CurrentUser user, CancellationToken cancellationToken = default)
        {
            var org = await CreateOrgDataAsync(dbFactory, user, cancellationToken);
            if (org == null) return null;

            var order = await CreateOrderDataAsync(dbFactory, id, user, cancellationToken);
            if (order == null) return null;

            var currentUser = await CreateUserDataAsync(dbFactory, user, cancellationToken);
            if (currentUser == null) return null;

            return new OrderTemplateData
            {
                Subject = dic.Get(nameof(OrderTemplateData.Subject)),
                User = currentUser,
                Org = org,
                Order = order,
                Dic = dic
            };
        }

        static async Task<object?> CreateOrderViewObjectAsync(IDbContextFactory<MyDbContext> dbFactory, long id, StringKeyDictionaryObject dic, CurrentUser user, CancellationToken cancellationToken = default)
        {
            return await CreateOrderViewAsync(dbFactory, id, dic, user, cancellationToken);
        }

        /// <summary>
        /// Create current user view data
        /// 创建当前用户视图数据
        /// </summary>
        /// <param name="dbFactory">Database context factory</param>
        /// <param name="user">Current user</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public static async Task<CurrentUserData?> CreateUserDataAsync(IDbContextFactory<MyDbContext> dbFactory, CurrentUser user, CancellationToken cancellationToken = default)
        {
            await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);

            return await db.Persons.AsNoTracking()
                .Where(p => p.Id == user.Oid)
                .Select(p => new CurrentUserData
                {
                    Name = p.Name,
                    PreferredName = p.PreferredName,
                    GivenName = p.GivenName,
                    FamilyName = p.FamilyName,
                    LatinFamilyName = p.LatinFamilyName,
                    LatinGivenName = p.LatinGivenName,
                    Avatar = p.Avatar,
                    Signature = p.CoreUser == null ? null : p.CoreUser.Signature,
                    Infos = p.Infos.Select(i => new PersonInfoViewItem
                    {
                        Kind = i.Kind,
                        Identifier = i.Identifier,
                        IsDefault = i.IsDefault,
                        IsVerified = i.IsVerified ?? false
                    }).ToList()
                })
                .FirstOrDefaultAsync(cancellationToken);
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
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10 * CacheMinutes);

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
                entry.SetAbsoluteExpiration(TimeSpan.FromMinutes(CacheMinutes));

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
                entry.SetAbsoluteExpiration(TimeSpan.FromMinutes(CacheMinutes));

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

        /// <summary>
        /// Get template by id
        /// 通过编号获取模板
        /// </summary>
        /// <param name="id">Template id</param>
        /// <returns>Result</returns>
        public static SystemTemplateItem? GetTemplate(int id)
        {
            systemTemplates.TryGetValue(id, out var item);
            return item;
        }

        /// <summary>
        /// Get template model
        /// 获取模板模型
        /// </summary>
        /// <param name="dbFactory">Database context factory</param>
        /// <param name="dic">Dictionary</param>
        /// <param name="kind">Template kind</param>
        /// <param name="targetId">Target id</param>
        /// <param name="user">User</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Result</returns>
        public static async Task<object?> GetTemplateModelAsync(IDbContextFactory<MyDbContext> dbFactory, StringKeyDictionaryObject dic, string kind, long targetId, CurrentUser user, CancellationToken cancellationToken = default)
        {
            object? model = kind switch
            {
                DocumentKind.CmsOrderData => await CreateOrderViewObjectAsync(dbFactory, targetId, dic, user, cancellationToken),
                _ => null
            };

            return model;
        }
    }
}
