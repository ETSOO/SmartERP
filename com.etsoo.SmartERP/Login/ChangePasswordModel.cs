using com.etsoo.Core.Application;
using com.etsoo.Core.Services;
using System.ComponentModel.DataAnnotations;

namespace com.etsoo.SmartERP.Login
{
    /// <summary>
    /// Change password model
    /// 修改密码模型
    /// </summary>
    public class ChangePasswordModel : LoginService.DataModel
    {
        /// <summary>
        /// Current (old) password
        /// 现在的（旧的）密码
        /// </summary>
        [Required]
        [StringLength(20, MinimumLength = 4)]
        public string OldPassword { get; set; }

        /// <summary>
        /// New password
        /// 新密码
        /// </summary>
        [Required]
        [StringLength(20, MinimumLength = 4)]
        public string NewPassword { get; set; }

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

            // Hash password
            paras.Add("old_password", service.Application.HashPassword(OldPassword));
            paras.Add("new_password", service.Application.HashPassword(NewPassword));
        }
    }
}
