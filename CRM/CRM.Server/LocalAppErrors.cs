using com.etsoo.CoreFramework.Application;
using CRM.Server.Properties;

namespace CRM.Server
{
    /// <summary>
    /// Local application errors
    /// 本地应用错误
    /// </summary>
    public class LocalAppErrors
    {
        /// <summary>
        /// Insufficient Stock
        /// 库存不足
        /// </summary>
        public static ApplicationError InsufficientStock => new("InsufficientStock", Resources.InsufficientStock);
    }
}
