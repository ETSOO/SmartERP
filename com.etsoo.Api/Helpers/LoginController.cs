using com.etsoo.Core.Services;
using com.etsoo.SmartERP.Applications;
using com.etsoo.SmartERP.Login;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace com.etsoo.Api.Helpers
{
    /// <summary>
    /// Common Login Controller
    /// 登录抽象控制器
    /// </summary>
    /// <typeparam name="S">MainApp login service</typeparam>
    public abstract class LoginController<S, T> : CommonController<S, T> where T : struct, IComparable where S : ILoginService<T>
    {
        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="service">Current service</param>
        /// <param name="distributedCache">Distributed cache</param>
        public LoginController(S service, IDistributedCache distributedCache) : base(service, distributedCache)
        {

        }

        /// <summary>
        /// Change password
        /// 修改密码
        /// </summary>
        /// <returns>Action task</returns>
        [HttpPost("ChangePassword")]
        public async Task ChangePassword(ChangePasswordModel model)
        {
            // Result
            var result = await Service.ChangePasswordAsync(model);

            // Output
            await ResultContentAsync(result);
        }

        /// <summary>
        /// Country list
        /// 国家列表
        /// </summary>
        /// <param name="organizationId">Limited organization id</param>
        [AllowAnonymous]
        [HttpGet("CountryList/{organizationId?}")]
        public async Task CountryList(int? organizationId)
        {
            await Service.Address.CountryListAsync(Response.Body, organizationId, DataFormat.Json);
        }

        private string CreateToken()
        {
            // Token handler
            var tokenHandler = new JwtSecurityTokenHandler();

            // Key bytes
            var key = Encoding.ASCII.GetBytes(Service.Application.Configuration.SymmetricKey);

            // Service user
            var serviceUser = Service.User;

            // Token descriptor
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, serviceUser.Id.ToString()),
                    new Claim(ClaimTypes.Role, string.Join(",", serviceUser.Roles)),
                    new Claim("OrganizationId", serviceUser.OrganizationId.ToString()),
                    new Claim("LanguageCid", serviceUser.LanguageCid),
                    new Claim(ClaimTypes.Locality, serviceUser.ClientIp.ToString())
                }),

                // Suggest to refresh it at 5 minutes interval, two times to update
                Expires = DateTime.UtcNow.AddMinutes(12),

                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            };

            // Create the token
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        private void DoLoginResult(OperationResult result)
        {
            if (result.OK)
            {
                // Hold the token value and then return to client
                result.Data["authorization"] = CreateToken();

                // Suggested refresh seconds
                result.Data["refresh_seconds"] = 300;
            }
        }

        /// <summary>
        /// Login for authentication
        /// 登录授权
        /// </summary>
        /// <param name="model">Data model</param>
        /// <returns>Result</returns>
        [AllowAnonymous]
        [HttpPost("Login")]
        public async Task Login(LoginModel model)
        {
            // Act
            var result = await Service.LoginAsync(model);

            // Do result
            DoLoginResult(result);

            // Output
            await ResultContentAsync(result);
        }

        /// <summary>
        /// Login with token
        /// 令牌登录
        /// </summary>
        /// <param name="model">Data model</param>
        /// <returns>Result</returns>
        [AllowAnonymous]
        [HttpPost("LoginToken")]
        public async Task LoginToken(LoginTokenModel model)
        {
            // Act
            var result = await Service.LoginTokenAsync(model);

            // Do result
            DoLoginResult(result);

            // Output
            await ResultContentAsync(result);
        }

        /// <summary>
        /// Refresh the user token
        /// </summary>
        /// <returns>Token string</returns>
        [HttpPut("RefreshToken")]
        public async Task RefreshToken()
        {
            // Result
            var result = new OperationResult();

            // Recreate the token
            result.Data["authorization"] = CreateToken();

            // Output
            await ResultContentAsync(result);
        }

        /// <summary>
        /// Current user service summary data
        /// 当前用户服务汇总信息
        /// </summary>
        /// <param name="id">Field of data</param>
        /// <returns>JSON data</returns>
        [HttpGet("ServiceSummary/{id}")]
        public async Task ServiceSummary(string id)
        {
            Response.ContentType = "application/json";
            await Service.ServiceSummaryAsync(Response.Body, id);
        }

        /// <summary>
        /// Signout
        /// 退出
        /// </summary>
        /// <param name="method">Login method</param>
        /// <param name="clear">Clear token</param>
        /// <returns>Action result</returns>
        [HttpPut("Signout")]
        public async Task Signout([FromQuery] LoginMethod method, bool clear)
        {
            // Result
            var result = await Service.SignoutAsync(method, clear);

            // Output
            await ResultContentAsync(result);
        }
    }
}