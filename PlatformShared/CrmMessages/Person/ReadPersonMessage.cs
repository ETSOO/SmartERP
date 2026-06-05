using com.etsoo.CoreFramework.Business;
using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.Person
{
    /// <summary>
    /// Read person message
    /// 读取人员消息
    /// </summary>
    public record ReadPersonMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "ReadPerson";

        /// <summary>
        /// Identity type, employee, customer, or supplier
        /// 标识类型，员工、客户或供应商
        /// </summary>
        public IdentityTypeFlags IdentityType { get; init; }

        public override Dictionary<string, object?>? GetJsonData() => new()
        {
            [nameof(IdentityType)] = IdentityType.ToString()
        };
    }
}
