using com.etsoo.ApiModel.Dto.Maps;
using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Models;
using com.etsoo.CoreFramework.User;
using com.etsoo.Database;
using com.etsoo.ServiceApp.SmartERP;
using com.etsoo.Utils.Actions;
using CRM.Server.Dto.PersonAddress;
using CRM.Server.RQ;
using CRM.Server.RQ.PersonAddress;
using Microsoft.EntityFrameworkCore;
using NpgsqlTypes;
using PlatformShared.Database;
using PlatformShared.Database.Models;
using PlatformShared.Extentions;
using System.Buffers;

namespace CRM.Server.Services
{
    /// <summary>
    /// Person Address Service
    /// 人员地址服务
    /// </summary>
    public class PersonAddressService : SEUserService, IPersonAddressService
    {
        readonly MyDbContext _db;
        readonly ICommonService _commonService;

        public PersonAddressService(
            MyDbContext db,
            ISEServiceApp app,
            CurrentUserAccessor userAccessor,
            ILogger<PersonAddressService> logger,
            ICommonService commonService
        )
            : base(app, userAccessor.UserSafe, "person_address", logger)
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
        public async Task<IActionResult> CreateAsync(AddressCreateRQ rq, CancellationToken cancellationToken = default)
        {
            // Organization id
            var orgId = User.OrganizationInt;

            var person = await _db.Persons.AsNoTracking()
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

            if (!await _commonService.HasIdentityPermissionAsync(person.IdentityType, nameof(Permissions.Customer.Edit), cancellationToken))
            {
                return ApplicationErrors.AccessDenied.AsResult();
            }

            // Validate parent id
            if (rq.ParentId.HasValue)
            {
                var parentExists = await _db.PersonAddresses.AsNoTracking()
                    .Where(a => a.Id == rq.ParentId && a.PersonId == person.Id && a.Kind != AddressKind.Location && a.ParentId == null)
                    .AnyAsync(cancellationToken);
                if (!parentExists)
                {
                    return ApplicationErrors.NoId.AsResult(nameof(rq.ParentId));
                }
            }

            // Find possible existing address
            var addressId = await _db.PersonAddresses.AsNoTracking()
                .Where(a => a.PersonId == person.Id && a.City == rq.City && a.FormattedAddress == rq.FormattedAddress)
                .Select(a => a.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (addressId < 1)
            {
                // New address
                var addr = rq.CreateAddressFromRQ(person.Id);

                // Add
                _db.PersonAddresses.Add(addr);

                // Save
                await _db.SaveChangesAsync(cancellationToken);

                // Get the id
                addressId = addr.Id;
            }

            // Return
            return ActionResult.Succeed(addressId);
        }

        /// <summary>
        /// Create address location
        /// 创建地址位置
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> CreateLocationAsync(AddressLocationCreateRQ rq, CancellationToken cancellationToken = default)
        {
            // Organization id
            var orgId = User.OrganizationInt;

            var addr = await _db.PersonAddresses
               .Where(p => p.Id == rq.ParentId && p.Person.OrgId == orgId && p.ParentId == null)
               .Select(p => new
               {
                   p.Id,
                   p.PersonId,
                   p.Region,
                   p.State,
                   p.City,
                   p.District,
                   p.Route,
                   p.Street,
                   p.PostalCode,
                   p.FormattedAddress,
                   p.Provider,
                   p.Person.IdentityType
               })
               .FirstOrDefaultAsync(cancellationToken);

            if (addr == null)
            {
                return ApplicationErrors.NoId.AsResult(nameof(rq.ParentId));
            }

            if (!await _commonService.HasIdentityPermissionAsync(addr.IdentityType, nameof(Permissions.Customer.Edit), cancellationToken))
            {
                return ApplicationErrors.AccessDenied.AsResult();
            }

            var doesExist = await _db.PersonAddresses
                .Where(p => p.PersonId == addr.PersonId && (p.Name == rq.Name || (p.PlaceId != null && p.PlaceId == rq.PlaceId)))
                .AnyAsync(cancellationToken);

            if (doesExist)
            {
                return ApplicationErrors.ItemExists.AsResult(nameof(rq.Name));
            }

            var newAddr = new PersonAddress
            {
                PersonId = addr.PersonId,
                Kind = AddressKind.Location,
                Name = rq.Name,
                PlaceId = rq.PlaceId,
                Region = addr.Region,
                State = addr.State,
                City = addr.City,
                District = addr.District,
                Route = addr.Route,
                Street = addr.Street,
                PostalCode = addr.PostalCode,
                FormattedAddress = addr.FormattedAddress,
                Location = null,
                Provider = addr.Provider,
                ParentId = addr.Id
            };

            // Add
            _db.PersonAddresses.Add(newAddr);

            // Save
            await _db.SaveChangesAsync(cancellationToken);

            return ActionResult.Succeed(newAddr.Id);
        }

        /// <summary>
        /// Delete address
        /// 删除地址
        /// </summary>
        /// <param name="id">Address id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            // Organization id
            var orgId = User.OrganizationInt;

            var addr = await _db.PersonAddresses
               .Where(a => a.Id == id && a.Person.OrgId == orgId)
               .Select(a => new { a.Person.IdentityType })
               .FirstOrDefaultAsync(cancellationToken);

            if (addr == null)
            {
                return ApplicationErrors.NoId.AsResult();
            }

            if (!await _commonService.HasIdentityPermissionAsync(addr.IdentityType, nameof(Permissions.Customer.Edit), cancellationToken))
            {
                return ApplicationErrors.AccessDenied.AsResult();
            }

            var result = await _db.PersonAddresses.AsNoTracking()
                .Where(p => p.Id == id)
                .ExecuteDeleteAsync(cancellationToken);

            if (result == 0)
            {
                return ApplicationErrors.NoId.AsResult();
            }
            else
            {
                return ActionResult.Succeed(id);
            }
        }

