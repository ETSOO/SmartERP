using Admin.Server.RQ.Operation;
using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.User;
using com.etsoo.ServiceApp.SmartERP;
using com.etsoo.Utils.Actions;
using Microsoft.EntityFrameworkCore;
using PlatformShared;
using PlatformShared.Database;
using PlatformShared.Extentions;
using PlatformShared.Messages;

namespace Admin.Server.Services
{
    /// <summary>
    /// Operation service
    /// 操作服务
    /// </summary>
    public class AdminService : SEUserService, IAdminService
    {
        readonly MyDbContext _db;
        readonly IQueueService _queueService;

        public AdminService(
            MyDbContext db,
            ISEServiceApp app,
            CurrentUserAccessor userAccessor,
            ILogger<AdminService> logger,
            IQueueService queueService
        )
            : base(app, userAccessor.UserSafe, "operation", logger)
        {
            _db = db;
            _queueService = queueService;
        }

        /// <summary>
        /// Application renew
        /// 应用续费
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> AppRenewAsync(AppRenewRQ rq, CancellationToken cancellationToken = default)
        {
            var result = await _db.CoreOrganizationApps.AsNoTracking()
                .Where(app => app.Id == rq.Id)
                .Join(_db.CoreOrganizationUsers.AsNoTracking(),
                    app => new { OrgId = app.CoreOrganizationId, UserId = rq.Requester },
                    requester => new { OrgId = requester.CoreOrganizationId, UserId = requester.Id },
                    (app, requester) => new { app, requester })
                .Join(_db.CoreOrganizationUsers.AsNoTracking(),
                    temp => new { OrgId = User.OrganizationInt, UserId = rq.Approver },
                    approver => new { OrgId = approver.CoreOrganizationId, UserId = approver.Id },
                    (temp, approver) => new
                    {
                        OrganizationId = temp.app.CoreOrganizationId,
                        AppName = temp.app.LocalName ?? temp.app.CoreApp.Name,
                        RequesterUserId = temp.requester.CoreUserId,
                        ApproverUserId = approver.CoreUserId
                    })
                .FirstOrDefaultAsync(cancellationToken);

            if (result == null)
            {
                return ApplicationErrors.NoId.AsResult();
            }

            // Push message
            var message = new AdminRenewAppMessage
            {
                Data = User.CreateMessageData(App.AppId, rq.Id, result.AppName),
                Months = rq.Months,
                Comment = rq.Comment,
                Requester = result.RequesterUserId,
                RequesterLocalId = rq.Requester,
                RequesterOrgId = result.OrganizationId,
                Approver = result.ApproverUserId,
                ApproverLocalId = rq.Approver
            };
            await _queueService.PushAsync(message, PlatformSharedContext.Default.AdminRenewAppMessage, cancellationToken);

            return ActionResult.Succeed(rq.Id);
        }

        /// <summary>
        /// Clear user frozen time
        /// 清除用户冻结时间
        /// </summary>
        /// <param name="userId">User id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> ClearUserFrozenAsync(int userId, CancellationToken cancellationToken = default)
        {
            // Validate user
            var user = await _db.CoreUsers.AsNoTracking()
                .Where(u => u.Id == userId)
                .Select(u => new { u.Name })
                .FirstOrDefaultAsync(cancellationToken);

            if (user == null)
            {
                return ApplicationErrors.NoId.AsResult();
            }

            // Push message
            var message = new AdminClearUserFrozenMessage
            {
                Data = User.CreateMessageData(App.AppId, userId, user.Name),
            };
            await _queueService.PushAsync(message, PlatformSharedContext.Default.AdminClearUserFrozenMessage, cancellationToken);

            return ActionResult.Succeed(userId);
        }
    }
}
