using com.etsoo.CoreFramework.Application;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Models;
using CRM.Server.Dto.PersonInfo;

namespace CRM.Server.RQ.PersonInfo
{
    /// <summary>
    /// Person info create request data
    /// 人员信息创建请求数据
    /// </summary>
    public record PersonInfoCreateRQ : IModelValidator
    {
        /// <summary>
        /// Person id
        /// 人员编号
        /// </summary>
        public long PersonId { get; init; }

        /// <summary>
        /// Items
        /// 项目
        /// </summary>
        public required IEnumerable<PersonInfoItem> Items { get; init; }

        /// <summary>
        /// Validate the model
        /// 验证模块
        /// </summary>
        /// <returns>Result</returns>
        public IActionResult? Validate()
        {
            var itemCount = Items.Count();

            if (itemCount < 1 || itemCount > 20 || Items.Any(item =>
            {
                var result = item.Validate();
                return result != null && !result.Ok;
            }))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Items));
            }

            return null;
        }
    }
}
