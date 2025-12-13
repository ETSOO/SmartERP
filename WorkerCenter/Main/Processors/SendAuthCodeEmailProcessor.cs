using com.etsoo.ApiModel;
using com.etsoo.ApiModel.Dto.SmartERP.MessageQueue;
using com.etsoo.MessageQueue;
using com.etsoo.MessageQueue.QueueProcessors;
using PlatformShared;
using PlatformShared.Dto;
using PlatformShared.Messages;
using WorkerCenter.Templates;

namespace WorkerCenter.Main.Processors
{
    /// <summary>
    /// Sending authentication code email processor
    /// 发送验证码邮件处理器
    /// </summary>
    public class SendAuthCodeEmailProcessor : CommonQueueProcessor<SendAuthCodeEmailMessage>
    {
        private static readonly Type CodeUserDataType = typeof(CodeUserData);

        private readonly IMessageQueueProducer _producer;

        public SendAuthCodeEmailProcessor(ILogger<SendAuthCodeEmailProcessor> logger, IMessageQueueProducer producer)
            : base(logger, PlatformSharedContext.Default.SendAuthCodeEmailMessage)
        {
            _producer = producer;
        }

        protected override async Task ProcessMessageAsync(SendAuthCodeEmailMessage message, MessageReceivedProperties properties, CancellationToken cancellationToken)
        {
            var model = message.Model;

            var template = model.Action.Template;
            if (!string.IsNullOrEmpty(template))
            {
                var body = await TemplateUtils.BuildTemplateAsync(template, model, [CodeUserDataType], cancellationToken);

                var labelId = model.Action.Id.ToString();
                var subject = model.Subject ?? Properties.Resources.ResourceManager.GetString(labelId) ?? labelId;

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
