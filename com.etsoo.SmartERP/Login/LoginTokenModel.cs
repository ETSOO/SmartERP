using com.etsoo.Core.Application;
using com.etsoo.Core.Services;
using System.ComponentModel.DataAnnotations;

namespace com.etsoo.SmartERP.Login
{
    /// <summary>
    /// Login token model
    /// 令牌登录模型
    /// </summary>
    public class LoginTokenModel : LoginService.IdDataModel
    {
        /// <summary>
        /// Login method
        /// 登录方式
        /// </summary>
        [Required]
        public LoginMethod Method { get; set; } = LoginMethod.Web;

        /// <summary>
        /// Saved token
        /// 保存的令牌
        /// </summary>
        [Required]
        public string Token { get; set; }

        /// <summary>
        /// Override to collect parameters
        /// 重写收集参数
        /// </summary>
        /// <param name="data">Data</param>
        /// <param name="service">Service</param>
        public override void Parameterize(OperationData data, IService<int> service)
        {
            base.Parameterize(data, service);

            // Add parameters
            var paras = data.Parameters;

            paras.Add("ip", service.User.ClientIp.ToString());
            paras.Add("method", (byte)Method);
            paras.Add("token", Token);
        }
    }
}