using com.etsoo.CoreFramework.Business;

namespace CRM.Server.Dto.Asset
{
    /// <summary>
    /// Asset query data
    /// 资产查询数据
    /// </summary>
    public record AssetQueryData
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public int Id { get; init; }

        /// <summary>
        /// Product name
        /// 产品名称
        /// </summary>
        public required string Product {  get; init; }

        /// <summary>
        /// Title
        /// 标题
        /// </summary>
        public required string Sn { get; init; }

        /// <summary>
        /// Description
        /// 描述
        /// </summary>
        public string? Description { get; init; }

        /// <summary>
        /// Expiry
        /// 到期时间
        /// </summary>
        public DateTimeOffset Expiry { get; init; }

        /// <summary>
        /// Remaining times
        /// 剩余次数
        /// </summary>
        public int? Times { get; init; }

        /// <summary>
        /// Remaining amount
        /// 剩余金额
        /// </summary>
        public decimal? Amount { get; init; }

        /// <summary>
        /// Status
        /// 状况
        /// </summary>
        public EntityStatus Status { get; init; }

        /// <summary>
        /// Creation
        /// 登记时间
        /// </summary>
        public DateTimeOffset Creation { get; init; }
    }
}
