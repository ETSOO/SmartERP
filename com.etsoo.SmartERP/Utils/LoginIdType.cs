namespace com.etsoo.SmartERP.Utils
{
    /// <summary>
    /// Login id type
    /// 登录编号类型
    /// </summary>
    public enum LoginIdType
    {
        /// <summary>
        /// Unknown, determined by logic
        /// 未知，根据逻辑自行判断
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Number id
        /// 数字编号
        /// </summary>
        Id = 1,

        /// <summary>
        /// Mobile phone number
        /// 手机号码
        /// </summary>
        Mobile = 2,

        /// <summary>
        /// Email address
        /// Email地址
        /// </summary>
        Email = 4,

        /// <summary>
        /// Cid
        /// 自定义编号
        /// </summary>
        Cid = 8
    }
}