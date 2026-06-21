using com.etsoo.ApiModel;
using com.etsoo.ApiModel.Dto.SmartERP.MessageQueue;
using com.etsoo.CoreFramework.Authentication;
using com.etsoo.MessageQueue;
using PlatformShared;
using PlatformShared.Database;
using PlatformShared.Database.Models;
using PlatformShared.Extentions;
using PlatformShared.Messages;
using System.Web;
using WebTemplates;
using PlatformShared.Dto;
using Microsoft.EntityFrameworkCore;

namespace WorkerCenter.Main.Processors
{
    /// <summary>
    /// Delete member processor
    /// 删除成员处理器
    /// </summary>
    public class DeleteMemberProcessor : LogQueueProcessor<DeleteMemberMessage>
    {
        private readonly IDbContextFactory<MyDbContext> _dbFactory;
        private readonly IMessageQueueProducer _producer;

        public DeleteMemberProcessor(ILogger<DeleteMemberProcessor> logger,
            IDbContextFactory<LogDbContext> logDbFactory,
            IDbContextFactory<MyDbContext> dbFactory,
            IMessageQueueProducer producer)
            : base(logger, PlatformSharedContext.Default.DeleteMemberMessage, logDbFactory)
        {
            _producer = producer;
            _dbFactory = dbFactory;
        }

        protected override async Task ProcessMessageAsync(DeleteMemberMessage message, MessageReceivedProperties properties, CancellationToken cancellationToken)
        {
            // Log
            await base.ProcessMessageAsync(message, properties, cancellationToken);

            // User name
            var userName = HttpUtility.HtmlEncode(message.Data.UserName);

            // Invitee name
            var inviteeName = HttpUtility.HtmlEncode(message.Data.TargetName);

            // Inviter name
            var inviterName = HttpUtility.HtmlEncode(message.InviterName);

            // Organization owner
            var organizationId = message.Data.OrganizationId.GetValueOrDefault();

            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);

            // Owners
            var owners = await db.QueryUsersAsync(organizationId, UserRole.Founder, cancellationToken);

            // Organization name
            var orgName = HttpUtility.HtmlEncode(message.OrgName);

            var subject = Properties.Resources.ActionNoticeSubject;

            // Emails
            var allEmails = await db.QueryUserIdentifiersAsync(CoreUserIdentifierType.Email, cancellationToken, [message.Data.UserId], [(int)message.Data.TargetId], owners);
            var emails = allEmails[0];
            var inviteeEmails = allEmails[1];
            var ownerEmails = allEmails[2];

            if (emails.Length > 0 || inviteeEmails.Length > 0 || ownerEmails.Length > 0)
            {
                var action = Properties.Resources.DeleteMember;
                action = $"{action} {inviteeName}";
                var detail = Properties.Resources.DeleteMemberDetail;

                // Load email template
                var data = new ActionNoticeData(message.Data,
                    string.Format(subject, action),
                    action,
                    string.Format(detail, userName, orgName, inviteeName, inviterName)
                );

                var body = await TemplateUtils.BuildActionNoticeAsync(message.Data.Culture, data);

                // Send email notice
                var email = new SendEmailMessage
                {
                    Subject = data.Subject,
                    Body = body,
                    To = emails,
                    Cc = inviteeEmails,
                    Bcc = ownerEmails
                };

                await _producer.SendJsonAsync(email, ApiModelJsonSerializerContext.Default.SendEmailMessage, SendEmailMessage.Type, cancellationToken);
            }
        }
    }
}
