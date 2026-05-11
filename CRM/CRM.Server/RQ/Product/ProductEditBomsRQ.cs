using com.etsoo.CoreFramework.Application;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Models;
using CRM.Server.Dto.Product;

namespace CRM.Server.RQ.Product
{
    /// <summary>
    /// Edit product BOMs request data
    /// 编辑产品物料清单请求数据
    /// </summary>
    public record ProductEditBomsRQ : IModelValidator
    {
        /// <summary>
        /// Parent product id
        /// 父级产品编号
        /// </summary>
        public int ParentId { get; init; }

        /// <summary>
        /// Items
        /// 项目
        /// </summary>
        public required IEnumerable<ProductBomItem> Items { get; init; }

        /// <summary>
        /// Validate the model
        /// 验证模块
        /// </summary>
        /// <returns>Result</returns>
        public IActionResult? Validate()
        {
            if (Items.Select(i => i.ProductId).Distinct().Count() != Items.Count())
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Items));
            }

            if (Items.Any(i => i.Qty <= 0))
            {
                return ApplicationErrors.NoValidData.AsResult("Qty");
            }

            return null;
        }
    }
}
