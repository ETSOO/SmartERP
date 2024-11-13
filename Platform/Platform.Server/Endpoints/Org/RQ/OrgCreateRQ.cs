using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Business;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Models;

namespace Platform.Server.Endpoints.Org.RQ
{
    /// <summary>
    /// Create organization request data
    /// 创建组织请求数据
    /// </summary>
    public record OrgCreateRQ : IModelValidator
    {
        /// <summary>
        /// Organization name
        /// 组织名称
        /// </summary>
        public required string Name { get; init; }

        /// <summary>
        /// Brand
        /// 品牌
        /// </summary>
        public string? Brand { get; init; }

        /// <summary>
        /// PIN, unique code
        /// PIN，唯一代码
        /// </summary>
        public string? Pin { get; init; }

        /// <summary>
        /// Parent id
        /// 父级编号
        /// </summary>
        public int? ParentId { get; init; }

        /// <summary>
        /// Status
        /// 状况
        /// </summary>
        public EntityStatus? Status { get; init; }

        /// <summary>
        /// Query keyword
        /// 查询关键字
        /// </summary>
        public string? QueryKeyword { get; init; }

        /// <summary>
        /// Validate the model
        /// 验证模块
        /// </summary>
        /// <returns>Result</returns>
        public IActionResult? Validate()
        {
            if (Name.Length is not (>= 2 and <= 128))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Name));
            }

            if (Brand != null && Brand.Length is not (>= 2 and <= 30))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Brand));
            }

            if (Pin != null && Pin.Length is not (>= 6 and <= 20))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Pin));
            }

            if (QueryKeyword != null && QueryKeyword.Length is not (>= 2 and <= 30))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(QueryKeyword));
            }

            return null;
        }
    }
}
