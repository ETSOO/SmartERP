using com.etsoo.ApiModel;
using com.etsoo.ApiModel.Dto.SmartERP.MessageQueue;
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
    public class ResetPasswordProcessor : LogQueueProcessor<ResetPasswordMessage>
    {
        private readonly IDbContextFactory<MyDbContext> _dbFactory;
        private readonly IMessageQueueProducer _producer;

        public ResetPasswordProcessor(ILogger<ResetPasswordProcessor> logger, IDbContextFactory<LogDbContext> logDbFactory,
            IDbContextFactory<MyDbContext> dbFactory, IMessageQueueProducer producer)
            : base(logger, PlatformSharedContext.Default.ResetPasswordMessage, logDbFactory)
        {
            _producer = producer;
            _dbFactory = dbFactory;
        }

        protected override async Task ProcessMessageAsync(ResetPasswordMessage message, MessageReceivedProperties properties, CancellationToken cancellationToken)
        {
            await base.ProcessMessageAsync(message, properties, cancellationToken);

            // User id
            var userId = message.Data.UserId;

            // Send email notice
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var emails = (await db.QueryUserIdentifiersAsync(CoreUserIdentifierType.Email, cancellationToken, [userId]))[0];

            if (emails.Length > 0)
            {
                // Load email template
                var subject = Properties.Resources.ActionNoticeSubject;
                var action = Properties.Resources.ResetPassword;

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
                    Importance = EmailImportance.High
                };

                await _producer.SendJsonAsync(email, ApiModelJsonSerializerContext.Default.SendEmailMessage, SendEmailMessage.Type, cancellationToken);
            }
        }
    }
}
