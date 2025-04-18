using com.etsoo.ApiModel;
using com.etsoo.ApiModel.Dto.SmartERP.MessageQueue;
using com.etsoo.MessageQueue;
using com.etsoo.MessageQueue.QueueProcessors;
using com.etsoo.SMTP;
using MimeKit;
using MimeKit.Text;

namespace WorkerCenter.Main.Processors
{
    /// <summary>
    /// Send email processor
    /// 发送邮件处理器
    /// </summary>
    public class SendEmailProcessor : CommonQueueProcessor<SendEmailMessage>
    {
        readonly ISMTPClient _smtpClient;

        public SendEmailProcessor(ILogger<SendEmailProcessor> logger, ISMTPClient smtpClient)
            : base(logger, ApiModelJsonSerializerContext.Default.SendEmailMessage)
        {
            _smtpClient = smtpClient;
        }

        IEnumerable<InternetAddress> ParseAddresss(IEnumerable<string> items)
        {
            foreach (var item in items.Distinct())
            {
                if (MailboxAddress.TryParse(item, out var address))
                    yield return address;
            }
        }

        protected override async Task ProcessMessageAsync(SendEmailMessage message, MessageReceivedProperties properties, CancellationToken cancellationToken)
        {
            // Email
            var email = new MimeMessage
            {
                Subject = message.Subject,
                Body = new TextPart(TextFormat.Html) { Text = message.Body }
            };

            // Add recipients
            // Avoid duplicate addresses

            email.To.AddRange(ParseAddresss(message.To));

            if (message.Cc != null)
                email.Cc.AddRange(ParseAddresss(message.Cc.Where(c => !message.To.Contains(c))));

            if (message.Bcc != null)
                email.Bcc.AddRange(ParseAddresss(message.Bcc.Where(b => !message.To.Contains(b) && (message.Cc == null || !message.Cc.Contains(b)))));

            if (message.Importance != null)
            {
                email.Importance = message.Importance switch
                {
                    EmailImportance.Low => MessageImportance.Low,
                    EmailImportance.High => MessageImportance.High,
                    _ => MessageImportance.Normal
                };
            }

            if (message.Priority != null)
            {
                email.Priority = message.Priority switch
                {
                    EmailPriority.NonUrgent => MessagePriority.NonUrgent,
                    EmailPriority.Urgent => MessagePriority.Urgent,
                    _ => MessagePriority.Normal
                };
            }

            // Send
            await _smtpClient.SendAsync(email, cancellationToken);
        }
    }
}
