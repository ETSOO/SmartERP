using Microsoft.EntityFrameworkCore;
using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.Person;
using PlatformShared.Database;

namespace WorkerCMS.Processors.Person
{
    public class ReadPersonProfileProcessor : LogQueueProcessor<ReadPersonProfileMessage>
    {
        public ReadPersonProfileProcessor(ILogger<ReadPersonProfileProcessor> logger, IDbContextFactory<LogDbContext> logDbFactory)
            : base(logger, CrmJsonSerializerContext.Default.ReadPersonProfileMessage, logDbFactory)
        {
        }
    }
}
