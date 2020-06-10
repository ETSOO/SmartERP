using com.etsoo.Core.Application;
using com.etsoo.Core.Services;
using com.etsoo.SmartERP.Utils;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace com.etsoo.SmartERP.Login
{
    /// <summary>
    /// Login model
    /// https://docs.microsoft.com/en-us/dotnet/api/system.componentmodel.dataannotations?view=netframework-4.8
    /// 登录模型
    /// </summary>
    public class LoginModel : LoginService.DataModel
    {
        [StringLength(8, MinimumLength = 4)]
        /// <summary>
        /// Veryfication code
        /// 验证码
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Id, like number id, mobile, or email
        /// 编号，像数字编号，手机号码或邮箱
        /// </summary>
        [Required]
        [StringLength(256, MinimumLength = 4)]
        public string Id { get; set; }

        /// <summary>
        /// Id type
        /// 编号类型
        /// </summary>
        public LoginIdType? IdType { get; set; }

        /// <summary>
        /// Language cid, like Simplified Chinese zh-CN
        /// 语言编号，如简体中文 zh-CN
        /// </summary>
        [RegularExpression(@"^[a-z]{2}(-[A-Z]{2})?$")]
        public string LanguageCid { get; set; }

        /// <summary>
        /// Login method
        /// 登录方式
        /// </summary>
        [Required]
        public LoginMethod Method { get; set; } = LoginMethod.Web;

        /// <summary>
        /// Current organization id
        /// 当前限定的登录机构
        /// </summary>
        public int? Org { get; set; }

        /// <summary>
        /// Raw password
        /// 原始密码
        /// </summary>
        [Required]
        [StringLength(30, MinimumLength = 4)]
        public string Password { get; set; }

        /// <summary>
        /// Is save login
        /// 是否保存登录
        /// </summary>
        public bool? Save { get; set; }

        private void CalculateIdType()
        {
            if (IdType == null || IdType == LoginIdType.Unknown)
            {
                if (Id.Contains("@"))
                {
                    IdType = LoginIdType.Email;
                }
                else
                {
                    if (Regex.IsMatch(Id, @"^\d+$"))
                    {
                        // Starting with zero or its length more than 10 presents mobile phone number
                        // 以0开头，或者长度大于10位，已经不方便通过数字编号记忆，识别为移动手机号码
                        if (Id.StartsWith("0") || Id.Length >= 10)
                        {
                            IdType = LoginIdType.Mobile;
                        }
                        else
                        {
                            IdType = LoginIdType.Id;
                        }
                    }
                    else
                    {
                        IdType = LoginIdType.Cid;
                    }
                }
            }
        }

        /// <summary>
        /// Override to collect parameters
        /// 重写收集参数
        /// </summary>
        /// <param name="data">Data</param>
        /// <param name="service">Service</param>
        public override void Parameterize(OperationData data, IService<int> service)
        {
            base.Parameterize(data, service);

            // Calculate id type
            CalculateIdType();

            // Add parameters
            var paras = data.Parameters;

            // Verification code
            paras.Add("has_code", true);

            paras.Add("id", Id);
            paras.Add("id_type", (byte)IdType);
            paras.Add("ip", service.User.ClientIp.ToString());
            paras.Add("language_cid", LanguageCid ?? service.User.LanguageCid);

            // Login method
            paras.Add("method", (byte)Method);

            paras.Add("org", Org);

            // Hash password
            paras.Add("password", service.Application.HashPassword(Password));

            paras.Add("save_login", Save.GetValueOrDefault());
        }
    }
}