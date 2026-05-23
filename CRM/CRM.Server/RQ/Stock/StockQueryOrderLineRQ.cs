using com.etsoo.CoreFramework.Application;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Models;

namespace CRM.Server.RQ.Stock
{

    /// <summary>
    /// Query order line stock request data
    /// 查询订单行库存请求数据
    /// </summary>
    public record StockQueryOrderLineRQ : IModelValidator
    {
        /// <summary>
        /// Customer or supplier id
        /// 客户或供应商编号
        /// </summary>
        public long PersonId { get; init; }

        /// <summary>
        /// Org's location id
        /// 机构位置编号
        /// </summary>
        public int LocationId { get; init; }

        /// <summary>
        /// Stock id
        /// 库存编号
        /// </summary>
        public long? StockId { get; init; }

        /// <summary>
        /// Order ids
        /// 订单编号
        /// </summary>
        public required IEnumerable<long> Orders { get; init; }

        /// <summary>
        /// Validate the model
        /// 验证模块
        /// </summary>
        /// <returns>Result</returns>
        public IActionResult? Validate()
        {
            var orderIds = Orders.ToArray();
            if (orderIds.Length == 0 || orderIds.Distinct().Count() != orderIds.Length)
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Orders));
            }

            return null;
        }
    }
}
