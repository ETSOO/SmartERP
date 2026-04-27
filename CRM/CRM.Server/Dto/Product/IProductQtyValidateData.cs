namespace CRM.Server.Dto.Product
{
    /// <summary>
    /// Product quantity validate data interface
    /// 产品数量验证数据接口
    /// </summary>
    public interface IProductQtyValidateData
    {
        /// <summary>
        /// Name
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Minimum purchase qty
        /// 最少购买量
        /// </summary>
        public decimal? MinQty { get; init; }

        /// <summary>
        /// Purchase minimum unit
        /// 购买最小单位
        /// </summary>
        public decimal? StepQty { get; init; }

        /// <summary>
        /// Maximum purchase qty
        /// 最大购买量
        /// </summary>
        public decimal? CapQty { get; init; }
    }
}
