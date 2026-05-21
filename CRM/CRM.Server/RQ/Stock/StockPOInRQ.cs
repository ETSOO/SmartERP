using com.etsoo.CoreFramework.Application;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Models;
using CRM.Server.Dto.Stock;

namespace CRM.Server.RQ.Stock
{
    /// <summary>
    /// Stock PO receiving
    /// 库存采购入库
    /// </summary>
    public record StockPOInRQ : IModelValidator
    {
        /// <summary>
        /// Supplier id
        /// 供应商编号
        /// </summary>
        public long SupplierId { get; init; }

        /// <summary>
        /// Shipping address id
        /// 发货地址编号
        /// </summary>
        public int LocationFromId { get; init; }

        /// <summary>
        /// Receiving address id
        /// 收货地址编号
        /// </summary>
        public int LocationToId { get; init; }

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
        /// Tracking number
        /// 物流编号
        /// </summary>
        public string? TrackingNumber { get; init; }

        /// <summary>
        /// POs
        /// 采购
        /// </summary>
        public required IEnumerable<long> POs { get; init; }

        /// <summary>
        /// Items
        /// 项目
        /// </summary>
        public required IEnumerable<StockOrderItem> Items { get; init; }

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

            if (TrackingNumber != null && TrackingNumber.Length is not (>= 1 and <= 20))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(TrackingNumber));
            }

            if (!POs.Any())
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(POs));
            }

            if (!Items.Any() || Items.Any(i => i.Qty <= 0) || !Items.IsOrderLineUnique())
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Items));
            }

            return null;
        }
    }
}
