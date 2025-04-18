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
        /// Adjust report to from old id to new id
        /// 批量调整汇报对象
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> AdjustReportToAsync(MemberAdjustReportToRQ rq, CancellationToken cancellationToken = default)
        {
            // Check ids
            var users = await _db.Persons.Users(User.OrganizationInt)
                .AsNoTracking()
                .Where(ou => ou.Id == rq.OldId || ou.Id == rq.NewId)
                .Select(ou => new { ou.Id, ou.CoreUserId, ou.Name })
                .ToListAsync(cancellationToken);

            if (users.Count != 2)
            {
                return ApplicationErrors.NoValidData.AsResult();
            }

            var oldUser = users.Find(u => u.Id == rq.OldId)!;

            var newUser = users.Find(u => u.Id == rq.NewId);
            if (newUser?.CoreUserId == null)
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(newUser));
            }

            // Update
            var count = await _db.Persons
                .Where(ou => ou.ReportTo == rq.OldId)
                .ExecuteUpdateAsync(ou => ou.SetProperty(ou => ou.ReportTo, rq.NewId), cancellationToken);

            // Push message
            var message = new AdjustReportToMessage
            {
                Data = User.CreateMessageData(App.AppId, oldUser.Id, oldUser.Name),
                Count = count,
                NewReportTo = newUser.CoreUserId.Value,
                NewReportToName = newUser.Name
            };
            await _queueService.PushAsync(message, PlatformSharedContext.Default.AdjustReportToMessage, cancellationToken);

            // Return
            return ActionResult.Success;
        }

        /// <summary>
        /// Delete member
        /// 删除成员
        /// </summary>
        /// <param name="id">Member id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var ou = await _db.Persons.Users(User.OrganizationInt)
                .AsNoTracking()
                .Where(ou => ou.Id == id
                    && ou.Status == EntityStatus.Deleted
                    && ou.UserRole < User.Role)
                .Select(ou => new { ou.InviterId, InviterName = ou.Inviter == null ? null : ou.Inviter.Name, ou.CoreUserId, ou.Name })
                .FirstOrDefaultAsync(cancellationToken);

            if (ou == null || !ou.CoreUserId.HasValue)
            {
                return ApplicationErrors.NoId.AsResult();
            }

            // Check reports
            if (await _db.Persons.AnyAsync(ou => ou.ReportTo == id, cancellationToken))
            {
                return ApplicationErrors.DeleteReferencedData.AsResult(nameof(Person.ReportTo));
            }

            // Delete
            await _db.Persons.Where(ou => ou.Id == id).ExecuteDeleteAsync(cancellationToken);

            // Push message
            var message = new DeleteMemberMessage
            {
                Data = User.CreateMessageData(App.AppId, ou.CoreUserId.Value, ou.Name),
                OrgName = User.OrganizationName ?? "Unknown",
                InviterId = ou.InviterId,
                InviterName = ou.InviterName
            };
            await _queueService.PushAsync(message, PlatformSharedContext.Default.DeleteMemberMessage, cancellationToken);

            return ActionResult.Succeed(id);
        }

        private IQueryable<Person> CreateQuery(MemberListRQ rq, Func<IQueryable<Person>, IQueryable<Person>>? filters = null)
        {
            var query = _db.Persons.AsNoTracking().Users(User.OrganizationInt)
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

                    if (rq.ReportTo.HasValue)
                    {
                        q = q.Where(ou => ou.ReportTo == rq.ReportTo);
                    }

                    if (rq.Keyword?.Length > 1)
                    {
                        var keyword = rq.Keyword;

                        if (keyword.IsComplexQueryKeywords())
                        {
                            q = q.QueryEtsooKeywords(keyword, DbUtils.ILikeMethod, ou => ou.Name, ou => ou.PreferredName);
                        }
                        else
                        {
                            q = q.Where(ou => EF.Functions.ILike(ou.Name, $"%{keyword}%")
                            || (ou.QueryKeyword != null && EF.Functions.ILike(ou.QueryKeyword, $"%{keyword}%"))
                            || (ou.PreferredName != null && EF.Functions.ILike(ou.PreferredName, $"%{keyword}%"))
                            || (ou.AssignedId != null && EF.Functions.ILike(ou.AssignedId, $"%{keyword}%")));
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
                var userExists = await _db.Persons.Users(orgId)
                    .AnyAsync(ou => ou.CoreUser != null && ou.CoreUser.CoreUserIdentifiers.Any(i => i.CoreUserId == ou.CoreUserId && i.Type == CoreUserIdentifierType.Email && i.Value == email), cancelToken);

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
                ou.Name
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
                Name = ou.Name,
                // ou.UserRole.GetValueOrDefault() will fail to translate with names to SQL
                // 当构建列的时候，需要特别留意类似的问题，莫名其妙的错误可能会导致调试非常浪费时间
                UserRole = ou.UserRole!.Value,
                AssignedId = ou.AssignedId,
                IsOwner = ou.Organization.OwnerId == User.IdInt,
                IsSelf = ou.CoreUserId == User.IdInt,
                IsEditable = ou.UserRole <= User.Role,
                DirectReports = ou.DirectReports.Count,
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
            await _db.Persons.Users(User.OrganizationInt)
                .AsNoTracking()
                .Where(ou => ou.Id == id)
                .Select(ou => new
                {
                    ou.Id,
                    ou.CoreUser!.Name,
                    ou.UserRole,
                    ou.IdentityType,
                    LocalName = ou.Name,
                    LocalAvatar = ou.Avatar,
                    ou.AssignedId,
                    ou.Creation,
                    ou.Expiry,
                    ou.RefreshTime,
                    ou.Status,
                    ou.CoreUser.Avatar,
                    Inviter = ou.Inviter == null ? null : ou.Inviter.Name,
                    DirectReports = ou.DirectReports.Count,
                    ReportTo = ou.ReportToUser == null ? null : ou.ReportToUser.Name
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
            var ou = await _db.Persons.Users(User.OrganizationInt).Where(o => o.Id == rq.Id
                && o.UserRole <= User.Role)
                .FirstOrDefaultAsync(cancellationToken);

            if (ou == null)
            {
                return ApplicationErrors.NoId.AsResult();
            }

            if (rq.ReportTo.HasValue && !await _db.Persons.Users(User.OrganizationInt).AnyAsync(o => o.Id == rq.ReportTo.Value, cancellationToken))
            {
                return ApplicationErrors.NoId.AsResult(nameof(rq.ReportTo));
            }

            // Name
            var name = await _db.Persons
                .AsNoTracking()
                .Where(ou => ou.Id == rq.Id)
                .Select(ou => ou.Name)
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

            if (rq.IsModified(nameof(rq.LocalName)) && !string.IsNullOrEmpty(rq.LocalName))
            {
                ou.Name = rq.LocalName;
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

            if (rq.IsModified(nameof(rq.ReportTo)) && isNotSelf)
            {
                ou.ReportTo = rq.ReportTo;
            }

            // Changes
            var changes = _db.ChangeTracker.Entries().GetChangedProperties();

            // Save
            await _db.SaveChangesAsync(cancellationToken);

            // Push message
            var message = new UpdateMemberMessage
            {
                Data = User.CreateMessageData(App.AppId, rq.Id, name),
                Changes = changes
            };
            await _queueService.FirePushAsync(message, PlatformSharedContext.Default.UpdateMemberMessage, cancellationToken);

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
            var ou = await _db.Persons.Users(User.OrganizationInt).AsNoTracking()
                .Where(ou => ou.Id == id)
                .Select(ou => new { LocalAvatar = ou.Avatar, ou.Name })
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
                await _db.Persons.Where(ou => ou.Id == id).ExecuteUpdateAsync(o => o.SetProperty(o => o.Avatar, url), cancellationToken);

                // Remove current avatar
                if (!string.IsNullOrEmpty(ou.LocalAvatar))
                    await _storage.DeleteUrlAsync(ou.LocalAvatar, cancellationToken);

                // Push message
                var message = new UpdateMemberAvatarMessage
                {
                    Data = User.CreateMessageData(App.AppId, id, ou.Name)
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
        /// <param name="id">Member id</param>
        /// <param name="writer">Writer to hold the data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task UpdateReadAsync(int id, IBufferWriter<byte> writer, CancellationToken cancellationToken = default)
        {
            var query = _db.Persons.Users(User.OrganizationInt)
                .AsNoTracking()
                .Where(ou => ou.Id == id
                    && ou.UserRole <= User.Role);

            var (hasContent, _) = await query.Select(ou => new
            {
                ou.Id,
                ou.CoreUser!.Name,
                IsSelf = ou.CoreUserId == User.IdInt,
                ou.UserRole,
                LocalName = ou.Name,
                ou.AssignedId,
                ou.Expiry,
                ou.Status,
                ou.ReportTo
            }).ToJsonObjectAsync(writer, cancellationToken: cancellationToken);
        }
    }
}
