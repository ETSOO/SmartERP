namespace Platform.Server.Dto.Auth
{
    /// <summary>
    /// Complete register data
    /// 完成注册数据
    /// </summary>
    public record CompleteRegisterData
    {
        /// <summary>
        /// Password
        /// 密码
        /// </summary>
        public required string Password { get; init; }

        /// <summary>
        /// Full name
        /// 姓名
        /// </summary>
        public required string Name { get; init; }

        /// <summary>
        /// Country or region code, like CN = China
        /// 国家或地区编号，如 CN = 中国
        /// </summary>
        public required string Region { get; init; }

        /// <summary>
        /// Device name
        /// 设备名称
        /// </summary>
        public required string DeviceName { get; init; }
    }
}
