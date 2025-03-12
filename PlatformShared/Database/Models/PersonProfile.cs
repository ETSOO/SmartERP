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
        Normal = 1,
        Schedule = 106,
        Finance = 108
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
        /// Core user id
        /// 核心用户编号
        /// </summary>
        public int CoreUserId { get; set; }

        /// <summary>
        /// User role used for privacy control
        /// 用于隐私控制的用户角色
        /// </summary>
        public UserRole UserRole { get; set; }

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
    }
}
