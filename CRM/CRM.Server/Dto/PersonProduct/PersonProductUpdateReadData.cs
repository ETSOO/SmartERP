using PlatformShared.Dto;

namespace CRM.Server.Dto.PersonProduct
{
    /// <summary>
    /// Person product update read data
    /// 人员个性化产品更新读取数据
    /// </summary>
    public record PersonProductUpdateReadData
    {
        /// <summary>
        /// Person id
        /// 人员编号
        /// </summary>
        public long PersonId { get; init; }

        /// <summary>
        /// Product id
        /// 产品编号
        /// </summary>
        public int ProductId { get; init; }

        /// <summary>
        /// Assigned ID
        /// 分配编号
        /// </summary>
        public string? AssignedId { get; init; }

        /// <summary>
        /// Json data
        /// JSON 数据
        /// </summary>
        public PersonProductJsonData? JsonData { get; init; }
    }
}
