namespace PlatformShared.Dto.Document.Order
{
    /// <summary>
    /// Order view's customer data
    /// 订单视图的客户数据
    /// </summary>
    public record OrderCustomerData
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public long Id { get; init; }

        /// <summary>
        /// Is legal person (enterprise)
        /// 是否为法人（企业）
        /// </summary>
        public bool IsLegalPerson { get; init; }

        /// <summary>
        /// Name
        /// 名称 / 姓名
        /// </summary>
        public required string Name { get; init; }

        /// <summary>
        /// Preferred name
        /// 首先名
        /// </summary>
        public string? PreferredName { get; set; }

        /// <summary>
        /// Assigned id
        /// 分配的编号
        /// </summary>
        public string? AssignedId { get; set; }

        /// <summary>
        /// Description
        /// 描述
        /// </summary>
        public string? Description { get; init; }

        /// <summary>
        /// Birthday
        /// 生日
        /// </summary>
        public DateTimeOffset? Birthday { get; init; }

        /// <summary>
        /// Categories
        /// 类目
        /// </summary>
        public IEnumerable<int>? Categories { get; init; }

        /// <summary>
        /// Infos
        /// 信息项目
        /// </summary>
        public required IEnumerable<PersonInfoViewItem> Infos { get; init; }
    }
}
