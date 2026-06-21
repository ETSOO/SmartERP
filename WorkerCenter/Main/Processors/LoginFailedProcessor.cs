using com.etsoo.ApiModel;
using com.etsoo.ApiModel.Dto.SmartERP.MessageQueue;
using com.etsoo.MessageQueue;
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
    /// Login failed processor
    /// 登录失败处理器
    /// </summary>
    public class LoginFailedProcessor : LogQueueProcessor<LoginFailedMessage>
    {
        private const int FreezeMinutes = 30;

        private readonly IDbContextFactory<MyDbContext> _dbFactory;
        private readonly IDbContextFactory<LogDbContext> _logDbFactory;
        private readonly IMessageQueueProducer _producer;

        public LoginFailedProcessor(ILogger<LoginFailedProcessor> logger,
            IDbContextFactory<LogDbContext> logDbFactory, IDbContextFactory<MyDbContext> dbFactory, IMessageQueueProducer producer)
            : base(logger, PlatformSharedContext.Default.LoginFailedMessage, logDbFactory)
        {
            _dbFactory = dbFactory;
            _logDbFactory = logDbFactory;
            _producer = producer;
        }

        protected override async Task ProcessMessageAsync(LoginFailedMessage message, MessageReceivedProperties properties, CancellationToken cancellationToken)
        {
            await base.ProcessMessageAsync(message, properties, cancellationToken);

            // User id
            var userId = message.Data.UserId;

            // 6 consecutive login failures, freeze the user
            var dateAgo = DateTimeOffset.UtcNow.AddMinutes(-FreezeMinutes);
            await using var logDb = await _logDbFactory.CreateDbContextAsync(cancellationToken);
            var count = await logDb.CoreLogs
                .Where(l => l.Kind == LoginFailedMessage.Type && l.UserId == userId && l.Creation >= dateAgo)
                .CountAsync(cancellationToken);

            if (count > 5)
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);

                // Freeze user
                var affacted = await db.CoreUsers.Where(u => u.Id == userId).ExecuteUpdateAsync(u => u.SetProperty(u => u.FrozenTime, DateTimeOffset.UtcNow.AddMinutes(FreezeMinutes)), cancellationToken);

                if (affacted > 0)
                {
                    // Emails
                    var emails = (await db.QueryUserIdentifiersAsync(CoreUserIdentifierType.Email, cancellationToken, [userId]))[0];

                    if (emails.Length > 0)
                    {
                        // Load email template
                        var subject = Properties.Resources.ActionNoticeSubject;
                        var action = Properties.Resources.LoginFailedFrozenAction;
                        var detail = Properties.Resources.LoginFailedFrozenDetail;

                        var data = new ActionNoticeData(message.Data,
                            string.Format(subject, action),
                            action,
                            detail
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
    }
}
