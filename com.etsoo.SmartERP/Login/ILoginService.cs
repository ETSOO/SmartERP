using com.etsoo.Core.Services;
using com.etsoo.SmartERP.Address;
using com.etsoo.SmartERP.Login;
using System;
using System.IO;
using System.Threading.Tasks;

namespace com.etsoo.SmartERP.Applications
{
    /// <summary>
    /// Main login service interface
    /// 主服务登录接口
    /// </summary>
    /// <typeparam name="T">Id type generic</typeparam>
    public interface ILoginService<T> : IAddressServiceHost, IMainService<T> where T : struct, IComparable
    {
        /// <summary>
        /// Change password
        /// 修改密码
        /// </summary>
        /// <param name="model">Data model</param>
        /// <returns>Action result</returns>
        Task<OperationResult> ChangePasswordAsync(ChangePasswordModel model);

        /// <summary>
        /// Async user login
        /// 异步用户登录
        /// </summary>
        /// <param name="model">Data model</param>
        /// <returns>Action result</returns>
        Task<OperationResult> LoginAsync(LoginModel model);

        /// <summary>
        /// Async user login with token
        /// 异步令牌登录
        /// </summary>
        /// <param name="model">Data model</param>
        /// <returns>Action result</returns>
        Task<OperationResult> LoginTokenAsync(LoginTokenModel model);

        /// <summary>
        /// Async view service summary data
        /// 异步浏览服务汇总数据
        /// </summary>
        /// <param name="stream">Stream to write</param>
        /// <param name="id">Field of data</param>
        Task ServiceSummaryAsync(Stream stream, string id);

        /// <summary>
        /// Async signout
        /// 异步退出
        /// </summary>
        /// <param name="method">Login method</param>
        /// <param name="clearToken">Clear token</param>
        /// <returns>Action result</returns>
        Task<OperationResult> SignoutAsync(LoginMethod method, bool clearToken);
    }
}