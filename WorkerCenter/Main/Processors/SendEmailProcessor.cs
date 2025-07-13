using com.etsoo.ApiModel;
using com.etsoo.ApiModel.Dto.SmartERP.MessageQueue;
using com.etsoo.MessageQueue;
using com.etsoo.MessageQueue.QueueProcessors;
using com.etsoo.SMTP;
using MimeKit;
using MimeKit.Text;
using PlatformShared.Services;

namespace WorkerCenter.Main.Processors
{
    /// <summary>
    /// Send email processor
    /// 发送邮件处理器
    /// </summary>
    public class SendEmailProcessor : CommonQueueProcessor<SendEmailMessage>
    {
        readonly ISmartERPCoordinator _erp;
        readonly ISMTPClient _smtpClient;

        public SendEmailProcessor(ILogger<SendEmailProcessor> logger,
            ISmartERPCoordinator erp,
            ISMTPClient smtpClient)
            : base(logger, ApiModelJsonSerializerContext.Default.SendEmailMessage)
        {
            _erp = erp;
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

        private async ValueTask<ISMTPClient> CreateClientAsync(int? orgId, CancellationToken cancellationToken)
        {
            if (orgId > 0)
            {
                var item = await _erp.GetSMTPApiAsync(orgId.Value, cancellationToken);
                if (item != null)
                {
                    var appSecret = _erp.DecriptData(item.AppSecret, ServiceConstants.CoreApiAppSecretEncryptionKey);

                    var options = new SMTPClientOptions(
                        item.Endpoint.Host,
                        item.Endpoint.Port,
                        item.Endpoint.Scheme.Equals(Uri.UriSchemeHttps),
                        item.Title,
                        item.AppId,
                        appSecret,
                        null,
                        item.Options.Cc,
                        item.Options.Bcc
                    );

                    return new SMTPClient(options);
                }
            }

            return _smtpClient;
        }

        protected override async Task ProcessMessageAsync(SendEmailMessage message, MessageReceivedProperties properties, CancellationToken cancellationToken)
        {
            // Create SMTP client
            var client = await CreateClientAsync(message.OrgId, cancellationToken);

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
            await client.SendAsync(email, cancellationToken);
        }
    }
}
