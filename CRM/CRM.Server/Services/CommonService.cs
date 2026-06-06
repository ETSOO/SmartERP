using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Business;
using com.etsoo.CoreFramework.User;
using com.etsoo.Utils.Actions;
using CRM.Server.Dto;
using CRM.Server.Dto.PersonProfile;
using CRM.Server.Dto.System;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using PlatformShared.Database;
using PlatformShared.Database.Models;
using PlatformShared.Extentions;
using PersonInfo = PlatformShared.Database.Models.PersonInfo;
using PersonProfile = PlatformShared.Database.Models.PersonProfile;
using ProductUnit = com.etsoo.CoreFramework.Business.ProductUnit;

namespace CRM.Server.Services
{
    /// <summary>
    /// Common service
    /// 通用服务
    /// </summary>
    public class CommonService : ICommonService
    {
        readonly MyDbContext _db;
        readonly CurrentUserAccessor _userAccessor;

        public CommonService(
            MyDbContext db,
            CurrentUserAccessor userAccessor
        )
        {
            _db = db;
            _userAccessor = userAccessor;
        }

        private static (IdentityTypeFlags, bool) GetIdentityType(bool[] permissions)
        {
            var type = IdentityTypeFlags.None;
            var count = 0;

            if (permissions[0])
            {
                type |= IdentityTypeFlags.User;
                count++;
            }

            if (permissions[1])
            {
                type |= IdentityTypeFlags.Customer;
                count++;
            }

            if (permissions[2])
            {
                type |= IdentityTypeFlags.Supplier;
                count++;
            }

            if (permissions[3])
            {
                type |= IdentityTypeFlags.Org;
                count++;
            }

            if (permissions[4])
            {
                type |= IdentityTypeFlags.Dept;
                count++;
            }

            return (type, count == permissions.Length);
        }

