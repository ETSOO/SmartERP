namespace PlatformShared.Dto
{
    /// <summary>
    /// Contact item
    /// 联系人项
    /// </summary>
    public record ContactItem : IdentityTypeData
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public long Id { get; init; }

        /// <summary>
        /// Preferred name
        /// 首先名
        /// </summary>
        public string? PreferredName { get; set; }

        /// <summary>
        /// Job title
        /// 职务
        /// </summary>
        public string? JobTitle { get; init; }
    }
}
