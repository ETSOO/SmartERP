using com.etsoo.Address;
using com.etsoo.ApiModel;
using com.etsoo.ApiModel.Dto.SmartERP.MessageQueue;
using com.etsoo.MessageQueue;
using com.etsoo.MessageQueue.QueueProcessors;
using com.etsoo.SMS;
using com.etsoo.Utils.String;
using System.Text.Json;

namespace WorkerCenter.Main.Processors
{
    /// <summary>
    /// Send SMS processor
    /// 发送短信处理器
    /// </summary>
    public class SendSMSProcessor : CommonQueueProcessor<SendSMSMessage>
    {
        readonly ISMSClient _smsClient;

        public SendSMSProcessor(ILogger<SendSMSProcessor> logger, ISMSClient smsClient)
            : base(logger, ApiModelJsonSerializerContext.Default.SendSMSMessage)
        {
            _smsClient = smsClient;
        }

        protected override async Task ProcessMessageAsync(SendSMSMessage message, MessageReceivedProperties properties, CancellationToken cancellationToken)
        {
            TemplateKind? kind = message.Kind switch
            {
                "Code" => TemplateKind.Code,
                "Notice" => TemplateKind.Notice,
                "Marketing" => TemplateKind.Marketing,
                _ => null
            };

            if (!kind.HasValue)
            {
                throw new ArgumentException("Invalid SMS message kind");
            }

            // Send
            if (!message.To.Any())
            {
                return;
            }

            var phones = AddressRegion.CreatePhones(message.To.Distinct(), message.Region);

            if (kind == TemplateKind.Code || message.TemplateId == null)
            {
                foreach (var phone in phones)
                {
                    var result = await _smsClient.SendCodeAsync(phone, message.Body, (TemplateItem?)null, cancellationToken);
                    if (!result.Ok)
                    {
                        // Log
                        logger.LogError("SMS sent to {mobile} failed: {@result}", phone.ToInternationalFormat(), result);
                    }
                }
            }
            else
            {
                var vars = JsonSerializer.Deserialize<Dictionary<string, string>>(message.Body) ?? [];
                var mobiles = phones.Select(t => t.ToInternationalFormat());
                var result = await _smsClient.SendAsync(kind.Value, mobiles, vars, message.TemplateId, cancellationToken);
                if (!result.Ok)
                {
                    // Log
                    var mobileItems = string.Join(", ", mobiles.Select(m => StringUtils.HideData(m)));
                    logger.LogError("SMS sent to {mobiles} failed: {@result}", mobileItems, result);
                }
            }
        }
    }
}
