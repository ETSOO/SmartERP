using com.etsoo.Database;

namespace CRM.Server.RQ.PersonProduct
{
    /// <summary>
    /// Person product query request data
    /// 人员个性化产品查询请求数据
    /// </summary>
    public record PersonProductQueryRQ
    {
        /// <summary>
        /// Person id
        /// 人员编号
        /// </summary>
        public long? PersonId { get; init; }

        /// <summary>
        /// Product id
        /// 产品编号
        /// </summary>
        public int? ProductId { get; init; }

        /// <summary>
        /// Assigned ID
        /// 分配编号
        /// </summary>
        public string? AssignedId { get; init; }

        /// <summary>
        /// Query paging data
        /// 查询分页数据
        /// </summary>
        public QueryPagingData? QueryPaging { get; init; }
    }
}
