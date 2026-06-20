using com.etsoo.ApiModel;
using com.etsoo.ApiModel.Dto.SmartERP.MessageQueue;
using com.etsoo.Localization;
using com.etsoo.MessageQueue;
using com.etsoo.MessageQueue.QueueProcessors;
using PlatformShared;
using PlatformShared.Dto;
using PlatformShared.Messages;
using System.Text.Json;
using WebTemplates;

namespace WorkerCenter.Main.Processors
{
    /// <summary>
    /// Sending authentication code email processor
    /// 发送验证码邮件处理器
    /// </summary>
    public class SendAuthCodeEmailProcessor : CommonQueueProcessor<SendAuthCodeEmailMessage>
    {
        private readonly IMessageQueueProducer _producer;

        public SendAuthCodeEmailProcessor(ILogger<SendAuthCodeEmailProcessor> logger, IMessageQueueProducer producer)
            : base(logger, PlatformSharedContext.Default.SendAuthCodeEmailMessage)
        {
            _producer = producer;
        }

        protected override async Task ProcessMessageAsync(SendAuthCodeEmailMessage message, MessageReceivedProperties properties, CancellationToken cancellationToken)
        {
            var model = message.Model;
            var contentType = properties.ContentType;
            var template = model.Action.Template;
            if (!string.IsNullOrEmpty(template))
            {
                var culture = model.Language;

                var ci = LocalizationUtils.SetCulture(culture, true);
                Properties.Resources.Culture = ci;

                template = TemplateUtils.FormatCulture(template, culture);

                string body;
                string? subject;
                if (contentType == nameof(AuthCodeMemberInvitationData))
                {
                    // Distinguish different data types by ContentType
                    // 通过 ContentType 区分不同的数据类型
                    var jsonData = model.Data ?? throw new ArgumentNullException(nameof(model.Data));

                    var data = JsonSerializer.Deserialize(jsonData, PlatformSharedContext.Default.AuthCodeMemberInvitationData) ?? throw new ArgumentNullException(nameof(model.Data));

                    var newModel = new InvitationAuthCodeEmailTemplateView(model, data);
                    body = await TemplateUtils.BuildAsync(template, newModel);
                    subject = newModel.Subject;
                }
                else
                {
                    body = await TemplateUtils.BuildAsync(template, model);
                    subject = model.Subject;
                }

                var labelId = model.Action.Id.ToString();
                subject ??= Properties.Resources.ResourceManager.GetString(labelId) ?? labelId;

                // Send email
                var inviteeeEmail = new SendEmailMessage
                {
                    Subject = subject,
                    Body = body,
                    To = message.To,
                    Importance = EmailImportance.High
                };

                await _producer.SendJsonAsync(inviteeeEmail, ApiModelJsonSerializerContext.Default.SendEmailMessage, SendEmailMessage.Type, cancellationToken);
            }
        }
    }
}
