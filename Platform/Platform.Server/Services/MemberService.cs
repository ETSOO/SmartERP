using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Authentication;
using com.etsoo.CoreFramework.Business;
using com.etsoo.CoreFramework.Models;
using com.etsoo.CoreFramework.User;
using com.etsoo.Database;
using com.etsoo.HTTP;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Storage;
using Microsoft.EntityFrameworkCore;
using Platform.Server.Application;
using Platform.Server.Dto.AuthCode;
using Platform.Server.Dto.Member;
using Platform.Server.Endpoints.Member.RQ;
using PlatformShared;
using PlatformShared.Database;
using PlatformShared.Database.Models;
using PlatformShared.Dto;
using PlatformShared.Extentions;
using PlatformShared.Messages;
using System.Buffers;
using System.Collections.Concurrent;

namespace Platform.Server.Services
{
    /// <summary>
    /// Member service
    /// 成员服务
    /// </summary>
    public class MemberService : CommonUserService, IMemberService
    {
        readonly MyDbContext _db;
        readonly IStorage _storage;
        readonly IAuthCodeService _authCodeService;
        readonly IQueueService _queueService;

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="db">Database EF</param>
        /// <param name="app">Application</param>
        /// <param name="userAccessor">User accessor</param>
        /// <param name="logger">Logger</param>
        /// <param name="storage">Storage</param>
        /// <param name="authCodeService">Auth code service</param>
        /// <param name="queueService">Queue service</param>
        /// 
        public MemberService(MyDbContext db, IMyApp app, CurrentUserAccessor userAccessor, ILogger<MemberService> logger,
            IStorage storage, IAuthCodeService authCodeService,
            IQueueService queueService)
            : base(app, userAccessor.UserSafe, "member", logger)
        {
            _db = db;
            _storage=storage;
            _authCodeService = authCodeService;
            _queueService = queueService;
        }

        /// <summary>
        /// Delete member
        /// 删除成员
        /// </summary>
        /// <param name="id">Organization id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var ou = await _db.CoreOrganizationUsers
                .AsNoTracking()
                .Where(ou => ou.Id == id
                    && ou.CoreOrganizationId == User.OrganizationInt
                    && ou.Status == EntityStatus.Deleted
                    && ou.UserRole < User.Role)
                .Select(ou => new { ou.InviterId, InviterName = ou.Inviter == null ? null : ou.Inviter.Name, ou.CoreUserId, Name = ou.LocalName ?? ou.CoreUser.Name })
                .FirstOrDefaultAsync(cancellationToken);

            if (ou == null)
            {
                return ApplicationErrors.NoId.AsResult();
            }

            // Delete
            await _db.CoreOrganizationUsers.Where(ou => ou.Id == id).ExecuteDeleteAsync(cancellationToken);

            // Push message
            var message = new DeleteMemberMessage
            {
                Data = User.CreateMessageData(ou.CoreUserId, ou.Name),
                OrgName = User.OrganizationName ?? "Unknown",
                InviterId = ou.InviterId,
                InviterName = ou.InviterName
            };
            await _queueService.PushAsync(message, PlatformSharedContext.Default.DeleteMemberMessage, cancellationToken);

            return ActionResult.Succeed(id);
        }

