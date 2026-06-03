using com.etsoo.CoreFramework.Business;
using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.Person
{
    /// <summary>
    /// Delete person message
    /// 移除人员消息
    /// </summary>
    public record DeletePersonMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "DeletePerson";

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
