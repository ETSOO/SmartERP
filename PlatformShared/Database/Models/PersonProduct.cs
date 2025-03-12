namespace PlatformShared.Database.Models
{
    /// <summary>
    /// Person product
    /// 个人产品
    /// </summary>
    public class PersonProduct
    {
        /// <summary>
        /// Person id
        /// 个人编号
        /// </summary>
        public long PersonId { get; set; }

        /// <summary>
        /// Product id
        /// 产品编号
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// Custom name
        /// 自定义名称
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Custom description
        /// 自定义描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Custom assigned id
        /// 自定义分配编号
        /// </summary>
        public string? AssignedId { get; set; }

        /// <summary>
        /// Currency
        /// 币种
        /// </summary>
        public string? Currency { get; set; }

        /// <summary>
        /// Retail price
        /// 销售价格
        /// </summary>
        public decimal? RetailPrice { get; set; }

        /// <summary>
        /// Updated time
        /// 上次更新时间
        /// </summary>
        public DateTimeOffset UpdatedTime { get; set; }
    }
}
