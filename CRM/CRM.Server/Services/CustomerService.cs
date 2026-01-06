using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Business;
using com.etsoo.CoreFramework.Models;
using com.etsoo.CoreFramework.User;
using com.etsoo.Database;
using com.etsoo.Localization;
using com.etsoo.ServiceApp.SmartERP;
using com.etsoo.Utils;
using com.etsoo.Utils.Actions;
using CRM.Server.Dto.Customer;
using CRM.Server.Dto.Person;
using CRM.Server.RQ;
using CRM.Server.RQ.Customer;
using Microsoft.EntityFrameworkCore;
using PlatformShared.Database;
using PlatformShared.Database.Models;
using PlatformShared.Dto;
using PlatformShared.Extentions;
using System.Buffers;

namespace CRM.Server.Services
{
    /// <summary>
    /// Customer service
    /// 客户服务
    /// </summary>
    public class CustomerService : SEUserService, ICustomerService
    {
        readonly MyDbContext _db;
        readonly ICommonService _commonService;

        public CustomerService(
            MyDbContext db,
            ISEServiceApp app,
            CurrentUserAccessor userAccessor,
            ILogger<CustomerService> logger,
            ICommonService commonService
        )
            : base(app, userAccessor.UserSafe, "customer", logger)
        {
            _db = db;
            _commonService = commonService;
        }

        /// <summary>
        /// Create
        /// 创建
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> CreateAsync(CustomerCreateRQ rq, CancellationToken cancellationToken = default)
        {
            var orgId = User.OrganizationInt;

            if (!await _commonService.HasPermissionAsync((short)Permissions.Customer.Add, cancellationToken))
            {
                return ApplicationErrors.AccessDenied.AsResult();
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

            // Create customer
            var lp = rq.IsLegalPerson;
            var customer = new Person
            {
                OrgId = orgId,
                UserId = User.Oid,
                IdentityType = IdentityTypeFlags.Customer,
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
                CategoryIds = rq.Categories?.ToList(),

                Infos = [.. duplicateItems.Select(d => new PersonInfo
                    {
                        Kind = d.Item1,
                        Identifier = d.Item2,
                        IsDefault = true
                    })]
            };

            if (rq.Tags?.Any() is true)
            {
                var tagKind = _commonService.GetTagKind(IdentityTypeFlags.Customer);
                var tagIds = await _commonService.AddTagsAsync(tagKind, rq.Tags, cancellationToken);
                customer.Tags = [.. tagIds];
            }

            if (rq.Address != null)
            {
                var addr = rq.Address.CreateAddressFromRQ(customer.Id);
                customer.Addresses = [addr];
            }

            if (contactId != null)
            {
                customer.Contacts = [new PersonRelation { 
                    RelationType = PersonRelationType.Unknown,
                    ContactId = contactId.Value,
                    IsDefault = true
                }];
            }

            _db.Persons.Add(customer);

            // Save changes
            await _db.SaveChangesAsync(cancellationToken);

            return ActionResult.Succeed(customer.Id);
        }

        private IQueryable<Person> CreateQuery(CustomerListRQ rq, Func<IQueryable<Person>, IQueryable<Person>>? filters = null)
        {
            var query = _db.Customers(User.OrganizationInt).AsNoTracking()
                .QueryEtsoo(rq, (c) => c.Id, (c) => c.Status, (q) =>
                {
                    if (rq.TagId != null)
                    {
                        q = q.Where(p => p.Tags != null && p.Tags.Contains(rq.TagId.Value));
                    }

                    if (rq.CategoryIdAll.HasValue)
                    {
                        q = q.Where(p => p.CategoryIds != null && _db.PersonCategories.Any(c => p.CategoryIds.Contains(c.Id)
                            && c.ParentIds != null && c.ParentIds.Contains(rq.CategoryIdAll.Value))
                        );
                    }
                    else if (rq.CategoryId.HasValue)
                    {
                        q = q.Where(p => p.CategoryIds != null && p.CategoryIds.Contains(rq.CategoryId.Value));
                    }
                    else if (rq.CategoryIds?.Any() is true)
                    {
                        q = q.Where(p => p.CategoryIds != null && rq.CategoryIds.Any(c => p.CategoryIds.Contains(c)));
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
        /// List customer JSON data
        /// 客户列表JSON数据
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="writer">Writer to hold the data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task ListAsync(CustomerListRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default)
        {
            await _commonService.UpdateTagAsync(rq, User.OrganizationInt, cancellationToken);

            await CreateQuery(rq)
                .Select(p => new CustomerListData
                {
                    Id = p.Id,
                    Name = p.Name,
                    PreferredName = p.PreferredName
                }).ToJsonAsync(writer, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Query customer
        /// 查询客户
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<CustomerQueryData[]> QueryAsync(CustomerQueryRQ rq, CancellationToken cancellationToken = default)
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
            }).Select(p => new CustomerQueryData
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
        /// Update
        /// 更新
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> UpdateAsync(CustomerUpdateRQ rq, CancellationToken cancellationToken = default)
        {
            // Permission check
            if (!await _commonService.HasPermissionAsync((short)Permissions.Customer.Edit, cancellationToken))
            {
                return ApplicationErrors.AccessDenied.AsResult();
            }

            // Organization id
            var orgId = User.OrganizationInt;

            var customer = await _db.Customers(orgId)
                .Where(p => p.Id == rq.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (customer == null)
            {
                return ApplicationErrors.NoId.AsResult();
            }

            if (rq.IsModified(nameof(rq.IsLegalPerson)) && rq.IsLegalPerson.HasValue)
            {
                customer.IsLegalPerson = rq.IsLegalPerson.Value;
            }

            if (rq.IsModified(nameof(rq.Name)) && !string.IsNullOrEmpty(rq.Name))
            {
                customer.Name = rq.Name;
            }

            if (rq.IsModified(nameof(rq.PreferredName)))
            {
                customer.PreferredName = rq.PreferredName;
            }

            if (rq.IsModified(nameof(rq.AssignedId)))
            {
                customer.AssignedId = rq.AssignedId?.ToUpper();
            }

            if (rq.IsModified(nameof(rq.Description)))
            {
                customer.Description = rq.Description;
            }

            if (rq.IsModified(nameof(rq.Birthday)))
            {
                customer.Birthday = rq.Birthday;
            }

            if (rq.IsModified(nameof(rq.Status)) && rq.Status.HasValue)
            {
                customer.Status = rq.Status.Value;
            }

            if (rq.IsModified(nameof(rq.Categories)))
            {
                customer.CategoryIds = rq.Categories?.ToList();
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
                    var tagKind = _commonService.GetTagKind(IdentityTypeFlags.Customer);
                    var tagIds = await _commonService.AddTagsAsync(tagKind, rq.Tags, cancellationToken);
                    customer.Tags = [.. tagIds];
                }
                else
                {
                    customer.Tags = null;
                }
            }

            // Save
            await _db.SaveChangesAsync(cancellationToken);

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
        public async Task<CustomerUpdateReadData?> UpdateReadAsync(long id, CancellationToken cancellationToken = default)
        {
            if (!await _commonService.HasPermissionAsync((short)Permissions.Customer.Edit, cancellationToken))
            {
                return null;
            }

            var orgId = User.OrganizationInt;

            return await _db.Customers(orgId).AsNoTracking()
                .Where(p => p.Id == id)
                .Select(p => new CustomerUpdateReadData
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