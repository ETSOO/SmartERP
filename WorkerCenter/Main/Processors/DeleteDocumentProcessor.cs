using com.etsoo.ApiModel;
using com.etsoo.ApiModel.Dto.SmartERP.MessageQueue;
using com.etsoo.CoreFramework.Authentication;
using com.etsoo.MessageQueue;
using PlatformShared;
using PlatformShared.Database;
using PlatformShared.Database.Models;
using PlatformShared.Extentions;
using PlatformShared.Messages;
using WebTemplates;
using PlatformShared.Dto;

namespace WorkerCenter.Main.Processors
{
    /// <summary>
    /// Delete document processor
    /// 移除业务文档处理器
    /// </summary>
    public class DeleteDocumentProcessor : LogQueueProcessor<DeleteDocumentMessage>
    {
        private readonly MyDbContext _db;
        private readonly IMessageQueueProducer _producer;

        public DeleteDocumentProcessor(ILogger<DeleteDocumentProcessor> logger, LogDbContext logDb,
            MyDbContext db, IMessageQueueProducer producer)
            : base(logger, PlatformSharedContext.Default.DeleteDocumentMessage, logDb)
        {
            _producer = producer;
            _db = db;
        }

        protected override async Task ProcessMessageAsync(DeleteDocumentMessage message, MessageReceivedProperties properties, CancellationToken cancellationToken)
        {
            await base.ProcessMessageAsync(message, properties, cancellationToken);

            // User id
            var userId = message.Data.UserId;

            // Organization id
            var orgId = message.OrganizationId;

            // All admins
            var admins = orgId.HasValue ? await _db.QueryUsersAsync(orgId.Value, UserRole.Admin, cancellationToken) : [];

            // Send email notice
            // Emails
            var allEmails = (await _db.QueryUserIdentifiersAsync(CoreUserIdentifierType.Email, cancellationToken, [userId], admins));
            var emails = allEmails[0];
            var adminEmails = allEmails[1];

            if (emails.Length > 0 || adminEmails.Length > 0)
            {
                // Load email template
                var subject = Properties.Resources.ActionNoticeSubject;
                var action = Properties.Resources.DeleteDocument + " - " + message.Data.TargetName;

                if (!orgId.Equals(message.Data.OrganizationId))
                {
                    action += $" ({Properties.Resources.PlatformAdminOperation})";
                }

                var data = new ActionNoticeData(message.Data,
                    string.Format(subject, action),
                    action
                );

                var body = await TemplateUtils.BuildActionNoticeAsync(message.Data.Culture, data);

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
