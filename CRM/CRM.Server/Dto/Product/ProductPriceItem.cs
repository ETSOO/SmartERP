using com.etsoo.WebUtils.Attributes;
using PlatformShared.Dto;

namespace CRM.Server.Dto.Product
{
    /// <summary>
    /// Product price item
    /// 产品价格项
    /// </summary>
    public record ProductPriceItem : ProductSimplePriceItem
    {
        /// <summary>
        /// Promotion price
        /// 促销价
        /// </summary>
        public decimal? PromotionPrice { get; init; }

        /// <summary>
        /// Channel price
        /// 渠道价
        /// </summary>
        public decimal? ChannelPrice { get; init; }

        /// <summary>
        /// Cost price
        /// 成本价
        /// </summary>
        public decimal? CostPrice { get; init; }

        /// <summary>
        /// Validate
        /// 验证
        /// </summary>
        /// <returns>Result</returns>
        public override bool Validate()
        {
            if (!base.Validate()
                || PromotionPrice < 0
                || ChannelPrice < 0
                || CostPrice < 0
                || PromotionPrice > RetailPrice
                || ChannelPrice > RetailPrice
                || CostPrice > RetailPrice
            )
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
