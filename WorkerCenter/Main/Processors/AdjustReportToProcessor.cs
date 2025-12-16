using com.etsoo.ApiModel;
using com.etsoo.ApiModel.Dto.SmartERP.MessageQueue;
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
    /// Batch adjusting report to processor
    /// 批量调整汇报对象处理器
    /// </summary>
    public class AdjustReportToProcessor : LogQueueProcessor<AdjustReportToMessage>
    {
        private readonly MyDbContext _db;
        private readonly IMessageQueueProducer _producer;

        public AdjustReportToProcessor(ILogger<AdjustReportToProcessor> logger, LogDbContext logDb,
            MyDbContext db, IMessageQueueProducer producer)
            : base(logger, PlatformSharedContext.Default.AdjustReportToMessage, logDb)
        {
            _producer = producer;
            _db = db;
        }

        protected override async Task ProcessMessageAsync(AdjustReportToMessage message, MessageReceivedProperties properties, CancellationToken cancellationToken)
        {
            await base.ProcessMessageAsync(message, properties, cancellationToken);

            // User id
            var userId = message.Data.UserId;
            var originalId = (int)message.Data.TargetId;

            // Send email notice
            // Emails
            var emails = (await _db.QueryUserIdentifiersAsync(CoreUserIdentifierType.Email, cancellationToken, [userId], [originalId, message.NewReportTo]));
            var authorEmails = emails[0];
            var targetEmails = emails[1];

            if (emails.Length > 0)
            {
                // Load email template
                var subject = Properties.Resources.ActionNoticeSubject;
                var action = Properties.Resources.AdjustReportTo;
                var detail = Properties.Resources.AdjustReportToDetail;

                var data = new ActionNoticeData(message.Data,
                    string.Format(subject, action),
                    action,
                    string.Format(detail, message.Data.TargetName, message.Count, message.NewReportToName)
                );

                var body = await TemplateUtils.BuildNoticeTemplateAsync(message.Data.Culture, data, cancellationToken);

                // Send email notice
                var email = new SendEmailMessage
                {
                    Subject = data.Subject,
                    Body = body,
                    To = authorEmails,
                    Cc = targetEmails
                };

                await _producer.SendJsonAsync(email, ApiModelJsonSerializerContext.Default.SendEmailMessage, SendEmailMessage.Type, cancellationToken);
            }
        }
    }
}
