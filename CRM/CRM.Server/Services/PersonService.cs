using com.etsoo.CoreFramework.Authentication;
using com.etsoo.CoreFramework.Models;
using com.etsoo.CoreFramework.User;
using com.etsoo.Database;
using com.etsoo.ServiceApp.SmartERP;
using CRM.Server.Dto.Person;
using CRM.Server.RQ.Person;
using Microsoft.EntityFrameworkCore;
using PlatformShared.Database;
using PlatformShared.Database.Models;
using PlatformShared.Dto;
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

        public PersonService(
            MyDbContext db,
            ISEServiceApp app,
            CurrentUserAccessor userAccessor,
            ILogger<PersonService> logger
        )
            : base(app, userAccessor.UserSafe, "person", logger)
        {
            _db = db;
        }

        private IQueryable<Person> CreateQuery(PersonListRQ rq, Func<IQueryable<Person>, IQueryable<Person>>? filters = null)
        {
            var query = _db.Persons.AsNoTracking()
                .Where(p => p.OrgId == User.OrganizationInt)
                .QueryEtsoo(rq, (p) => p.Id, (p) => p.Status, (q) =>
                {
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
            var query = CreateQuery(rq);

            await query.Select(p => new ContactItem
            {
                Id = p.Id,
                Name = p.Name,
                IdentityType = p.IdentityType,
                PreferredName = p.PreferredName,
                IsOrg = p.OrgId == p.CoreOrganizationId
            }).ToJsonAsync(writer, cancellationToken: cancellationToken);
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
            var query = CreateQuery(rq, (q) =>
            {
                return q;
            });

            var (hasContent, commandText) = await query.Select(p => new PersonQueryData
            {
                Id = p.Id,
                Name = p.Name,
                PreferredName = p.PreferredName,
                AssignedId = p.AssignedId,
                IdentityType = p.IdentityType,
                JobTitle = p.JobTitle,
                IsOrg = p.OrgId == p.CoreOrganizationId,
                Status = p.Status,
                Creation = p.Creation
            }).ToJsonAsync(writer, cancellationToken: cancellationToken);

            if (_db.IsSensitiveDataLoggingEnabled)
            {
                Logger.LogInformation("QueryAsync is {hasContent} with {commandText}", hasContent, commandText);
            }
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
            var orgId = User.OrganizationInt;
            var userId = User.IdInt;
            var isPrivate = User.Role >= UserRole.Manager;

            return await _db.Persons.AsNoTracking()
                .Where(p => p.Id == id && p.OrgId == orgId)
                .Select(p => new PersonViewData
                {
                    Id = p.Id,
                    Uid = p.Uid,
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
                    IsOrg = p.OrgId == p.CoreOrganizationId,
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