        private IQueryable<CoreOrganizationUser> CreateQuery(MemberListRQ rq, Func<IQueryable<CoreOrganizationUser>, IQueryable<CoreOrganizationUser>>? filters = null)
        {
            var query = _db.CoreOrganizationUsers
                .AsNoTracking()
                .Where(ou => ou.CoreOrganizationId == User.OrganizationInt && ou.IdentityType.HasFlag(IdentityTypeFlags.User))
                .QueryEtsoo(rq, (ou) => ou.Id, (ou) => ou.Status, (q) =>
                {
                    if (rq.ExcludeSelf is true)
                    {
                        q = q.Where(ou => ou.CoreUserId != User.IdInt);
                    }

                    if (rq.UserRole.HasValue)
                    {
                        q = q.Where(ou => ou.UserRole == rq.UserRole);
                    }

                    if (rq.UserRoleStart.HasValue)
                    {
                        q = q.Where(ou => ou.UserRole >= rq.UserRoleStart);
                    }

                    if (rq.InviterId.HasValue)
                    {
                        q = q.Where(ou => ou.InviterId == rq.InviterId);
                    }

                    if (rq.Keyword?.Length > 1)
                    {
                        var keyword = rq.Keyword;

                        if (keyword.IsComplexQueryKeywords())
                        {
                            q = q.QueryEtsooKeywords(keyword, DbUtils.ILikeMethod, ou => ou.LocalName ?? ou.CoreUser.Name);
                        }
                        else
                        {
                            q = q.Where(ou => EF.Functions.ILike(ou.LocalName ?? ou.CoreUser.Name, $"%{keyword}%") ||(ou.AssignedId != null && EF.Functions.ILike(ou.AssignedId, $"%{keyword}%")));
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
        /// Invite member
        /// 邀请成员
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <returns>Task</returns>
        public async ValueTask<IActionResult> InviteAsync(MemberInviteRQ rq)
        {
            // Current org must exist
            if (string.IsNullOrEmpty(User.OrganizationName))
            {
                return ApplicationErrors.AccessDenied.AsResult(nameof(User.OrganizationName));
            }

            // Validate role
            var userRole = rq.UserRole;
            if (userRole > UserRole.User)
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(rq.UserRole));
            }

            // Data
            var orgId = User.OrganizationInt;
            var data = new CodeUserData
            {
                Name = User.Name,
                FamilyName = User.FamilyName,
                GivenName = User.GivenName,
                OrganizationId = orgId,
                OrganizationName = User.OrganizationName
            };

            // Tasks
            var items = new ConcurrentBag<string>();
            await Parallel.ForEachAsync(rq.Emails.Distinct(), async (email, cancelToken) =>
            {
                // User already exists
                var userExists = await _db.CoreOrganizationUsers.AnyAsync(ou => ou.CoreOrganizationId == orgId
                    && ou.CoreUser.CoreUserIdentifiers.Any(i => i.CoreUserId == ou.CoreUserId && i.Type == CoreUserIdentifierType.Email && i.Value == email), cancelToken);

                if (userExists)
                {
                    return;
                }

                var view = new SendEmailData<AuthCodeMemberInvitationData>
                {
                    Action = AuthCodeAction.MemberInvitationEmailCode,
                    Email = email,
                    Region = User.Region,
                    TimeZone = (User.TimeZone ?? TimeZoneInfo.Local).Id,
                    Data = new AuthCodeMemberInvitationData
                    {
                        UserData = data,
                        WebUrl = App.Configuration.WebUrl,
                        UserRole = userRole,
                        Message = rq.Message,
                    }
                };

                // Send email
                var result = await _authCodeService.SendEmailAsync(view, MyJsonSerializerContext.Default.AuthCodeMemberInvitationData, cancelToken);
                if (result.Ok)
                {
                    items.Add(email);
                }
                else
                {
                    Logger.LogError("InviteAsync email {email} failed with {error}", email, result.Title);
                }
            });

            // Results
            if (items.IsEmpty)
            {
                return ApplicationErrors.EmailExists.AsResult("Results");
            }

            // Result
            return ActionResult.Succeed(string.Join(", ", items));
        }

        /// <summary>
        /// List member JSON data
        /// 成员列表JSON数据
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="writer">Writer to hold the data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task ListAsync(MemberListRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default)
        {
            var query = CreateQuery(rq);

            await query.Select(ou => new
            {
                ou.Id,
                Name = ou.LocalName ?? ou.CoreUser.Name
            }).ToJsonAsync(writer, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Query member JSON data
        /// 查询成员JSON数据
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="writer">Writer to hold the data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task QueryAsync(MemberQueryRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default)
        {
            var query = CreateQuery(rq, (q) =>
            {
                if (rq.AssignedId?.Length > 1)
                {
                    q = q.Where(ou => ou.AssignedId != null && EF.Functions.ILike(ou.AssignedId, $"%{rq.AssignedId}%"));
                }

                return q;
            });

            var (hasContent, commandText) = await query.Select(ou => new MemberQueryData
            {
                Id = ou.Id,
                Name = ou.LocalName ?? ou.CoreUser.Name,
                UserRole = ou.UserRole,
                AssignedId = ou.AssignedId,
                IsOwner = ou.CoreOrganization.OwnerId == User.IdInt,
                IsSelf = ou.CoreUserId == User.IdInt,
                IsEditable = ou.UserRole <= User.Role,
                Status = ou.Status,
                Creation = ou.Creation
            }).ToJsonAsync(writer, cancellationToken: cancellationToken);

            if (_db.IsSensitiveDataLoggingEnabled)
            {
                Logger.LogInformation("QueryAsync is {hasContent} with {commandText}", hasContent, commandText);
            }
        }

        /// <summary>
        /// Read member data for view
        /// 读取用于浏览的成员数据
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="writer">Writer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task ReadAsync(int id, IBufferWriter<byte> writer, CancellationToken cancellationToken = default)
        {
            await _db.CoreOrganizationUsers
                .AsNoTracking()
                .Where(ou => ou.Id == id && ou.CoreOrganizationId == User.OrganizationInt && ou.IdentityType.HasFlag(IdentityTypeFlags.User))
                .Select(ou => new
                {
                    ou.Id,
                    ou.CoreUser.Name,
                    ou.UserRole,
                    ou.IdentityType,
                    ou.LocalName,
                    ou.LocalAvatar,
                    ou.AssignedId,
                    ou.Creation,
                    ou.Expiry,
                    ou.RefreshTime,
                    ou.Status,
                    Inviter = ou.Inviter == null ? null : ou.Inviter.Name
                }).ToJsonObjectAsync(writer, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Update member
        /// 更新成员
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> UpdateAsync(MemberUpdateRQ rq, CancellationToken cancellationToken = default)
        {
            var ou = await _db.CoreOrganizationUsers.Where(o => o.Id == rq.Id
                && o.CoreOrganizationId == User.OrganizationInt
                && o.UserRole <= User.Role)
                .FirstOrDefaultAsync(cancellationToken);

            if (ou == null)
            {
                return ApplicationErrors.NoId.AsResult();
            }

            // Name
            var name = await _db.CoreOrganizationUsers
                .AsNoTracking()
                .Where(ou => ou.Id == rq.Id)
                .Select(ou => ou.LocalName ?? ou.CoreUser.Name)
                .FirstOrDefaultAsync(cancellationToken);

            // Is not self
            var isNotSelf = ou.CoreUserId != User.IdInt && ou.UserRole < User.Role;

            // Update
            if (rq.IsModified(nameof(rq.UserRole)) && rq.UserRole.HasValue && isNotSelf)
            {
                // Except the founder, the user role should be lower than the current user
                if (User.Role != UserRole.Founder && rq.UserRole.Value >= User.Role)
                {
                    return ApplicationErrors.NoValidData.AsResult(nameof(rq.UserRole));
                }

                ou.UserRole = rq.UserRole.Value;
            }

            if (rq.IsModified(nameof(rq.LocalName)))
            {
                ou.LocalName = rq.LocalName;
            }

            if (rq.IsModified(nameof(rq.AssignedId)))
            {
                ou.AssignedId = rq.AssignedId?.ToUpper();
            }

            if (rq.IsModified(nameof(rq.Expiry)) && isNotSelf)
            {
                ou.Expiry = rq.Expiry?.ToUniversalTime();
            }

            if (rq.IsModified(nameof(rq.Status)) && rq.Status.HasValue && isNotSelf)
            {
                ou.Status = rq.Status.Value;
            }

            // Changes
            var changes = _db.ChangeTracker.Entries().GetChangedProperties();

            // Save
            await _db.SaveChangesAsync(cancellationToken);

            // Push message
            var message = new UpdateMemberMessage
            {
                Data = User.CreateMessageData(rq.Id, name),
                Changes = changes
            };
            await _queueService.PushAsync(message, PlatformSharedContext.Default.UpdateMemberMessage, cancellationToken);

            // Return
            return ActionResult.Succeed(rq.Id);
        }

        /// <summary>
        /// Update local avatar
        /// 更新本地头像
        /// </summary>
        /// <param name="id">Organization id</param>
        /// <param name="avatarStream">Avatar stream</param>
        /// <param name="contentType">Cotent type</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>New URL</returns>
        public async Task<IActionResult> UpdateAvatarAsync(int id, Stream avatarStream, string contentType, CancellationToken cancellationToken = default)
        {
            // Check the stream
            if (avatarStream.Length is not > 10240 and < 102400000)
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(avatarStream));
            }

            // Check the avatar
            var ou = await _db.CoreOrganizationUsers.AsNoTracking()
                .Where(ou => ou.Id == id && ou.CoreOrganizationId == User.OrganizationInt)
                .Select(ou => new { ou.LocalAvatar, Name = ou.LocalName ?? ou.CoreUser.Name })
                .FirstOrDefaultAsync(cancellationToken);
            if (ou == null)
            {
                return ApplicationErrors.NoId.AsResult();
            }

            var extension = MimeTypeMap.TryGetExtension(contentType);
            if (string.IsNullOrEmpty(extension))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(contentType));
            }

            // File path
            var path = "/OUAvatar/" + DateTime.UtcNow.ToString("yyyyMM") + "/" + Path.GetRandomFileName() + extension;

            // Save the stream to file directly
            var saveResult = await _storage.WriteAsync(path, avatarStream, WriteCase.CreateNew, cancellationToken: cancellationToken);

            if (saveResult)
            {
                // New avatar URL
                var url = _storage.GetUrl(path);

                // Update
                await _db.CoreOrganizationUsers.Where(ou => ou.Id == id).ExecuteUpdateAsync(o => o.SetProperty(o => o.LocalAvatar, url), cancellationToken);

                // Remove current avatar
                if (!string.IsNullOrEmpty(ou.LocalAvatar))
                    await _storage.DeleteUrlAsync(ou.LocalAvatar, cancellationToken);

                // Push message
                var message = new UpdateMemberAvatarMessage
                {
                    Data = User.CreateMessageData(id, ou.Name)
                };
                await _queueService.PushAsync(message, PlatformSharedContext.Default.UpdateMemberAvatarMessage, cancellationToken);

                // Return
                return ActionResult.Succeed(url);
            }
            else
            {
                Logger.LogError("Avatar write path is {path}", path);
                return ApplicationErrors.DataProcessingFailed.AsResult();
            }
        }

        /// <summary>
        /// Read member data for update
        /// 读取用于更新的成员数据
        /// </summary>
        /// <param name="id">Organization id</param>
        /// <param name="writer">Writer to hold the data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task UpdateReadAsync(int id, IBufferWriter<byte> writer, CancellationToken cancellationToken = default)
        {
            var query = _db.CoreOrganizationUsers
                .AsNoTracking()
                .Where(ou => ou.Id == id
                    && ou.CoreOrganizationId == User.OrganizationInt
                    && ou.IdentityType.HasFlag(IdentityTypeFlags.User)
                    && ou.UserRole <= User.Role);

            var (hasContent, _) = await query.Select(ou => new
            {
                ou.Id,
                ou.CoreUser.Name,
                IsSelf = ou.CoreUserId == User.IdInt,
                ou.UserRole,
                ou.LocalName,
                ou.AssignedId,
                ou.Expiry,
                ou.Status
            }).ToJsonObjectAsync(writer, cancellationToken: cancellationToken);
        }
    }
}
