using com.etsoo.ApiModel;
using com.etsoo.ApiModel.Dto.SmartERP.MessageQueue;
using com.etsoo.MessageQueue;
using com.etsoo.MessageQueue.QueueProcessors;
using PlatformShared;
using PlatformShared.Dto;
using PlatformShared.Messages;
using RazorEngineCore;
using System.Text.Json;
using WorkerCenter.Templates;

namespace WorkerCenter.Main.Processors
{
    public class MemberTemplate : RazorEngineTemplateBase<AuthCodeEmailTemplateView>
    {
        public AuthCodeMemberInvitationData Data { get; set; } = default!;
    }


    /// <summary>
    /// Sending authentication code email processor
    /// 发送验证码邮件处理器
    /// </summary>
    public class SendAuthCodeEmailProcessor : CommonQueueProcessor<SendAuthCodeEmailMessage>
    {
        private static readonly Type AuthCodeMemberInvitationDataType = typeof(AuthCodeMemberInvitationData);

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
                template = TemplateUtils.FormatCultureTemplate(template, model.Language);

                string body;
                if (contentType == nameof(AuthCodeMemberInvitationData))
                {
                    // Distinguish different data types by ContentType
                    // 通过 ContentType 区分不同的数据类型
                    var jsonData = model.Data ?? throw new ArgumentNullException(nameof(model.Data));

                    var data = JsonSerializer.Deserialize(jsonData, PlatformSharedContext.Default.AuthCodeMemberInvitationData) ?? throw new ArgumentNullException(nameof(model.Data));
                    
                    body = await TemplateUtils.BuildTemplateAsync<MemberTemplate, AuthCodeEmailTemplateView>(template, (t) =>
                    {
                        t.Data = data;
                        t.Model = model;
                    }, [AuthCodeMemberInvitationDataType], cancellationToken);
                }
                else
                {
                    body = await TemplateUtils.BuildTemplateAsync(template, model, cancellationToken);
                }

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
