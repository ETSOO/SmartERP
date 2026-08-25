using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Authentication;
using com.etsoo.CoreFramework.Business;
using com.etsoo.CoreFramework.Models;
using com.etsoo.CoreFramework.User;
using com.etsoo.Database;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Serialization;
using CRM.Server.Application;
using CRM.Server.Dto.Person;
using CRM.Server.RQ.Person;
using Microsoft.EntityFrameworkCore;
using PlatformShared;
using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.Person;
using PlatformShared.Database;
using PlatformShared.Database.Models;
using PlatformShared.Dto;
using PlatformShared.Extentions;
using System.Buffers;

namespace CRM.Server.Services
{
    /// <summary>
    /// Person service
    /// 人员服务
    /// </summary>
    public class PersonService : MyUserService, IPersonService
    {
        readonly MyDbContext _db;
        readonly ICommonService _commonService;
        readonly IQueueService _queueService;

        public PersonService(
            MyDbContext db,
            IMyApp app,
            CurrentUserAccessor userAccessor,
            ILogger<PersonService> logger,
            ICommonService commonService,
            IQueueService queueService
        )
            : base(app, userAccessor.UserSafe, "person", logger)
        {
            _db = db;
            _commonService = commonService;
            _queueService = queueService;
        }

        /// <summary>
        /// Choose persons
        /// 选择人员
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<ChoosePersonsData> ChoosePersonsAsync(ChoosePersonsRQ rq, CancellationToken cancellationToken = default)
        {
            // Max items
            var maxItems = rq.MaxItems > 0 ? rq.MaxItems : 20;

            // Users
            var users = await _db.Users(User.OrganizationInt).AsNoTracking()
                .OrderByDescending(p => p.RefreshTime)
                .Take(maxItems)
                .Select(p => new PersonListItem
                {
                    Id = p.Id,
                    Name = p.Name,
                    JobTitle = p.JobTitle
                })
                .ToListAsync(cancellationToken);

            // Contacts
            var contacts = await _db.PersonRelations.AsNoTracking()
                .Where(r => r.PersonId == rq.PersonId && r.Person.OrgId == User.OrganizationInt)
                .OrderByDescending(r => r.Contact.RefreshTime)
                .Take(maxItems)
                .Select(r => new PersonListItem
                {
                    Id = r.Contact.Id,
                    Name = r.Contact.Name,
                    JobTitle = r.Contact.JobTitle
                })
                .ToListAsync(cancellationToken);

            // Return
            return new ChoosePersonsData
            {
                Users = users,
                Contacts = contacts
            };
        }

