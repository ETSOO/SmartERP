using com.etsoo.MessageQueue;
using Microsoft.EntityFrameworkCore;
using PlatformShared;
using PlatformShared.Database;
using PlatformShared.Database.Models;
using PlatformShared.Messages;
using System.Globalization;
using WorkerCenter.Templates;

namespace WorkerCenter.Main.Processors
{
    /// <summary>
    /// Change password processor
    /// 修改密码处理器
    /// </summary>
    public class ChangePasswordProcessor : LogQueueProcessor<ChangePasswordMessage>
    {
        private readonly MyDbContext _db;
        private readonly IMessageQueueProducer _producer;

        public ChangePasswordProcessor(ILogger<ChangePasswordProcessor> logger, LogDbContext logDb,
            MyDbContext db, IMessageQueueProducer producer)
            : base(logger, PlatformSharedContext.Default.ChangePasswordMessage, logDb)
        {
            _producer = producer;
            _db = db;
        }

        protected override async Task ProcessMessageAsync(ChangePasswordMessage message, MessageReceivedProperties properties, CancellationToken cancellationToken)
        {
            await base.ProcessMessageAsync(message, properties, cancellationToken);

            // User id
            var userId = message.Data.UserId;

            // Send email notice
            // Emails
            var emails = await _db.CoreUserIdentifiers
                .AsNoTracking()
                .Where(i => i.CoreUserId == userId && i.Type == CoreUserIdentifierType.Email)
                .Select(i => i.Value)
                .ToArrayAsync(cancellationToken);

            if (emails.Length > 0)
            {
                // Load email template
                var culture = message.Data.Culture;
                var ci = CultureInfo.GetCultureInfo(culture);
                var notice = Properties.Resources.ResourceManager.GetString(nameof(Properties.Resources.ActionNoticeSubject), ci)!;
                var subject = Properties.Resources.ResourceManager.GetString(nameof(Properties.Resources.ChangePassword), ci)!;

                var data = new ActionNoticeData
                {
                    Language = culture,
                    Subject = string.Format(notice, subject),
                    Action = "",
                    IP = message.Data.IP,
                    UserName = message.Data.UserName
                };

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
