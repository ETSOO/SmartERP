using com.etsoo.ApiModel;
using com.etsoo.ApiModel.Dto.SmartERP.MessageQueue;
using com.etsoo.CoreFramework.Business;
using com.etsoo.Localization;
using com.etsoo.MessageQueue;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PlatformShared.Database;
using PlatformShared.Database.Models;
using PlatformShared.Dto.Document;
using PlatformShared.Dto.Document.Asset;
using PlatformShared.Extentions;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using WebTemplates;
using WorkerCenter.Workers;

namespace WorkerCenter.Periods
{
    internal record AssetExiryWorkerOptions : WorkerOptions
    {
        /// <summary>
        /// Days before expiry
        /// 到期前的天数
        /// </summary>
        [Range(1, 180)]
        public int Days { get; set; } = 30;
    }

    internal class AssetExiryWorker : SchedulerBackgroundService
    {
        private readonly IDbContextFactory<MyDbContext> _dbFactory;
        private readonly ILogger<AssetExiryWorker> _logger;
        private readonly IMessageQueueProducer _producer;
        private readonly int _days;

        public AssetExiryWorker(ILogger<AssetExiryWorker> logger,
            IDbContextFactory<MyDbContext> dbFactory,
            IOptions<AssetExiryWorkerOptions> options,
            IMessageQueueProducer producer) : base(options.Value.Cron)
        {
            _logger = logger;
            _dbFactory = dbFactory;
            _producer = producer;
            _days = options.Value.Days;
        }

        protected override async Task ExecuteJobAsync(CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;
            var earlyDate = now.AddDays(_days);

            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);

            var assets = await db.PersonAssets.AsNoTracking()
                .Where(a => a.ExpiryCheck == true && a.Status < EntityStatus.Inactivated && a.Expiry > now && a.Expiry <= earlyDate)
                .Select(a => new AssetViewData
                {
                    Id = a.Id,
                    PersonId = a.PersonId,
                    PersonName = a.Person.Name,
                    IsLegalPerson = a.Person.IsLegalPerson,
                    IdentityType = a.Person.IdentityType,
                    PersonUserId = a.Person.UserId,
                    Cultures = a.Person.Cultures,
                    CoreUserId = a.CoreUserId,

                    ProductName = a.Product.Name,
                    Sn = a.Sn,
                    Times = a.Times ?? 0,
                    Amount = a.Amount ?? 0,
                    Expiry = a.Expiry,
                    OrgId = a.OrgId,

                    NoticeOwner = a.Data == null ? false : a.Data.NoticeOwner ?? false
                })
                .ToArrayAsync(cancellationToken);

            // DbContext is not thread-safe, so multiple parallel operations cannot share the same DbContext instance
            await Parallel.ForEachAsync(assets, cancellationToken, async (asset, token) =>
            {
                var orgId = asset.OrgId;

                var orgData = await DocumentTemplateUtils.CreateOrgDataAsync(_dbFactory, orgId, 0, token);
                if (orgData == null) return;

                var personId = asset.PersonId;

                var userId = asset.PersonUserId;
                var userEmails = await DocumentTemplateUtils.GetPersonAndLineIdentifiersAsync(_dbFactory, orgId, userId, CoreUserIdentifierType.Email, token);

                var noticeOwner = asset.NoticeOwner;

                List<string> to = [];
                List<string> bcc = [];

                if (noticeOwner)
                {
                    var _db = await _dbFactory.CreateDbContextAsync(token);
                    var personEmails = (await _db.QueryPersonIdentifiersAsync(orgId, CoreUserIdentifierType.Email, token, [personId]))[0];
                    if (personEmails.Length > 0)
                    {
                        to.AddRange(personEmails);
                    }
                }

                if (userEmails != null)
                {
                    if (to.Count > 0)
                    {
                        bcc.AddRange(userEmails);
                    }
                    else
                    {
                        to.AddRange(userEmails);
                    }
                }

                var culture = asset.Cultures?.FirstOrDefault() ?? orgData.Cultures?.FirstOrDefault();
                if (culture == null)
                {
                    culture = CultureInfo.CurrentCulture.Name;
                }
                else
                {
                    var ci = LocalizationUtils.SetCulture(culture, true);
                    Properties.Resources.Culture = ci;
                }

                var labels = await DocumentTemplateUtils.CreateOrgCulturesAsync(_dbFactory, orgId, culture, token);
                if (labels != null)
                {
                    orgData.Labels.AddRange(labels);
                }

                var model = new AssetTemplateData
                {
                    Asset = asset,
                    Org = orgData
                };

                var body = await TemplateUtils.BuildAssetExpiryNoticeAsync(culture, model);

                var subject = model.Subject ?? "Asset Expiry Notice";

                // Send email notice
                var email = new SendEmailMessage
                {
                    Subject = subject,
                    Body = body,
                    To = to,
                    Bcc = bcc,
                    Importance = EmailImportance.High
                };

                await _producer.SendJsonAsync(email, ApiModelJsonSerializerContext.Default.SendEmailMessage, SendEmailMessage.Type, token);
            });
        }
    }
}