        private IQueryable<PersonAddress> CreateQuery(long[] personIds, AddressListRQ rq, Func<IQueryable<PersonAddress>, IQueryable<PersonAddress>>? filters = null)
        {
            var orgId = User.OrganizationInt;

            var query = _db.PersonAddresses.AsNoTracking()
                .Where(q => personIds.Contains(q.PersonId) && q.Person.OrgId == orgId)
                .QueryEtsoo(rq, (a) => a.Id, null, (q) =>
                {
                    if (rq.Kind.HasValue)
                    {
                        q = q.Where(a => a.Kind == rq.Kind);
                    }

                    if (rq.ParentId.HasValue)
                    {
                        q = q.Where(a => a.ParentId == rq.ParentId);
                    }

                    if (rq.IsLocation.HasValue)
                    {
                        if (rq.IsLocation.Value)
                        {
                            q = q.Where(a => a.Kind == AddressKind.Location && a.ParentId != null);
                        }
                        else
                        {
                            q = q.Where(a => a.Kind != AddressKind.Location && a.ParentId == null);
                        }
                    }

                    if (!string.IsNullOrEmpty(rq.PlaceId))
                    {
                        q = q.Where(a => a.PlaceId == rq.PlaceId);
                    }

                    if (rq.Keyword?.Length > 1)
                    {
                        var keyword = rq.Keyword;
                        q = q.Where(a => EF.Functions.ILike(a.Name, $"%{keyword}%") || EF.Functions.ILike(a.FormattedAddress, $"%{keyword}%"));
                    }

                    if (filters != null)
                    {
                        q = filters(q);
                    }

                    return q;
                });

            return query;
        }

        private async ValueTask<long[]> FormatRQAsync(AddressListRQ rq, CancellationToken cancellationToken)
        {
            List<long> ids = [rq.PersonId];

            if (rq.IncludeOwner.GetValueOrDefault())
            {
                ids.Add(User.Pid);

                var orgId = User.OrganizationInt;

                var ownerIds = await _db.Persons(orgId).AsNoTracking()
                    .Where(p => p.Id == rq.PersonId)
                    .SelectMany(p => p.ContactOwners.Select(o => o.PersonId)).ToArrayAsync(cancellationToken);

                ids.AddRange(ownerIds);
            }

            return [.. ids.Distinct()];
        }

