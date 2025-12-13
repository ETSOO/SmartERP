using com.etsoo.CoreFramework.Business;
using PlatformShared.Database.Models;

namespace CRM.Server.Dto.Person
{
    /// <summary>
    /// Contact query data
    /// 联系人查询数据
    /// </summary>
    public record ContactQueryData
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public long Id { get; init; }

        /// <summary>
        /// Contact person id
        /// 联系人人员编号
        /// </summary>
        public long ContactId { get; init; }

        /// <summary>
        /// Relation type
        /// 关系类型
        /// </summary>
        public PersonRelationType RelationType { get; init; }

        /// <summary>
        /// Name
        /// 名称
        /// </summary>
        public required string Name { get; init; }

        /// <summary>
        /// Description
        /// 描述
        /// </summary>
        public string? Description { get; init; }

        /// <summary>
        /// Creation
        /// 登记时间
        /// </summary>
        public DateTimeOffset Creation { get; init; }

        /// <summary>
        /// Status
        /// 状态
        /// </summary>
        public EntityStatus Status { get; init; }
    }
}
