namespace PlatformShared.Database.Models
{
    /// <summary>
    /// Person info kind
    /// 个人信息类型
    /// </summary>
    public enum PersonInfoKind : byte
    {
        Email = 1,
        Mobile = 5,
        Phone = 9,
        QQ = 13,
        WeChat = 17,
        Weibo = 21,
        Facebook = 25,
        Twitter = 29,
        LinkedIn = 33,
        Instagram = 37,
        Website = 100
    }

    /// <summary>
    /// Person info
    /// 个人信息
    /// </summary>
    public class PersonInfo
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Person id
        /// 人员编号
        /// </summary>
        public long PersonId { get; set; }

        /// <summary>
        /// Kind
        /// 类型
        /// </summary>
        public PersonInfoKind Kind { get; set; }

        /// <summary>
        /// Identifier
        /// 标识
        /// </summary>
        public string Identifier { get; set; } = null!;

        /// <summary>
        /// Description
        /// 描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Creation
        /// 登记时间
        /// </summary>
        public DateTime Creation { get; set; }

        /// <summary>
        /// Core user id
        /// 核心用户编号
        /// </summary>
        public int CoreUserId { get; set; }

        /// <summary>
        /// Subscribed or not
        /// 是否订阅
        /// </summary>
        public bool? Subscribed { get; set; }

        /// <summary>
        /// Is default or not
        /// 是否默认
        /// </summary>
        public bool IsDefault { get; set; }

        /// <summary>
        /// Person belongs to
        /// 所属人员
        /// </summary>
        public Person Person { get; set; } = null!;
    }
}
