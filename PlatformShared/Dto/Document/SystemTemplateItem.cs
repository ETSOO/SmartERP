using com.etsoo.CoreFramework.User;
using com.etsoo.Utils.String;
using Microsoft.EntityFrameworkCore;
using PlatformShared.Database;
using System.Text.Json;

namespace PlatformShared.Dto.Document
{
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
        public required Func<IDbContextFactory<MyDbContext>, long, StringKeyDictionaryObject, CurrentUser, CancellationToken, Task<object?>> Data { get; init; }
    }
}
