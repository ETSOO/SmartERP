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
            var emails = (await _db.QueryUserIdentifiersAsync(CoreUserIdentifierType.Email, cancellationToken, [userId]))[0];

            if (emails.Length > 0)
            {
                // Load email template
                var subject = Properties.Resources.ActionNoticeSubject;
                var action = Properties.Resources.ChangePassword;

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
