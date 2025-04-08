using com.etsoo.CoreFramework.Authentication;
using com.etsoo.CoreFramework.Business;

namespace PlatformShared.Database.Models
{
    /// <summary>
    /// Person profile kind
    /// 个人资料种类
    /// </summary>
    public enum PersonProfileKind : byte
    {
        /// <summary>
        /// Normal
        /// 默认
        /// </summary>
        Normal = 1,

        /// <summary>
        /// Schedule
        /// 日程
        /// </summary>
        Schedule = 106,

        /// <summary>
        /// Finance
        /// 财务
        /// </summary>
        Finance = 108,

        /// <summary>
        /// Agile
        /// 敏捷管理
        /// </summary>
        Agile = 110
    }

    /// <summary>
    /// Person profile importance
    /// 人员资料重要性
    /// </summary>
    public enum PersonProfileImportance : byte
    {
        /// <summary>
        /// Low
        /// 低
        /// </summary>
        Low = 1,

        /// <summary>
        /// Normal
        /// 一般
        /// </summary>
        Normal = 3,

        /// <summary>
        /// Important
        /// 重要
        /// </summary>
        Important = 6,

        /// <summary>
        /// Urgent
        /// 紧急
        /// </summary>
        Urgent = 9
    }

    /// <summary>
    /// Person profile
    /// 个人资料
    /// </summary>
    public class PersonProfile
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Person id
        /// 个人编号
        /// </summary>
        public long PersonId { get; set; }

        /// <summary>
        /// Other participants
        /// 其他参与者
        /// </summary>
        public List<long>? Persons { get; set; }

        /// <summary>
        /// Order id
        /// 订单编号
        /// </summary>
        public long? OrderId { get; set; }

        /// <summary>
        /// Kind
        /// 类型
        /// </summary>
        public PersonProfileKind Kind { get; set; }

        /// <summary>
        /// Title
        /// 标题
        /// </summary>
        public string Title { get; set; } = default!;

        /// <summary>
        /// Comment
        /// 评价
        /// </summary>
        public string Comment { get; set; } = default!;

        /// <summary>
        /// Location
        /// 位置
        /// </summary>
        public string? Location { get; set; }

        /// <summary>
        /// Location id
        /// 位置编号
        /// </summary>
        public int? LocationId { get; set; }

        /// <summary>
        /// Happen date
        /// 发生日期
        /// </summary>
        public DateTimeOffset HappenDate { get; set; }

        /// <summary>
        /// Happen date end
        /// 发生日期结束
        /// </summary>
        public DateTimeOffset? HappenDateEnd { get; set; }

        /// <summary>
        /// User id
        /// 用户编号
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// User role used for privacy control
        /// 用于隐私控制的用户角色
        /// </summary>
        public UserRole? UserRole { get; set; }

        /// <summary>
        /// Creation
        /// 登记时间
        /// </summary>
        public DateTimeOffset Creation { get; set; }

        /// <summary>
        /// Status
        /// 状态
        /// </summary>
        public EntityStatus Status { get; set; }

        /// <summary>
        /// JSON data
        /// JSON 数据
        /// </summary>
        public string? Data { get; set; }

        /// <summary>
        /// Index key
        /// 索引键
        /// </summary>
        public string? IndexKey { get; set; }

        /// <summary>
        /// Importance
        /// 重要性
        /// </summary>
        public PersonProfileImportance? Importance { get; set; }

        /// <summary>
        /// Assignee id
        /// 经办人编号
        /// </summary>
        public long? AssigneeId { get; set; }

        /// <summary>
        /// User who assigned the profile
        /// 档案经办人用户
        /// </summary>
        public Person? Assignee { get; }

        /// <summary>
        /// User who created the profile
        /// 创建档案的用户
        /// </summary>
        public Person User { get; } = default!;

        /// <summary>
        /// Order or purcahse
        /// 订单或采购
        /// </summary>
        public OrderHeader? Order { get; }

        /// <summary>
        /// Person related
        /// 关联的人员
        /// </summary>
        public Person Person { get; } = default!;

        /// <summary>
        /// Attachments
        /// 附件
        /// </summary>
        public ICollection<PersonProfileAttachment> Attachments { get; } = default!;

        /// <summary>
        /// Links
        /// 关联
        /// </summary>
        public ICollection<PersonProfileLink> Links { get; } = default!;

        /// <summary>
        /// Target links
        /// 目标关联
        /// </summary>
        public ICollection<PersonProfileLink> TargetLinks { get; } = default!;
    }
}
