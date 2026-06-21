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
using Microsoft.EntityFrameworkCore;

namespace WorkerCenter.Main.Processors
{
    /// <summary>
    /// Create API key processor
    /// 创建API密钥处理器
    /// </summary>
    public class CreateApiKeyProcessor : LogQueueProcessor<CreateApiKeyMessage>
    {
        private readonly IDbContextFactory<MyDbContext> _dbFactory;
        private readonly IMessageQueueProducer _producer;

        public CreateApiKeyProcessor(ILogger<CreateApiKeyProcessor> logger, IDbContextFactory<LogDbContext> logDb,
            IDbContextFactory<MyDbContext> dbFactory, IMessageQueueProducer producer)
            : base(logger, PlatformSharedContext.Default.CreateApiKeyMessage, logDb)
        {
            _producer = producer;
            _dbFactory = dbFactory;
        }

        protected override async Task ProcessMessageAsync(CreateApiKeyMessage message, MessageReceivedProperties properties, CancellationToken cancellationToken)
        {
            await base.ProcessMessageAsync(message, properties, cancellationToken);

            // User id
            var userId = message.Data.UserId;

            // Organization id
            var orgId = message.Data.OrganizationId.GetValueOrDefault();

            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);

            // All admins
            var admins = await db.QueryUsersAsync(orgId, UserRole.Admin, cancellationToken);

            // Send email notice
            // Emails
            var allEmails = (await db.QueryUserIdentifiersAsync(CoreUserIdentifierType.Email, cancellationToken, [userId], admins));
            var emails = allEmails[0];
            var adminEmails = allEmails[1];

            if (emails.Length > 0 || adminEmails.Length > 0)
            {
                // Load email template
                var subject = Properties.Resources.ActionNoticeSubject;
                var action = Properties.Resources.CreateApiKey;

                var data = new ActionNoticeData(message.Data,
                    string.Format(subject, action),
                    $"{action} ({message.Data.TargetName})"
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
