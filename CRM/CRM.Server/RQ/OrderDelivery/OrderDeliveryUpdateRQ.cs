using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Models;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Models;
using PlatformShared.Database.Models;

namespace CRM.Server.RQ.OrderDelivery
{
    /// <summary>
    /// Update order delivery request data
    /// 更新订单配送方式请求数据
    /// </summary>
    public record OrderDeliveryUpdateRQ : UpdateModel<int>, IModelValidator
    {
        /// <summary>
        /// Kind
        /// 类型
        /// </summary>
        public OrderDeliveryKind? Kind { get; init; }

        /// <summary>
        /// Title
        /// 标题
        /// </summary>
        public string? Title { get; init; }

        /// <summary>
        /// Description
        /// 描述
        /// </summary>
        public string? Description { get; init; }

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
            if (Title != null && Title.Length is not (>= 1 and <= 128))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Title));
            }

            if (Description != null && Description.Length is not (>= 1 and <= 1280))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Description));
            }

            return null;
        }
    }
}
