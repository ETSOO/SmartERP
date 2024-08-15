namespace Platform.Server.Dto.User
{
    /// <summary>
    /// Core user data
    /// 核心用户数据
    /// </summary>
    public record UserData
    {
        /// <summary>
        /// Name
        /// 姓名
        /// </summary>
        public required string Name { get; init; }

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
        /// Organization name
        /// 机构名称
        /// </summary>
        public required string OrganizationName { get; init; }

        /// <summary>
        /// Organization trade as
        /// 机构交易名称
        /// </summary>
        public string? TradeAs { get; init; }

        /// <summary>
        /// Organization brand
        /// 机构品牌
        /// </summary>
        public string? Brand { get; init; }
    }
}
