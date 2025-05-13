using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Business;
using com.etsoo.CoreFramework.User;
using com.etsoo.Database;
using com.etsoo.Localization;
using com.etsoo.ServiceApp.SmartERP;
using com.etsoo.Utils.Actions;
using CRM.Server.Dto.System;
using CRM.Server.RQ.System;
using Microsoft.EntityFrameworkCore;
using PlatformShared.Database;
using PlatformShared.Database.Models;

namespace CRM.Server.Services
{
    /// <summary>
    /// System service
    /// 系统服务
    /// </summary>
    public class SystemService : SEUserService, ISystemService
    {
        /// <summary>
        /// Read system settings
        /// 读取系统设置
        /// </summary>
        /// <param name="db">Database</param>
        /// <param name="orgId">Organization id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public static Task<SystemSettings?> ReadSystemSettingsAsync(MyDbContext db, int orgId, CancellationToken cancellationToken = default)
        {
            return db.SettingCrms.AsNoTracking()
                .Where(s => s.Id == orgId)
                .Select(s => new SystemSettings
                {
                    PersonId = s.PersonId,
                    MainCustomerType = s.MainCustomerType,
                    Currencies = s.Currencies,
                    SupplierCurrencies = s.SupplierCurrencies,
                    Cultures = s.Cultures,
                    HasInventory = s.HasInventory
                })
                .FirstOrDefaultAsync(cancellationToken);
        }

        readonly MyDbContext _db;
        readonly ICommonService _commonService;

        public SystemService(
            MyDbContext db,
            ISEServiceApp app,
            CurrentUserAccessor userAccessor,
            ILogger<SystemService> logger,
            ICommonService commonService
        )
            : base(app, userAccessor.UserSafe, "system", logger)
        {
            _db = db;
            _commonService = commonService;
        }

        /// <summary>
        /// Get all permission items
        /// 获取所有权限项
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public Task<Dto.System.PermissionItem[]> PermissionItemsAsync(CancellationToken cancellationToken = default)
        {
            return _db.PermissionItems
                .AsNoTracking()
                .Select(p => new Dto.System.PermissionItem
                {
                    Id = p.Id,
                    Module = p.Module,
                    Name = p.Name
                })
                .ToArrayAsync(cancellationToken);
        }

        /// <summary>
        /// Read system settings
        /// 读取系统设置
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public Task<SystemSettings?> ReadSettingsAsync(CancellationToken cancellationToken = default)
        {
            return ReadSystemSettingsAsync(_db, User.OrganizationInt, cancellationToken);
        }

        /// <summary>
        /// Update system settings
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> UpdateSettingsAsync(UpdateSettingsRQ rq, CancellationToken cancellationToken = default)
        {
            // Permission check
            if (!await _commonService.HasPermissionAsync((short)Permissions.Org.UpdateSettings, cancellationToken))
            {
                return ApplicationErrors.AccessDenied.AsResult();
            }

            var orgId = User.OrganizationInt;

            var settings = await _db.SettingCrms
                .Where(s => s.Id == orgId)
                .FirstOrDefaultAsync(cancellationToken);

            if (settings is null)
            {
                // Check settings
                var currencies = rq.Currencies;
                if (currencies is null || !currencies.Any())
                {
                    return ApplicationErrors.NoValidData.AsResult(nameof(rq.Currencies));
                }

                // Find the person
                var person = await _db.Persons
                    .Where(p => p.OrgId == orgId && p.CoreOrganizationId == orgId)
                    .FirstOrDefaultAsync(cancellationToken);

                if (person == null)
                {
                    var name = User.OrganizationName ?? "Organization";
                    person = new Person
                    {
                        OrgId = orgId,
                        CoreOrganizationId = orgId,
                        IdentityType = IdentityTypeFlags.Org,
                        IsLegalPerson = true,
                        Name = name,
                        QueryKeyword = ChineseUtils.GetPinyin(name, true).ToInitials(),
                        UserId = User.IdInt
                    };
                    _db.Persons.Add(person);

                    await _db.SaveChangesAsync(cancellationToken);
                }

                settings = new SettingCrm
                {
                    Id = User.OrganizationInt,
                    PersonId = person.Id,
                    MainCustomerType = rq.MainCustomerType.GetValueOrDefault(CustomerType.Business),
                    Currencies = [.. currencies],
                    Cultures = rq.Cultures?.ToList() ?? [App.Configuration.Cultures[0]],
                    SupplierCurrencies = rq.SupplierCurrencies?.ToList() ?? [currencies.First()],
                    HasInventory = rq.HasInventory.GetValueOrDefault()
                };

                _db.SettingCrms.Add(settings);
            }
            else
            {
                if (rq.IsModified(nameof(rq.MainCustomerType)) && rq.MainCustomerType.HasValue)
                {
                    settings.MainCustomerType = rq.MainCustomerType.Value;
                }

                if (rq.IsModified(nameof(rq.Currencies)) && rq.Currencies?.Any() is true)
                {
                    settings.Currencies = [.. rq.Currencies];
                }

                if (rq.IsModified(nameof(rq.SupplierCurrencies)) && rq.SupplierCurrencies?.Any() is true)
                {
                    settings.SupplierCurrencies = [.. rq.SupplierCurrencies];
                }

                if (rq.IsModified(nameof(rq.Cultures)) && rq.Cultures?.Any() is true)
                {
                    settings.Cultures = [.. rq.Cultures];
                }

                if (rq.IsModified(nameof(rq.HasInventory)) && rq.HasInventory.HasValue)
                {
                    settings.HasInventory = rq.HasInventory.Value;
                }
            }

            await _db.SaveChangesAsync(cancellationToken);

            // Return
            return ActionResult.Success;
        }
    }
}
