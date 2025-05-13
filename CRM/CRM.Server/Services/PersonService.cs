using com.etsoo.CoreFramework.Authentication;
using com.etsoo.CoreFramework.Business;
using com.etsoo.CoreFramework.Models;
using com.etsoo.CoreFramework.User;
using com.etsoo.Database;
using com.etsoo.ServiceApp.SmartERP;
using com.etsoo.Utils.Serialization;
using CRM.Server.Dto.Person;
using CRM.Server.RQ.Person;
using Microsoft.EntityFrameworkCore;
using PlatformShared;
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
    public class PersonService : SEUserService, IPersonService
    {
        readonly MyDbContext _db;
        readonly ICommonService _commonService;

        public PersonService(
            MyDbContext db,
            ISEServiceApp app,
            CurrentUserAccessor userAccessor,
            ILogger<PersonService> logger,
            ICommonService commonService
        )
            : base(app, userAccessor.UserSafe, "person", logger)
        {
            _db = db;
            _commonService = commonService;
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
            var users = await _db.Persons.AsNoTracking().Users(User.OrganizationInt)
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

        private IQueryable<Person> CreateQuery(PersonListRQ rq, Func<IQueryable<Person>, IQueryable<Person>>? filters = null)
        {
            var query = _db.Persons.AsNoTracking()
                .UserPersons(User)
                .QueryEtsoo(rq, (p) => p.Id, (p) => p.Status, (q) =>
                {
                    if (rq.IdentityType.HasValue)
                    {
                        q = q.Where(p => (p.IdentityType & rq.IdentityType.Value) > 0);
                    }

                    if (!string.IsNullOrEmpty(rq.JobTitle))
                    {
                        q = q.Where(p => p.JobTitle != null && EF.Functions.ILike(p.JobTitle, $"%{rq.JobTitle}%"));
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
                            || (p.PreferredName != null && EF.Functions.ILike(p.PreferredName, $"%{keyword}%"))
                            || (p.AssignedId != null && EF.Functions.ILike(p.AssignedId, $"%{keyword}%")));
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

        private void FormatListRQ(PersonListRQ rq)
        {
            if (rq.Id == 0)
            {
                rq.Id = User.Oid;
            }
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
            var identityType = await _commonService.GetPersonIdentityTypeAsync(cancellationToken);
            if (identityType == IdentityTypeFlags.None)
            {
                return [];
            }

            rq.IdentityType = _commonService.MergeIdentityType(rq.IdentityType, identityType);
            if (rq.IdentityType == IdentityTypeFlags.None)
            {
                return [];
            }

            FormatListRQ(rq);

            var query = CreateQuery(rq);

            return await query.Select(p => new ContactItem
            {
                Id = p.Id,
                Name = p.Name,
                IdentityType = p.IdentityType,
                Owner = p.IdentityType.HasFlag(IdentityTypeFlags.Contact) ? p.ContactOwners.Select(o => new IdentityTypeDataBase
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
            var identityType = await _commonService.GetPersonIdentityTypeAsync(cancellationToken);
            if (identityType == IdentityTypeFlags.None)
            {
                return [];
            }

            rq.IdentityType = _commonService.MergeIdentityType(rq.IdentityType, identityType);
            if (rq.IdentityType == IdentityTypeFlags.None)
            {
                return [];
            }

            FormatListRQ(rq);

            var query = CreateQuery(rq, (q) =>
            {
                return q;
            });

            return await query.Select(p => new PersonQueryData
            {
                Id = p.Id,
                Name = p.Name,
                PreferredName = p.PreferredName,
                AssignedId = p.AssignedId,
                IdentityType = p.IdentityType,
                Owner = p.IdentityType.HasFlag(IdentityTypeFlags.Contact) ? p.ContactOwners.Select(o => new IdentityTypeDataBase
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
            var identityType = await _commonService.GetPersonIdentityTypeAsync(cancellationToken);
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

            return await _db.Persons.AsNoTracking()
                .Where(p => p.OrgId == orgId && p.Id == id && (p.IdentityType & identityType) > 0)
                .Select(p => new PersonViewData
                {
                    Id = p.Id,
                    Uid = p.Uid,
                    IdentityType = p.IdentityType,
                    Owner = p.IdentityType.HasFlag(IdentityTypeFlags.Contact) ? p.ContactOwners.Select(o => new IdentityTypeDataBase
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
                    Avatar = p.Avatar ?? (p.CoreUser == null ? null : p.CoreUser.Avatar),
                    JobTitle = p.JobTitle,
                    AssignedId = p.AssignedId,
                    Categories = p.CategoryIds == null ? null : _db.PersonCategories.Where(c => p.CoreOrganizationId == orgId && p.CategoryIds.Contains(c.Id)).Select(c => new CategoryItem { Id = c.Id, Names = c.Names }).ToList(),
                    Keywords = p.Keywords == null ? null : _db.FeatureKeywords.Where(k => p.CoreOrganizationId == orgId && p.Keywords.Contains(k.Id)).Select(k => k.Tag).ToList(),
                    Addresses = p.Addresses == null ? null : _db.Addresses.Where(a => p.CoreOrganizationId == orgId && p.Addresses.Contains(a.Id)).Select(a => new AddressItem { Id = a.Id, Kind = a.Kind, Name = a.Name, FormattedAddress = a.FormattedAddress }).ToList(),
                    ReportTo = p.ReportTo,
                    ReportToName = p.ReportToUser == null ? null : p.ReportToUser.Name,
                    Creation = p.Creation,
                    Status = p.Status,
                    QueryKeyword = p.QueryKeyword,
                    Regions = p.Regions,
                    Currencies = p.Currencies,
                    Cultures = p.Cultures,

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

                }).FirstOrDefaultAsync(cancellationToken);
        }
    }
}
