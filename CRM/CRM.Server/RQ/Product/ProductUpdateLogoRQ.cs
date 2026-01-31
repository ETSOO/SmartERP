namespace CRM.Server.RQ.Product
{
    /// <summary>
    /// Update product logo request data
    /// 更新产品标志请求数据
    /// </summary>
    public record ProductUpdateLogoRQ
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public int Id { get; init; }

        /// <summary>
        /// Url
        /// 网址
        /// </summary>
        public required string Url { get; init; }
    }
}