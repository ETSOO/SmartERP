using com.etsoo.CoreFramework.Models;
using com.etsoo.Utils.String;

namespace Platform.Server.Endpoints.Document.RQ
{
    /// <summary>
    /// Document generate result request data
    /// 文档生成结果请求数据
    /// </summary>
    public record DocumentGenerateRQ
    {
        /// <summary>
        /// Document id
        /// 文档编号
        /// </summary>
        public int Id { get; init; }

        /// <summary>
        /// Related target id
        /// 关联对象编号
        /// </summary>
        public long TargetId { get; init; }

        /// <summary>
        /// No cache
        /// 禁用缓存
        /// </summary>
        public bool? NoCache { get; init; }

        /// <summary>
        /// Culture
        /// 文化
        /// </summary>
        public string? Culture { get; init; }

        /// <summary>
        /// Action signed data
        /// 操作签名数据
        /// </summary>
        public required AppActionData Action { get; init; }

        /// <summary>
        /// Additional parameters
        /// 更多参数
        /// </summary>
        public StringKeyDictionaryObject Data { get; init; } = [];
    }
}
