using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Business;
using com.etsoo.CoreFramework.Models;
using com.etsoo.CoreFramework.User;
using com.etsoo.Database;
using com.etsoo.Localization;
using com.etsoo.ServiceApp.SmartERP;
using com.etsoo.Utils.Actions;
using CRM.Server.Dto.Supplier;
using CRM.Server.RQ;
using CRM.Server.RQ.Customer;
using CRM.Server.RQ.Supplier;
using Microsoft.EntityFrameworkCore;
using PlatformShared.Database;
using PlatformShared.Database.Models;
using PlatformShared.Dto;
using PlatformShared.Extentions;
using System.Buffers;

namespace CRM.Server.Services
{
    /// <summary>
    /// Supplier service
    /// 供应商服务
    /// </summary>
    public class SupplierService : SEUserService, ISupplierService
    {
        readonly MyDbContext _db;
        readonly ICommonService _commonService;

        public SupplierService(
            MyDbContext db,
            ISEServiceApp app,
            CurrentUserAccessor userAccessor,
            ILogger<SupplierService> logger,
            ICommonService commonService
        )
            : base(app, userAccessor.UserSafe, "supplier", logger)
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
        public async Task<IActionResult> CreateAsync(SupplierCreateRQ rq, CancellationToken cancellationToken = default)
        {
            var orgId = User.OrganizationInt;

            if (!await _commonService.HasPermissionAsync((short)Permissions.Supplier.Add, cancellationToken))
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

            if (duplicateItems.Count > 0)
            {
                var duplicateResult = await _db.PersonInfoDuplicateAsync(orgId, null, duplicateItems, cancellationToken);
                if (!duplicateResult.Ok)
                {
                    return duplicateResult;
                }
            }

            // Parse name
            var nd = LocalizationUtils.ParseName(rq.Name);

            // Create supplier
            var supplier = new Person
            {
                OrgId = orgId,
                UserId = User.Oid,
                IdentityType = IdentityTypeFlags.Supplier,
                IsLegalPerson = rq.IsLegalPerson,
                Name = rq.Name,
                QueryKeyword = nd.PinyinInitials,
                FamilyName = nd.FamilyName,
                GivenName = nd.GivenName,
                LatinGivenName = nd.LatinGivenName,
                LatinFamilyName = nd.LatinFamilyName,
                PreferredName = rq.PreferredName,
                AssignedId = rq.AssignedId,
                Description = rq.Description,
                Birthday = rq.Birthday,
                Status = rq.Status ?? EntityStatus.Normal,
                CategoryIds = rq.Categories?.ToList()
            };

            if (rq.Tags?.Any() is true)
            {
                var tagKind = _commonService.GetTagKind(IdentityTypeFlags.Supplier);
                var tagIds = await _commonService.AddTagsAsync(tagKind, rq.Tags, cancellationToken);
                supplier.Tags = [.. tagIds];
            }

            _db.Persons.Add(supplier);

            await _db.SaveChangesAsync(cancellationToken);

            // Contact info
            foreach (var item in duplicateItems)
            {
                var info = new PersonInfo
                {
                    PersonId = supplier.Id,
                    Kind = item.Item1,
                    Identifier = item.Item2,
                    IsDefault = true
                };
                _db.PersonInfos.Add(info);
            }

            // Address
            if (rq.Address != null)
            {
                var addr = rq.Address.CreateAddressFromRQ(supplier.Id);
                _db.PersonAddresses.Add(addr);
            }

            // Save changes
            await _db.SaveChangesAsync(cancellationToken);

            return ActionResult.Succeed(supplier.Id);
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
        /// List supplier JSON data
        /// 供应商列表JSON数据
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="writer">Writer to hold the data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task ListAsync(SupplierListRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default)
        {
            await _commonService.UpdatePersonTagAsync(rq, User.OrganizationInt, cancellationToken);

            await CreateQuery(rq)
                .Select(p => new SupplierListData
                {
                    Id = p.Id,
                    Name = p.Name,
                    PreferredName = p.PreferredName
                }).ToJsonAsync(writer, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Query supplier JSON data
        /// 查询供应商JSON数据
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="writer">Writer to hold the data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task QueryAsync(SupplierQueryRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default)
        {
            var orgId = User.OrganizationInt;

            await _commonService.UpdatePersonTagAsync(rq, orgId, cancellationToken);

            await CreateQuery(rq, (q) =>
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
                    Categories = p.CategoryIds == null ? null : _db.PersonCategories.Where(c => c.CoreOrganizationId == orgId && p.CategoryIds.Contains(c.Id)).OrderBy(t => p.CategoryIds.IndexOf(t.Id)).Select(c => new CategoryItem { Id = c.Id, Names = c.Names }).ToList(),
                    PreferredName = p.PreferredName,
                    Description = p.Description,
                    Creation = p.Creation
                }).ToJsonAsync(writer, cancellationToken: cancellationToken);
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

            var customer = await _db.Suppliers(orgId)
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
                var pinInfo = await _db.PersonInfos
                    .Where(i => i.PersonId == rq.Id && i.Kind == PersonInfoKind.Pin)
                    .FirstOrDefaultAsync(cancellationToken);

                if (string.IsNullOrEmpty(rq.Pin))
                {
                    if (pinInfo != null)
                    {
                        _db.PersonInfos.Remove(pinInfo);
                    }
                }
                else
                {
                    if (pinInfo != null)
                    {
                        pinInfo.Identifier = rq.Pin.Trim().ToLower();
                    }
                    else
                    {
                        pinInfo = new PersonInfo
                        {
                            PersonId = rq.Id,
                            Kind = PersonInfoKind.Pin,
                            Identifier = rq.Pin.Trim().ToLower(),
                            IsDefault = true
                        };
                        _db.PersonInfos.Add(pinInfo);
                    }
                }
            }

            if (rq.IsModified(nameof(rq.Tags)))
            {
                if (rq.Tags?.Any() is true)
                {
                    var tagKind = _commonService.GetTagKind(IdentityTypeFlags.Supplier);
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
        public async Task<SupplierUpdateReadData?> UpdateReadAsync(long id, CancellationToken cancellationToken = default)
        {
            if (!await _commonService.HasPermissionAsync((short)Permissions.Customer.Edit, cancellationToken))
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
                    Pin = p.Infos.Where(i => i.PersonId == p.Id && i.Kind == PersonInfoKind.Pin).Select(i => i.Identifier).FirstOrDefault()
                }).FirstOrDefaultAsync(cancellationToken);
        }
    }
}