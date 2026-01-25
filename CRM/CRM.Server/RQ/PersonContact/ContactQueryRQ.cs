using com.etsoo.CoreFramework.Application;
using com.etsoo.Utils.Actions;

namespace CRM.Server.RQ.PersonContact
{
    /// <summary>
    /// Contact query request data
    /// 联系人查询请求数据
    /// </summary>
    public record ContactQueryRQ : ContactListRQ
    {
        /// <summary>
        /// Job title
        /// 职位
        /// </summary>
        public string? JobTitle { get; init; }

        /// <summary>
        /// Description
        /// 描述
        /// </summary>
        public string? Description { get; init; }

        /// <summary>
        /// Contact information
        /// 联系信息
        /// </summary>
        public string? Info { get; init; }

        /// <summary>
        /// Address
        /// 地址
        /// </summary>
        public string? Address { get; init; }

        public override IActionResult? Validate()
        {
            var result = base.Validate();

            if (result != null)
                return result;

            if (JobTitle != null && JobTitle.Length is not (>= 1 and <= 50))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(JobTitle));
            }

            if (Description != null && Description.Length is not (>= 1 and <= 128))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Description));
            }

            if (Info != null && Info.Length is not (>= 1 and <= 256))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Info));
            }

            if (Address != null && Address.Length is not (>= 1 and <= 128))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Address));
            }

            return result;
        }
    }
}
