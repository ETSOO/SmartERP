using com.etsoo.CoreFramework.Application;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Models;
using com.etsoo.Utils.String;
using Platform.Server.Dto.Org;

namespace Platform.Server.Endpoints.Org.RQ
{
    /// <summary>
    /// Create resource request data
    /// 创建资源请求数据
    /// </summary>
    public record OrgCreateResourceRQ : IModelValidator, IOrgRQ
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public int? Id { get; init; }

        /// <summary>
        /// Key
        /// 键名
        /// </summary>
        public string? Key { get; init; }

        /// <summary>
        /// Organization Id, null means global
        /// 所属机构，null 表示全局
        /// </summary>
        public int? OrgId { get; set; }

        /// <summary>
        /// Items
        /// 项目
        /// </summary>
        public IEnumerable<OrgResourceItem>? Items { get; init; }

        /// <summary>
        /// Validate the model
        /// 验证模块
        /// </summary>
        /// <returns>Result</returns>
        public IActionResult? Validate()
        {
            if (Key != null && Key.Length is not (>= 1 and <= 50))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Key));
            }

            if (Items != null && Items.Any(item => item.Culture.Length is not (>= 2 and <= 10)
                || (item.Title != null && item.Title.Length is not (>= 1 and <= 256))
                || (item.Description != null && item.Description.Length is not (>= 1 and <= 2560))
                || (item.JsonData != null && !item.JsonData.IsJson())))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Items));
            }

            return null;
        }
    }
}
