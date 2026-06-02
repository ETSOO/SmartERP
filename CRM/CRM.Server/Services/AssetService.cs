using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Business;
using com.etsoo.CoreFramework.Models;
using com.etsoo.CoreFramework.User;
using com.etsoo.Database;
using com.etsoo.ServiceApp.SmartERP;
using com.etsoo.Utils.Actions;
using CRM.Server.Dto.Asset;
using CRM.Server.Dto.PersonProfile;
using CRM.Server.Dto.Product;
using CRM.Server.Properties;
using CRM.Server.RQ.Asset;
using Microsoft.EntityFrameworkCore;
using PlatformShared.Database;
using PlatformShared.Database.Models;
using PlatformShared.Extentions;
using System.Buffers;
using System.Text.Json;
using System.Web;
using static System.Runtime.InteropServices.JavaScript.JSType;
using BusinessProductUnit = com.etsoo.CoreFramework.Business.ProductUnit;

namespace CRM.Server.Services
{
    /// <summary>
    /// Asset service
    /// 资产服务
    /// </summary>
    public class AssetService : SEUserService, IAssetService
    {
        readonly MyDbContext _db;
        readonly ICommonService _commonService;

        public AssetService(
            MyDbContext db,
            ISEServiceApp app,
            CurrentUserAccessor userAccessor,
            ILogger<AssetService> logger,
            ICommonService commonService
        )
            : base(app, userAccessor.UserSafe, "asset", logger)
        {
            _db = db;
            _commonService = commonService;
        }

        private string GetEncryptionKey(int productId)
        {
            return "Asset" + productId;
        }

        private string EncryptSensitiveData(int productId, string sensitiveData)
        {
            return App.EncriptData(sensitiveData, GetEncryptionKey(productId));
        }

        private ActionResult CheckProductUnit(BusinessProductUnit unit, decimal? amount, int? times, bool isUpdating = false)
        {
            if (unit == BusinessProductUnit.TIME)
            {
                if (amount.HasValue)
                {
                    return ApplicationErrors.NoValidData.AsResult(nameof(amount));
                }

                return ActionResult.Success;
            }

            if (unit == BusinessProductUnit.MONEY)
            {
                if (times.HasValue)
                {
                    return ApplicationErrors.NoValidData.AsResult(nameof(times));
                }

                return ActionResult.Success;
            }

            if (amount.HasValue || times.HasValue)
            {
                return ApplicationErrors.NoValidData.AsResult("NoAsset");
            }

            return ActionResult.Success;
        }

        /// <summary>
        /// Create
        /// 创建
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> CreateAsync(AssetCreateRQ rq, CancellationToken cancellationToken = default)
        {
            if (!await _commonService.HasPermissionAsync((short)Permissions.Org.Manage, cancellationToken))
            {
                return ApplicationErrors.AccessDenied.AsResult();
            }

            var orgId = User.OrganizationInt;

            // Check owner
            var personId = rq.PersonId;
            if (!await _db.Persons(orgId).Where(p => p.Id == personId).AnyAsync(cancellationToken))
            {
                return ApplicationErrors.NoId.AsResult(nameof(rq.PersonId));
            }

            // Check product
            var productId = rq.ProductId;
            var product = await _db.Products(orgId)
                .Where(p => p.Id == productId)
                .Select(p => new
                {
                    p.Unit.BaseUnit
                })
                .FirstOrDefaultAsync(cancellationToken);
            if (product == null)
            {
                return ApplicationErrors.NoId.AsResult(nameof(rq.ProductId));
            }

            var unitResult = CheckProductUnit(product.BaseUnit, rq.Amount, rq.Times);
            if (!unitResult.Ok)
            {
                return unitResult;
            }

            // Check supplier
            var supplierId = rq.SupplierId;
            if (supplierId.HasValue && !await _db.Suppliers(orgId).Where(p => p.Id == supplierId.Value).AnyAsync(cancellationToken))
            {
                return ApplicationErrors.NoId.AsResult(nameof(rq.SupplierId));
            }

            // Duplicate test
            // Sn
            var sn = rq.Sn.ToLower();
            if (await _db.Assets(orgId).AnyAsync(p => p.ProductId == productId && p.Sn == sn, cancellationToken))
            {
                return ApplicationErrors.ItemExists.AsResult(nameof(rq.Sn));
            }

            var sensitiveData = rq.SensitiveData;
            if (!string.IsNullOrEmpty(sensitiveData))
            {
                sensitiveData = EncryptSensitiveData(productId, sensitiveData);
            }

            var asset = new PersonAsset
            {
                OrgId = orgId,
                PersonId = personId,
                ProductId = productId,
                SupplierId = supplierId,
                Sn = sn,
                Description = rq.Description,
                Expiry = rq.Expiry,
                Times = rq.Times,
                Amount = rq.Amount,
                SensitiveData = sensitiveData,
                Data = rq.Data,
                HealthCheckUrl = rq.HealthCheckUrl,
                Status = rq.Status ?? EntityStatus.Normal,
                CoreUserId = User.IdInt
            };

            _db.PersonAssets.Add(asset);

            // Save changes
            await _db.SaveChangesAsync(cancellationToken);

            var id = asset.Id;

            return ActionResult.Succeed(id);
        }

