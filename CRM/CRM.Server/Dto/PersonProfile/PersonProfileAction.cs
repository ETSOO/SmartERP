using com.etsoo.CoreFramework.Authentication;
using PlatformShared.Database.Models;

namespace CRM.Server.Dto.PersonProfile
{
    /// <summary>
    /// Person profile action
    /// 人员档案操作
    /// </summary>
    public record PersonProfileAction
    {
        /// <summary>
        /// Person id
        /// 个人编号
        /// </summary>
        public long PersonId { get; init; }

        /// <summary>
        /// Other participants
        /// 其他参与者
        /// </summary>
        public List<long>? Persons { get; init; }

        /// <summary>
        /// Order id
        /// 订单编号
        /// </summary>
        public long? OrderId { get; init; }

        /// <summary>
        /// Kind
        /// 类型
        /// </summary>
        public PersonProfileKind? Kind { get; init; }

        /// <summary>
        /// Title
        /// 标题
        /// </summary>
        public required string Title { get; init; }

        /// <summary>
        /// Comment
        /// 评价
        /// </summary>
        public required string Comment { get; init; }

        /// <summary>
        /// Location
        /// 位置
        /// </summary>
        public string? Location { get; init; }

        /// <summary>
        /// Location id
        /// 位置编号
        /// </summary>
        public int? LocationId { get; init; }

        /// <summary>
        /// Happen date
        /// 发生日期
        /// </summary>
        public DateTimeOffset? HappenDate { get; init; }

        /// <summary>
        /// Happen date end
        /// 发生日期结束
        /// </summary>
        public DateTimeOffset? HappenDateEnd { get; init; }

        /// <summary>
        /// User role used for privacy control
        /// 用于隐私控制的用户角色
        /// </summary>
        public UserRole? UserRole { get; init; }

        /// <summary>
        /// JSON data
        /// JSON 数据
        /// </summary>
        public string? Data { get; init; }

        /// <summary>
        /// Index key
        /// 索引键
        /// </summary>
        public string? IndexKey { get; init; }

        /// <summary>
        /// Importance
        /// 重要性
        /// </summary>
        public PersonProfileImportance? Importance { get; init; }

        /// <summary>
        /// Assignee id
        /// 经办人编号
        /// </summary>
        public long? AssigneeId { get; init; }
    }
}
