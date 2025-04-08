using com.etsoo.CoreFramework.Authentication;
using CRM.Server.Dto.Person;

namespace CRM.Server.Dto.PersonProfile
{
    /// <summary>
    /// Person profile inner view data
    /// 人员档案查询浏览数据
    /// </summary>
    public record PersonProfileInnerViewData
    {
        /// <summary>
        /// Person id
        /// 人员编号
        /// </summary>
        public long PersonId { get; init; }

        /// <summary>
        /// Person name
        /// 人员名称 / 姓名
        /// </summary>
        public required string PersonName { get; init; }

        /// <summary>
        /// Other persons
        /// 其他参与者
        /// </summary>
        public IEnumerable<PersonListItem>? Persons { get; init; }

        /// <summary>
        /// Order / Purcahse id
        /// 订单 / 采购编号
        /// </summary>
        public long? OrderId { get; init; }

        /// <summary>
        /// Order title
        /// 订单标题
        /// </summary>
        public string? OrderTitle { get; init; }

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
        /// User id
        /// 用户编号
        /// </summary>
        public long UserId { get; init; }

        /// <summary>
        /// User role for privacy control
        /// 隐私控制的用户角色
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
        /// Assignee id
        /// 执行人编号
        /// </summary>
        public long? AssigneeId { get; init; }

        /// <summary>
        /// Assignee name
        /// 执行人姓名
        /// </summary>
        public string? AssigneeName { get; init; }

        /// <summary>
        /// Is admin
        /// 是否为管理员
        /// </summary>
        public bool IsAdmin { get; init; }

        /// <summary>
        /// Links
        /// 链接
        /// </summary>
        public required IEnumerable<PersonProfileLinkItem> Links { get; init; }

        /// <summary>
        /// Attachments
        /// 附件
        /// </summary>
        public required IEnumerable<PersonProfileAttachmentItem> Attachments { get; init; }
    }
}
