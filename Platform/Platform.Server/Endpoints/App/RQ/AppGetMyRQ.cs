using com.etsoo.CoreFramework.Business;

namespace Platform.Server.Endpoints.App.RQ
{
    /// <summary>
    /// Get user's latest accessed applications request data
    /// 获取用户最近访问的应用请求数据
    /// </summary>
    public record AppGetMyRQ
    {
        /// <summary>
        /// Max items
        /// 最大项数
        /// </summary>
        public byte MaxItems { get; init; } = 10;

        /// <summary>
        /// Identity type
        /// 识别类型
        /// </summary>
        public IdentityType IdentityType { get; init; } = IdentityType.User;
    }
}
