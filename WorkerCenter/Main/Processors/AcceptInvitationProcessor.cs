using com.etsoo.ApiModel;
using com.etsoo.ApiModel.Dto.SmartERP.MessageQueue;
using com.etsoo.CoreFramework.Authentication;
using com.etsoo.Localization;
using com.etsoo.MessageQueue;
using com.etsoo.MessageQueue.QueueProcessors;
using PlatformShared;
using PlatformShared.Database;
using PlatformShared.Database.Models;
using PlatformShared.Extentions;
using PlatformShared.LogDatabase.Models;
using PlatformShared.Messages;
using System.Net;
using System.Web;
using WorkerCenter.Templates;

namespace WorkerCenter.Main.Processors
{
    /// <summary>
    /// Accept invitation processor
    /// 接受邀请处理器
    /// </summary>
    public class AcceptInvitationProcessor : CommonQueueProcessor<AcceptInvitationMessage>
    {
        private readonly LogDbContext _logDb;
        private readonly MyDbContext _db;
        private readonly IMessageQueueProducer _producer;

        public AcceptInvitationProcessor(ILogger<AcceptInvitationProcessor> logger,
            LogDbContext logDb,
            MyDbContext db,
            IMessageQueueProducer producer)
            : base(logger, PlatformSharedContext.Default.AcceptInvitationMessage)
        {
            _producer = producer;
            _logDb = logDb;
            _db = db;
        }

        private async Task LogAsync(AcceptInvitationMessage message, int userId, string kind, string kindText, CancellationToken cancellationToken)
        {
            var data = message.Data;
            var orgId = message.UserData.OrganizationId;

            var title = kindText;
            if (message.Data.UserId == userId)
            {
                // Invitee
                // 受邀人
                title = $"{title} - {message.UserData.Name}";
            }
            else
            {
                title = $"{title} - {message.Data.UserName}";
            }

            var log = new CoreLog
            {
                Culture = data.Culture,
                Data = message.GetMoreData(),
                DeviceId = data.DeviceId,
                Ip = IPAddress.Parse(data.IP),
                OrganizationId = orgId,
                Title = title,
                UserId = userId,
                Kind = kind,
                AppId = data.AppId
            };
            _logDb.CoreLogs.Add(log);

            await _logDb.SaveChangesAsync(cancellationToken);
        }

        protected override async Task ProcessMessageAsync(AcceptInvitationMessage message, MessageReceivedProperties properties, CancellationToken cancellationToken)
        {
            // Notice inviter
            // 通知邀请人
            var inviterId = (int)message.Data.TargetId;
            var inviter = HttpUtility.HtmlEncode(message.UserData.Name);

            // Invitee
            var userId = message.Data.UserId;
            var invitee = HttpUtility.HtmlEncode(message.Data.UserName);

            // Organization name
            var orgName = HttpUtility.HtmlEncode(message.UserData.OrganizationName);

            // Organization owner
            var organizationId = message.UserData.OrganizationId;

            // Owners
            var owners = await _db.QueryUsersAsync(organizationId, UserRole.Founder, cancellationToken);

            var ci = LocalizationUtils.SetCulture(message.Data.Culture, true);
            Properties.Resources.Culture = ci;

            var subject = Properties.Resources.ActionNoticeSubject;

            // Emails
            var emails = await _db.QueryUserIdentifiersAsync(CoreUserIdentifierType.Email, cancellationToken, [userId], [inviterId], owners);

            // Log
            await LogAsync(message, inviterId, nameof(Properties.Resources.InvitationAccepted), Properties.Resources.InvitationAccepted, cancellationToken);

            var inviterEmails = emails[1];
            var ownerEmails = emails[2];

            if (inviterEmails.Length > 0 || ownerEmails.Length > 0)
            {
                var action = Properties.Resources.InvitationAccepted;
                var detail = Properties.Resources.InvitationAcceptedDetail;

                // Load email template
                var data = new ActionNoticeData(message.Data,
                    string.Format(subject, $"{action} - {invitee}"),
                    $"{action} ({inviter})",
                    string.Format(detail, invitee, orgName)
                );

                var body = await TemplateUtils.BuildTemplateAsync(TemplateUtils.ActionNoticeTemplate, data, cancellationToken);

                // Send email notice
                var email = new SendEmailMessage
                {
                    Subject = data.Subject,
                    Body = body,
                    To = inviterEmails,
                    Cc = ownerEmails
                };

                await _producer.SendJsonAsync(email, ApiModelJsonSerializerContext.Default.SendEmailMessage, SendEmailMessage.Type, cancellationToken);
            }

            // Log
            await LogAsync(message, userId, nameof(Properties.Resources.AcceptInvitation), Properties.Resources.AcceptInvitation, cancellationToken);

            // 通知受邀人
            var inviteeEmails = emails[0];
            if (inviteeEmails.Length > 0)
            {
                var action = Properties.Resources.AcceptInvitation;
                var detail = Properties.Resources.AcceptInvitationDetail;

                // Load email template
                var inviteeData = new ActionNoticeData(message.Data,
                    string.Format(subject, $"{action} - {inviter}"),
                    action,
                    string.Format(detail, inviter, orgName)
                );

                var inviteeBody = await TemplateUtils.BuildTemplateAsync(TemplateUtils.ActionNoticeTemplate, inviteeData, cancellationToken);

                // Send email notice
                var inviteeeEmail = new SendEmailMessage
                {
                    Subject = inviteeData.Subject,
                    Body = inviteeBody,
                    To = inviteeEmails
                };

                await _producer.SendJsonAsync(inviteeeEmail, ApiModelJsonSerializerContext.Default.SendEmailMessage, SendEmailMessage.Type, cancellationToken);
            }
        }
    }
}
