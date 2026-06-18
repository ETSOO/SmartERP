using com.etsoo.CoreFramework.User;
using com.etsoo.Utils.String;
using Microsoft.EntityFrameworkCore;
using PlatformShared.Database;
using System.Text.Json;

namespace PlatformShared.Dto.Document
{
    /// <summary>
    /// System template data delegate
    /// 系统模板数据委托
    /// </summary>
    /// <param name="dbContextFactory">Database context factory</param>
    /// <param name="id">Target id</param>
    /// <param name="parameters">Additional parameters</param>
    /// <param name="currentUser">Current user</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result</returns>
    public delegate Task<object?> SystemTemplateDataDelegate(
        IDbContextFactory<MyDbContext> dbContextFactory,
        long id,
        StringKeyDictionaryObject parameters,
        CurrentUser currentUser,
        CancellationToken cancellationToken
    );

    /// <summary>
    /// System template item
    /// 系统模板项目
    /// </summary>
    public record SystemTemplateItem
    {
        /// <summary>
        /// Kind
        /// 类型
        /// </summary>
        public required string Kind { get; init; }

        /// <summary>
        /// Subject
        /// 主题
        /// </summary>
        public required string Subject { get; init; }

        /// <summary>
        /// Template path
        /// 模板路径
        /// </summary>
        public required string Template { get; init; }

        /// <summary>
        /// Parameters
        /// 参数
        /// </summary>
        public JsonDocument? Parameters { get; init; }

        /// <summary>
        /// Data function
        /// 数据函数
        /// </summary>
        public required SystemTemplateDataDelegate Data { get; init; }
    }
}
