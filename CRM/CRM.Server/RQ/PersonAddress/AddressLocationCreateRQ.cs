using com.etsoo.CoreFramework.Application;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Models;

namespace CRM.Server.RQ.PersonAddress
{
    /// <summary>
    /// Address location create request data
    /// 地址位置创建请求数据
    /// </summary>
    public record AddressLocationCreateRQ : IModelValidator
    {
        /// <summary>
        /// Name
        /// 名称
        /// </summary>
        public required string Name { get; init; }

        /// <summary>
        /// Parent address id
        /// 父地址编号
        /// </summary>
        public int ParentId { get; init; }

        /// <summary>
        /// Place id
        /// 地址编号
        /// </summary>
        public string? PlaceId { get; init; }

        /// <summary>
        /// Validate the model
        /// 验证模块
        /// </summary>
        /// <returns>Result</returns>
        public IActionResult? Validate()
        {
            if (Name.Length is not (>= 1 and <= 64))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Name));
            }

            if (PlaceId != null && PlaceId.Length is not (>= 1 and <= 30))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(PlaceId));
            }

            return null;
        }
    }
}
