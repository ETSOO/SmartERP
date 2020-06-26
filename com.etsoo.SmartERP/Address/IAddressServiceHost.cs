namespace com.etsoo.SmartERP.Address
{
    /// <summary>
    /// Address service host
    /// 地址服务宿主接口
    /// </summary>
    public interface IAddressServiceHost
    {
        /// <summary>
        /// Address service interface
        /// 地址服务接口
        /// </summary>
        public IAddressService Address { get; }
    }
}