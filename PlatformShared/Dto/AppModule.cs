namespace PlatformShared.Dto
{
    /// <summary>
    /// Application module
    /// 应用模块
    /// </summary>
    public enum AppModule : byte
    {
        /// <summary>
        /// Organization
        /// 机构
        /// </summary>
        Org = 1,

        /// <summary>
        /// Department
        /// 部门
        /// </summary>
        Dept = 2,

        /// <summary>
        /// User
        /// 用户
        /// </summary>
        User = 3,

        /// <summary>
        /// Customer
        /// 客户
        /// </summary>
        Customer = 6,

        /// <summary>
        /// Supplier
        /// 供应商
        /// </summary>
        Supplier = 7,

        /// <summary>
        /// Product
        /// 产品
        /// </summary>
        Product = 8,

        /// <summary>
        /// Order
        /// 订单
        /// </summary>
        Order = 9,

        /// <summary>
        /// Purchase Order
        /// 采购订单
        /// </summary>
        PO = 10,

        /// <summary>
        /// Inventory
        /// 库存
        /// </summary>
        Inventory = 11,

        /// <summary>
        /// Finance
        /// 财务
        /// </summary>
        Finance = 18
    }
}
