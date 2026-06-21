using Microsoft.EntityFrameworkCore;
using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.Person;
using PlatformShared.Database;
namespace WorkerCMS.Processors.Person
{
    public class DeletePersonAddressProcessor : LogQueueProcessor<DeletePersonAddressMessage>
    {
        /// <summary>
        /// Delete person address message processor
        /// 删除人员地址消息处理器
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="logDbFactory"></param>
        public DeletePersonAddressProcessor(ILogger<DeletePersonAddressProcessor> logger, IDbContextFactory<LogDbContext> logDbFactory)
            : base(logger, CrmJsonSerializerContext.Default.DeletePersonAddressMessage, logDbFactory)
        {
        }
    }
}
