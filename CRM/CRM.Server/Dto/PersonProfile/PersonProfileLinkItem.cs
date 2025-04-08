using PlatformShared.Database.Models;

namespace CRM.Server.Dto.PersonProfile
{
    /// <summary>
    /// Person profile link item
    /// 人员档案链接项
    /// </summary>
    public record PersonProfileLinkItem
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public long Id { get; init; }

        /// <summary>
        /// Kind
        /// 类型
        /// </summary>
        public PersonProfileLinkKind Kind { get; init; }

        /// <summary>
        /// Target profile id
        /// 目标档案编号
        /// </summary>
        public long? TargetProfileId { get; init; }

        /// <summary>
        /// Target profile title
        /// 目标档案标题
        /// </summary>
        public string? TargetProfileTitle { get; init; }

        /// <summary>
        /// Content
        /// 内容
        /// </summary>
        public string? Content { get; init; }

        /// <summary>
        /// User id
        /// 用户编号
        /// </summary>
        public long UserId { get; init; }

        /// <summary>
        /// User name
        /// 用户姓名
        /// </summary>
        public required string UserName { get; init; }

        /// <summary>
        /// Creation
        /// 登记时间
        /// </summary>
        public DateTimeOffset Creation { get; init; }

        /// <summary>
        /// Is the author of self
        /// 自己是否为作者
        /// </summary>
        public bool IsSelf { get; init; }
    }
}
