using com.etsoo.MessageQueue;
using com.etsoo.MessageQueue.QueueProcessors;
using Microsoft.EntityFrameworkCore;
using PlatformShared;
using PlatformShared.Database;
using PlatformShared.Database.Models;
using PlatformShared.LogDatabase.Models;
using PlatformShared.Messages;
using System.Globalization;
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

        private async Task LogAsync(AcceptInvitationMessage message, int userId, string kind, CancellationToken cancellationToken)
        {
            var data = message.Data;

            var title = Properties.Resources.ResourceManager.GetString(kind) ?? kind;
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
                OrganizationId = data.OrganizationId,
                Title = title,
                UserId = userId,
                Kind = kind
            };
            _logDb.CoreLogs.Add(log);

            await _logDb.SaveChangesAsync(cancellationToken);
        }

        protected override async Task ProcessMessageAsync(AcceptInvitationMessage message, MessageReceivedProperties properties, CancellationToken cancellationToken)
        {
            // Notice inviter
            // 通知邀请人
            var inviterId = message.InviterId;

            // Inviter name
            var inviter = HttpUtility.HtmlEncode(message.UserData.Name);

            // Invitee name
            var invitee = HttpUtility.HtmlEncode(message.Data.UserName);

            // Organization name
            var orgName = HttpUtility.HtmlEncode(message.UserData.OrganizationName);

            var culture = message.Data.Culture;
            var ci = CultureInfo.GetCultureInfo(culture);
            var subject = Properties.Resources.ResourceManager.GetString(nameof(Properties.Resources.ActionNoticeSubject), ci)!;

            // Send email notice
            var inviterEmails = await _db.CoreUserIdentifiers
                .AsNoTracking()
                .Where(i => i.CoreUserId == inviterId && i.Type == CoreUserIdentifierType.Email)
                .Select(i => i.Value)
                .ToArrayAsync(cancellationToken);

            if (inviterEmails.Length > 0)
            {
                // Log
                await LogAsync(message, inviterId, nameof(Properties.Resources.InvitationAccepted), cancellationToken);

                var action = Properties.Resources.ResourceManager.GetString(nameof(Properties.Resources.InvitationAccepted), ci)!;
                var detail = Properties.Resources.ResourceManager.GetString(nameof(Properties.Resources.InvitationAcceptedDetail), ci)!;

                // Load email template
                var data = new ActionNoticeData
                {
                    Language = culture,
                    Subject = string.Format(subject, $"{action} - {invitee}"),
                    Action = action,
                    Detail = string.Format(detail, invitee, orgName),
                    IP = message.Data.IP,
                    UserName = inviter,
                    TimeZone = message.Data.TimeZone,
                    TimeStamp = message.Data.TimeStamp
                };

                var body = await TemplateUtils.BuildTemplateAsync(TemplateUtils.ActionNoticeTemplate, data);

                // Send email notice
                var email = new SendEmailMessage
                {
                    Subject = data.Subject,
                    Body = body,
                    To = inviterEmails
                };

                await _producer.SendJsonAsync(email, PlatformSharedContext.Default.SendEmailMessage, SendEmailMessage.Type, cancellationToken);
            }

            // 通知受邀人
            var userId = message.Data.UserId;

            var emails = await _db.CoreUserIdentifiers
                .AsNoTracking()
                .Where(i => i.CoreUserId == userId && i.Type == CoreUserIdentifierType.Email)
                .Select(i => i.Value)
                .ToArrayAsync(cancellationToken);

            if (emails.Length > 0)
            {
                // Log
                await LogAsync(message, userId, nameof(Properties.Resources.AcceptInvitation), cancellationToken);

                var action = Properties.Resources.ResourceManager.GetString(nameof(Properties.Resources.AcceptInvitation), ci)!;
                var detail = Properties.Resources.ResourceManager.GetString(nameof(Properties.Resources.AcceptInvitationDetail), ci)!;

                // Load email template
                var data = new ActionNoticeData
                {
                    Language = culture,
                    Subject = string.Format(subject, $"{action} - {inviter}"),
                    Action = action,
                    Detail = string.Format(detail, inviter, orgName),
                    IP = message.Data.IP,
                    UserName = invitee,
                    TimeZone = message.Data.TimeZone,
                    TimeStamp = message.Data.TimeStamp
                };

                var body = await TemplateUtils.BuildTemplateAsync(TemplateUtils.ActionNoticeTemplate, data);

                // Send email notice
                var email = new SendEmailMessage
                {
                    Subject = data.Subject,
                    Body = body,
                    To = emails
                };

                await _producer.SendJsonAsync(email, PlatformSharedContext.Default.SendEmailMessage, SendEmailMessage.Type, cancellationToken);
            }
        }
    }
}
