using com.etsoo.ApiModel.Dto.Maps;
using com.etsoo.CoreFramework.Application;
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
                return ApplicationErrors.AccessDenied.AsResult();
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
                    Location = a.Location == null ? null : new Location((float)a.Location.Value.X, (float)a.Location.Value.Y)
                }).FirstOrDefaultAsync(cancellationToken);
        }
    }
}
