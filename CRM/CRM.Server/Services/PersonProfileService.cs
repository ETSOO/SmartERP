using com.etsoo.ApiProxy.Defs;
using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Authentication;
using com.etsoo.CoreFramework.Business;
using com.etsoo.CoreFramework.Models;
using com.etsoo.CoreFramework.User;
using com.etsoo.Database;
using com.etsoo.ServiceApp.SmartERP;
using com.etsoo.Utils.Actions;
using CRM.Server.Dto.Person;
using CRM.Server.Dto.PersonProfile;
using CRM.Server.RQ;
using CRM.Server.RQ.PersonProfile;
using Microsoft.EntityFrameworkCore;
using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.Person;
using PlatformShared.Database;
using PlatformShared.Database.Models;
using PlatformShared.Extentions;
using PlatformShared.Services;
using System.Buffers;

namespace CRM.Server.Services
{
    /// <summary>
    /// Person profile service
    /// 人员档案服务
    /// </summary>
    public class PersonProfileService : SEUserService, IPersonProfileService
    {
        readonly MyDbContext _db;
        readonly IQueueService _queueService;
        readonly ISmartERPProxy _core;
        readonly ICommonService _commonService;

        public PersonProfileService(
            MyDbContext db,
            ISEServiceApp app,
            CurrentUserAccessor userAccessor,
            ILogger<PersonService> logger,
            IQueueService queueService,
            ISmartERPProxy core,
            ICommonService commonService
        )
            : base(app, userAccessor.UserSafe, "person_profile", logger)
        {
            _db = db;
            _queueService = queueService;
            _core = core;
            _commonService = commonService;
        }

        /// <summary>
        /// Create person profile
        /// 创建人员档案
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="indexKey">Index key</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> CreateAsync(PersonProfileCreateRQ rq, string? indexKey = null, CancellationToken cancellationToken = default)
        {
            // Organization id
            var orgId = User.OrganizationInt;

            // Validate person ids
            var persons = rq.Persons?.Distinct().ToList();
            var ids = new List<long>([rq.PersonId]);
            if (persons != null)
            {
                ids.AddRange(persons);
            }
            if (rq.AssigneeId.HasValue)
            {
                ids.Add(rq.AssigneeId.Value);
            }

            if (!(await _db.CheckPersonsAsync(ids, orgId, cancellationToken)))
            {
                return ApplicationErrors.NoId.AsResult();
            }

            // Validate the order id
            if (rq.OrderId.HasValue
                && !(await _db.CheckOrdersAsync([rq.OrderId.Value], orgId, cancellationToken)))
            {
                return ApplicationErrors.NoId.AsResult(nameof(rq.OrderId));
            }

            // No same title in a month
            var sameTitle = await _db.UserProfiles(User).AsNoTracking()
                .Where(p => p.Title == rq.Title && p.HappenDate >= DateTimeOffset.UtcNow.AddMonths(-1))
                .AnyAsync(cancellationToken);

            if (sameTitle)
            {
                return ApplicationErrors.ItemExists.AsResult(nameof(rq.Title));
            }

            // Person identity type
            var identityType = await _db.Persons.AsNoTracking()
                .Where(p => p.Id == rq.PersonId)
                .Select(p => p.IdentityType)
                .FirstOrDefaultAsync(cancellationToken);

            if (!await _commonService.HasIdentityPermissionAsync(identityType, nameof(Permissions.Org.AddProfile), cancellationToken))
            {
                return ApplicationErrors.AccessDenied.AsResult(nameof(identityType));
            }

            var happenDate = rq.HappenDate.GetValueOrDefault(DateTimeOffset.UtcNow).ToUniversalTime();

            // Add 1 hour default for schedule profile
            var happenDateEnd = rq.Kind == PersonProfileKind.Schedule
                ? rq.HappenDateEnd.GetValueOrDefault(happenDate.AddHours(1)).ToUniversalTime()
                : rq.HappenDateEnd?.ToUniversalTime();

            // Format content
            var comment = await _core.Org.FormatHtmlContentAsync(rq.Auth, rq.Comment, cancellationToken);

            var profile = new PersonProfile
            {
                PersonId = rq.PersonId,
                Persons = persons,
                OrderId = rq.OrderId,
                Kind = rq.Kind,
                Title = rq.Title,
                Comment = comment,
                Location = rq.Location,
                LocationId = rq.LocationId,
                HappenDate = happenDate,
                HappenDateEnd = happenDateEnd,
                UserId = User.Oid,
                UserRole = rq.UserRole,
                Status = rq.Status,
                Data = rq.Data,
                Importance = rq.Importance,
                AssigneeId = rq.AssigneeId,
                IndexKey = indexKey
            };

            // Add
            _db.PersonProfiles.Add(profile);

            // Save
            await _db.SaveChangesAsync(cancellationToken);

            // Return
            return ActionResult.Succeed(profile.Id);
        }

