using System;
using System.Linq;
using System.Net;

namespace com.etsoo.Core.Services
{
    /// <summary>
    /// Current user
    /// 当前用户
    /// </summary>
    public class CurrentUser : ICurrentUser
    {
        /// <summary>
        /// Client IP address
        /// 客户端IP地址
        /// </summary>
        public IPAddress ClientIp { get; }

        /// <summary>
        /// Is authenticated
        /// 是否已授权
        /// </summary>
        public bool IsAuthenticated { get; private set; }

        /// <summary>
        /// User id
        /// 用户编号
        /// </summary>
        public int? Id { get; private set; }

        /// <summary>
        /// Client language Cid
        /// 客户端语言编号
        /// </summary>
        public string LanguageCid { get; private set; }

        /// <summary>
        /// Current module
        /// 当前模块
        /// </summary>
        public byte ModuleId { get; }

        /// <summary>
        /// Current organization id
        /// 当前机构编号
        /// </summary>
        public int? OrganizationId { get; private set; }

        /// <summary>
        /// User roles
        /// 用户角色
        /// </summary>
        public string[] Roles { get; private set; }

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="moduleId">User module id</param>
        /// <param name="ip">Client IP</param>
        public CurrentUser(byte moduleId, IPAddress ip)
        {
            ModuleId = moduleId;
            ClientIp = ip;
        }

        /// <summary>
        /// Login
        /// 登录
        /// </summary>
        /// <param name="id">Current user id</param>
        /// <param name="organizationId">User's organization</param>
        /// <param name="languageCid">Language cid</param>
        /// <param name="roles">Roles</param>
        public void Login(int id, int organizationId, string languageCid, string[] roles)
        {
            IsAuthenticated = true;
            Id = id;
            OrganizationId = organizationId;
            LanguageCid = languageCid;
            Roles = roles;
        }

        /// <summary>
        /// Is in role
        /// 是否在角色中
        /// </summary>
        /// <param name="role">Role</param>
        /// <returns>Result</returns>
        public bool InRole(string role)
        {
            if (Roles == null || Roles.Length == 0)
                return false;

            return Roles.Any(r => r.Equals(role, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Sign out
        /// 退出
        /// </summary>
        public void SignOut()
        {
            IsAuthenticated = false;
            Id = null;
            OrganizationId = null;
            Roles = null;
        }
    }
}