        /// <summary>
        /// List
        /// 列表
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="writer">Writer to hold the data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task ListAsync(AddressListRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default)
        {
            var personIds = await FormatRQAsync(rq, cancellationToken);

            await CreateQuery(personIds, rq)
                .OrderBy(c => c.Name)
                .Select(c => new AddressListData
                {
                    Id = c.Id,
                    Kind = c.Kind,
                    Name = c.Name,
                    City = c.City,
                    ParentName = c.Parent != null ? c.Parent.Name : null
                }).ToJsonAsync(writer, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Query
        /// 查询
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<AddressQueryData[]> QueryAsync(AddressListRQ rq, CancellationToken cancellationToken = default)
        {
            var personIds = await FormatRQAsync(rq, cancellationToken);

            return await CreateQuery(personIds, rq)
                .Select(c => new AddressQueryData
                {
                    Id = c.Id,
                    Kind = c.Kind,
                    Name = c.Name,
                    FormattedAddress = c.FormattedAddress,
                    ParentName = c.Parent != null ? c.Parent.Name : null,
                    Creation = c.Creation
                }).ToArrayAsync(cancellationToken);
        }

        /// <summary>
        /// Update
        /// 更新
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> UpdateAsync(AddressUpdateRQ rq, CancellationToken cancellationToken = default)
        {
            // Organization id
            var orgId = User.OrganizationInt;

            var addr = await _db.PersonAddresses
                .Where(a => a.Id == rq.Id && a.Person.OrgId == orgId)
                .Include(a => a.Person)
                .Select(a => new PersonAddress
                {
                    Id = a.Id,
                    PersonId = a.PersonId,
                    Kind = a.Kind,
                    Name = a.Name,
                    PlaceId = a.PlaceId,
                    Region = a.Region,
                    State = a.State,
                    City = a.City,
                    District = a.District,
                    Route = a.Route,
                    Street = a.Street,
                    PostalCode = a.PostalCode,
                    FormattedAddress = a.FormattedAddress,
                    Location = a.Location,
                    Provider = a.Provider,
                    ParentId = a.ParentId,
                    Person = new Person
                    {
                        Id = a.Person.Id,
                        IdentityType = a.Person.IdentityType
                    }
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (addr == null)
            {
                return ApplicationErrors.NoId.AsResult();
            }

            if (!await _commonService.HasIdentityPermissionAsync(addr.Person.IdentityType, nameof(Permissions.Customer.Edit), cancellationToken))
            {
                return ApplicationErrors.AccessDenied.AsResult();
            }

            // Validate parent id
            if (rq.ParentId.HasValue)
            {
                var parentExists = await _db.PersonAddresses.AsNoTracking()
                    .Where(a => a.Id == rq.ParentId && a.PersonId == addr.PersonId && a.Kind != AddressKind.Location && a.ParentId == null)
                    .AnyAsync(cancellationToken);
                if (!parentExists)
                {
                    return ApplicationErrors.NoId.AsResult(nameof(rq.ParentId));
                }
            }

            _db.PersonAddresses.Attach(addr);

            if (rq.IsModified(nameof(rq.PersonId)) && rq.PersonId.HasValue && rq.PersonId != addr.PersonId)
            {
                // Check if the person exists
                var person = await _db.Persons
                    .Where(p => p.Id == rq.PersonId && p.OrgId == orgId)
                    .Select(p => new Person
                    {
                        Id = p.Id,
                        IdentityType = p.IdentityType
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                if (person == null)
                {
                    return ApplicationErrors.NoId.AsResult(nameof(rq.PersonId));
                }

                if (!await _commonService.HasIdentityPermissionAsync(person.IdentityType, nameof(Permissions.Customer.Edit), cancellationToken))
                {
                    return ApplicationErrors.AccessDenied.AsResult(nameof(rq.PersonId));
                }

                addr.PersonId = person.Id;
            }

            if (rq.IsModified(nameof(rq.Kind)) && rq.Kind.HasValue)
            {
                addr.Kind = rq.Kind.Value;
            }

            if (rq.IsModified(nameof(rq.Name)) && !string.IsNullOrEmpty(rq.Name))
            {
                addr.Name = rq.Name;
            }

            if (rq.IsModified(nameof(rq.PlaceId)))
            {
                addr.PlaceId = rq.PlaceId;
            }

            if (rq.IsModified(nameof(rq.Region)) && !string.IsNullOrEmpty(rq.Region))
            {
                addr.Region = rq.Region;
            }

            if (rq.IsModified(nameof(rq.State)) && !string.IsNullOrEmpty(rq.State))
            {
                addr.State = rq.State;
            }

            if (rq.IsModified(nameof(rq.City)) && !string.IsNullOrEmpty(rq.City))
            {
                addr.City = rq.City;
            }

            if (rq.IsModified(nameof(rq.District)))
            {
                addr.District = rq.District;
            }

            if (rq.IsModified(nameof(rq.Route)))
            {
                addr.Route = rq.Route;
            }

            if (rq.IsModified(nameof(rq.Street)))
            {
                addr.Street = rq.Street;
            }

            if (rq.IsModified(nameof(rq.PostalCode)))
            {
                addr.PostalCode = rq.PostalCode;
            }

            if (rq.IsModified(nameof(rq.FormattedAddress)) && !string.IsNullOrEmpty(rq.FormattedAddress))
            {
                addr.FormattedAddress = rq.FormattedAddress;
            }

            if (rq.IsModified(nameof(rq.Location)))
            {
                addr.Location = rq.Location == null ? null : new NpgsqlPoint(rq.Location.Lng, rq.Location.Lat);
            }

            if (rq.IsModified(nameof(rq.Provider)) && rq.Provider.HasValue)
            {
                addr.Provider = rq.Provider.Value;
            }

            if (rq.IsModified(nameof(rq.ParentId)))
            {
                addr.ParentId = rq.ParentId;
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
        /// <param name="id">Address id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<AddressUpdateReadData?> UpdateReadAsync(int id, CancellationToken cancellationToken = default)
        {
            var orgId = User.OrganizationInt;

            return await _db.PersonAddresses.AsNoTracking()
                .Where(a => a.Id == id)
                .Select(a => new AddressUpdateReadData
                {
                    Id = a.Id,
                    PersonId = a.PersonId,
                    Kind = a.Kind,
                    Provider = a.Provider,
                    PlaceId = a.PlaceId,
                    Name = a.Name,
                    Region = a.Region,
                    State = a.State,
                    City = a.City,
                    District = a.District,
                    Route = a.Route,
                    Street = a.Street,
                    PostalCode = a.PostalCode,
                    FormattedAddress = a.FormattedAddress,
                    ParentId = a.ParentId,
                    Location = a.Location == null ? null : new Location((float)a.Location.Value.X, (float)a.Location.Value.Y)
                }).FirstOrDefaultAsync(cancellationToken);
        }
    }
}
