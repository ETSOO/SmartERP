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
using System.Globalization;
using WebTemplates;
using WorkerCenter.Workers;

namespace WorkerCenter.Periods
{
    internal record AssetCheckWorkerOptions : WorkerOptions
    {
    }

    internal class AssetCheckWorker : SchedulerBackgroundService
    {
        private readonly IDbContextFactory<MyDbContext> _dbFactory;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<AssetCheckWorker> _logger;
        private readonly IMessageQueueProducer _producer;

        public AssetCheckWorker(
            IDbContextFactory<MyDbContext> dbFactory,
            IHttpClientFactory httpClientFactory,
            ILogger<AssetCheckWorker> logger,
            IOptions<AssetCheckWorkerOptions> options,
            IMessageQueueProducer producer
        ) : base(options.Value.Cron)
        {
            _dbFactory = dbFactory;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _producer = producer;
        }

        protected override async Task ExecuteJobAsync(CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;

            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);

            var assets = await db.PersonAssets.AsNoTracking()
                .Where(a => a.HealthCheckUrl != null && a.Status < EntityStatus.Inactivated && (a.HealthCheckSchedule == null || a.HealthCheckSchedule <= now))
                .Select(a => new AssetCheckData
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
                    HealthCheckUrl = a.HealthCheckUrl!,
                    HealthCheckSchedule = a.HealthCheckSchedule,
                    Data = a.Data,
                    OrgId = a.OrgId,

                    NoticeOwner = a.Data == null ? false : a.Data.NoticeOwner ?? false
                })
                .ToArrayAsync(cancellationToken);

            await Parallel.ForEachAsync(assets, cancellationToken, async (asset, token) =>
            {
                var checkUrl = asset.HealthCheckUrl;

                // Substitute the 'sn'
                checkUrl = checkUrl.Replace("{sn}", asset.Sn);

                // HTTP GET request to the check URL
                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(10);

                // Error message
                string? errorMessage = null;
                bool isException = false;

                try
                {
                    using var response = await client.GetAsync(checkUrl, HttpCompletionOption.ResponseHeadersRead, token);
                    if (!response.IsSuccessStatusCode)
                    {
                        errorMessage = $"HTTP {(int)response.StatusCode} - {response.ReasonPhrase}";
                    }
                }
                catch (Exception ex)
                {
                    isException = true;
                    errorMessage = ex.Message;
                    _logger.LogError(ex, "Exception when checking asset - {ProductName} ({Sn}) from {PersonName}", asset.ProductName, asset.Sn, asset.PersonName);
                }

                // Update asset
                var data = asset.Data ?? new PersonAssetData();

                // Last error message
                var lastError = data.LastError;
                data.LastError = errorMessage;

                var nextScheduleMinutes = data.IntervalMinutes == null || data.IntervalMinutes < 1 ? 5 : data.IntervalMinutes.Value;
                if (lastError != null)
                {
                    // If there was a previous error, increase the interval
                    nextScheduleMinutes *= 3;
                }

                var nextSchedule = DateTime.UtcNow.AddMinutes(nextScheduleMinutes);

                await using var updateDb = await _dbFactory.CreateDbContextAsync(token);

                var assetEntity = new PersonAsset
                {
                    Id = asset.Id,
                    HealthCheckSchedule = nextSchedule,
                    Data = data
                };
                updateDb.PersonAssets.Attach(assetEntity);

                updateDb.Entry(assetEntity).Property(x => x.HealthCheckSchedule).IsModified = true;
                updateDb.Entry(assetEntity).Reference(x => x.Data).TargetEntry?.State = EntityState.Modified;

                await updateDb.SaveChangesAsync(token);

                if (isException)
                {
                    // No necessary to report the exception
                    return;
                }

                // Update health check message
                asset.HealthCheckSchedule = nextSchedule;
                asset.HealthCheckMessage = errorMessage ?? lastError;

                // When no error, and also no last error, no need to send message
                if (asset.HealthCheckMessage == null)
                {
                    return;
                }

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
                    await using var db = await _dbFactory.CreateDbContextAsync(token);
                    var personEmails = (await db.QueryPersonIdentifiersAsync(orgId, CoreUserIdentifierType.Email, token, [personId]))[0];
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

                var model = new AssetCheckTemplateData
                {
                    Asset = asset,
                    Org = orgData
                };

                var body = errorMessage == null ? await TemplateUtils.BuildAssetCheckSuccessNoticeAsync(culture, model)
                    : await TemplateUtils.BuildAssetCheckFailureNoticeAsync(culture, model);

                var subject = model.Subject ?? "Asset Check Notice";

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
