using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Business;
using com.etsoo.CoreFramework.Models;
using com.etsoo.CoreFramework.User;
using com.etsoo.Database;
using com.etsoo.Localization;
using com.etsoo.Utils;
using com.etsoo.Utils.Actions;
using CRM.Server.Application;
using CRM.Server.Dto.PersonInfo;
using CRM.Server.Dto.Supplier;
using CRM.Server.RQ;
using CRM.Server.RQ.Supplier;
using Microsoft.EntityFrameworkCore;
using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.Person;
using PlatformShared.Database;
using PlatformShared.Database.Models;
using PlatformShared.Dto;
using PlatformShared.Extentions;
using System.Buffers;
using System.Text.Json;

namespace CRM.Server.Services
{
    /// <summary>
    /// Supplier service
    /// 供应商服务
    /// </summary>
    public class SupplierService : MyUserService, ISupplierService
    {
        readonly MyDbContext _db;
        readonly ICommonService _commonService;
        readonly IQueueService _queueService;

        public SupplierService(
            MyDbContext db,
            IMyApp app,
            MyAppConfiguration config,
            CurrentUserAccessor userAccessor,
            ILogger<SupplierService> logger,
            ICommonService commonService,
            IQueueService queueService
        )
            : base(app, config, userAccessor.UserSafe, "supplier", logger)
        {
            _db = db;
            _commonService = commonService;
            _queueService = queueService;
        }

        /// <summary>
        /// Create
        /// 创建
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> CreateAsync(SupplierCreateRQ rq, CancellationToken cancellationToken = default)
        {
            var orgId = User.OrganizationInt;

            if (!await _commonService.HasPermissionAsync((short)Permissions.Supplier.Add, cancellationToken))
            {
                return ApplicationErrors.AccessDenied.AsResult();
            }

            // Categories
            var categoryIds = rq.Categories;
            var (result, ids) = await _commonService.ValidatePersonCategoriesAsync(categoryIds, orgId, cancellationToken);
            if (!result.Ok)
            {
                return result;
            }

            // Organization scope duplicate check
            var duplicateItems = new List<(PersonInfoKind, string)>();

            if (!string.IsNullOrEmpty(rq.Mobile))
            {
                duplicateItems.Add((PersonInfoKind.Mobile, rq.Mobile.Trim().ToLower()));
            }

            if (!string.IsNullOrEmpty(rq.Email))
            {
                duplicateItems.Add((PersonInfoKind.Email, rq.Email.Trim().ToLower()));
            }

            if (!string.IsNullOrEmpty(rq.Phone))
            {
                duplicateItems.Add((PersonInfoKind.Phone, rq.Phone.Trim().ToLower()));
            }

            if (!string.IsNullOrEmpty(rq.Pin))
            {
                duplicateItems.Add((PersonInfoKind.Pin, rq.Pin.Trim().ToLower()));
            }

            if (!string.IsNullOrEmpty(rq.TaxId))
            {
                duplicateItems.Add((PersonInfoKind.TaxId, rq.TaxId.Trim().ToLower()));
            }

            if (duplicateItems.Count > 0)
            {
                var duplicateResult = await _db.PersonInfoDuplicateAsync(orgId, null, duplicateItems, cancellationToken);
                if (!duplicateResult.Ok)
                {
                    return duplicateResult;
                }
            }

            // Contact
            var contactItems = duplicateItems.RemoveAndReturn(d => d.Item1 == PersonInfoKind.Mobile || d.Item1 == PersonInfoKind.Email);
            long? contactId = null;

            if (!string.IsNullOrEmpty(rq.Contact) && contactItems.Count > 0)
            {
                var cnd = LocalizationUtils.ParseName(rq.Contact);

                var cc = new Person
                {
                    OrgId = orgId,
                    UserId = User.Oid,
                    IdentityType = IdentityTypeFlags.None,
                    Name = rq.Contact,
                    QueryKeyword = cnd.PinyinInitials,
                    FamilyName = cnd.FamilyName,
                    GivenName = cnd.GivenName,
                    LatinGivenName = cnd.LatinGivenName,
                    LatinFamilyName = cnd.LatinFamilyName,

                    Infos = [.. contactItems.Select(d => new PersonInfo
                    {
                        Kind = d.Item1,
                        Identifier = d.Item2,
                        IsDefault = true
                    })]
                };

                _db.Persons.Add(cc);

                await _db.SaveChangesAsync(cancellationToken);

                contactId = cc.Id;
            }

            // Parse name
            var nd = LocalizationUtils.ParseName(rq.Name);

            // Create supplier
            var lp = rq.IsLegalPerson;
            var supplier = new Person
            {
                OrgId = orgId,
                UserId = User.Oid,
                IdentityType = IdentityTypeFlags.Supplier,
                IsLegalPerson = lp,
                Name = rq.Name,
                QueryKeyword = nd.PinyinInitials,
                FamilyName = lp ? null : nd.FamilyName,
                GivenName = lp ? null : nd.GivenName,
                LatinGivenName = lp ? null : nd.LatinGivenName,
                LatinFamilyName = lp ? null : nd.LatinFamilyName,
                PreferredName = rq.PreferredName,
                AssignedId = rq.AssignedId,
                Description = rq.Description,
                Birthday = rq.Birthday,
                Status = rq.Status ?? EntityStatus.Normal,
                Data = rq.Data,
                CategoryIds = categoryIds?.ToList(),
                CategoryIdsAll = ids?.ToList(),

                Infos = [.. duplicateItems.Select(d => new PersonInfo
                    {
                        Kind = d.Item1,
                        Identifier = d.Item2,
                        IsDefault = true
                    })]
            };

            if (rq.Tags?.Any() is true)
            {
                var tagIds = await _commonService.AddTagsAsync(FeatureTagKind.Supplier, rq.Tags, cancellationToken);
                supplier.Tags = [.. tagIds];
            }

            if (rq.Address != null)
            {
                var addr = rq.Address.CreateAddressFromRQ(supplier.Id);
                supplier.Addresses = [addr];
            }

            if (contactId != null)
            {
                supplier.Contacts = [new PersonRelation {
                    RelationType = PersonRelationType.Unknown,
                    ContactId = contactId.Value,
                    IsDefault = true
                }];
            }

            _db.Persons.Add(supplier);

            // Save changes
            await _db.SaveChangesAsync(cancellationToken);

            var id = supplier.Id;

            // Push message
            var message = new CreateSupplierMessage
            {
                Data = User.CreateMessageData(App.AppId, id, supplier.Name),
                JsonData = JsonSerializer.Serialize(rq, MyJsonSerializerContext.Default.SupplierCreateRQ)
            };
            await _queueService.PushAsync(message, CrmJsonSerializerContext.Default.CreateSupplierMessage, cancellationToken);

            return ActionResult.Succeed(id);
        }

