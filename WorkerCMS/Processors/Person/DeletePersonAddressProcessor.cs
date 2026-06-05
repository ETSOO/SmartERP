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
        /// <param name="logDb"></param>
        public DeletePersonAddressProcessor(ILogger<DeletePersonAddressProcessor> logger, LogDbContext logDb)
            : base(logger, CrmJsonSerializerContext.Default.DeletePersonAddressMessage, logDb)
        {
        }
    }
}
