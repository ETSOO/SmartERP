using com.etsoo.ApiModel;
using com.etsoo.ApiModel.Dto.SmartERP.MessageQueue;
using com.etsoo.MessageQueue;
using com.etsoo.Utils.String;
using Microsoft.EntityFrameworkCore;
using PlatformShared;
using PlatformShared.Database;
using PlatformShared.Database.Models;
using PlatformShared.Dto;
using PlatformShared.Extentions;
using PlatformShared.Messages;
using WebTemplates;

namespace WorkerCenter.Main.Processors
{
    /// <summary>
    /// Add user identifier processor
    /// 添加用户标识处理器
    /// </summary>
    public class AddUserIdentifierProcessor : LogQueueProcessor<AddUserIdentifierMessage>
    {
        private readonly IDbContextFactory<MyDbContext> _dbFactory;
        private readonly IMessageQueueProducer _producer;

        public AddUserIdentifierProcessor(ILogger<AddUserIdentifierProcessor> logger, IDbContextFactory<LogDbContext> logDbFactory,
            IDbContextFactory<MyDbContext> dbFactory, IMessageQueueProducer producer)
            : base(logger, PlatformSharedContext.Default.AddUserIdentifierMessage, logDbFactory)
        {
            _producer = producer;
            _dbFactory = dbFactory;
        }

        protected override async Task ProcessMessageAsync(AddUserIdentifierMessage message, MessageReceivedProperties properties, CancellationToken cancellationToken)
        {
            await base.ProcessMessageAsync(message, properties, cancellationToken);

            // User id
            var userId = message.Data.UserId;

            // Send email notice
            // Emails
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var emails = (await db.QueryUserIdentifiersAsync(CoreUserIdentifierType.Email, cancellationToken, [userId]))[0];

            if (emails.Length > 0)
            {
                // Load email template
                var subject = Properties.Resources.ActionNoticeSubject;
                var action = Properties.Resources.AddUserIdentifier;
                var detail = Properties.Resources.AddUserIdentifierDetail;

                var data = new ActionNoticeData(message.Data,
                    string.Format(subject, action),
                    action,
                    string.Format(detail, message.IdentifierType.ToString(), StringUtils.HideEmail(message.IdentifierValue))
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
