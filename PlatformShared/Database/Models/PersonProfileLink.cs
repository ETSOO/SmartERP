namespace PlatformShared.Database.Models
{
    /// <summary>
    /// Person profile link kind
    /// 人员资料关联种类
    /// </summary>
    public enum PersonProfileLinkKind : byte
    {
        /// <summary>
        /// Comment
        /// 评论
        /// </summary>
        Comment = 1,

        /// <summary>
        /// Related
        /// 相关
        /// </summary>
        Related = 3,

        /// <summary>
        /// Sequel
        /// 续集
        /// </summary>
        Sequel = 6,

        /// <summary>
        /// Link
        /// 链接
        /// </summary>
        Link = 9
    }

    /// <summary>
    /// Person profile link
    /// 人员资料关联
    /// </summary>
    public class PersonProfileLink
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Profile id
        /// 档案编号
        /// </summary>
        public long ProfileId { get; set; }

        /// <summary>
        /// Kind
        /// 类型
        /// </summary>
        public PersonProfileLinkKind Kind { get; set; }

        /// <summary>
        /// Target profile id
        /// 目标档案编号
        /// </summary>
        public long? TargetProfileId { get; set; }

        /// <summary>
        /// Content
        /// 内容
        /// </summary>
        public string? Content { get; set; }

        /// <summary>
        /// User id
        /// 用户编号
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// Creation
        /// 登记日期
        /// </summary>
        public DateTimeOffset Creation { get; set; }

        /// <summary>
        /// Profile
        /// 档案
        /// </summary>
        public PersonProfile Profile { get; } = null!;

        /// <summary>
        /// Target profile
        /// 目标档案
        /// </summary>
        public PersonProfile? TargetProfile { get; }

        /// <summary>
        /// User
        /// 用户
        /// </summary>
        public Person User { get; } = null!;
    }
}
