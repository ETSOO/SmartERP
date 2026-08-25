using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.User;
using com.etsoo.Database;
using com.etsoo.Utils.Actions;
using CRM.Server.Application;
using CRM.Server.Dto.PersonProduct;
using CRM.Server.RQ.PersonProduct;
using Microsoft.EntityFrameworkCore;
using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.Person;
using PlatformShared.Database;
using PlatformShared.Database.Models;
using PlatformShared.Extentions;
using System.Text.Json;

namespace CRM.Server.Services
{
    /// <summary>
    /// Person product service
    /// 人员个性化产品服务
    /// </summary>
    public class PersonProductService : MyUserService, IPersonProductService
    {
        readonly MyDbContext _db;
        readonly ICommonService _commonService;
        readonly IQueueService _queueService;

        public PersonProductService(
            MyDbContext db,
            IMyApp app,
            CurrentUserAccessor userAccessor,
            ILogger<PersonProductService> logger,
            ICommonService commonService,
            IQueueService queueService
        )
            : base(app, userAccessor.UserSafe, "person_product", logger)
        {
            _db = db;
            _commonService = commonService;
            _queueService = queueService;
        }

        /// <summary>
        /// Create
        /// 创建
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> CreateAsync(PersonProductCreateRQ rq, CancellationToken cancellationToken = default)
        {
            // Permission check
            if (!await _commonService.HasPermissionAsync((short)Permissions.Product.Manage, cancellationToken))
            {
                return ApplicationErrors.AccessDenied.AsResult();
            }

            var orgId = User.OrganizationInt;

            // Validate person and product
            var personId = rq.PersonId;
            var hasPerson = await _db.Persons(orgId).AsNoTracking().AnyAsync(p => p.Id == personId, cancellationToken);

            if (!hasPerson)
            {
                return ApplicationErrors.NoId.AsResult(nameof(rq.PersonId));
            }

            var productId = rq.ProductId;
            var hasProduct = await _db.Products(orgId).AsNoTracking().AnyAsync(p => p.Id == productId, cancellationToken);
            if (!hasProduct)
            {
                return ApplicationErrors.NoId.AsResult(nameof(rq.ProductId));
            }

            var hasItem = await _db.PersonProducts.AsNoTracking()
                .AnyAsync(p => p.PersonId == personId && p.ProductId == productId, cancellationToken);
            if (hasItem)
            {
                return ApplicationErrors.ItemExists.AsResult();
            }

            var assignedId = rq.AssignedId?.ToUpper();
            if (!string.IsNullOrEmpty(assignedId))
            {
                var hasAssignedId = await _db.PersonProducts.AsNoTracking()
                    .AnyAsync(p => p.Product.CoreOrganizationId == orgId && p.AssignedId == assignedId, cancellationToken);

                if (hasAssignedId)
                {
                    return ApplicationErrors.ItemExists.AsResult(nameof(rq.AssignedId));
                }
            }

            var product = new PersonProduct
            {
                PersonId = personId,
                ProductId = productId,
                AssignedId = assignedId,
                JsonData = rq.JsonData
            };

            // Add
            _db.PersonProducts.Add(product);

            // Save
            await _db.SaveChangesAsync(cancellationToken);

            // Push message
            var message = new CreatePersonProductMessage
            {
                Data = User.CreateMessageData(App.AppId, personId),
                JsonData = JsonSerializer.Serialize(rq, MyJsonSerializerContext.Default.PersonCategoryCreateRQ)
            };
            await _queueService.PushAsync(message, CrmJsonSerializerContext.Default.CreatePersonProductMessage, cancellationToken);

            return ActionResult.Success;
        }

        /// <summary>
        /// Delete
        /// 删除
        /// </summary>
        /// <param name="personId">Person id</param>
        /// <param name="productId">Product id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> DeleteAsync(long personId, int productId, CancellationToken cancellationToken = default)
        {
            // Permission check
            if (!await _commonService.HasPermissionAsync((short)Permissions.Product.Manage, cancellationToken))
            {
                return ApplicationErrors.AccessDenied.AsResult();
            }

            var orgId = User.OrganizationInt;

            // Remove
            var task1 = _db.PersonProducts.AsNoTracking()
                .Where(p => p.PersonId == personId && p.ProductId == productId && p.Product.CoreOrganizationId == orgId)
                .ExecuteDeleteAsync(cancellationToken);

            // Push message
            var message = new DeletePersonProductMessage
            {
                Data = User.CreateMessageData(App.AppId, personId)
            };
            var task2 = _queueService.PushAsync(message, CrmJsonSerializerContext.Default.DeletePersonProductMessage, cancellationToken);

            await Task.WhenAll(task1, task2);

            var results = task1.Result;

            return results > 0 ? ActionResult.Success : ApplicationErrors.NoId.AsResult();
        }

