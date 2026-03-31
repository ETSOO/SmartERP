using com.etsoo.CoreFramework.Application;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Models;
using PlatformShared.Database.Models;

namespace CRM.Server.RQ.OrderDelivery
{
    /// <summary>
    /// Create order delivery request data
    /// 创建订单配送方式请求数据
    /// </summary>
    public record OrderDeliveryCreateRQ : IModelValidator
    {
        /// <summary>
        /// Kind
        /// 类型
        /// </summary>
        public required OrderDeliveryKind Kind { get; init; }

        /// <summary>
        /// Title
        /// 标题
        /// </summary>
        public required string Title { get; init; }

        /// <summary>
        /// Is valid
        /// 是否有效
        /// </summary>
        public bool? IsValid { get; init; }

        /// <summary>
        /// Order index
        /// 排序数
        /// </summary>
        public short? OrderIndex { get; init; }

        /// <summary>
        /// Validate the model
        /// 验证模块
        /// </summary>
        /// <returns>Result</returns>
        public IActionResult? Validate()
        {
            if (Title.Length is not (>= 1 and <= 256))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Title));
            }

            return null;
        }
    }
}
