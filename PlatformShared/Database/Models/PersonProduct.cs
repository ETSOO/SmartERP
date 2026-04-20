using PlatformShared.Dto;

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
        /// Custom assigned id
        /// 自定义分配编号
        /// </summary>
        public string? AssignedId { get; set; }

        /// <summary>
        /// Json data
        /// JSON 数据
        /// </summary>
        public PersonProductJsonData? JsonData { get; set; }

        /// <summary>
        /// Updated time
        /// 上次更新时间
        /// </summary>
        public DateTimeOffset UpdatedTime { get; set; }

        /// <summary>
        /// Person
        /// 人员
        /// </summary>
        public Person Person { get; set; } = default!;

        /// <summary>
        /// Product
        /// 产品
        /// </summary>
        public Product Product { get; set; } = default!;
    }
}
