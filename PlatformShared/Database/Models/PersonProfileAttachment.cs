namespace PlatformShared.Database.Models
{
    /// <summary>
    /// Person profile attachment
    /// 个人资料附件
    /// </summary>
    public class PersonProfileAttachment
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Profile id
        /// 资料编号
        /// </summary>
        public long ProfileId { get; set; }

        /// <summary>
        /// File name
        /// 文件名
        /// </summary>
        public string FileName { get; set; } = default!;

        /// <summary>
        /// File size
        /// 文件大小
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// Content type
        /// 文件类型
        /// </summary>
        public string ContentType { get; set; } = default!;

        /// <summary>
        /// Core user id
        /// 核心用户编号
        /// </summary>
        public int CoreUserId { get; set; }

        /// <summary>
        /// Creation
        /// 登记时间
        /// </summary>
        public DateTimeOffset Creation { get; set; }
    }
}
