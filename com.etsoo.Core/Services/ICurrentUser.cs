using System.Net;

namespace com.etsoo.Core.Services
{
    /// <summary>
    /// Current user interface
    /// 当前用户接口
    /// </summary>
    public interface ICurrentUser
    {
        /// <summary>
        /// Client IP address
        /// 客户端IP地址
        /// </summary>
        IPAddress ClientIp { get; }

        /// <summary>
        /// Is authenticated
        /// 是否已授权
        /// </summary>
        bool IsAuthenticated { get; }

        /// <summary>
        /// User id
        /// 用户编号
        /// </summary>
        int? Id { get; }

        /// <summary>
        /// Client language Cid
        /// 客户端语言编号
        /// </summary>
        string LanguageCid { get; }

        /// <summary>
        /// Current module
        /// 当前模块
        /// </summary>
        byte ModuleId { get; }

        /// <summary>
        /// Organization id
        /// 当前机构编号
        /// </summary>
        int? OrganizationId { get; }

        /// <summary>
        /// User roles
        /// 用户角色
        /// </summary>
        string[] Roles { get; }

        /// <summary>
        /// Login
        /// 登录
        /// </summary>
        /// <param name="id">Current user id</param>
        /// <param name="organizationId">User's organization</param>
        /// <param name="languageCid">Language cid</param>
        /// <param name="roles">Roles</param>
        void Login(int id, int organizationId, string languageCid, string[] roles);

        /// <summary>
        /// Is in role
        /// 是否在角色中
        /// </summary>
        /// <param name="role">Role</param>
        /// <returns>Result</returns>
        bool InRole(string role);

        /// <summary>
        /// Sign out
        /// 退出
        /// </summary>
        void SignOut();
    }
}