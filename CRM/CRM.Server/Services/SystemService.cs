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
        readonly MyDbContext _db;

        public SystemService(
            MyDbContext db,
            ISEServiceApp app,
            CurrentUserAccessor userAccessor,
            ILogger<SystemService> logger
        )
            : base(app, userAccessor.UserSafe, "system", logger)
        {
            _db = db;
        }

        /// <summary>
        /// Read system settings
        /// 读取系统设置
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public Task<SystemSettings?> ReadSettingsAsync(CancellationToken cancellationToken = default)
        {
            return _db.SettingCrms.AsNoTracking()
                .Where(s => s.Id == User.OrganizationInt)
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

        /// <summary>
        /// Update system settings
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> UpdateSettingsAsync(UpdateSettingsRQ rq, CancellationToken cancellationToken = default)
        {
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
