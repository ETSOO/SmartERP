using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Business;
using com.etsoo.CoreFramework.Models;
using com.etsoo.CoreFramework.User;
using com.etsoo.Database;
using com.etsoo.Localization;
using com.etsoo.ServiceApp.SmartERP;
using com.etsoo.Utils.Actions;
using CRM.Server.Dto.PersonContact;
using CRM.Server.RQ.PersonContact;
using Microsoft.EntityFrameworkCore;
using PlatformShared.Database;
using PlatformShared.Database.Models;
using PlatformShared.Extentions;
using System.Buffers;

namespace CRM.Server.Services
{
    /// <summary>
    /// Person contact service
    /// 人员联系人服务
    /// </summary>
    public class PersonContactService : SEUserService, IPersonContactService
    {
        readonly MyDbContext _db;
        readonly ICommonService _commonService;

        public PersonContactService(
            MyDbContext db,
            ISEServiceApp app,
            CurrentUserAccessor userAccessor,
            ILogger<PersonInfoService> logger,
            ICommonService commonService
        )
            : base(app, userAccessor.UserSafe, "person_contact", logger)
        {
            _db = db;
            _commonService = commonService;
        }

        /// <summary>
        /// Add a new contact relation
        /// 添加一个新的联系人关系
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> AddAsync(ContactRelationAddRQ rq, CancellationToken cancellationToken = default)
        {
            var orgId = User.OrganizationInt;
            var personId = rq.PersonId;
            var contactId = rq.ContactId;

            var result = await _db.Persons.AsNoTracking()
                .Where(p => p.Id == contactId && p.OrgId == orgId)
                .Select(p => new
                {
                    IdValid = true,
                    RelationExists = p.ContactOwners.Any(co => co.PersonId == personId)
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (result?.IdValid is not true)
            {
                return ApplicationErrors.NoId.AsResult(nameof(rq.ContactId));
            }

            if (result.RelationExists)
            {
                return ApplicationErrors.ItemExists.AsResult(nameof(rq.ContactId));
            }

            var cr = new PersonRelation
            {
                PersonId = personId,
                ContactId = contactId,
                RelationType = rq.RelationType,
                IsDefault = rq.IsDefault,
                Description = rq.Description,
                Data = rq.Data
            };

            _db.PersonRelations.Add(cr);
            await _db.SaveChangesAsync(cancellationToken);

            if (rq.IsDefault is true)
            {
                await _db.PersonRelations.AsNoTracking()
                    .Where(r => r.PersonId == personId && r.RelationType == rq.RelationType && r.IsDefault == true && r.ContactId != contactId)
                    .ExecuteUpdateAsync(r => r.SetProperty(r => r.IsDefault, false), cancellationToken);
            }

            return ActionResult.Succeed(cr.Id);
        }

        /// <summary>
        /// Create
        /// 创建
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> CreateAsync(ContactCreateRQ rq, CancellationToken cancellationToken = default)
        {
            // Organization id
            var orgId = User.OrganizationInt;

            var person = await _db.Persons
               .AsNoTracking()
               .Where(p => p.Id == rq.PersonId && p.OrgId == orgId)
               .Select(p => new
               {
                   p.Id,
                   p.IdentityType
               })
               .FirstOrDefaultAsync(cancellationToken);

            if (person == null)
            {
                return ApplicationErrors.NoId.AsResult(nameof(rq.PersonId));
            }

            if (!await _commonService.HasIdentityPermissionAsync(person.IdentityType, nameof(Permissions.Customer.AddContact), cancellationToken))
            {
                return ApplicationErrors.AccessDenied.AsResult();
            }

            // Is contact exists with same name
            var contactExists = await _db.PersonRelations.AsNoTracking()
                .AnyAsync(r => r.PersonId == rq.PersonId && r.Contact.Name.ToUpper() == rq.Name.ToUpper(), cancellationToken);

            if (contactExists)
            {
                return ApplicationErrors.ItemExists.AsResult(nameof(rq.Name));
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

            if (duplicateItems.Count > 0)
            {
                var duplicateResult = await _db.PersonInfoDuplicateAsync(orgId, null, duplicateItems, cancellationToken);
                if (!duplicateResult.Ok)
                {
                    return duplicateResult;
                }
            }

            // Parse name
            var nd = LocalizationUtils.ParseName(rq.Name, rq.FamilyName, rq.GivenName);

            // Create new contact
            var contact = new Person
            {
                OrgId = orgId,
                UserId = User.Oid,
                IdentityType = IdentityTypeFlags.None,
                Name = rq.Name,
                QueryKeyword = nd.PinyinInitials,
                FamilyName = nd.FamilyName,
                GivenName = nd.GivenName,
                LatinGivenName = nd.LatinGivenName,
                LatinFamilyName = nd.LatinFamilyName,
                PreferredName = rq.PreferredName,
                JobTitle = rq.JobTitle,
                Title = rq.Title,
                Gender = rq.Gender,
                Birthday = rq.Birthday,
                CategoryIds = rq.Categories?.ToList(),
                Regions = rq.Regions?.ToList(),
                Cultures = rq.Cultures?.ToList()
            };

            if (rq.Tags?.Any() is true)
            {
                var tagKind = _commonService.GetTagKind(IdentityTypeFlags.None);
                var tagIds = await _commonService.AddTagsAsync(tagKind, rq.Tags, cancellationToken);
                contact.Tags = [.. tagIds];
            }

            _db.Persons.Add(contact);

            await _db.SaveChangesAsync(cancellationToken);

            // Contact info
            foreach (var item in duplicateItems)
            {
                var info = new PersonInfo
                {
                    PersonId = contact.Id,
                    Kind = item.Item1,
                    Identifier = item.Item2,
                    IsDefault = true
                };
                _db.PersonInfos.Add(info);
            }

            // Add relation
            var relation = new PersonRelation
            {
                PersonId = person.Id,
                ContactId = contact.Id,
                RelationType = rq.RelationType,
                Description = rq.Description
            };

            _db.PersonRelations.Add(relation);

            // Save changes
            await _db.SaveChangesAsync(cancellationToken);

            return ActionResult.Succeed(contact.Id);
        }

        private IQueryable<PersonRelation> CreateQuery(ContactListRQ rq, Func<IQueryable<PersonRelation>, IQueryable<PersonRelation>>? filters = null)
        {
            var query = _db.PersonRelations(User.OrganizationInt, rq.PersonId).AsNoTracking()
                .QueryEtsoo(rq, (q) => q.ContactId, (q) => q.Contact.Status, (q) =>
                {
                    if (rq.RelationType.HasValue)
                    {
                        q = q.Where(r => r.RelationType == rq.RelationType.Value);
                    }

                    if (rq.TagId != null)
                    {
                        q = q.Where(p => p.Contact.Tags != null && p.Contact.Tags.Contains(rq.TagId.Value));
                    }

                    if (rq.CategoryId.HasValue)
                    {
                        q = q.Where(p => p.Contact.CategoryIds != null && p.Contact.CategoryIds.Contains(rq.CategoryId.Value));
                    }
                    else if (rq.CategoryIds?.Any() is true)
                    {
                        q = q.Where(p => p.Contact.CategoryIds != null && rq.CategoryIds.Any(c => p.Contact.CategoryIds.Contains(c)));
                    }

                    if (!string.IsNullOrEmpty(rq.City))
                    {
                        q = q.Where(p => p.Contact.Addresses.Any(a => a.City == rq.City));
                    }

                    if (rq.Keyword?.Length > 1)
                    {
                        var keyword = rq.Keyword;

                        if (keyword.IsComplexQueryKeywords())
                        {
                            q = q.QueryEtsooKeywords(keyword, DbUtils.ILikeMethod, p => p.Contact.Name, p => p.Contact.PreferredName);
                        }
                        else
                        {
                            q = q.Where(p => EF.Functions.ILike(p.Contact.Name, $"%{keyword}%")
                            || (p.Contact.QueryKeyword != null && EF.Functions.ILike(p.Contact.QueryKeyword, $"%{keyword}%"))
                            || (p.Contact.PreferredName != null && EF.Functions.ILike(p.Contact.PreferredName, $"%{keyword}%")));
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
        /// Delete
        /// 删除
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> DeleteAsync(long id, CancellationToken cancellationToken = default)
        {
            // Organization id
            var orgId = User.OrganizationInt;

            var relation = await _db.PersonRelations
                .Where(r => r.Id == id && r.Person.OrgId == orgId)
                .Include(r => r.Person)
                .Select(r => new
                {
                    r.Person.IdentityType
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (relation == null)
            {
                return ApplicationErrors.NoId.AsResult();
            }

            if (!await _commonService.HasIdentityPermissionAsync(relation.IdentityType, nameof(Permissions.Customer.AddContact), cancellationToken))
            {
                return ApplicationErrors.AccessDenied.AsResult();
            }

            // Delete
            await _db.PersonRelations.AsNoTracking()
                .Where(r => r.Id == id)
                .ExecuteDeleteAsync(cancellationToken);

            // Return
            return ActionResult.Succeed(id);
        }

        /// <summary>
        /// List
        /// 联系人
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="writer">Writer to hold the data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task ListAsync(ContactListRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default)
        {
            await _commonService.UpdateTagAsync(rq, User.OrganizationInt, cancellationToken);

            await CreateQuery(rq)
                .Select(c => new
                {
                    Id = c.ContactId,
                    c.RelationType,
                    c.Contact.Name
                }).ToJsonAsync(writer, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Query contact JSON data
        /// 查询联系人JSON数据
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="writer">Writer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        public async Task QueryAsync(ContactQueryRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default)
        {
            // Organization id
            var orgId = User.OrganizationInt;

            var person = await _db.Persons
               .AsNoTracking()
               .Where(p => p.Id == rq.PersonId && p.OrgId == orgId)
               .Select(p => new
               {
                   p.Id,
                   p.IdentityType
               })
               .FirstOrDefaultAsync(cancellationToken);

            if (person == null)
            {
                return;
            }

            if (!await _commonService.HasIdentityPermissionAsync(person.IdentityType, nameof(Permissions.Customer.QueryContact), cancellationToken))
            {
                return;
            }

            await _commonService.UpdateTagAsync(rq, orgId, cancellationToken);

            var query = CreateQuery(rq, (q) =>
            {
                if (!string.IsNullOrEmpty(rq.JobTitle))
                {
                    q = q.Where(r => r.Contact.JobTitle != null && EF.Functions.ILike(r.Contact.JobTitle, $"%{rq.JobTitle}%"));
                }

                if (rq.Description?.Length > 1)
                {
                    var description = rq.Description;

                    if (description.IsComplexQueryKeywords())
                    {
                        q = q.QueryEtsooKeywords(description, DbUtils.ILikeMethod, r => r.Description, r => r.Contact.Description);
                    }
                    else
                    {
                        q = q.Where(r => (r.Description != null && EF.Functions.ILike(r.Description, $"%{description}%"))
                        || (r.Contact.Description != null && EF.Functions.ILike(r.Contact.Description, $"%{description}%")));
                    }
                }

                if (!string.IsNullOrEmpty(rq.Info))
                {
                    var info = rq.Info.Trim().ToLower();
                    q = q.Where(r => r.Contact.Infos.Any(i => i.Identifier == info));
                }

                if (!string.IsNullOrEmpty(rq.Address))
                {
                    q = q.Where(r => r.Contact.Addresses.Any(a => EF.Functions.ILike(a.FormattedAddress, $"%{rq.Address}%")));
                }

                return q;
            });

            var (hasContent, commandText) = await query.Select(r => new ContactQueryData
            {
                Id = r.Id,
                ContactId = r.ContactId,
                RelationType = r.RelationType,
                Name = r.Contact.Name,
                Description = r.Description,
                Creation = r.Creation,
                Status = r.Contact.Status
            }).ToJsonAsync(writer, cancellationToken: cancellationToken);

            if (_db.IsSensitiveDataLoggingEnabled && Logger.IsEnabled(LogLevel.Information))
            {
                Logger.LogInformation("QueryContactAsync is {hasContent} with {commandText}", hasContent, commandText);
            }
        }

        /// <summary>
        /// Update contact relation
        /// 更新联系人关系
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> UpdateRelationAsync(ContactRelationUpdateRQ rq, CancellationToken cancellationToken = default)
        {
            // Organization id
            var orgId = User.OrganizationInt;

            var relation = await _db.PersonRelations
                .Where(r => r.Id == rq.Id && r.Person.OrgId == orgId)
                .Include(r => r.Person)
                .Select(r => new PersonRelation
                {
                    Id = r.Id,
                    ContactId = r.ContactId,
                    RelationType = r.RelationType,
                    Description = r.Description,
                    Data = r.Data,
                    Person = new Person
                    {
                        Id = r.Id,
                        IdentityType = r.Person.IdentityType
                    }
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (relation == null)
            {
                return ApplicationErrors.NoId.AsResult();
            }

            if (!await _commonService.HasIdentityPermissionAsync(relation.Person.IdentityType, nameof(Permissions.Customer.AddContact), cancellationToken))
            {
                return ApplicationErrors.AccessDenied.AsResult();
            }

            var data = _db.PersonRelations.Attach(relation);

            if (rq.IsModified(nameof(rq.ContactId)) && rq.ContactId.HasValue && rq.ContactId != relation.ContactId)
            {
                // Check if the contact id exists
                if (await _db.Persons(orgId).AnyAsync(c => c.Id == rq.ContactId, cancellationToken) is false)
                {
                    return ApplicationErrors.NoId.AsResult(nameof(rq.ContactId));
                }

                relation.ContactId = rq.ContactId.Value;
            }

            if (rq.IsModified(nameof(rq.RelationType)) && rq.RelationType.HasValue)
            {
                relation.RelationType = rq.RelationType.Value;
            }

            if (rq.IsModified(nameof(rq.Description)))
            {
                relation.Description = rq.Description;
            }

            if (rq.IsModified(nameof(rq.Data)))
            {
                relation.Data = rq.Data;
            }

            if (rq.IsModified(nameof(rq.IsDefault)))
            {
                relation.IsDefault = rq.IsDefault;

                if (rq.IsDefault is true)
                {
                    await _db.PersonRelations.AsNoTracking()
                        .Where(r => r.PersonId == relation.PersonId && r.RelationType == relation.RelationType && r.IsDefault == true && r.ContactId != relation.Id)
                        .ExecuteUpdateAsync(r => r.SetProperty(r => r.IsDefault, false), cancellationToken);
                }
            }

            // Save
            await _db.SaveChangesAsync(cancellationToken);

            // Return
            return ActionResult.Succeed(rq.Id);
        }

        /// <summary>
        /// Read contact relation data for update
        /// 读取用于更新联系人关系的数据
        /// </summary>
        /// <param name="id">Relation id</param>
        /// <param name="writer">Writer to hold the data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public Task UpdateRelationReadAsync(long id, IBufferWriter<byte> writer, CancellationToken cancellationToken = default)
        {
            var orgId = User.OrganizationInt;

            return _db.PersonRelations.AsNoTracking()
                .Where(r => r.Id == id && r.Person.OrgId == orgId)
                .Select(r => new ContactRelationUpdateReadData
                {
                    Id = r.Id,
                    ContactId = r.ContactId,
                    RelationType = r.RelationType,
                    Description = r.Description,
                    Data = r.Data,
                    Creation = r.Creation
                }).ToJsonObjectAsync(writer, cancellationToken: cancellationToken);
        }
    }
}
