using PlatformShared.Database.Models;
using System.Text.Json.Serialization;

namespace CRM.Server.RQ.PersonProfile
{
    /// <summary>
    /// Person profile list request data
    /// 人员档案列表请求数据
    /// </summary>
    [JsonDerivedType(typeof(PersonProfileQueryRQ))]
    public record PersonProfileListRQ : QueryLongRQ
    {
        /// <summary>
        /// Person id
        /// 人员编号
        /// </summary>
        public long? PersonId { get; set; }

        /// <summary>
        /// Participant id, 0 for current user
        /// 参与者编号，0为当前用户
        /// </summary>
        public long? ParticipantId { get; set; }

        /// <summary>
        /// Kind
        /// 类型
        /// </summary>
        public PersonProfileKind? Kind { get; init; }

        /// <summary>
        /// Order / PO id
        /// 订单 / 采购编号
        /// </summary>
        public long? OrderId { get; init; }

        /// <summary>
        /// Happen date start
        /// 发生日期开始
        /// </summary>
        public DateTimeOffset? HappenDateStart { get; init; }

        /// <summary>
        /// Happen date end
        /// 发生日期结束
        /// </summary>
        public DateTimeOffset? HappenDateEnd { get; init; }

        /// <summary>
        /// Creation start
        /// 登记日期开始
        /// </summary>
        public DateTimeOffset? CreationStart { get; init; }

        /// <summary>
        /// Creation end
        /// 登记日期结束
        /// </summary>
        public DateTimeOffset? CreationEnd { get; init; }

        /// <summary>
        /// Importance
        /// 重要性
        /// </summary>
        public PersonProfileImportance? Importance { get; init; }

        /// <summary>
        /// Assignee id
        /// 经办人编号
        /// </summary>
        public int? AssigneeId { get; init; }

        /// <summary>
        /// Owner user id
        /// 所有者用户编号
        /// </summary>
        public long? UserId { get; init; }

        /// <summary>
        /// Is task or not
        /// 是否为任务
        /// </summary>
        public bool? IsTask { get; init; }
    }
}