        private IQueryable<PersonAsset> CreateQuery(AssetListRQ rq, Func<IQueryable<PersonAsset>, IQueryable<PersonAsset>>? filters = null)
        {
            var orgId = User.OrganizationInt;
            var query = _db.Assets(orgId).AsNoTracking()
                .QueryEtsoo(rq, (a) => a.Id, (a) => a.Status, (q) =>
                {
                    if (rq.PersonId.HasValue)
                    {
                        q = q.Where(a => a.PersonId == rq.PersonId.Value);
                    }

                    if (rq.ProductId.HasValue)
                    {
                        q = q.Where(a => a.ProductId == rq.ProductId.Value);
                    }

                    if (rq.SupplierId.HasValue)
                    {
                        q = q.Where(a => a.SupplierId == rq.SupplierId.Value);
                    }

                    if (!string.IsNullOrEmpty(rq.Sn))
                    {
                        q = q.Where(a => a.Sn == rq.Sn.ToLower());
                    }

                    if (rq.Keyword?.Length > 2)
                    {
                        var keyword = rq.Keyword;

                        if (keyword.IsComplexQueryKeywords())
                        {
                            q = q.QueryEtsooKeywords(keyword, DbUtils.ILikeMethod, a => a.Description);
                        }
                        else
                        {
                            q = q.Where(ou => EF.Functions.ILike(ou.Sn, $"%{keyword}%")
                            || (ou.Description != null && EF.Functions.ILike(ou.Description, $"%{keyword}%"))
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
        /// List asset JSON data
        /// 资产列表JSON数据
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="writer">Writer to hold the data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task ListAsync(AssetListRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default)
        {
            await CreateQuery(rq)
                .Select(a => new AssetListData
                {
                    Id = a.Id,
                    SupplierId = a.SupplierId,
                    Product = a.Product.Name,
                    Sn = a.Sn,
                    Expiry = a.Expiry
                }).ToJsonAsync(writer, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Query asset JSON data
        /// 查询资产JSON数据
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public Task<AssetQueryData[]> QueryAsync(AssetQueryRQ rq, CancellationToken cancellationToken = default)
        {
            return CreateQuery(rq, (q) =>
            {
                if (rq.UserId.HasValue)
                {
                    return q = q.Where(a => a.Person.UserId == rq.UserId.Value);
                }

                return q;
            })
            .Select(a => new AssetQueryData
            {
                Id = a.Id,
                Owner = a.Person.Name,
                Product = a.Product.Name,
                Sn = a.Sn,
                Description = a.Description,
                Expiry = a.Expiry,
                Times = a.Times,
                Amount = a.Amount,
                Status = a.Status,
                Creation = a.Creation
            }).ToArrayAsync(cancellationToken);
        }

        /// <summary>
        /// Read data for view
        /// 读取用于浏览的数据
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<AssetViewData?> ReadAsync(int id, CancellationToken cancellationToken = default)
        {
            // Permission check
            if (!await _commonService.HasPermissionAsync((short)Permissions.Org.Manage, cancellationToken))
            {
                return null;
            }

            // Organization id
            var orgId = User.OrganizationInt;

            return await _db.Assets(orgId).AsNoTracking()
                .Where(p => p.Id == id)
                .Select(a => new AssetViewData
                {
                    Id = a.Id,
                    PersonId = a.PersonId,
                    PersonIdentityType = a.Person.IdentityType,
                    PersonName = a.Person.Name,
                    ProductId = a.ProductId,
                    ProductName = a.Product.Name,
                    SupplierId = a.SupplierId,
                    SupplierName = a.Supplier != null ? a.Supplier.Name : null,
                    Sn = a.Sn,
                    Description = a.Description,
                    Expiry = a.Expiry,
                    Times = a.Times,
                    Amount = a.Amount,
                    SensitiveData = a.SensitiveData == null ? null : "***",
                    HealthCheckUrl = a.HealthCheckUrl,
                    HealthCheckSchedule = a.HealthCheckSchedule,
                    Status = a.Status,
                    Creation = a.Creation
                }).FirstOrDefaultAsync(cancellationToken);
        }

        /// <summary>
        /// Read sensitive data
        /// 读取敏感数据
        /// </summary>
        /// <param name="id">Asset id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<string?> ReadSensitiveDataAsync(int id, CancellationToken cancellationToken = default)
        {
            if (!await _commonService.HasPermissionAsync((short)Permissions.Org.Manage, cancellationToken))
            {
                return null;
            }

            var orgId = User.OrganizationInt;

            var data = await _db.Assets(orgId).AsNoTracking()
                .Where(p => p.Id == id)
                .Select(a => new { a.ProductId, a.SensitiveData })
                .FirstOrDefaultAsync(cancellationToken);

            if (data != null && !string.IsNullOrEmpty(data.SensitiveData))
            {
                var sensitiveData = App.DecriptData(data.SensitiveData, GetEncryptionKey(data.ProductId));
                return EncryptWeb(sensitiveData, id.ToString());
            }

            return null;
        }

        /// <summary>
        /// Update
        /// 更新
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> UpdateAsync(AssetUpdateRQ rq, CancellationToken cancellationToken = default)
        {
            // Permission check
            if (!await _commonService.HasPermissionAsync((short)Permissions.Org.Manage, cancellationToken))
            {
                return ApplicationErrors.AccessDenied.AsResult();
            }

            // Organization id
            var orgId = User.OrganizationInt;

            var asset = await _db.Assets(orgId)
                .Where(p => p.Id == rq.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (asset == null)
            {
                return ApplicationErrors.NoId.AsResult();
            }

            // Check owner
            var personId = rq.PersonId;
            if (personId.HasValue && !await _db.Persons(orgId).Where(p => p.Id == personId).AnyAsync(cancellationToken))
            {
                return ApplicationErrors.NoId.AsResult(nameof(rq.PersonId));
            }

            // Check product
            var productId = rq.ProductId;
            var productQueryId = productId ?? asset.ProductId;
            var product = await _db.Products(orgId)
                .Where(p => p.Id == productQueryId)
                .Select(p => new ProductListData
                {
                    Id = p.Id,
                    Name = p.Name,
                    BaseUnit = p.Unit.BaseUnit,
                    AssignedId = p.AssignedId
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (product == null)
            {
                return ApplicationErrors.NoId.AsResult(nameof(rq.ProductId));
            }

            if (productId.HasValue)
            {
                var unitResult = CheckProductUnit(product.BaseUnit, rq.Amount, rq.Times, true);
                if (!unitResult.Ok)
                {
                    return unitResult;
                }
            }

            // Check supplier
            var supplierId = rq.SupplierId;
            if (supplierId.HasValue && !await _db.Suppliers(orgId).Where(p => p.Id == supplierId.Value).AnyAsync(cancellationToken))
            {
                return ApplicationErrors.NoId.AsResult(nameof(rq.SupplierId));
            }

            if (rq.IsModified(nameof(rq.ProductId)) && productId.HasValue)
            {
                asset.ProductId = productId.Value;
            }

            var hasFinanceChange = false;

            // Duplicate test
            // Sn
            if (rq.IsModified(nameof(rq.Sn)) && !string.IsNullOrEmpty(rq.Sn))
            {
                var sn = rq.Sn.ToLower();
                if (await _db.Assets(orgId).AnyAsync(p => p.Id != rq.Id && p.ProductId == asset.ProductId && p.Sn == sn, cancellationToken))
                {
                    return ApplicationErrors.ItemExists.AsResult(nameof(rq.Sn));
                }

                asset.Sn = sn;
                hasFinanceChange = true;
            }

            if (rq.IsModified(nameof(rq.PersonId)) && personId.HasValue)
            {
                asset.PersonId = personId.Value;
            }

            if (rq.IsModified(nameof(rq.SupplierId)))
            {
                asset.SupplierId = supplierId;
            }

            if (rq.IsModified(nameof(rq.Description)))
            {
                asset.Description = rq.Description;
            }

            if (rq.IsModified(nameof(rq.Expiry)) && rq.Expiry.HasValue)
            {
                asset.Expiry = rq.Expiry.Value;
                hasFinanceChange = true;
            }

            if (rq.IsModified(nameof(rq.Times)))
            {
                asset.Times = rq.Times;
                hasFinanceChange = true;
            }

            if (rq.IsModified(nameof(rq.Amount)))
            {
                asset.Amount = rq.Amount;
                hasFinanceChange = true;
            }

            if (rq.IsModified(nameof(rq.SensitiveData)))
            {
                var sensitiveData = rq.SensitiveData;
                if (string.IsNullOrEmpty(sensitiveData))
                {
                    asset.SensitiveData = null;
                }
                else
                {
                    asset.SensitiveData = EncryptSensitiveData(asset.ProductId, sensitiveData);
                }
            }

            if (rq.IsModified(nameof(rq.Data)))
            {
                asset.Data = rq.Data;
            }

            if (rq.IsModified(nameof(rq.HealthCheckUrl)))
            {
                asset.HealthCheckUrl = rq.HealthCheckUrl;
            }

            if (rq.IsModified(nameof(rq.Status)) && rq.Status.HasValue)
            {
                asset.Status = rq.Status.Value;
            }

            // Changes
            var changes = _db.ChangeTracker.Entries().GetChangedProperties();

            // Save
            await _db.SaveChangesAsync(cancellationToken);

            if (hasFinanceChange)
            {
                // Add profile
                var title = $"{Resources.AssetUpdate} - {product.Name} (${asset.Sn})";
                var comment = $"<p>{string.Join("<br/>", changes.Select(c => $"{c.Name}: {HttpUtility.HtmlEncode(c.OriginalValue)} => {HttpUtility.HtmlEncode(c.CurrentValue)}"))}</p>";
                var data = JsonSerializer.Serialize(changes, ModelJsonSerializerContext.Default.IEnumerableEntityChangedProperty);

                var profile = new PersonProfileAction
                {
                    PersonId = asset.PersonId,
                    Kind = PersonProfileKind.Finance,
                    Title = title,
                    Comment = comment,
                    Data = data
                };
                await _commonService.AddProfileAsync(profile, cancellationToken);
            }

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
        public async Task<AssetUpdateReadData?> UpdateReadAsync(int id, CancellationToken cancellationToken = default)
        {
            // Permission check
            if (!await _commonService.HasPermissionAsync((short)Permissions.Org.Manage, cancellationToken))
            {
                return null;
            }

            // Organization id
            var orgId = User.OrganizationInt;

            return await _db.Assets(orgId).AsNoTracking()
                .Where(p => p.Id == id)
                .Select(a => new AssetUpdateReadData
                {
                    Id = a.Id,
                    PersonId = a.PersonId,
                    ProductId = a.ProductId,
                    SupplierId = a.SupplierId,
                    Sn = a.Sn,
                    Description = a.Description,
                    Expiry = a.Expiry,
                    Times = a.Times,
                    Amount = a.Amount,
                    SensitiveData = a.SensitiveData == null ? null : "***",
                    HealthCheckUrl = a.HealthCheckUrl,
                    Data = a.Data,
                    Status = a.Status
                }).FirstOrDefaultAsync(cancellationToken);
        }
    }
}