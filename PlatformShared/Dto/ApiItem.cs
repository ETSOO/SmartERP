using System.Diagnostics.CodeAnalysis;

namespace PlatformShared.Dto
{
    /// <summary>
    /// Core API item
    /// 核心接口项目
    /// </summary>
    public record ApiItem
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public int Id { get; init; }

        /// <summary>
        /// Organization Id
        /// 机构编号
        /// </summary>
        public int OrgId { get; init; }

        /// <summary>
        /// Title
        /// 标题
        /// </summary>
        public required string Title { get; init; }

        /// <summary>
        /// Endpoint
        /// 端点
        /// </summary>
        public Uri? Endpoint { get; init; }

        /// <summary>
        /// App or user ID
        /// 程序或用户编号
        /// </summary>
        public required string AppId { get; init; }

        /// <summary>
        /// App or user secret
        /// 程序或用户密钥
        /// </summary>
        public required string AppSecret { get; init; }

        /// <summary>
        /// JSON options
        /// JSON 选项
        /// </summary>
        public string? JsonOptions { get; init; }

        /// <summary>
        /// Rate policy
        /// 频次政策
        /// </summary>
        public short? RatePolicy { get; init; }

        /// <summary>
        /// Access token
        /// 访问令牌
        /// </summary>
        public string? AccessToken { get; init; }

        /// <summary>
        /// Refresh time
        /// 刷新时间
        /// </summary>
        public DateTimeOffset? RefreshTime { get; init; }
    }

    /// <summary>
    /// API item with options
    /// 接口项目与选项
    /// </summary>
    /// <typeparam name="T">Generic options type</typeparam>
    public record ApiItem<T> : ApiItem where T : class
    {
        /// <summary>
        /// Options
        /// 选项
        /// </summary>
        public T Options { get; }

        [SetsRequiredMembers]
        public ApiItem(ApiItem item, T options) : base(item)
        {
            Options = options;
        }
    }
}
