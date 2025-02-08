using com.etsoo.MessageQueue;
using com.etsoo.Utils.String;
using PlatformShared;
using PlatformShared.Database;
using PlatformShared.Database.Models;
using PlatformShared.Extentions;
using PlatformShared.Messages;
using System.Globalization;
using WorkerCenter.Templates;

namespace WorkerCenter.Main.Processors
{
    /// <summary>
    /// Delete user identifier processor
    /// 删除用户标识处理器
    /// </summary>
    public class DeleteUserIdentifierProcessor : LogQueueProcessor<DeleteUserIdentifierMessage>
    {
        private readonly MyDbContext _db;
        private readonly IMessageQueueProducer _producer;

        public DeleteUserIdentifierProcessor(ILogger<DeleteUserIdentifierProcessor> logger, LogDbContext logDb,
            MyDbContext db, IMessageQueueProducer producer)
            : base(logger, PlatformSharedContext.Default.DeleteUserIdentifierMessage, logDb)
        {
            _producer = producer;
            _db = db;
        }

        protected override async Task ProcessMessageAsync(DeleteUserIdentifierMessage message, MessageReceivedProperties properties, CancellationToken cancellationToken)
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
                var culture = message.Data.Culture;
                var ci = CultureInfo.GetCultureInfo(culture);
                var subject = Properties.Resources.ResourceManager.GetString(nameof(Properties.Resources.ActionNoticeSubject), ci)!;
                var action = Properties.Resources.ResourceManager.GetString(nameof(Properties.Resources.DeleteUserIdentifier), ci)!;
                var detail = Properties.Resources.ResourceManager.GetString(nameof(Properties.Resources.AddUserIdentifierDetail), ci)!;

                var data = new ActionNoticeData(message.Data,
                    string.Format(action, subject),
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