        /// <summary>
        /// Create person task
        /// 创建人员任务
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public Task<IActionResult> CreateTaskAsync(PersonTaskCreateRQ rq, CancellationToken cancellationToken = default)
        {
            return CreateAsync(rq.ProfileFromTask(User.Oid), null, cancellationToken);
        }

        private IQueryable<PersonProfile> CreateQuery(PersonProfileListRQ rq, IdentityTypeFlags identity, bool all, Func<IQueryable<PersonProfile>, IQueryable<PersonProfile>>? filters = null)
        {
            var query = _db.UserProfiles(User).AsNoTracking()
                .QueryEtsoo(rq, (p) => p.Id, (p) => p.Status, (q) =>
                {
                    if (rq.IdentityType.HasValue)
                    {
                        var value = rq.IdentityType.Value;
                        if (value == IdentityTypeFlags.None)
                            q = q.Where(p => p.Person.IdentityType == IdentityTypeFlags.None);
                        else
                            q = q.Where(p => (p.Person.IdentityType & value) == value);
                    }
                    else if (!all)
                    {
                        q = q.Where(p => (p.Person.IdentityType & identity) > 0);
                    }

                    if (rq.PersonId.HasValue)
                    {
                        q = q.Where(p => p.PersonId == rq.PersonId.Value);
                    }

                    if (rq.ParticipantId.HasValue)
                    {
                        var participantId = rq.ParticipantId.Value;
                        q = q.Where(p => p.PersonId == participantId || p.UserId == participantId || p.AssigneeId == participantId || (p.Persons != null && p.Persons.Contains(participantId)));
                    }

                    if (rq.UserId.HasValue)
                    {
                        q = q.Where(p => p.UserId == rq.UserId.Value);
                    }

                    if (rq.Kind.HasValue)
                    {
                        q = q.Where(p => p.Kind == rq.Kind.Value);
                    }

                    if (rq.OrderId.HasValue)
                    {
                        q = q.Where(p => p.OrderId == rq.OrderId.Value);
                    }

                    if (rq.HappenDateStart.HasValue)
                    {
                        q = q.Where(p => p.HappenDateEnd >= rq.HappenDateStart.Value);
                    }

                    if (rq.HappenDateEnd.HasValue)
                    {
                        q = q.Where(p => p.HappenDate < rq.HappenDateEnd.Value);
                    }

                    if (rq.CreationStart.HasValue)
                    {
                        q = q.Where(p => p.Creation >= rq.CreationStart.Value);
                    }

                    if (rq.CreationEnd.HasValue)
                    {
                        q = q.Where(p => p.Creation < rq.CreationEnd.Value);
                    }

                    if (rq.Importance.HasValue)
                    {
                        q = q.Where(p => p.Importance == rq.Importance.Value);
                    }

                    if (rq.AssigneeId.HasValue)
                    {
                        q = q.Where(p => p.AssigneeId == rq.AssigneeId.Value);
                    }

                    if (rq.IsTask is true)
                    {
                        q = q.Where(p => p.Kind == PersonProfileKind.Schedule || p.Kind == PersonProfileKind.Agile);
                    }
                    else if (rq.IsTask is false)
                    {
                        q = q.Where(p => p.Kind != PersonProfileKind.Schedule && p.Kind != PersonProfileKind.Agile);
                    }

                    if (rq.Keyword?.Length > 1)
                    {
                        var keyword = rq.Keyword;

                        if (keyword.IsComplexQueryKeywords())
                        {
                            q = q.QueryEtsooKeywords(keyword, DbUtils.ILikeMethod, p => p.Title, p => p.Comment);
                        }
                        else
                        {
                            q = q.Where(p => EF.Functions.ILike(p.Title, $"%{keyword}%")
                            || EF.Functions.ILike(p.Comment, $"%{keyword}%"));
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
        /// Create person profile link
        /// 创建人员档案关联
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> CreateLinkAsync(PersonProfileLinkCreateRQ rq, CancellationToken cancellationToken = default)
        {
            // Validate profiles
            List<long> ids = [rq.ProfileId];
            if (rq.TargetProfileId.HasValue)
            {
                ids.Add(rq.TargetProfileId.Value);
            }

            if (await _db.UserProfiles(User, ids).AsNoTracking().CountAsync(cancellationToken) != ids.Count)
            {
                return ApplicationErrors.NoId.AsResult();
            }

            // Get identity type
            var identityType = await _db.PersonProfiles.AsNoTracking()
                .Where(p => p.Id == rq.ProfileId)
                .Select(p => p.Person.IdentityType)
                .FirstOrDefaultAsync(cancellationToken);

            // Check permission
            if (!await _commonService.HasIdentityPermissionAsync(identityType, nameof(Permissions.Org.AddComment), cancellationToken))
            {
                return ApplicationErrors.AccessDenied.AsResult(nameof(identityType));
            }

            // Content
            var content = string.IsNullOrEmpty(rq.Content)
                ? null
                : await _core.Org.FormatHtmlContentAsync(rq.Auth, rq.Content, cancellationToken);

            // Create link
            var link = new PersonProfileLink
            {
                ProfileId = rq.ProfileId,
                TargetProfileId = rq.TargetProfileId,
                Kind = rq.Kind,
                Content = content,
                UserId = User.Oid
            };

            // Add
            _db.PersonProfileLinks.Add(link);
            await _db.SaveChangesAsync(cancellationToken);

            return ActionResult.Succeed(link.Id);
        }

        /// <summary>
        /// Delete attachment
        /// 删除附件
        /// </summary>
        /// <param name="id">Attachment id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> DeleteAttachmentAsync(long id, CancellationToken cancellationToken = default)
        {
            var result = await _db.PersonProfileAttachments.AsNoTracking()
                .CheckAttachmentEditable(User, id)
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
        /// Delete link
        /// 删除链接
        /// </summary>
        /// <param name="id">Link id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> DeleteLinkAsync(long id, CancellationToken cancellationToken = default)
        {
            var result = await _db.PersonProfileLinks.AsNoTracking()
                .CheckLinkEditable(User, id)
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
        /// List person JSON data
        /// 人员列表JSON数据
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="writer">Writer to hold the data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task ListAsync(PersonProfileListRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default)
        {
            var (identityType, all) = await _commonService.GetProfileIdentityTypeAsync(cancellationToken);
            if (identityType == IdentityTypeFlags.None)
            {
                return;
            }

            if (rq.IdentityType.HasValue && (identityType & rq.IdentityType.Value) == 0)
            {
                return;
            }

            FormatListRQ(rq);

            var query = CreateQuery(rq, identityType, all);

            await query.Select(p => new PersonProfileListData
            {
                Id = p.Id,
                Title = p.Title,
                Creation = p.Creation
            }).ToJsonAsync(writer, cancellationToken: cancellationToken);
        }

        private void FormatListRQ(PersonProfileListRQ rq)
        {
            if (rq.ParticipantId == 0)
            {
                rq.ParticipantId = User.Oid;
            }

            if (rq.PersonId == 0)
            {
                rq.PersonId = User.Oid;
            }
        }

        /// <summary>
        /// Query person JSON data
        /// 查询人员JSON数据
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="writer">Writer to hold the data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task QueryAsync(PersonProfileQueryRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default)
        {
            var (identityType, all) = await _commonService.GetProfileIdentityTypeAsync(cancellationToken);
            if (identityType == IdentityTypeFlags.None)
            {
                return;
            }

            if (rq.IdentityType.HasValue && rq.IdentityType.Value != IdentityTypeFlags.None && (identityType & rq.IdentityType.Value) == 0)
            {
                return;
            }

            FormatListRQ(rq);

            var query = CreateQuery(rq, identityType, all, (q) =>
            {
                if (!string.IsNullOrEmpty(rq.Location))
                {
                    q = q.Where(p => p.Location != null && EF.Functions.ILike(p.Location, $"%{rq.Location}%"));
                }

                return q;
            });

            var oid = User.Oid;

            var (hasContent, commandText) = await query.Select(p => new PersonProfileQueryData
            {
                Id = p.Id,
                Kind = p.Kind,
                Title = p.Title,
                UserName = p.User.Name,
                HappenDate = p.HappenDate,
                HappenDateEnd = p.HappenDateEnd,
                Importance = p.Importance,
                IsSelf = p.UserId == oid,
                Status = p.Status,
                Creation = p.Creation
            }).ToJsonAsync(writer, cancellationToken: cancellationToken);

            if (_db.IsSensitiveDataLoggingEnabled && Logger.IsEnabled(LogLevel.Information))
            {
                Logger.LogInformation("QueryAsync is {hasContent} with {commandText}", hasContent, commandText);
            }
        }

        /// <summary>
        /// Read person profile data for view
        /// 读取用于浏览的人员档案数据
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<PersonProfileViewData?> ReadAsync(long id, CancellationToken cancellationToken = default)
        {
            // Person identity type
            var identityType = await _db.PersonProfiles.AsNoTracking()
                .Where(p => p.Id == id)
                .Select(p => p.Person.IdentityType)
                .FirstOrDefaultAsync(cancellationToken);

            if (!await _commonService.HasIdentityPermissionAsync(identityType, nameof(Permissions.Org.ViewProfile), cancellationToken))
            {
                return null;
            }

            var orgId = User.OrganizationInt;
            var oid = User.Oid;
            var isAdmin = User.Role >= UserRole.Admin;

            return await _db.UserProfiles(User, id).AsNoTracking()
                .Select(p => new PersonProfileViewData
                {
                    Id = p.Id,
                    PersonId = p.PersonId,
                    PersonName = p.Person.Name,
                    PersonIdentityType = p.Person.IdentityType,
                    Persons = p.Persons == null ? null : _db.Persons.Where(o => p.Person.OrgId == orgId && p.Persons.Contains(o.Id)).Select(o => new PersonListItem { Id = o.Id, Name = o.Name }).ToList(),
                    OrderId = p.OrderId,
                    OrderTitle = p.Order == null ? null : p.Order.Title,
                    Kind = p.Kind,
                    Title = p.Title,
                    Comment = p.Comment,
                    Location = p.Location,
                    HappenDate = p.HappenDate,
                    HappenDateEnd = p.HappenDateEnd,
                    UserId = p.UserId,
                    UserName = p.User.Name,
                    UserRole = p.UserRole,
                    Status = p.Status,
                    Data = p.Data,
                    Creation = p.Creation,
                    IndexKey = p.IndexKey,
                    Importance = p.Importance,
                    AssigneeId = p.AssigneeId,
                    AssigneeName = p.Assignee == null ? null : p.Assignee.Name,
                    IsAdmin = isAdmin,
                    IsSelf = p.UserId == oid,
                    Links = _db.PersonProfileLinks.Where(l => l.ProfileId == p.Id).Select(l => new PersonProfileLinkItem
                    {
                        Id = l.Id,
                        Kind = l.Kind,
                        TargetProfileId = l.TargetProfileId,
                        TargetProfileTitle = l.TargetProfile == null ? null : l.TargetProfile.Title,
                        Content = l.Content,
                        UserId = l.UserId,
                        UserName = l.User.Name,
                        Creation = l.Creation,
                        IsSelf = l.UserId == oid
                    }).Take(16).ToList(),
                    Attachments = _db.PersonProfileAttachments.Where(a => a.ProfileId == p.Id).Select(a => new PersonProfileAttachmentItem
                    {
                        Id = a.Id,
                        FileSize = a.FileSize,
                        ContentType = a.ContentType,
                        Description = a.Description,
                        Extension = Path.GetExtension(a.FileName),
                        UserId = a.UserId,
                        UserName = a.User.Name,
                        Creation = a.Creation,
                        IsSelf = a.UserId == oid
                    }).Take(16).ToList()
                }).FirstOrDefaultAsync(cancellationToken);
        }

        /// <summary>
        /// Read inner person profile data for view
        /// 读取用于浏览的人员档案里层数据，用于查询浏览界面
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<PersonProfileInnerViewData?> ReadInnerAsync(long id, CancellationToken cancellationToken = default)
        {
            // Person identity type
            var identityType = await _db.PersonProfiles.AsNoTracking()
                .Where(p => p.Id == id)
                .Select(p => p.Person.IdentityType)
                .FirstOrDefaultAsync(cancellationToken);

            if (!await _commonService.HasIdentityPermissionAsync(identityType, nameof(Permissions.Org.ViewProfile), cancellationToken))
            {
                return null;
            }

            var orgId = User.OrganizationInt;
            var oid = User.Oid;
            var isAdmin = User.Role >= UserRole.Admin;

            return await _db.UserProfiles(User, id).AsNoTracking()
                .Select(p => new PersonProfileInnerViewData
                {
                    PersonId = p.PersonId,
                    PersonName = p.Person.Name,
                    PersonIdentityType = p.Person.IdentityType,
                    Persons = p.Persons == null ? null : _db.Persons.Where(o => p.Person.OrgId == orgId && p.Persons.Contains(o.Id)).Select(o => new PersonListItem { Id = o.Id, Name = o.Name }).ToList(),
                    OrderId = p.OrderId,
                    OrderTitle = p.Order == null ? null : p.Order.Title,
                    Comment = p.Comment,
                    Location = p.Location,
                    UserId = p.UserId,
                    UserRole = p.UserRole,
                    Data = p.Data,
                    IndexKey = p.IndexKey,
                    AssigneeId = p.AssigneeId,
                    AssigneeName = p.Assignee == null ? null : p.Assignee.Name,
                    IsAdmin = isAdmin,
                    Links = _db.PersonProfileLinks.Where(l => l.ProfileId == p.Id).Select(l => new PersonProfileLinkItem
                    {
                        Id = l.Id,
                        Kind = l.Kind,
                        TargetProfileId = l.TargetProfileId,
                        TargetProfileTitle = l.TargetProfile == null ? null : l.TargetProfile.Title,
                        Content = l.Content,
                        UserId = l.UserId,
                        UserName = l.User.Name,
                        Creation = l.Creation,
                        IsSelf = l.UserId == oid
                    }).Take(16).ToList(),
                    Attachments = _db.PersonProfileAttachments.Where(a => a.ProfileId == p.Id).Select(a => new PersonProfileAttachmentItem
                    {
                        Id = a.Id,
                        FileSize = a.FileSize,
                        ContentType = a.ContentType,
                        Description = a.Description,
                        Extension = Path.GetExtension(a.FileName),
                        UserId = a.UserId,
                        UserName = a.User.Name,
                        Creation = a.Creation,
                        IsSelf = a.UserId == oid
                    }).Take(16).ToList()
                }).FirstOrDefaultAsync(cancellationToken);
        }

        /// <summary>
        /// Update person profile
        /// 更新人员档案
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> UpdateAsync(PersonProfileUpdateRQ rq, CancellationToken cancellationToken = default)
        {
            var profile = await _db.UserProfiles(User, rq.Id).FirstOrDefaultAsync(cancellationToken);
            if (profile == null)
            {
                return ApplicationErrors.NoId.AsResult();
            }

            var ids = new List<long>();

            if (rq.IsModified(nameof(rq.PersonId)) && rq.PersonId.HasValue)
            {
                profile.PersonId = rq.PersonId.Value;
                ids.Add(rq.PersonId.Value);
            }

            if (rq.IsModified(nameof(rq.Persons)))
            {
                var persons = rq.Persons?.Distinct().ToList();
                profile.Persons = persons;
                if (persons != null)
                {
                    ids.AddRange(persons);
                }
            }

            if (rq.IsModified(nameof(rq.OrderId)))
            {
                if (rq.OrderId.HasValue)
                {
                    if (!(await _db.CheckOrdersAsync([rq.OrderId.Value], User.OrganizationInt, cancellationToken)))
                    {
                        return ApplicationErrors.NoId.AsResult(nameof(rq.OrderId));
                    }
                }

                profile.OrderId = rq.OrderId;
            }

            if (rq.IsModified(nameof(rq.Kind)) && rq.Kind.HasValue)
            {
                profile.Kind = rq.Kind.Value;
            }

            if (rq.IsModified(nameof(rq.Title)) && !string.IsNullOrEmpty(rq.Title))
            {
                profile.Title = rq.Title;
            }

            if (rq.IsModified(nameof(rq.Comment)) && !string.IsNullOrEmpty(rq.Comment))
            {
                var comment = await _core.Org.FormatHtmlContentAsync(rq.Auth, rq.Comment, cancellationToken);
                profile.Comment = comment;
            }

            if (rq.IsModified(nameof(rq.Location)))
            {
                profile.Location = rq.Location;
            }

            if (rq.IsModified(nameof(rq.LocationId)))
            {
                profile.LocationId = rq.LocationId;
            }

            if (rq.IsModified(nameof(rq.HappenDate)) && rq.HappenDate.HasValue)
            {
                profile.HappenDate = rq.HappenDate.Value.ToUniversalTime();
            }

            if (rq.IsModified(nameof(rq.HappenDateEnd)))
            {
                profile.HappenDateEnd = rq.HappenDateEnd?.ToUniversalTime();
            }

            if (rq.IsModified(nameof(rq.UserRole)))
            {
                profile.UserRole = rq.UserRole;
            }

            if (rq.IsModified(nameof(rq.Data)))
            {
                profile.Data = rq.Data;
            }

            if (rq.IsModified(nameof(rq.Status)) && rq.Status.HasValue)
            {
                profile.Status = rq.Status.Value;
            }

            if (rq.IsModified(nameof(rq.Importance)))
            {
                profile.Importance = rq.Importance;
            }

            if (rq.IsModified(nameof(rq.AssigneeId)))
            {
                profile.AssigneeId = rq.AssigneeId;
                if (rq.AssigneeId.HasValue)
                {
                    ids.Add(rq.AssigneeId.Value);
                }
            }

            // Validate person ids
            if (!(await _db.CheckPersonsAsync(ids, User.OrganizationInt, cancellationToken)))
            {
                return ApplicationErrors.NoId.AsResult();
            }

            // Changes
            var changes = _db.ChangeTracker.Entries().GetChangedProperties();

            // Save
            await _db.SaveChangesAsync(cancellationToken);

            // Push message
            var message = new UpdatePersonProfileMessage
            {
                Data = User.CreateMessageData(App.AppId, rq.Id, profile.Title),
                Changes = changes
            };
            await _queueService.FirePushAsync(message, CrmJsonSerializerContext.Default.UpdatePersonProfileMessage, cancellationToken);

            // Return
            return ActionResult.Succeed(rq.Id);
        }

        /// <summary>
        /// Update attachment
        /// 更新附件
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> UpdateAttachmentAsync(PersonProfileAttachmentUpdateRQ rq, CancellationToken cancellationToken = default)
        {
            var result = await _db.PersonProfileAttachments.AsNoTracking()
                .CheckAttachmentEditable(User, rq.Id)
                .ExecuteUpdateAsync(a => a.SetProperty(p => p.Description, rq.Description), cancellationToken);

            if (result == 0)
            {
                return ApplicationErrors.NoId.AsResult();
            }
            else
            {
                return ActionResult.Succeed(rq.Id);
            }
        }

        /// <summary>
        /// Update person profile link
        /// 更新人员档案链接
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> UpdateLinkAsync(PersonProfileLinkUpdateRQ rq, CancellationToken cancellationToken = default)
        {
            var link = await _db.PersonProfileLinks.CheckLinkEditable(User, rq.Id).FirstOrDefaultAsync(cancellationToken);
            if (link == null)
            {
                return ApplicationErrors.NoId.AsResult();
            }

            if (rq.IsModified(nameof(rq.ProfileId)) && rq.ProfileId.HasValue)
            {
                link.ProfileId = rq.ProfileId.Value;
            }

            if (rq.IsModified(nameof(rq.TargetProfileId)))
            {
                link.TargetProfileId = rq.TargetProfileId;
            }

            if (rq.IsModified(nameof(rq.Kind)) && rq.Kind.HasValue)
            {
                link.Kind = rq.Kind.Value;
            }

            if (rq.IsModified(nameof(rq.Content)))
            {
                var content = string.IsNullOrEmpty(rq.Content)
                    ? null
                    : await _core.Org.FormatHtmlContentAsync(rq.Auth, rq.Content, cancellationToken);

                link.Content = content;
            }

            // Changes
            // var changes = _db.ChangeTracker.Entries().GetChangedProperties();

            // Save
            await _db.SaveChangesAsync(cancellationToken);

            // Return
            return ActionResult.Succeed(rq.Id);
        }

        /// <summary>
        /// Read person profile data for update
        /// 读取用于更新的人员档案数据
        /// </summary>
        /// <param name="id">Profile id</param>
        /// <param name="writer">Writer to hold the data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public Task UpdateReadAsync(long id, IBufferWriter<byte> writer, CancellationToken cancellationToken = default)
        {
            return _db.UserProfiles(User, id).AsNoTracking()
                .Select(p => new
                {
                    p.Id,
                    p.PersonId,
                    p.Persons,
                    p.OrderId,
                    p.Kind,
                    p.Title,
                    p.Comment,
                    p.Location,
                    p.LocationId,
                    p.HappenDate,
                    p.HappenDateEnd,
                    p.Status,
                    p.UserRole,
                    p.Data,
                    p.Importance,
                    p.AssigneeId
                }).ToJsonObjectAsync(writer, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Create upload files action data
        /// 创建上传文件的动作数据
        /// </summary>
        /// <param name="id">Profile id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<AppActionData?> UploadFilesActionAsync(long id, CancellationToken cancellationToken = default)
        {
            // Person identity type
            var identityType = await _db.PersonProfiles.AsNoTracking()
                .Where(p => p.Id == id)
                .Select(p => p.Person.IdentityType)
                .FirstOrDefaultAsync(cancellationToken);

            if (!await _commonService.HasIdentityPermissionAsync(identityType, nameof(Permissions.Org.ViewProfile), cancellationToken))
            {
                return null;
            }

            return App.SignAction(ServiceConstants.ActionUploadProfileFiles, id);
        }
    }
}
