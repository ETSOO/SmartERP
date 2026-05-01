namespace CRM.Server.RQ.POLine
{
    /// <summary>
    /// Complete execution request data
    /// 完成执行请求数据
    /// </summary>
    public record POLineCompleteRQ
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public long Id { get; init; }

        /// <summary>
        /// Asset id
        /// 资产编号
        /// </summary>
        public int? AssetId { get; init; }
    }
}
