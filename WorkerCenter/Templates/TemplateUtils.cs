using Microsoft.CodeAnalysis.CSharp.Syntax;
using RazorEngineCore;

namespace WorkerCenter.Templates
{
    /// <summary>
    /// Template utilities
    /// 模板工具
    /// </summary>
    public static class TemplateUtils
    {
        private static readonly Dictionary<string, string[]> SupportedCultures = new()
        {
            ["zh-Hans"] = ["zh-CN"],
            ["zh-Hant"] = ["zh-TW", "zh-HK"]
        };

        /// <summary>
        /// Action notice template
        /// 操作通知模板
        /// </summary>
        public const string ActionNoticeTemplate = "Templates/EmailActionNotice_{culture}.cshtml";

        /// <summary>
        /// Build notice template
        /// 创建通知模板
        /// </summary>
        /// <param name="culture">Culture</param>
        /// <param name="data">Model</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Result</returns>
        public static async Task<string> BuildNoticeTemplateAsync(string culture, ActionNoticeData data, CancellationToken cancellationToken = default)
        {
            return await BuildTemplateAsync(FormatCultureTemplate(ActionNoticeTemplate, culture), data, cancellationToken);
        }

        /// <summary>
        /// Build template
        /// 创建模板
        /// </summary>
        /// <typeparam name="M">Generic template model</typeparam>
        /// <param name="file">File</param>
        /// <param name="model">Data model</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Content</returns>
        public static Task<string> BuildTemplateAsync<M>(string file, M model, CancellationToken cancellationToken = default) where M : class
        {
            return BuildTemplateAsync<M>(file, model, [], cancellationToken);
        }

        /// <summary>
        /// Build template
        /// 创建模板
        /// </summary>
        /// <typeparam name="M">Generic template model</typeparam>
        /// <param name="file">File</param>
        /// <param name="model">Data model</param>
        /// <param name="references">Additional references</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Content</returns>
        public static async Task<string> BuildTemplateAsync<M>(string file, M model, Type[] references, CancellationToken cancellationToken = default) where M : class
        {
            return await BuildTemplateAsync<RazorEngineTemplateBase<M>, M>(file, (t) => t.Model = model, references, cancellationToken);
        }

        /// <summary>
        /// Build template
        /// 创建模板
        /// </summary>
        /// <typeparam name="T">Generic template type</typeparam>
        /// <typeparam name="M">Generic template model type</typeparam>
        /// <param name="file">File</param>
        /// <param name="action">Action</param>
        /// <param name="references">Additional references</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Content</returns>
        public static async Task<string> BuildTemplateAsync<T, M>(string file, Action<T> action, Type[] references, CancellationToken cancellationToken = default) where M : class where T : RazorEngineTemplateBase<M>
        {
            // File should not starts with '/'
            var templateFile = Path.Combine(AppContext.BaseDirectory, file.TrimStart('/'));

            var template = await File.ReadAllTextAsync(templateFile, cancellationToken);

            // Engine
            var razorEngine = new RazorEngine();

            // Compile
            var compiledTemplate = await razorEngine.CompileAsync<T, M>(template, builder =>
            {
                foreach (var reference in references)
                {
                    builder.AddAssemblyReference(reference);
                }
            }, cancellationToken);

            // Execute
            return await compiledTemplate.ExecuteAsync(action);
        }

        /// <summary>
        /// Format culture template
        /// 格式化文化模板
        /// </summary>
        /// <param name="template">Template</param>
        /// <param name="culture">Culture</param>
        /// <returns>Result</returns>
        public static string FormatCultureTemplate(string template, string culture)
        {
            var c = "en";
            foreach (var (key, values) in SupportedCultures)
            {
                if (culture.Equals(key, StringComparison.OrdinalIgnoreCase) || values.Contains(culture, StringComparer.OrdinalIgnoreCase))
                {
                    c = key;
                    break;
                }
            }

            return template.Replace("{culture}", c);
        }

        /// <summary>
        /// Format notice date time
        /// 格式化通知日期时间
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string FormatDateTime(this ActionNoticeData data)
        {
            var localTime = TimeZoneInfo.ConvertTime(data.TimeStamp, data.TZ);
            return $"{localTime} ({data.TZ.StandardName})";
        }
    }
}
