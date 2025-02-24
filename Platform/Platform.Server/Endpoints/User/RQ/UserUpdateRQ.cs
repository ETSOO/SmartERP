using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Models;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Models;

namespace Platform.Server.Endpoints.User.RQ
{
    /// <summary>
    /// Update user request data
    /// 更新用户请求数据
    /// </summary>
    public record UserUpdateRQ : UpdateModel<int>, IModelValidator
    {
        /// <summary>
        /// Name
        /// 姓名
        /// </summary>
        public string? Name { get; init; }

        /// <summary>
        /// Given name
        /// 名
        /// </summary>
        public string? GivenName { get; init; }

        /// <summary>
        /// Family name
        /// 姓
        /// </summary>
        public string? FamilyName { get; init; }

        /// <summary>
        /// Latin given name
        /// 拉丁名（拼音）
        /// </summary>
        public string? LatinGivenName { get; init; }

        /// <summary>
        /// Latin family name
        /// 拉丁姓（拼音）
        /// </summary>
        public string? LatinFamilyName { get; init; }

        /// <summary>
        /// Preferred name
        /// 首选名
        /// </summary>
        public string? PreferredName { get; init; }

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

            if (GivenName != null && GivenName.Length is not (>= 1 and <= 50))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(GivenName));
            }

            if (FamilyName != null && FamilyName.Length is not (>= 1 and <= 50))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(FamilyName));
            }

            if (LatinGivenName != null && LatinGivenName.Length is not (>= 1 and <= 50))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(LatinGivenName));
            }

            if (LatinFamilyName != null && LatinFamilyName.Length is not (>= 1 and <= 50))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(LatinFamilyName));
            }

            if (PreferredName != null && PreferredName.Length is not (>= 1 and <= 128))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(PreferredName));
            }

            return null;
        }
    }
}
