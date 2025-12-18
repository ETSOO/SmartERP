using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Business;
using com.etsoo.Utils.Actions;
using CRM.Server.RQ.Person;
using PlatformShared.Database.Models;

namespace CRM.Server.RQ.Supplier
{
    /// <summary>
    /// Create supplier request data
    /// 创建供应商请求数据
    /// </summary>
    public record SupplierCreateRQ
    {
        /// <summary>
        /// Is legal person (enterprise)
        /// 是否为法人（企业）
        /// </summary>
        public bool IsLegalPerson { get; init; }

        /// <summary>
        /// Name
        /// 名称 / 姓名
        /// </summary>
        public required string Name { get; init; }

        /// <summary>
        /// Preferred name
        /// 首先名
        /// </summary>
        public string? PreferredName { get; init; }

        /// <summary>
        /// Assigned id
        /// 分配的编号
        /// </summary>
        public string? AssignedId { get; init; }

        /// <summary>
        /// Description
        /// 描述
        /// </summary>
        public string? Description { get; init; }

        /// <summary>
        /// Birthday
        /// 生日
        /// </summary>
        public DateTimeOffset? Birthday { get; init; }

        /// <summary>
        /// Phone
        /// 电话
        /// </summary>
        public string? Phone { get; init; }

        /// <summary>
        /// Mobile
        /// 手机
        /// </summary>
        public string? Mobile { get; init; }

        /// <summary>
        /// Email
        /// 电子邮箱
        /// </summary>
        public string? Email { get; init; }

        /// <summary>
        /// PIN
        /// 身份证号码
        /// </summary>
        public string? Pin { get; init; }

        /// <summary>
        /// Address
        /// 地址
        /// </summary>
        public AddressCreateRQ? Address { get; init; }

        /// <summary>
        /// Categories
        /// 类目
        /// </summary>
        public IEnumerable<int>? Categories { get; init; }

        /// <summary>
        /// Keywords
        /// 关键词
        /// </summary>
        public IEnumerable<string>? Tags { get; init; }

        /// <summary>
        /// Status
        /// 状况
        /// </summary>
        public EntityStatus? Status { get; init; }

        /// <summary>
        /// Validate the model
        /// 验证模块
        /// </summary>
        /// <returns>Result</returns>
        public IActionResult? Validate()
        {
            if (Name.Length is not (>= 1 and <= 128))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Name));
            }

            if (PreferredName != null && PreferredName.Length is not (>= 1 and <= 128))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(PreferredName));
            }

            if (AssignedId != null && AssignedId.Length is not (>= 1 and <= 20))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(AssignedId));
            }

            if (Pin != null && Pin.Length is not (>= 6 and <= 20))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Pin));
            }

            if (Description != null && Description.Length is not (>= 1 and <= 1280))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Description));
            }

            if (Phone != null)
            {
                if (Phone.Length is < 1 or > 20)
                {
                    return ApplicationErrors.NoValidData.AsResult(nameof(Phone));
                }

                var phoneResult = RQExtentions.ValidatePersonInfo(PersonInfoKind.Phone, Phone);
                if (phoneResult != null)
                    return phoneResult;
            }

            if (Mobile != null)
            {
                if (Mobile.Length is < 1 or > 20)
                {
                    return ApplicationErrors.NoValidData.AsResult(nameof(Mobile));
                }

                var mobileResult = RQExtentions.ValidatePersonInfo(PersonInfoKind.Mobile, Mobile);
                if (mobileResult != null)
                    return mobileResult;
            }

            if (Email != null)
            {
                if (Email.Length is < 1 or > 256)
                {
                    return ApplicationErrors.NoValidData.AsResult(nameof(Mobile));
                }

                var emailResult = RQExtentions.ValidatePersonInfo(PersonInfoKind.Email, Email);
                if (emailResult != null)
                    return emailResult;
            }

            if (Address != null)
            {
                var addressResult = Address.Validate();
                if (addressResult != null)
                    return addressResult;
            }

            if (Tags != null && Tags.Any(t => t.Length is < 1 or > 128))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Tags));
            }

            return null;
        }
    }
}