        public async Task AddOrUpdatePersonInfoAsync(long personId, PersonInfoKind kind, string? identifier, CancellationToken cancellationToken = default)
        {
            var pinInfo = await _db.PersonInfos
                .Where(i => i.PersonId == personId && i.Kind == kind)
                .OrderByDescending(i => i.IsDefault)
                .FirstOrDefaultAsync(cancellationToken);

            identifier = identifier?.Trim().ToLower();

            if (string.IsNullOrEmpty(identifier))
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
                    pinInfo.Identifier = identifier;

                    // Remove verification status
                    pinInfo.IsVerified = null;
                }
                else
                {
                    pinInfo = new PersonInfo
                    {
                        PersonId = personId,
                        Kind = kind,
                        Identifier = identifier,
                        IsDefault = true
                    };

                    _db.PersonInfos.Add(pinInfo);
                }
            }
        }

        /// <summary>
        /// Add profile
        /// 添加档案
        /// </summary>
        /// <param name="action">Action data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        public Task AddProfileAsync(PersonProfileAction action, CancellationToken cancellationToken = default)
        {
            var userId = _userAccessor.UserSafe.Oid;

            var profile = new PersonProfile
            {
                PersonId = action.PersonId,
                Persons = action.Persons,
                OrderId = action.OrderId,
                Kind = action.Kind ?? PersonProfileKind.Finance,
                Title = action.Title,
                Comment = action.Comment,
                Location = action.Location,
                LocationId = action.LocationId,
                HappenDate = action.HappenDate ?? DateTimeOffset.UtcNow,
                HappenDateEnd = action.HappenDateEnd,
                UserId = userId,
                UserRole = action.UserRole,
                Data = action.Data,
                IndexKey = action.IndexKey,
                Importance = action.Importance,
                AssigneeId = action.AssigneeId
            };

            _db.PersonProfiles.Add(profile);
            return _db.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Add tags
        /// 添加标签
        /// </summary>
        /// <param name="kind">Kind</param>
        /// <param name="tags">Tags</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Tag ids</returns>
        public Task<int[]> AddTagsAsync(FeatureTagKind kind, IEnumerable<string> tags, CancellationToken cancellationToken = default)
        {
            var orgId = _userAccessor.UserSafe.OrganizationInt;

            var orgIdSP = new NpgsqlParameter<int>("p_org_id", orgId);
            var kindSP = new NpgsqlParameter<short>("p_kind", (short)kind);
            var tagsSP = new NpgsqlParameter<string[]>("p_tags", [.. tags]);

            return _db.Database
                .SqlQuery<int>($"SELECT * FROM add_tags({orgIdSP}, {kindSP}, {tagsSP})")
                .ToArrayAsync(cancellationToken);
        }

        /// <summary>
        /// Get culture key
        /// 获取文化键
        /// </summary>
        /// <param name="id">Related id</param>
        /// <param name="kind">Kind</param>
        /// <returns>Result</returns>
        public string GetCultureKey(long id, CustomCultureKind kind)
        {
            return $"etsoo{kind}{id}";
        }

        /// <summary>
        /// Get organization's default currency
        /// 获得机构默认货币
        /// </summary>
        /// <param name="orgId">Organization id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public Task<string?> GetDefaultCurrency(int orgId, CancellationToken cancellationToken = default)
        {
            return _db.SettingCrms
                .AsNoTracking()
                .Where(s => s.Id == orgId)
                .Select(s => s.Currencies.FirstOrDefault())
                .FirstOrDefaultAsync(cancellationToken);
        }

        /// <summary>
        /// Get organization's default culture
        /// 获得机构默认文化
        /// </summary>
        /// <param name="orgId">Organization id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public Task<string?> GetDefaultCulture(int orgId, CancellationToken cancellationToken = default)
        {
            return _db.SettingCrms
                .AsNoTracking()
                .Where(s => s.Id == orgId)
                .Select(s => s.Cultures.FirstOrDefault())
                .FirstOrDefaultAsync(cancellationToken);
        }

        /// <summary>
        /// Get person's permission identity type
        /// 获取个人的权限身份类型
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<(IdentityTypeFlags, bool)> GetPersonIdentityTypeAsync(CancellationToken cancellationToken = default)
        {
            short[] ids = [
                (short)Permissions.User.Query,
                (short)Permissions.Customer.Query,
                (short)Permissions.Supplier.Query,
                (short)Permissions.Org.Query,
                (short)Permissions.Dept.Query
            ];

            var permissions = await HasPermissionsAsync(ids, cancellationToken);

            return GetIdentityType(permissions);
        }

        /// <summary>
        /// Get profile's permission identity type
        /// 获取个人资料的权限身份类型
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<(IdentityTypeFlags, bool)> GetProfileIdentityTypeAsync(CancellationToken cancellationToken = default)
        {
            short[] ids = [
                (short)Permissions.User.QueryProfile,
                (short)Permissions.Customer.QueryProfile,
                (short)Permissions.Supplier.QueryProfile,
                (short)Permissions.Org.QueryProfile,
                (short)Permissions.Dept.QueryProfile
            ];

            var permissions = await HasPermissionsAsync(ids, cancellationToken);

            return GetIdentityType(permissions);
        }

        /// <summary>
        /// Get tag kind from identity type
        /// 从身份类型获取标签类型
        /// </summary>
        /// <param name="type">Identity type</param>
        /// <returns>Tag kind</returns>
        public FeatureTagKind GetTagKind(IdentityTypeFlags type)
        {
            if (type.HasFlag(IdentityTypeFlags.User))
                return FeatureTagKind.User;
            else if (type.HasFlag(IdentityTypeFlags.Customer))
                return FeatureTagKind.Customer;
            else if (type.HasFlag(IdentityTypeFlags.Supplier))
                return FeatureTagKind.Supplier;
            else if (type.HasFlag(IdentityTypeFlags.Org))
                return FeatureTagKind.Org;
            else if (type.HasFlag(IdentityTypeFlags.Dept))
                return FeatureTagKind.Dept;
            else
                return FeatureTagKind.Contact;
        }

        /// <summary>
        /// Check if the user has identity permission of the specified item
        /// 检查用户是否有指定身份的权限
        /// </summary>
        /// <param name="identityType">Identity type</param>
        /// <param name="name">Permission item</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async ValueTask<bool> HasIdentityPermissionAsync(IdentityTypeFlags identityType, string name, CancellationToken cancellationToken)
        {
            if (identityType == IdentityTypeFlags.None) return false;

            List<short> ids = [];

            if (identityType.HasFlag(IdentityTypeFlags.User))
            {
                if (Enum.TryParse<Permissions.User>(name, out var user))
                {
                    ids.Add((short)user);
                }
            }

            if (identityType.HasFlag(IdentityTypeFlags.Customer))
            {
                if (Enum.TryParse<Permissions.Customer>(name, out var customer))
                {
                    ids.Add((short)customer);
                }
            }

            if (identityType.HasFlag(IdentityTypeFlags.Supplier))
            {
                if (Enum.TryParse<Permissions.Supplier>(name, out var supplier))
                {
                    ids.Add((short)supplier);
                }
            }

            if (identityType.HasFlag(IdentityTypeFlags.Org))
            {
                if (Enum.TryParse<Permissions.Org>(name, out var org))
                {
                    ids.Add((short)org);
                }
            }

            if (identityType.HasFlag(IdentityTypeFlags.Dept))
            {
                if (Enum.TryParse<Permissions.Dept>(name, out var dept))
                {
                    ids.Add((short)dept);
                }
            }

            if (ids.Count == 0)
            {
                return false;
            }

            var permissions = await HasPermissionsAsync(ids, cancellationToken);

            return permissions.Any(p => p);
        }

        /// <summary>
        /// Check if the user has permission
        /// 检查用户是否有权限
        /// </summary>
        /// <param name="permissionItemId">Permission item id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public Task<bool> HasPermissionAsync(short permissionItemId, CancellationToken cancellationToken = default)
        {
            return _db.HasPermissionAsync(_userAccessor.UserSafe.Oid, permissionItemId, cancellationToken);
        }

        /// <summary>
        /// Check if the user has permissions
        /// 检查用户是否有权限
        /// </summary>
        /// <param name="permissionItemIds">Permission item ids</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public Task<bool[]> HasPermissionsAsync(IEnumerable<short> permissionItemIds, CancellationToken cancellationToken = default)
        {
            return _db.HasPermissionsAsync(_userAccessor.UserSafe.Oid, permissionItemIds, cancellationToken);
        }

        /// <summary>
        /// Read tag id by tag and organization id
        /// 通过标签和机构编号读取标签编号
        /// </summary>
        /// <param name="tag">Tag</param>
        /// <param name="orgId">Organization id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public Task<int> ReadTagIdAsync(string tag, int orgId, CancellationToken cancellationToken = default)
        {
            return _db.FeatureTags
                .AsNoTracking()
                .Where(t => t.CoreOrganizationId == orgId && t.Tag == tag)
                .Select(t => t.Id)
                .FirstOrDefaultAsync(cancellationToken);
        }

        /// <summary>
        /// Sync asset
        /// 同步资产
        /// </summary>
        /// <param name="personId">Person id</param>
        /// <param name="assetId">Asset id</param>
        /// <param name="assetQty">Asset quantity</param>
        /// <param name="qty">Quantity</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> SyncAssetAsync(long personId, int assetId, int assetQty, decimal qty, CancellationToken cancellationToken = default)
        {
            var asset = await _db.PersonAssets.Where(a => a.Id == assetId && a.PersonId == personId)
                .Select(a => new
                {
                    a.Expiry,
                    a.Product.Validity,
                    a.Product.Unit.BaseUnit
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (asset == null)
            {
                return ApplicationErrors.NoId.AsResult("AssetId");
            }

            var unit = asset.BaseUnit;

            if (!Constants.IsAssetUnit(unit))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(asset.BaseUnit));
            }

            try
            {
                if (unit == ProductUnit.TIME)
                {
                    // 次卡有效期自动延长1年
                    var days = asset.Validity.GetValueOrDefault(366);
                    var totalTimes = Convert.ToInt32(assetQty * qty);

                    await _db.PersonAssets.Where(a => a.Id == assetId)
                        .ExecuteUpdateAsync(a => a.SetProperty(p => p.Times, p => p.Times.GetValueOrDefault() + totalTimes).SetProperty(p => p.Expiry, p => p.Expiry.AddDays(days)), cancellationToken);
                }
                else if (unit == ProductUnit.MONEY)
                {
                    // 储值卡有效期自动延长3年
                    var days = asset.Validity.GetValueOrDefault(732);
                    var totalAmount = assetQty * qty;
                    await _db.PersonAssets.Where(a => a.Id == assetId)
                        .ExecuteUpdateAsync(a => a.SetProperty(p => p.Amount, p => p.Amount.GetValueOrDefault() + totalAmount).SetProperty(p => p.Expiry, p => p.Expiry.AddDays(days)), cancellationToken);
                }
                else
                {
                    var expiry = asset.Expiry;
                    var totalQty = Convert.ToInt32(assetQty * qty);

                    switch (unit)
                    {
                        case ProductUnit.HOUR:
                            expiry = expiry.AddHours(totalQty);
                            break;
                        case ProductUnit.DAY:
                            expiry = expiry.AddDays(totalQty);
                            break;
                        case ProductUnit.WEEK:
                            expiry = expiry.AddDays(totalQty * 7);
                            break;
                        case ProductUnit.FORTNIGHT:
                            expiry = expiry.AddDays(totalQty * 14);
                            break;
                        case ProductUnit.FOURWEEK:
                            expiry = expiry.AddDays(totalQty * 28);
                            break;
                        case ProductUnit.MONTH:
                            expiry = expiry.AddMonths(totalQty);
                            break;
                        case ProductUnit.BIMONTH:
                            expiry = expiry.AddMonths(totalQty * 2);
                            break;
                        case ProductUnit.QUATER:
                            expiry = expiry.AddMonths(totalQty * 3);
                            break;
                        case ProductUnit.HALFYEAR:
                            expiry = expiry.AddMonths(totalQty * 6);
                            break;
                        case ProductUnit.YEAR:
                            expiry = expiry.AddYears(totalQty);
                            break;
                    }

                    await _db.PersonAssets.Where(a => a.Id == assetId)
                        .ExecuteUpdateAsync(a => a.SetProperty(p => p.Expiry, expiry), cancellationToken);
                }
            }
            catch (Exception ex)
            {
                return ActionResult.From(ex);
            }

            return ActionResult.Success;
        }

        /// <summary>
        /// Update tag
        /// 更新标签
        /// </summary>
        /// <param name="tag">Tag data</param>
        /// <param name="orgId">Organization id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        public async ValueTask UpdateTagAsync(IQueryTag tag, int orgId, CancellationToken cancellationToken = default)
        {
            if (tag.TagId == null && !string.IsNullOrEmpty(tag.Tag))
            {
                tag.TagId = await ReadTagIdAsync(tag.Tag, orgId, cancellationToken);
            }
        }

        /// <summary>
        /// Validate person categories
        /// 验证人员类目
        /// </summary>
        /// <param name="ids">Ids</param>
        /// <param name="orgId">Organization id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async ValueTask<(ActionResult result, IEnumerable<int>? ids)> ValidatePersonCategoriesAsync(IEnumerable<int>? ids, int orgId, CancellationToken cancellationToken = default)
        {
            if (ids == null || !ids.Any())
            {
                return (ActionResult.Success, null);
            }

            var items = await _db.PersonCategories(orgId)
                .AsNoTracking()
                .Where(c => ids.Contains(c.Id))
                .Select(c => new { c.Id, c.ParentIds })
                .ToArrayAsync(cancellationToken);

            if (items.Length != ids.Count())
            {
                return (ApplicationErrors.NoId.AsResult(), null);
            }

            var allIds = items.SelectMany(i => i.ParentIds == null ? [i.Id] : new[] { i.Id }.Concat(i.ParentIds)).Distinct();

            return (ActionResult.Success, allIds);
        }

        /// <summary>
        /// Validate product categories
        /// 验证产品类目
        /// </summary>
        /// <param name="ids">Ids</param>
        /// <param name="orgId">Organization id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async ValueTask<(ActionResult result, IEnumerable<int>? ids)> ValidateProductCategoriesAsync(IEnumerable<int>? ids, int orgId, CancellationToken cancellationToken = default)
        {
            if (ids == null || !ids.Any())
            {
                return (ActionResult.Success, null);
            }

            var items = await _db.ProductCategories(orgId)
                .AsNoTracking()
                .Where(c => ids.Contains(c.Id))
                .Select(c => new { c.Id, c.ParentIds })
                .ToArrayAsync(cancellationToken);

            if (items.Length != ids.Count())
            {
                return (ApplicationErrors.NoId.AsResult(), null);
            }

            var allIds = items.SelectMany(i => i.ParentIds == null ? [i.Id] : new[] { i.Id }.Concat(i.ParentIds)).Distinct();

            return (ActionResult.Success, allIds);
        }
    }
}