        /// <summary>
        /// Query person product
        /// 查询人员个性化产品
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public Task<PersonProductQueryData[]> QueryAsync(PersonProductQueryRQ rq, CancellationToken cancellationToken = default)
        {
            var orgId = User.OrganizationInt;
            var q = _db.PersonProducts.AsNoTracking().Where(p => p.Product.CoreOrganizationId == orgId);

            if (rq.PersonId != null)
            {
                q = q.Where(p => p.PersonId == rq.PersonId);
            }

            if (rq.ProductId != null)
            {
                q = q.Where(p => p.ProductId == rq.ProductId);
            }

            var assignedId = rq.AssignedId;
            if (assignedId != null)
            {
                q = q.Where(p => p.AssignedId == assignedId || p.Product.AssignedId == assignedId);
            }

            var paging = rq.QueryPaging;
            if (paging != null)
            {
                q = q.QueryEtsooPaging(paging);
            }

            return q.Select(p => new PersonProductQueryData
            {
                PersonId = p.PersonId,
                ProductId = p.ProductId,
                ProductName = p.Product.Name,
                ProductAssignedId = p.Product.AssignedId,
                AssignedId = p.AssignedId,
                Cultures = p.JsonData == null ? null : p.JsonData.Cultures
            }).ToArrayAsync(cancellationToken);
        }

        /// <summary>
        /// Update
        /// 更新
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> UpdateAsync(PersonProductUpdateRQ rq, CancellationToken cancellationToken = default)
        {
            // Permission check
            if (!await _commonService.HasPermissionAsync((short)Permissions.Product.Manage, cancellationToken))
            {
                return ApplicationErrors.AccessDenied.AsResult();
            }

            var orgId = User.OrganizationInt;

            var product = await _db.PersonProducts
                .Where(p => p.PersonId == rq.PersonId && p.ProductId == rq.ProductId && p.Product.CoreOrganizationId == orgId)
                .FirstOrDefaultAsync(cancellationToken);

            if (product == null)
            {
                return ApplicationErrors.NoId.AsResult();
            }

            if (rq.IsModified(nameof(rq.AssignedId)))
            {
                product.AssignedId = rq.AssignedId?.ToUpper();
            }

            if (rq.IsModified(nameof(rq.JsonData)))
            {
                product.JsonData = rq.JsonData;
            }

            // Changes
            var changes = _db.ChangeTracker.Entries().GetChangedProperties();

            // Save
            var task1 = _db.SaveChangesAsync(cancellationToken);

            // Push message
            var message = new UpdatePersonProductMessage
            {
                Data = User.CreateMessageData(App.AppId, product.PersonId),
                Changes = changes
            };
            var task2 = _queueService.PushAsync(message, CrmJsonSerializerContext.Default.UpdatePersonProductMessage, cancellationToken);

            await Task.WhenAll(task1, task2);

            // Return
            return ActionResult.Success;
        }

        /// <summary>
        /// Read for update
        /// 读取用于更新的数据
        /// </summary>
        /// <param name="personId">Person id</param>
        /// <param name="productId">Product id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<PersonProductUpdateReadData?> UpdateReadAsync(long personId, int productId, CancellationToken cancellationToken = default)
        {
            // Permission check
            if (!await _commonService.HasPermissionAsync((short)Permissions.Product.Manage, cancellationToken))
            {
                return null;
            }

            var orgId = User.OrganizationInt;

            return await _db.PersonProducts.AsNoTracking()
                .Where(p => p.PersonId == personId && p.ProductId == productId && p.Product.CoreOrganizationId == orgId)
                .Select(a => new PersonProductUpdateReadData
                {
                    PersonId = a.PersonId,
                    ProductId = a.ProductId,
                    AssignedId = a.AssignedId,
                    JsonData = a.JsonData
                }).FirstOrDefaultAsync(cancellationToken);
        }
    }
}
