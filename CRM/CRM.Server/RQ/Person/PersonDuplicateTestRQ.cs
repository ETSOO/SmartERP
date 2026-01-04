using com.etsoo.CoreFramework.Application;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Models;
using PlatformShared.Database.Models;

namespace CRM.Server.RQ.Person
{
    /// <summary>
    /// Person duplicate test request data
    /// 人员重复测试请求数据
    /// </summary>
    public record PersonDuplicateTestRQ : IModelValidator
    {
        /// <summary>
        /// Excluded id
        /// 排除的编号
        /// </summary>
        public long? ExcludedId { get; init; }

        /// <summary>
        /// Name
        /// 名称 / 姓名
        /// </summary>
        public string? Name { get; init; }

        /// <summary>
        /// Info kind
        /// 信息类型
        /// </summary>
        public PersonInfoKind? InfoKind { get; init; }

        /// <summary>
        /// Info identifier
        /// 信息标识
        /// </summary>
        public string? Identifier { get; init; }

        /// <summary>
        /// Address
        /// 地址
        /// </summary>
        public string? Address { get; init; }

        /// <summary>
        /// Assigned id
        /// 分配的编号
        /// </summary>
        public string? AssignedId { get; init; }

        /// <summary>
        /// Validate the model
        /// 验证模块
        /// </summary>
        /// <returns>Result</returns>
        public IActionResult? Validate()
        {
            if (Name != null && Name.Length is not (>= 2 and <= 128))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Name));
            }

            if (Identifier != null && Identifier.Length is not (>= 3 and <= 256))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Identifier));
            }

            if (Address != null && Address.Length is not (>= 3 and <= 256))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Address));
            }

            if (AssignedId != null && AssignedId.Length is not (>= 3 and <= 20))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(AssignedId));
            }

            return null;
        }
    }
}
