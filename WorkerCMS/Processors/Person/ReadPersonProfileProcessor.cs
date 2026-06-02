using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.Person;
using PlatformShared.Database;

namespace WorkerCMS.Processors.Person
{
    public class ReadPersonProfileProcessor : LogQueueProcessor<ReadPersonProfileMessage>
    {
        public ReadPersonProfileProcessor(ILogger<ReadPersonProfileProcessor> logger, LogDbContext logDb)
            : base(logger, CrmJsonSerializerContext.Default.ReadPersonProfileMessage, logDb)
        {
        }
    }
}
