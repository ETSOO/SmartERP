using com.etsoo.Utils.String;

namespace PlatformShared.Dto.Document.Order
{
    /// <summary>
    /// Order template data
    /// 订单模板数据
    /// </summary>
    public record OrderTemplateData : IDocumentTemplateData
    {
        /// <summary>
        /// Subject
        /// 主题
        /// </summary>
        public string? Subject { get; set; }

        /// <summary>
        /// Web URL
        /// Web 网址
        /// </summary>
        public string WebUrl { get; set; } = string.Empty;

        /// <summary>
        /// Current user data
        /// 当前用户数据
        /// </summary>
        public required CurrentUserData User { get; init; }

        /// <summary>
        /// Organization view data
        /// 机构视图数据
        /// </summary>
        public required OrgViewData Org { get; init; }

        /// <summary>
        /// Order view data
        /// 订单视图数据
        /// </summary>
        public required OrderViewData Order { get; init; }

        /// <summary>
        /// Dictionary data
        /// 字典数据
        /// </summary>
        public required StringKeyDictionaryObject Dic { get; init; }

        /// <summary>
        /// Target name
        /// 目标对象名称
        /// </summary>
        public string TargetName => Order.Title;
    }
}
