using com.etsoo.WebUtils.Attributes;

namespace PlatformShared.Dto
{
    /// <summary>
    /// Product simple price item
    /// 产品简单价格项
    /// </summary>
    public record ProductSimplePriceItem
    {
        /// <summary>
        /// Currency
        /// 币种
        /// </summary>
        public required string Currency { get; init; }

        /// <summary>
        /// Retail price
        /// 零售价
        /// </summary>
        public required decimal RetailPrice { get; init; }

        /// <summary>
        /// Validate
        /// 验证
        /// </summary>
        /// <returns>Result</returns>
        public virtual bool Validate()
        {
            if (!new CurrencyAttribute().IsValid(Currency)
                || RetailPrice < 0
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
