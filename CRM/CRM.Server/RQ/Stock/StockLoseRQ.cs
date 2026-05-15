using com.etsoo.CoreFramework.Application;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Models;
using CRM.Server.Dto.Stock;

namespace CRM.Server.RQ.Stock
{
    /// <summary>
    /// Stock loss request data
    /// 报损请求数据
    /// </summary>
    public record StockLoseRQ : IModelValidator
    {
        /// <summary>
        /// Organization location id
        /// 机构位置编号
        /// </summary>
        public int LocationId { get; set; }

        /// <summary>
        /// Title
        /// 标题
        /// </summary>
        public required string Title { get; init; }

        /// <summary>
        /// Description
        /// 描述
        /// </summary>
        public string? Description { get; init; }

        /// <summary>
        /// Items
        /// 项目
        /// </summary>
        public required IEnumerable<StockItem> Items { get; init; }

        /// <summary>
        /// Validate the model
        /// 验证模块
        /// </summary>
        /// <returns>Result</returns>
        public IActionResult? Validate()
        {
            if (Title.Length is not (>= 1 and <= 128))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Title));
            }

            if (Description != null && Description.Length is not (>= 1 and <= 1280))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Description));
            }

            if (!Items.Any() || Items.Any(i => i.Qty <= 0))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Items));
            }

            return null;
        }
    }
}
