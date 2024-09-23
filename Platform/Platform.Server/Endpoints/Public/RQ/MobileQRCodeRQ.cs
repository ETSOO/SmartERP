namespace Platform.Server.Endpoints.Public.RQ
{
    /// <summary>
    /// Get mobile QRCode image Base64 string request data
    /// 获取移动端QRCode图片的Base64字符串请求数据
    /// </summary>
    public record MobileQRCodeRQ
    {
        /// <summary>
        /// User id
        /// 用户编号
        /// </summary>
        public string? Id { get; init; }

        /// <summary>
        /// Host address
        /// 主机地址
        /// </summary>
        public string? Host { get; set; }
    }
}
