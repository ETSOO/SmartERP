using com.etsoo.CoreFramework.Business;
using PlatformShared.Dto;

namespace CRM.Server.Dto.Customer
{
    /// <summary>
    /// Customer query data
    /// 客户查询数据
    /// </summary>
    public record CustomerQueryData
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public long Id { get; init; }

        /// <summary>
        /// Name
        /// 名称
        /// </summary>
        public required string Name { get; init; }

        /// <summary>
        /// Assigend id
        /// 分配的编号
        /// </summary>
        public string? AssignedId { get; init; }

        /// <summary>
        /// Categories
        /// 类目
        /// </summary>
        public IEnumerable<CategoryItem>? Categories { get; init; }

        /// <summary>
        /// Preferred Name
        /// 首选名称
        /// </summary>
        public string? PreferredName { get; init; }

        /// <summary>
        /// Description
        /// 描述
        /// </summary>
        public string? Description { get; init; }

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
