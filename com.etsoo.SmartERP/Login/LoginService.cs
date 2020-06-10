using com.etsoo.Core.Services;
using com.etsoo.SmartERP.Applications;
using System;
using System.Threading.Tasks;

namespace com.etsoo.SmartERP.Login
{
    /// <summary>
    /// Login service for user, customer and supplier
    /// 登录服务，适用用户，客户和供应商
    /// </summary>
    public abstract class LoginService : MainService<int>
    {
        private OperationData GetChangePasswordData(ChangePasswordModel model)
        {
            // Create operation data
            var data = CreateOperationData("change_password");

            // Parameterize modeal
            model.Parameterize(data, this);

            // Return
            return data;
        }

        /// <summary>
        /// Change password
        /// 修改密码
        /// </summary>
        /// <param name="model">Data model</param>
        /// <returns>Action result</returns>
        public async Task<OperationResult> ChangePasswordAsync(ChangePasswordModel model)
        {
            // Validate model
            var result = ValidateModel(model);

            // Invalid result return anyway
            if (!result.OK)
                return result;

            // Action
            return await ExecuteAsync(GetChangePasswordData(model));
        }

        /// <summary>
        /// Login process
        /// 登录处理
        /// </summary>
        /// <param name="result">Operation result</param>
        /// <returns>Operation result</returns>
        private OperationResult LoginAction(OperationResult result)
        {
            if (result.OK)
            {
                // Make sure the module is equal
                if (ModuleId == User.ModuleId)
                {
                    // Logined user id
                    var userId = result.Data.Get("token_user_id", 0);

                    // User role
                    var role = result.Data.Get("role", UserRole.User);

                    // User organization
                    var organizationId = result.Data.Get("organization_id", 0);

                    // Language
                    var languageCid = result.Data.Get("language_cid");

                    // Service login
                    User.Login(userId, organizationId, languageCid, new string[] { role.ToString() });
                }
                else
                {
                    throw new InvalidOperationException($"Current service module {ModuleId} is not equal the user modeul {User.ModuleId} ");
                }
            }

            // Return
            return result;
        }

        private OperationData GetLoginData(LoginModel model)
        {
            // Create operation data
            var data = CreateOperationData("login");

            // Parameterize modeal
            model.Parameterize(data, this);

            // Return
            return data;
        }

        /// <summary>
        /// Async user login
        /// 异步用户登录
        /// </summary>
        /// <param name="model">Data model</param>
        /// <returns>Action result</returns>
        public async Task<OperationResult> LoginAsync(LoginModel model)
        {
            // Validate model
            var result = ValidateModel(model);

            // Invalid result return anyway
            if (!result.OK)
                return result;

            // Access database to valid
            return LoginAction(await ExecuteAsync(GetLoginData(model), false));
        }

        private OperationData GetLoginTokenData(LoginTokenModel model)
        {
            // Create operation data
            var data = CreateOperationData("login_with_token");

            // Parameterize modeal
            model.Parameterize(data, this);

            // Return
            return data;
        }

        /// <summary>
        /// Async user login with token
        /// 异步令牌登录
        /// </summary>
        /// <param name="model">Data model</param>
        /// <returns>Action result</returns>
        public async Task<OperationResult> LoginTokenAsync(LoginTokenModel model)
        {
            // Validate model
            var result = ValidateModel(model);

            // Invalid result return anyway
            if (!result.OK)
                return result;

            // Access database to valid
            return LoginAction(await ExecuteAsync(GetLoginTokenData(model), false));
        }

        private OperationData GetSignoutData(LoginMethod method, bool clearToken)
        {
            // Create operation data
            var data = CreateOperationData("signout");

            // Add parameters
            data.Parameters.Add("method", (byte)method);
            data.Parameters.Add("clear_token", clearToken);

            // Return
            return data;
        }

        /// <summary>
        /// Async signout
        /// 异步退出
        /// </summary>
        /// <param name="method">Login method</param>
        /// <param name="clearToken">Clear token</param>
        /// <returns>Action result</returns>
        public async Task<OperationResult> SignoutAsync(LoginMethod method, bool clearToken)
        {
            // Access database to valid
            var result = await ExecuteAsync(GetSignoutData(method, clearToken));

            if (result.OK)
            {
                User.SignOut();
            }

            // Return
            return result;
        }
    }
}