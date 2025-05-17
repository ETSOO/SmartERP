using com.etsoo.CoreFramework.Business;

namespace CRM.Server.Dto.Dept
{
    /// <summary>
    /// Department query data
    /// 部门查询数据
    /// </summary>
    public record DeptQueryData
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
        /// Leader
        /// 主管
        /// </summary>
        public string? Leader { get; init; }

        /// <summary>
        /// Staff
        /// 人员
        /// </summary>
        public int Staff { get; init; }

        /// <summary>
        /// Status
        /// 状态
        /// </summary>
        public EntityStatus Status { get; init; }

        /// <summary>
        /// Creation
        /// 创建时间
        /// </summary>
        public DateTimeOffset Creation { get; init; }
    }
}