        private IQueryable<Person> CreateQuery(SupplierListRQ rq, Func<IQueryable<Person>, IQueryable<Person>>? filters = null)
        {
            var query = _db.Suppliers(User.OrganizationInt)
                .QueryEtsoo(rq, (c) => c.Id, (c) => c.Status, (q) =>
                {
                    if (rq.TagId != null)
                    {
                        q = q.Where(p => p.Tags != null && p.Tags.Contains(rq.TagId.Value));
                    }

                    if (rq.CategoryIdAll.HasValue)
                    {
                        q = q.Where(p => p.CategoryIdsAll != null && p.CategoryIdsAll.Contains(rq.CategoryIdAll.Value));
                    }
                    else if (rq.CategoryId.HasValue)
                    {
                        q = q.Where(p => p.CategoryIds != null && p.CategoryIds.Contains(rq.CategoryId.Value));
                    }
                    else if (rq.CategoryIds?.Any() is true)
                    {
                        q = q.Where(p => p.CategoryIds != null && rq.CategoryIds.Any(c => p.CategoryIds.Contains(c)));
                    }

                    if (rq.ProductId.HasValue)
                    {
                        q = q.Where(p => p.Products.Any(pr => pr.ProductId == rq.ProductId.Value));
                    }

                    if (!string.IsNullOrEmpty(rq.City))
                    {
                        q = q.Where(p => p.Addresses.Any(a => a.City == rq.City));
                    }

                    if (rq.Keyword?.Length > 1)
                    {
                        var keyword = rq.Keyword;

                        if (keyword.IsComplexQueryKeywords())
                        {
                            q = q.QueryEtsooKeywords(keyword, DbUtils.ILikeMethod, c => c.Name, c => c.PreferredName, c => c.Description);
                        }
                        else
                        {
                            q = q.Where(c => EF.Functions.ILike(c.Name, $"%{keyword}%")
                            || (c.QueryKeyword != null && EF.Functions.ILike(c.QueryKeyword, $"%{keyword}%"))
                            || (c.PreferredName != null && EF.Functions.ILike(c.PreferredName, $"%{keyword}%"))
                            || (c.Description != null && EF.Functions.ILike(c.Description, $"%{keyword}%"))
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
        /// List supplier JSON data
        /// 供应商列表JSON数据
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="writer">Writer to hold the data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task ListAsync(SupplierListRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default)
        {
            await _commonService.UpdateTagAsync(rq, User.OrganizationInt, cancellationToken);

            await CreateQuery(rq)
                .Select(p => new SupplierListData
                {
                    Id = p.Id,
                    Name = p.Name,
                    PreferredName = p.PreferredName
                }).ToJsonAsync(writer, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Query supplier
        /// 查询供应商
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<SupplierQueryData[]> QueryAsync(SupplierQueryRQ rq, CancellationToken cancellationToken = default)
        {
            var orgId = User.OrganizationInt;

            await _commonService.UpdateTagAsync(rq, orgId, cancellationToken);

            return await CreateQuery(rq, (q) =>
            {
                if (!string.IsNullOrEmpty(rq.AssignedId))
                {
                    q = q.Where(p => p.AssignedId != null && EF.Functions.ILike(p.AssignedId, $"{rq.AssignedId}%"));
                }

                if (!string.IsNullOrEmpty(rq.Info))
                {
                    var info = rq.Info.Trim().ToLower();
                    q = q.Where(p => p.Infos.Any(i => i.Identifier == info));
                }

                return q;
            }).Select(p => new SupplierQueryData
            {
                Id = p.Id,
                Name = p.Name,
                AssignedId = p.AssignedId,
                Categories = p.CategoryIds == null ? null : _db.PersonCategories.Where(c => c.CoreOrganizationId == orgId && p.CategoryIds.Contains(c.Id)).OrderBy(t => p.CategoryIds.IndexOf(t.Id)).Select(c => new CategoryItem { Id = c.Id, Names = c.Names }).ToList(),
                PreferredName = p.PreferredName,
                Description = p.Description,
                Creation = p.Creation
            }).ToArrayAsync(cancellationToken);
        }

        /// <summary>
        /// Read supplier data for purchase
        /// 读取采购用的供应商数据
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<SupplierReadForPurchaseData?> ReadForPurchaseAsync(SupplierReadForPurchaseRQ rq, CancellationToken cancellationToken = default)
        {
            var orgId = User.OrganizationInt;
            var now = DateTime.UtcNow;

            var currency = rq.Currency;

            var data = new SupplierReadForPurchaseData();

            if (rq.SupplierId > 0)
            {
                var supplierId = rq.SupplierId.Value;

                var supplier = await _db.Suppliers(orgId).AsNoTracking()
                    .Where(p => p.Id == supplierId && p.Status < EntityStatus.Inactivated)
                    .Select(p => new SupplierPurchaseData
                    {
                        Id = p.Id,
                        Name = p.Name,
                        PreferredName = p.PreferredName,
                        IsLegalPerson = p.IsLegalPerson
                    }).FirstOrDefaultAsync(cancellationToken);

                if (supplier == null)
                {
                    return null;
                }

                var promotions = await _db.Promotions(orgId)
                    .AsNoTracking()
                    .Where(pr => pr.Status < EntityStatus.Inactivated
                        && pr.ValidStart <= now
                        && pr.ValidEnd >= now
                        && pr.Currency == currency
                        && pr.ProductIds == null
                        && pr.ProductCategoryIds == null
                        && (pr.PersonIds != null && pr.PersonIds.Contains(supplierId))
                    )
                    .Select(pr => new PromotionItem
                    {
                        Id = pr.Id,
                        Code = pr.Code,
                        Title = pr.Title,
                        MinAmount = pr.MinAmount,
                        Discount = pr.Discount,
                        Stackable = pr.Stackable
                    })
                    .ToArrayAsync(cancellationToken);

                supplier.Promotions = promotions;

                data.Supplier = supplier;
            }

            return data;
        }

        /// <summary>
        /// Update
        /// 更新
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> UpdateAsync(SupplierUpdateRQ rq, CancellationToken cancellationToken = default)
        {
            // Permission check
            if (!await _commonService.HasPermissionAsync((short)Permissions.Supplier.Edit, cancellationToken))
            {
                return ApplicationErrors.AccessDenied.AsResult();
            }

            // Organization id
            var orgId = User.OrganizationInt;

            var supplier = await _db.Suppliers(orgId)
                .Where(p => p.Id == rq.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (supplier == null)
            {
                return ApplicationErrors.NoId.AsResult();
            }

            if (rq.IsModified(nameof(rq.IsLegalPerson)) && rq.IsLegalPerson.HasValue)
            {
                supplier.IsLegalPerson = rq.IsLegalPerson.Value;
            }

            if (rq.IsModified(nameof(rq.Name)) && !string.IsNullOrEmpty(rq.Name))
            {
                supplier.Name = rq.Name;
            }

            if (rq.IsModified(nameof(rq.PreferredName)))
            {
                supplier.PreferredName = rq.PreferredName;
            }

            if (rq.IsModified(nameof(rq.AssignedId)))
            {
                supplier.AssignedId = rq.AssignedId?.ToUpper();
            }

            if (rq.IsModified(nameof(rq.Description)))
            {
                supplier.Description = rq.Description;
            }

            if (rq.IsModified(nameof(rq.Birthday)))
            {
                supplier.Birthday = rq.Birthday;
            }

            if (rq.IsModified(nameof(rq.Status)) && rq.Status.HasValue)
            {
                supplier.Status = rq.Status.Value;
            }

            if (rq.IsModified(nameof(rq.Data)))
            {
                supplier.Data = rq.Data;
            }

            if (rq.IsModified(nameof(rq.Categories)))
            {
                // Categories
                var categoryIds = rq.Categories;
                var (result, ids) = await _commonService.ValidatePersonCategoriesAsync(categoryIds, orgId, cancellationToken);
                if (!result.Ok)
                {
                    return result;
                }

                supplier.CategoryIds = categoryIds?.ToList();
                supplier.CategoryIdsAll = ids?.ToList();
            }

            if (rq.IsModified(nameof(rq.Pin)))
            {
                await _commonService.AddOrUpdatePersonInfoAsync(rq.Id, PersonInfoKind.Pin, rq.Pin, cancellationToken);
            }

            if (rq.IsModified(nameof(rq.TaxId)))
            {
                await _commonService.AddOrUpdatePersonInfoAsync(rq.Id, PersonInfoKind.TaxId, rq.TaxId, cancellationToken);
            }

            if (rq.IsModified(nameof(rq.Tags)))
            {
                if (rq.Tags?.Any() is true)
                {
                    var tagIds = await _commonService.AddTagsAsync(FeatureTagKind.Supplier, rq.Tags, cancellationToken);
                    supplier.Tags = [.. tagIds];
                }
                else
                {
                    supplier.Tags = null;
                }
            }

            // Changes
            var changes = _db.ChangeTracker.Entries().GetChangedProperties();

            // Save
            var task1 = _db.SaveChangesAsync(cancellationToken);

            // Push message
            var message = new UpdateSupplierMessage
            {
                Data = User.CreateMessageData(App.AppId, rq.Id, supplier.Name),
                Changes = changes
            };
            var task2 = _queueService.PushAsync(message, CrmJsonSerializerContext.Default.UpdateSupplierMessage, cancellationToken);

            await Task.WhenAll(task1, task2);

            // Return
            return ActionResult.Succeed(rq.Id);
        }

        /// <summary>
        /// Read data for update
        /// 读取用于更新的数据
        /// </summary>
        /// <param name="id">Person id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<SupplierUpdateReadData?> UpdateReadAsync(long id, CancellationToken cancellationToken = default)
        {
            if (!await _commonService.HasPermissionAsync((short)Permissions.Supplier.Edit, cancellationToken))
            {
                return null;
            }

            var orgId = User.OrganizationInt;

            return await _db.Suppliers(orgId).AsNoTracking()
                .Where(p => p.Id == id)
                .Select(p => new SupplierUpdateReadData
                {
                    Id = p.Id,
                    IsLegalPerson = p.IsLegalPerson,
                    Name = p.Name,
                    PreferredName = p.PreferredName,
                    AssignedId = p.AssignedId,
                    Description = p.Description,
                    Birthday = p.Birthday,
                    Status = p.Status,
                    Categories = p.CategoryIds,
                    Tags = p.Tags == null ? null : _db.FeatureTags.Where(k => k.CoreOrganizationId == orgId && p.Tags.Contains(k.Id)).OrderByDescending(t => t.Total).ThenBy(t => t.Tag).Select(k => k.Tag).ToList(),
                    Infos = p.Infos
                        .Where(i => i.PersonId == p.Id && (i.Kind == PersonInfoKind.Pin || i.Kind == PersonInfoKind.TaxId))
                        .Select(i => new PersonInfoUpdateItem
                        {
                            Kind = i.Kind,
                            Identifier = MyDbFunctions.HideData(i.Identifier, default),
                            IsDefault = i.IsDefault
                        })
                        .ToList()
                }).FirstOrDefaultAsync(cancellationToken);
        }
    }
}