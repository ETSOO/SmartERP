using com.etsoo.CoreFramework.Business;
using PlatformShared.Dto;

namespace CRM.Server.Dto.Person
{
    /// <summary>
    /// Person query data
    /// 人员查询数据
    /// </summary>
    public record PersonQueryData : ContactItem
    {
        /// <summary>
        /// Assigned id
        /// 分配的编号
        /// </summary>
        public string? AssignedId { get; init; }

        /// <summary>
        /// Status
        /// 状况
        /// </summary>
        public EntityStatus Status { get; init; }

        /// <summary>
        /// Creation
        /// 登记时间
        /// </summary>
        public DateTimeOffset Creation { get; init; }
    }
}
