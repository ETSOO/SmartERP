using com.etsoo.WebUtils.Attributes;

namespace PlatformShared.Dto
{
    /// <summary>
    /// Product custom data
    /// 产品自定义数据
    /// </summary>
    public record ProductCustomData
    {
        /// <summary>
        /// Culture
        /// 文化
        /// </summary>
        public required string Culture { get; init; }

        /// <summary>
        /// Name
        /// 名称
        /// </summary>
        public required string Name { get; init; }

        /// <summary>
        /// Description
        /// 描述
        /// </summary>
        public string? Description { get; init; }

        /// <summary>
        /// Validate
        /// 验证
        /// </summary>
        /// <returns>Result</returns>
        public virtual bool Validate()
        {
            if (!new LanguageCodeAttribute().IsValid(Culture)
                || Name.Length is not (>= 1 and <= 256)
                || (Description != null && Description.Length is not (>= 1 and <= 2560))
            )
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
