using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Models;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Models;
using PlatformShared.Database.Models;

namespace CRM.Server.RQ.OrderPayment
{
    /// <summary>
    /// Update order payment request data
    /// 更新订单支付方式请求数据
    /// </summary>
    public record OrderPaymentUpdateRQ : UpdateModel<int>, IModelValidator
    {
        /// <summary>
        /// Kind
        /// 类型
        /// </summary>
        public OrderPaymentKind? Kind { get; init; }

        /// <summary>
        /// Title
        /// 标题
        /// </summary>
        public string? Title { get; init; }

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
            if (Title != null && Title.Length is not (>= 1 and <= 256))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Title));
            }

            return null;
        }
    }
}
