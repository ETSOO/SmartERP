using PlatformShared.Database.Models;

namespace CRM.Server.RQ.Product
{
    /// <summary>
    /// Product query request data
    /// 产品查询请求数据
    /// </summary>
    public record ProductQueryRQ : ProductListRQ
    {
        /// <summary>
        /// Currency
        /// 币种
        /// </summary>
        public string? Currency { get; init; }

        /// <summary>
        /// Unit id
        /// 单位编号
        /// </summary>
        public int? UnitId { get; init; }
    }
}
