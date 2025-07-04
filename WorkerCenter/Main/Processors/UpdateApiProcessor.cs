using com.etsoo.ApiModel;
using com.etsoo.ApiModel.Dto.SmartERP.MessageQueue;
using com.etsoo.CoreFramework.Authentication;
using com.etsoo.MessageQueue;
using PlatformShared;
using PlatformShared.Database;
using PlatformShared.Database.Models;
using PlatformShared.Extentions;
using PlatformShared.Messages;
using WorkerCenter.Templates;

namespace WorkerCenter.Main.Processors
{
    /// <summary>
    /// Update external API processor
    /// 更新外部接口处理器
    /// </summary>
    public class UpdateApiProcessor : LogQueueProcessor<UpdateApiMessage>
    {
        private readonly MyDbContext _db;
        private readonly IMessageQueueProducer _producer;

        public UpdateApiProcessor(ILogger<UpdateApiProcessor> logger, LogDbContext logDb,
            MyDbContext db, IMessageQueueProducer producer)
            : base(logger, PlatformSharedContext.Default.UpdateApiMessage, logDb)
        {
            _producer = producer;
            _db = db;
        }

        protected override async Task ProcessMessageAsync(UpdateApiMessage message, MessageReceivedProperties properties, CancellationToken cancellationToken)
        {
            await base.ProcessMessageAsync(message, properties, cancellationToken);

            // User id
            var userId = message.Data.UserId;

            // Organization id
            var orgId = message.OrganizationId;

            // All admins
            var admins = await _db.QueryUsersAsync(orgId, UserRole.Admin, cancellationToken);

            // Send email notice
            // Emails
            var allEmails = (await _db.QueryUserIdentifiersAsync(CoreUserIdentifierType.Email, cancellationToken, [userId], admins));
            var emails = allEmails[0];
            var adminEmails = allEmails[1];

            if (emails.Length > 0 || adminEmails.Length > 0)
            {
                // Load email template
                var subject = Properties.Resources.ActionNoticeSubject;
                var action = Properties.Resources.UpdateApi + " - " + message.Data.TargetName;

                if (!orgId.Equals(message.Data.OrganizationId))
                {
                    action += $" ({Properties.Resources.PlatformAdminOperation})";
                }

                var data = new ActionNoticeData(message.Data,
                    string.Format(subject, action),
                    action
                );

                var body = await TemplateUtils.BuildTemplateAsync(TemplateUtils.ActionNoticeTemplate, data);

                // Send email notice
                var email = new SendEmailMessage
                {
                    Subject = data.Subject,
                    Body = body,
                    To = emails,
                    Cc = adminEmails,
                    Importance = EmailImportance.High
                };

                await _producer.SendJsonAsync(email, ApiModelJsonSerializerContext.Default.SendEmailMessage, SendEmailMessage.Type, cancellationToken);
            }
        }
    }
}
