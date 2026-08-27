using Admin.Server.Application;
using Admin.Server.RQ.Admin;
using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Business;
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
            IMyApp app,
            MyAppConfiguration config,
            CurrentUserAccessor userAccessor,
            ILogger<AdminService> logger,
            IQueueService queueService
        )
            : base(app, config, userAccessor.UserSafe, "admin", logger)
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
            if (rq.Requester == User.IdInt)
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(rq.Requester));
            }
            else if (rq.Approver == User.IdInt)
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(rq.Approver));
            }

            var data = await _db.CoreOrganizationApps.AsNoTracking()
                .Where(app => app.Id == rq.Id)
                .Join(_db.Persons.Where(u => u.CoreUserId != null),
                    app => new { OrgId = app.CoreOrganizationId, UserId = rq.Requester },
                    requester => new { requester.OrgId, UserId = requester.CoreUserId.GetValueOrDefault(0) },
                    (app, requester) => new
                    {
                        OrganizationId = app.CoreOrganizationId,
                        OrgName = app.CoreOrganization.Name,
                        AppName = app.LocalName ?? app.CoreApp.Name,
                        RequesterId = requester.Id,
                        Approver = _db.Users(User.OrganizationInt)
                            .Where(u => u.CoreUserId == rq.Approver && u.Status <= EntityStatus.Approved)
                            .Select(u => new { ApproverId = u.Id })
                            .FirstOrDefault()
                    })
                .FirstOrDefaultAsync(cancellationToken);

            if (data == null || data.Approver == null)
            {
                return ApplicationErrors.NoId.AsResult();
            }

            // Push message
            var message = new AdminRenewAppMessage
            {
                Data = User.CreateMessageData(App.AppId, rq.Id, data.AppName),
                Months = rq.Months,
                Comment = rq.Comment,
                Requester = rq.Requester,
                RequesterLocalId = data.RequesterId,
                RequesterOrgId = data.OrganizationId,
                RequesterOrgName = data.OrgName,
                Approver = rq.Approver,
                ApproverLocalId = data.Approver.ApproverId
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
                .Select(u => new { u.Name, u.FrozenTime })
                .FirstOrDefaultAsync(cancellationToken);

            if (user == null || user.FrozenTime == null)
            {
                return ApplicationErrors.NoId.AsResult();
            }

            // Push message
            var message = new AdminClearUserFrozenMessage
            {
                Data = User.CreateMessageData(App.AppId, userId, user.Name),
                FrozenTime = user.FrozenTime.Value
            };
            await _queueService.PushAsync(message, PlatformSharedContext.Default.AdminClearUserFrozenMessage, cancellationToken);

            return ActionResult.Succeed(userId);
        }
    }
}
