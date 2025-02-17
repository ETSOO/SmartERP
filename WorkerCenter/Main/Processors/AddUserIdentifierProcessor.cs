using com.etsoo.MessageQueue;
using com.etsoo.Utils.String;
using PlatformShared;
using PlatformShared.Database;
using PlatformShared.Database.Models;
using PlatformShared.Extentions;
using PlatformShared.Messages;
using WorkerCenter.Templates;

namespace WorkerCenter.Main.Processors
{
    /// <summary>
    /// Add user identifier processor
    /// 添加用户标识处理器
    /// </summary>
    public class AddUserIdentifierProcessor : LogQueueProcessor<AddUserIdentifierMessage>
    {
        private readonly MyDbContext _db;
        private readonly IMessageQueueProducer _producer;

        public AddUserIdentifierProcessor(ILogger<AddUserIdentifierProcessor> logger, LogDbContext logDb,
            MyDbContext db, IMessageQueueProducer producer)
            : base(logger, PlatformSharedContext.Default.AddUserIdentifierMessage, logDb)
        {
            _producer = producer;
            _db = db;
        }

        protected override async Task ProcessMessageAsync(AddUserIdentifierMessage message, MessageReceivedProperties properties, CancellationToken cancellationToken)
        {
            await base.ProcessMessageAsync(message, properties, cancellationToken);

            // User id
            var userId = message.Data.UserId;

            // Send email notice
            // Emails
            var emails = (await _db.QueryUserIdentifiersAsync(CoreUserIdentifierType.Email, cancellationToken, [userId]))[0];

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

                var body = await TemplateUtils.BuildTemplateAsync(TemplateUtils.ActionNoticeTemplate, data);

                // Send email notice
                var email = new SendEmailMessage
                {
                    Subject = data.Subject,
                    Body = body,
                    To = emails,
                    Importance = EmailImportance.High
                };

                await _producer.SendJsonAsync(email, PlatformSharedContext.Default.SendEmailMessage, SendEmailMessage.Type, cancellationToken);
            }
        }
    }
}
