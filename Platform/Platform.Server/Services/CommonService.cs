using com.etsoo.CoreFramework.Services;
using com.etsoo.CoreFramework.User;
using Platform.Server.Application;

namespace Platform.Server.Services
{
    /// <summary>
    /// Common service
    /// 通用服务
    /// </summary>
    public abstract class CommonService : ServiceBase<IMyApp, CurrentUser>, ICommonService
    {
        protected CommonService(IMyApp app, MyAppConfiguration configuration, CurrentUser? user, string flag, ILogger logger)
            : base(app, configuration, user, flag, logger)
        {
        }

        /// <summary>
        /// Is valid photo stream
        /// 是否为有效的照片流
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="small">Is small file</param>
        /// <returns>Result</returns>
        protected bool IsValidPhoto(Stream stream, bool small = false)
        {
            if (small)
            {
                // 4KB - 2MB
                return stream.Length is (>= 4_096 and <= 2_097_152);
            }
            else
            {
                // 10KB - 10MB
                return stream.Length is (>= 10_240 and <= 10_485_760);
            }
        }
    }
}