        private IQueryable<Person> CreateQuery(PersonListRQ rq, IdentityTypeFlags identity, bool all, Func<IQueryable<Person>, IQueryable<Person>>? filters = null)
        {
            var query = _db.Persons(User.OrganizationInt).AsNoTracking()
                .QueryEtsoo(rq, (p) => p.Id, (p) => p.Status, (q) =>
                {
                    if (rq.IdentityType.HasValue)
                    {
                        var value = rq.IdentityType.Value;
                        if (value == IdentityTypeFlags.None)
                            q = q.Where(p => p.IdentityType == IdentityTypeFlags.None);
                        else
                            q = q.Where(p => (p.IdentityType & value) > 0);
                    }
                    else if (!all)
                    {
                        q = q.Where(p => (p.IdentityType & identity) > 0);
                    }

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

                    if (rq.Education.HasValue)
                    {
                        q = q.Where(p => p.Education == rq.Education.Value);
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
                            q = q.QueryEtsooKeywords(keyword, DbUtils.ILikeMethod, p => p.Name, p => p.PreferredName);
                        }
                        else
                        {
                            q = q.Where(p => EF.Functions.ILike(p.Name, $"%{keyword}%")
                            || (p.QueryKeyword != null && EF.Functions.ILike(p.QueryKeyword, $"%{keyword}%"))
                            || (p.PreferredName != null && EF.Functions.ILike(p.PreferredName, $"%{keyword}%")));
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
        /// Is person deletable
        /// 人员是否可删除
        /// </summary>
        /// <param name="id">Person id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<bool> IsDeletableAsync(long id, CancellationToken cancellationToken = default)
        {
            // Organization id
            var orgId = User.OrganizationInt;

            var person = await _db.Persons
               .Where(p => p.Id == id
                    && p.OrgId == orgId
                    && !p.Contacts.Any()
                    && !p.Profiles.Any()
                    && !p.Orders.Any()
                ).Select(p => new
                   {
                       p.IdentityType
                   }
                ).FirstOrDefaultAsync(cancellationToken);

            if (person == null)
            {
                return false;
            }

            if (!await _commonService.HasIdentityPermissionAsync(person.IdentityType, nameof(Permissions.Customer.Delete), cancellationToken))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Delete person
        /// 删除人员
        /// </summary>
        /// <param name="id">Person id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> DeleteAsync(long id, CancellationToken cancellationToken = default)
        {
            var isDeletable = await IsDeletableAsync(id, cancellationToken);
            if (!isDeletable)
            {
                return ApplicationErrors.AccessDenied.AsResult();
            }

            // Read info
            var person = await _db.Persons.AsNoTracking()
                .Where(p => p.Id == id)
                .Select(p => new { p.IdentityType, p.Name, p.ReportTo, p.UserId })
                .FirstOrDefaultAsync(cancellationToken);

            if (person == null)
            {
                return ApplicationErrors.NoId.AsResult();
            }

            await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                // Remove infos
                await _db.PersonInfos.AsNoTracking()
                    .Where(pi => pi.PersonId == id)
                    .ExecuteDeleteAsync(cancellationToken);

                // Remove addresses
                await _db.PersonAddresses.AsNoTracking()
                    .Where(pa => pa.PersonId == id)
                    .ExecuteDeleteAsync(cancellationToken);

                // More safe deletes
                // ...
  
                // Remove
                await _db.Persons.AsNoTracking()
                    .Where(p => p.Id == id)
                    .ExecuteDeleteAsync(cancellationToken);

                // Commit
                await transaction.CommitAsync(cancellationToken);

                // Push message
                var message = new DeletePersonMessage
                {
                    Data = User.CreateMessageData(App.AppId, id, person.Name),
                    IdentityType = person.IdentityType,
                    UserId = person.UserId,
                    ReportTo = person.ReportTo
                };
                await _queueService.PushAsync(message, CrmJsonSerializerContext.Default.DeletePersonMessage, cancellationToken);
            }
            catch (Exception ex)
            {
                // Rollback
                await transaction.RollbackAsync(cancellationToken);

                // Log and return the result
                return LogException(ex);
            }

            return ActionResult.Succeed(id);
        }

        private void FormatListRQ(PersonListRQ rq)
        {
            if (rq.Id == 0)
            {
                rq.Id = User.Oid;
            }
        }

        /// <summary>
        /// Duplicate test
        /// 重复测试
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async ValueTask<PersonDuplicateTestData[]?> DuplicateTestAsync(PersonDuplicateTestRQ rq, CancellationToken cancellationToken = default)
        {
            var orgId = User.OrganizationInt;

            var q = _db.Persons.AsNoTracking().Where(p => p.OrgId == orgId);
            var hasFilter = false;

            if (rq.ExcludedId.HasValue)
            {
                q = q.Where(p => p.Id != rq.ExcludedId.Value);
            }

            if (!string.IsNullOrEmpty(rq.Name))
            {
                q = q.Where(p => p.Name.ToLower() == rq.Name.ToLower());
                hasFilter = true;
            }

            if (!string.IsNullOrEmpty(rq.Identifier))
            {
                var identifier = rq.Identifier.Trim().ToLower();
                q = q.Where(p => p.Infos.Any(i => i.Identifier == identifier && (!rq.InfoKind.HasValue || i.Kind == rq.InfoKind)));
                hasFilter = true;
            }

            if (!string.IsNullOrEmpty(rq.Address))
            {
                q = q.Where(p => p.Addresses.Any(a => a.FormattedAddress.ToLower() == rq.Address.ToLower()));
                hasFilter = true;
            }

            if (!string.IsNullOrEmpty(rq.AssignedId))
            {
                q = q.Where(p => p.AssignedId != null && p.AssignedId == rq.AssignedId.ToUpper());
                hasFilter = true;
            }

            if (!hasFilter) return null;

            var (identityType, all) = await _commonService.GetPersonIdentityTypeAsync(cancellationToken);
            if (identityType == IdentityTypeFlags.None)
            {
                return null;
            }

            return await q.Select(p => new PersonDuplicateTestData
            {
                Id = p.Id,
                Name = MyDbFunctions.HideData(p.Name, default),
                IdentityType = p.IdentityType
            }).ToArrayAsync(cancellationToken);
        }

        /// <summary>
        /// List person JSON data
        /// 人员列表JSON数据
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IEnumerable<ContactItem>> ListAsync(PersonListRQ rq, CancellationToken cancellationToken = default)
        {
            var (identityType, all) = await _commonService.GetPersonIdentityTypeAsync(cancellationToken);
            if (identityType == IdentityTypeFlags.None)
            {
                return [];
            }

            if (rq.IdentityType.HasValue && rq.IdentityType.Value != IdentityTypeFlags.None && (identityType & rq.IdentityType.Value) == 0)
            {
                return [];
            }

            FormatListRQ(rq);

            var query = CreateQuery(rq, identityType, all);

            return await query.Select(p => new ContactItem
            {
                Id = p.Id,
                Name = p.Name,
                IdentityType = p.IdentityType,
                Owner = p.IdentityType == IdentityTypeFlags.None ? p.ContactOwners.Select(o => new IdentityTypeDataBase
                {
                    Name = o.Person.Name,
                    IdentityType = o.Person.IdentityType
                }).FirstOrDefault() : null,
                JobTitle = p.JobTitle,
                PreferredName = p.PreferredName
            }).ToListAsync(cancellationToken);
        }

        /// <summary>
        /// List person JSON data
        /// 人员列表JSON数据
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="writer">Writer to hold the data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task ListAsync(PersonListRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default)
        {
            var orgs = await ListAsync(rq, cancellationToken);
            await writer.SerializeAsync(orgs, PlatformSharedContext.Default.IEnumerableContactItem);
        }

        /// <summary>
        /// Query person JSON data
        /// 查询人员JSON数据
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IEnumerable<PersonQueryData>> QueryAsync(PersonQueryRQ rq, CancellationToken cancellationToken = default)
        {
            var (identityType, all) = await _commonService.GetPersonIdentityTypeAsync(cancellationToken);
            if (identityType == IdentityTypeFlags.None)
            {
                return [];
            }

            if (rq.IdentityType.HasValue && rq.IdentityType.Value != IdentityTypeFlags.None && (identityType & rq.IdentityType.Value) == 0)
            {
                return [];
            }

            await _commonService.UpdateTagAsync(rq, User.OrganizationInt, cancellationToken);

            FormatListRQ(rq);

            var query = CreateQuery(rq, identityType, all, (q) =>
            {
                if (!string.IsNullOrEmpty(rq.AssignedId))
                {
                    q = q.Where(p => p.AssignedId != null && EF.Functions.ILike(p.AssignedId, $"{rq.AssignedId}%"));
                }

                if (!string.IsNullOrEmpty(rq.JobTitle))
                {
                    q = q.Where(p => p.JobTitle != null && EF.Functions.ILike(p.JobTitle, $"%{rq.JobTitle}%"));
                }

                if (!string.IsNullOrEmpty(rq.Description))
                {
                    q = q.Where(p => p.Description != null && EF.Functions.ILike(p.Description, $"%{rq.Description}%"));
                }

                if (!string.IsNullOrEmpty(rq.Info))
                {
                    var info = rq.Info.Trim().ToLower();
                    q = q.Where(p => p.Infos.Any(i => i.Identifier == info));
                }

                if (!string.IsNullOrEmpty(rq.Address))
                {
                    q = q.Where(p => p.Addresses.Any(a => EF.Functions.ILike(a.FormattedAddress, $"%{rq.Address}%")));
                }

                return q;
            });

            return await query.Select(p => new PersonQueryData
            {
                Id = p.Id,
                Name = p.Name,
                PreferredName = p.PreferredName,
                AssignedId = p.AssignedId,
                IdentityType = p.IdentityType,
                Owner = p.IdentityType == IdentityTypeFlags.None ? p.ContactOwners.Select(o => new IdentityTypeDataBase
                {
                    Name = o.Person.Name,
                    IdentityType = o.Person.IdentityType
                }).FirstOrDefault() : null,
                JobTitle = p.JobTitle,
                Status = p.Status,
                Creation = p.Creation
            }).ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Query person JSON data
        /// 查询人员JSON数据
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="writer">Writer to hold the data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task QueryAsync(PersonQueryRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default)
        {
            var persons = await QueryAsync(rq, cancellationToken);
            await writer.SerializeAsync(persons, MyJsonSerializerContext.Default.IEnumerablePersonQueryData);
        }

        /// <summary>
        /// Read person data for view
        /// 读取用于浏览的人员数据
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<PersonViewData?> ReadAsync(long id, CancellationToken cancellationToken = default)
        {
            var (identityType, all) = await _commonService.GetPersonIdentityTypeAsync(cancellationToken);
            if (identityType == IdentityTypeFlags.None)
            {
                return null;
            }

            var orgId = User.OrganizationInt;
            var userId = User.IdInt;
            var isPrivate = User.Role >= UserRole.Manager;

            if (id == 0)
            {
                // Current user
                id = User.Oid;
            }

            var data = await _db.Persons.AsNoTracking()
                .Include(p => p.ContactOwners).ThenInclude(o => o.Person)
                .Where(p => p.OrgId == orgId && p.Id == id && (all || (p.IdentityType & identityType) > 0))
                .Select(p => new PersonViewData
                {
                    Id = p.Id,
                    Uid = p.Uid,
                    IdentityType = p.IdentityType,
                    Owner = p.IdentityType == IdentityTypeFlags.None ? p.ContactOwners.Select(o => new IdentityTypeDataBase
                    {
                        Name = o.Person.Name,
                        IdentityType = o.Person.IdentityType
                    }).FirstOrDefault() : null,
                    IsLegalPerson = p.IsLegalPerson,
                    Name = p.Name,
                    GivenName = p.GivenName,
                    FamilyName = p.FamilyName,
                    LatinGivenName = p.LatinGivenName,
                    LatinFamilyName = p.LatinFamilyName,
                    PreferredName = p.PreferredName,
                    Title = p.Title,
                    Description = p.Description,
                    Avatar = p.Avatar ?? (p.CoreUser == null ? (p.CoreOrganization == null ? null : p.CoreOrganization.Logo) : p.CoreUser.Avatar),
                    JobTitle = p.JobTitle,
                    AssignedId = p.AssignedId,
                    Categories = p.CategoryIds == null ? null : _db.PersonCategories.Where(c => c.CoreOrganizationId == orgId && p.CategoryIds.Contains(c.Id)).OrderBy(t => p.CategoryIds.IndexOf(t.Id)).Select(c => new CategoryItem { Id = c.Id, Names = c.Names }).ToList(),
                    Tags = p.Tags == null ? null : _db.FeatureTags.Where(k => k.CoreOrganizationId == orgId && p.Tags.Contains(k.Id)).OrderByDescending(t => t.Total).ThenBy(t => t.Tag).Select(k => k.Tag).ToList(),
                    Addresses = p.Addresses.Where(a => a.ParentId == null).Select(a => new AddressItem { Id = a.Id, Kind = a.Kind, Name = a.Name, FormattedAddress = a.FormattedAddress }).Take(3),
                    ReportTo = p.ReportTo,
                    ReportToName = p.ReportToUser == null ? null : p.ReportToUser.Name,
                    Creation = p.Creation,
                    Status = p.Status,
                    QueryKeyword = p.QueryKeyword,
                    Regions = p.Regions,
                    Currencies = p.Currencies,
                    Cultures = p.Cultures,

                    // Groups
                    ContactOwners = p.ContactOwners.Select(o => new LongIdItem
                    {
                        Id = o.PersonId,
                        Title = o.Person.Name
                    }),

                    /** Private **/
                    PrivateData = isPrivate || p.CoreUserId == userId || p.ReportTo == userId || p.UserId == userId ? new PersonPrivateData
                    {
                        Gender = p.Gender,
                        Birthday = p.Birthday,
                        Ethnicity = p.Ethnicity,
                        Height = p.Height,
                        Weight = p.Weight,
                        MaritalStatus = p.MaritalStatus,
                        Education = p.Education,
                        Degree = p.Degree,
                        PoliticalStatus = p.PoliticalStatus,
                    } : null,

                    /** Extention **/
                    Data = p.Data,

                    /** User **/
                    UserRole = p.UserRole,
                    Expiry = p.Expiry,
                    InviterName = p.Inviter == null ? null : p.Inviter.Name,
                    RefreshTime = p.RefreshTime

                }).FirstAsync(cancellationToken);

            // Push message
            var message = new ReadPersonMessage
            {
                Data = User.CreateMessageData(App.AppId, id, data.Name),
                IdentityType = data.IdentityType
            };
            await _queueService.PushAsync(message, CrmJsonSerializerContext.Default.ReadPersonMessage, cancellationToken);

            return data;
        }

        /// <summary>
        /// Update
        /// 更新
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> UpdateAsync(PersonUpdateRQ rq, CancellationToken cancellationToken = default)
        {
            // Organization id
            var orgId = User.OrganizationInt;

            var person = await _db.Persons
               .Where(p => p.Id == rq.Id && p.OrgId == orgId)
               .FirstOrDefaultAsync(cancellationToken);

            if (person == null)
            {
                return ApplicationErrors.NoId.AsResult();
            }

            if (!await _commonService.HasIdentityPermissionAsync(person.IdentityType, nameof(Permissions.Customer.Edit), cancellationToken))
            {
                return ApplicationErrors.AccessDenied.AsResult();
            }

            var userId = User.IdInt;
            var isPrivate = User.Role >= UserRole.Manager || person.CoreUserId == userId || person.ReportTo == userId || person.UserId == userId;

            if (rq.ReportTo.HasValue && !await _db.Users(orgId).AnyAsync(u => u.Id == rq.ReportTo.Value, cancellationToken))
            {
                return ApplicationErrors.NoId.AsResult(nameof(rq.ReportTo));
            }

            if (rq.IsModified(nameof(rq.IdentityType)) && rq.IdentityType.HasValue)
            {
                if (!await _commonService.HasIdentityPermissionAsync(person.IdentityType, nameof(Permissions.Customer.Edit), cancellationToken))
                {
                    return ApplicationErrors.AccessDenied.AsResult(nameof(rq.IdentityType));
                }

                person.IdentityType = rq.IdentityType.Value;
            }

            if (rq.IsModified(nameof(rq.IsLegalPerson)) && rq.IsLegalPerson.HasValue)
            {
                person.IsLegalPerson = rq.IsLegalPerson.Value;
            }

            if (rq.IsModified(nameof(rq.Name)) && !string.IsNullOrEmpty(rq.Name))
            {
                person.Name = rq.Name;
            }

            if (rq.IsModified(nameof(rq.GivenName)))
            {
                person.GivenName = rq.GivenName;
            }

            if (rq.IsModified(nameof(rq.FamilyName)))
            {
                person.FamilyName = rq.FamilyName;
            }

            if (rq.IsModified(nameof(rq.LatinGivenName)))
            {
                person.LatinGivenName = rq.LatinGivenName;
            }

            if (rq.IsModified(nameof(rq.LatinFamilyName)))
            {
                person.LatinFamilyName = rq.LatinFamilyName;
            }

            if (rq.IsModified(nameof(rq.PreferredName)))
            {
                person.PreferredName = rq.PreferredName;
            }

            if (rq.IsModified(nameof(rq.JobTitle)))
            {
                person.JobTitle = rq.JobTitle;
            }

            if (rq.IsModified(nameof(rq.Title)))
            {
                person.Title = rq.Title;
            }

            if (rq.IsModified(nameof(rq.Description)))
            {
                person.Description = rq.Description;
            }

            if (rq.IsModified(nameof(rq.AssignedId)))
            {
                person.AssignedId = rq.AssignedId?.ToUpper();
            }

            if (rq.IsModified(nameof(rq.QueryKeyword)))
            {
                person.AssignedId = rq.QueryKeyword?.ToUpper();
            }

            if (rq.IsModified(nameof(rq.Categories)))
            {
                var (result, ids) = await _commonService.ValidatePersonCategoriesAsync(rq.Categories, orgId, cancellationToken);
                if (!result.Ok)
                {
                    return result;
                }

                person.CategoryIds = rq.Categories?.ToList();
                person.CategoryIdsAll = ids?.ToList();
            }

            if (rq.IsModified(nameof(rq.Tags)))
            {
                if (rq.Tags?.Any() is true)
                {
                    var tagKind = _commonService.GetTagKind(person.IdentityType);
                    var tagIds = await _commonService.AddTagsAsync(tagKind, rq.Tags, cancellationToken);
                    person.Tags = [.. tagIds];
                }
                else
                {
                    person.Tags = null;
                }
            }

            if (rq.IsModified(nameof(rq.ReportTo)))
            {
                person.ReportTo = rq.ReportTo;
            }

            if (rq.IsModified(nameof(rq.Regions)))
            {
                person.Regions = rq.Regions?.ToList();
            }

            if (rq.IsModified(nameof(rq.Currencies)))
            {
                person.Currencies = rq.Currencies?.ToList();
            }

            if (rq.IsModified(nameof(rq.Cultures)))
            {
                person.Cultures = rq.Cultures?.ToList();
            }

            if (rq.IsModified(nameof(rq.Expiry)))
            {
                person.Expiry = rq.Expiry?.ToUniversalTime();
            }

            if (rq.IsModified(nameof(rq.Status)) && rq.Status.HasValue)
            {
                person.Status = rq.Status.Value;
            }

            if (rq.IsModified(nameof(rq.Data)))
            {
                person.Data = rq.Data;
            }

            if (isPrivate && rq.PrivateData != null)
            {
                // Private data
                // If the user is not allowed to modify private data, skip these fields

                if (rq.PrivateData.IsModified(nameof(rq.PrivateData.Gender)))
                {
                    person.Gender = rq.PrivateData.Gender?.ToUpper();
                }

                if (rq.PrivateData.IsModified(nameof(rq.PrivateData.Birthday)))
                {
                    person.Birthday = rq.PrivateData.Birthday?.ToUniversalTime();
                }

                if (rq.PrivateData.IsModified(nameof(rq.PrivateData.Ethnicity)))
                {
                    person.Ethnicity = rq.PrivateData.Ethnicity;
                }

                if (rq.PrivateData.IsModified(nameof(rq.PrivateData.Height)))
                {
                    person.Height = rq.PrivateData.Height;
                }

                if (rq.PrivateData.IsModified(nameof(rq.PrivateData.Weight)))
                {
                    person.Weight = rq.PrivateData.Weight;
                }

                if (rq.PrivateData.IsModified(nameof(rq.PrivateData.MaritalStatus)))
                {
                    person.MaritalStatus = rq.PrivateData.MaritalStatus;
                }

                if (rq.PrivateData.IsModified(nameof(rq.PrivateData.Education)))
                {
                    person.Education = rq.PrivateData.Education;
                }

                if (rq.PrivateData.IsModified(nameof(rq.PrivateData.Degree)))
                {
                    person.Degree = rq.PrivateData.Degree;
                }

                if (rq.PrivateData.IsModified(nameof(rq.PrivateData.PoliticalStatus)))
                {
                    person.PoliticalStatus = rq.PrivateData.PoliticalStatus;
                }
            }

            // Changes
            var changes = _db.ChangeTracker.Entries().GetChangedProperties();

            // Save
            var task1 = _db.SaveChangesAsync(cancellationToken);

            // Push message
            var message = new UpdatePersonMessage
            {
                Data = User.CreateMessageData(App.AppId, rq.Id, person.Name),
                Changes = changes
            };
            var task2 = _queueService.PushAsync(message, CrmJsonSerializerContext.Default.UpdatePersonMessage, cancellationToken);

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
        public async Task<PersonUpdateReadData?> UpdateReadAsync(long id, CancellationToken cancellationToken = default)
        {
            var (identityType, all) = await _commonService.GetPersonIdentityTypeAsync(cancellationToken);
            if (identityType == IdentityTypeFlags.None)
            {
                return null;
            }

            var orgId = User.OrganizationInt;
            var userId = User.IdInt;
            var isPrivate = User.Role >= UserRole.Manager;

            return await _db.Persons.AsNoTracking()
                .Include(p => p.ContactOwners).ThenInclude(o => o.Person)
                .Where(p => p.OrgId == orgId && p.Id == id && (p.IdentityType & identityType) > 0)
                .Select(p => new PersonUpdateReadData
                {
                    Id = p.Id,
                    IdentityType = p.IdentityType,
                    IsLegalPerson = p.IsLegalPerson,
                    Name = p.Name,
                    GivenName = p.GivenName,
                    FamilyName = p.FamilyName,
                    LatinGivenName = p.LatinGivenName,
                    LatinFamilyName = p.LatinFamilyName,
                    PreferredName = p.PreferredName,
                    Title = p.Title,
                    Description = p.Description,
                    JobTitle = p.JobTitle,
                    AssignedId = p.AssignedId,
                    Categories = p.CategoryIds,
                    Tags = p.Tags == null ? null : _db.FeatureTags.Where(k => k.CoreOrganizationId == orgId && p.Tags.Contains(k.Id)).OrderByDescending(t => t.Total).ThenBy(t => t.Tag).Select(k => k.Tag).ToList(),
                    ReportTo = p.ReportTo,
                    QueryKeyword = p.QueryKeyword,
                    Regions = p.Regions,
                    Currencies = p.Currencies,
                    Cultures = p.Cultures,
                    Data = p.Data,
                    Expiry = p.Expiry,
                    Status = p.Status,

                    /** Private **/
                    PrivateData = isPrivate || p.CoreUserId == userId || p.ReportTo == userId || p.UserId == userId ? new PersonPrivateData
                    {
                        Gender = p.Gender,
                        Birthday = p.Birthday,
                        Ethnicity = p.Ethnicity,
                        Height = p.Height,
                        Weight = p.Weight,
                        MaritalStatus = p.MaritalStatus,
                        Education = p.Education,
                        Degree = p.Degree,
                        PoliticalStatus = p.PoliticalStatus,
                    } : null
                }).FirstOrDefaultAsync(cancellationToken);
        }
    }
}
