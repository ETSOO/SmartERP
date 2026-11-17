using com.etsoo.ApiModel;
using com.etsoo.ApiModel.Dto.SmartERP.MessageQueue;
using com.etsoo.Localization;
using com.etsoo.MessageQueue;
using com.etsoo.MessageQueue.QueueProcessors;
using Microsoft.EntityFrameworkCore;
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
    /// Admin merge accounts processor
    /// 管理员合并账户处理器
    /// </summary>
    public class AdminMergeAccountsProcessor : CommonQueueProcessor<AdminMergeAccountsMessage>
    {
        private readonly IDbContextFactory<LogDbContext> _logDbFactory;
        private readonly IDbContextFactory<MyDbContext> _dbFactory;
        private readonly IMessageQueueProducer _producer;

        public AdminMergeAccountsProcessor(ILogger<AdminMergeAccountsProcessor> logger,
            IDbContextFactory<LogDbContext> logDbFactory,
            IDbContextFactory<MyDbContext> dbFactory,
            IMessageQueueProducer producer)
            : base(logger, PlatformSharedContext.Default.AdminMergeAccountsMessage)
        {
            _producer = producer;
            _logDbFactory = logDbFactory;
            _dbFactory = dbFactory;
        }

        private async Task LogAsync(AdminMergeAccountsMessage message, int userId, int? orgId, CancellationToken cancellationToken)
        {
            var data = message.Data;
            var title = $"{Properties.Resources.AdminMergeAccounts} ({data.TargetName})";

            await using var logDb = await _logDbFactory.CreateDbContextAsync(cancellationToken);
            await logDb.LogAsync(message, title, userId, orgId, null, cancellationToken);
        }

        protected override async Task ProcessMessageAsync(AdminMergeAccountsMessage message, MessageReceivedProperties properties, CancellationToken cancellationToken)
        {
            // Culture
            var ci = LocalizationUtils.SetCulture(message.Data.Culture, true);
            Properties.Resources.Culture = ci;

            // Log the operation
            var title = $"{Properties.Resources.AdminMergeAccounts} ({message.Data.TargetName})";
            await using var logDb = await _logDbFactory.CreateDbContextAsync(cancellationToken);
            await logDb.LogAsync(message, title, cancellationToken);

            // Email notice
            // Emails
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var emails = await db.QueryUserIdentifiersAsync(CoreUserIdentifierType.Email, cancellationToken, [message.SourceUserId], [message.Data.UserId]);

            var action = Properties.Resources.AdminMergeAccounts;
            var detail = Properties.Resources.AdminMergeAccountsDetail;

            // Load email template
            var data = new ActionNoticeData(message.Data,
                title,
                action,
                string.Format(detail, message.MergedUserName, message.SourceUserName, message.Description)
            );

            var body = await TemplateUtils.BuildActionNoticeAsync(message.Data.Culture, data);

            // Send email notice
            var email = new SendEmailMessage
            {
                Subject = data.Subject,
                Body = body,
                To = emails[0],
                Cc = emails[1]
            };

            await _producer.SendJsonAsync(email, ApiModelJsonSerializerContext.Default.SendEmailMessage, SendEmailMessage.Type, cancellationToken);
        }
    }
}
