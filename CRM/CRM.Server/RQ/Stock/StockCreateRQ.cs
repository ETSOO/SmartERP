using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Models;

namespace CRM.Server.RQ.Stock
{
    /// <summary>
    /// Create stock request data
    /// 创建库存请求数据
    /// </summary>
    public record StockCreateRQ : IModelValidator
    {

        /// <summary>
        /// Validate the model
        /// 验证模块
        /// </summary>
        /// <returns>Result</returns>
        public IActionResult? Validate()
        {
            return null;
        }
    }
}
