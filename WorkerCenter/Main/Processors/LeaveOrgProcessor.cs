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
    /// Leave organization processor
    /// 离开机构处理器
    /// </summary>
    public class LeaveOrgProcessor : LogQueueProcessor<LeaveOrgMessage>
    {
        private readonly IDbContextFactory<MyDbContext> _dbFactory;
        private readonly IMessageQueueProducer _producer;

        public LeaveOrgProcessor(ILogger<AcceptInvitationProcessor> logger,
            IDbContextFactory<LogDbContext> logDbFactory,
            IDbContextFactory<MyDbContext> dbFactory,
            IMessageQueueProducer producer)
            : base(logger, PlatformSharedContext.Default.LeaveOrgMessage, logDbFactory)
        {
            _producer = producer;
            _dbFactory = dbFactory;
        }

        protected override async Task ProcessMessageAsync(LeaveOrgMessage message, MessageReceivedProperties properties, CancellationToken cancellationToken)
        {
            // Log
            await base.ProcessMessageAsync(message, properties, cancellationToken);

            // User name
            var userName = HttpUtility.HtmlEncode(message.Data.UserName);

            // Inviter name
            var inviterName = HttpUtility.HtmlEncode(message.InviterName);

            // Organization name
            var orgName = HttpUtility.HtmlEncode(message.OrgName);

            // Organization owner
            var organizationId = (int)message.Data.TargetId;

            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);

            // Owners
            var owners = await db.QueryUsersAsync(organizationId, UserRole.Founder, cancellationToken);

            if (message.InviterId.HasValue)
            {
                // Add the inviter to receive the notice
                owners.Add(message.InviterId.Value);
            }

            var subject = Properties.Resources.ActionNoticeSubject;

            // Emails
            var allEmails = await db.QueryUserIdentifiersAsync(CoreUserIdentifierType.Email, cancellationToken, [message.Data.UserId], owners);

            var emails = allEmails[0];
            var ownerEmails = allEmails[1];

            if (emails.Length > 0 || ownerEmails.Length > 0)
            {
                var action = Properties.Resources.LeaveOrg;
                action = $"{userName} {action}";
                var detail = Properties.Resources.LeaveOrgDetail;

                // Load email template
                var data = new ActionNoticeData(message.Data,
                    string.Format(subject, action),
                    action,
                    string.Format(detail, userName, inviterName, orgName)
                );

                var body = await TemplateUtils.BuildActionNoticeAsync(message.Data.Culture, data);

                // Send email notice
                var email = new SendEmailMessage
                {
                    Subject = data.Subject,
                    Body = body,
                    To = emails,
                    Bcc = ownerEmails
                };

                await _producer.SendJsonAsync(email, ApiModelJsonSerializerContext.Default.SendEmailMessage, SendEmailMessage.Type, cancellationToken);
            }
        }
    }
}
