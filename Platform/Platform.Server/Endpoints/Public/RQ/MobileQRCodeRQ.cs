using com.etsoo.CoreFramework.Application;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Models;

namespace Platform.Server.Endpoints.Public.RQ
{
    /// <summary>
    /// Get mobile QRCode image Base64 string request data
    /// 获取移动端QRCode图片的Base64字符串请求数据
    /// </summary>
    public record MobileQRCodeRQ : IModelValidator
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public string? Id { get; init; }

        /// <summary>
        /// Host address
        /// 主机地址
        /// </summary>
        public string? Host { get; set; }

        /// <summary>
        /// Validate the model
        /// 验证模块
        /// </summary>
        /// <returns>Result</returns>
        public IActionResult? Validate()
        {
            if (Id?.Length > 512)
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Id));
            }

            if (Host?.Length > 512)
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Host));
            }

            return null;
        }
    }
}
