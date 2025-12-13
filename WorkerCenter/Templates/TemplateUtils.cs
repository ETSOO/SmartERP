using RazorEngineCore;

namespace WorkerCenter.Templates
{
    /// <summary>
    /// Template utilities
    /// 模板工具
    /// </summary>
    public static class TemplateUtils
    {
        /// <summary>
        /// Action notice template
        /// 操作通知模板
        /// </summary>
        public const string ActionNoticeTemplate = "Templates/EmailActionNotice.cshtml";

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
            // File should not starts with '/'
            var templateFile = Path.Combine(AppContext.BaseDirectory, file);

            var template = await File.ReadAllTextAsync(templateFile, cancellationToken);

            // Engine
            var razorEngine = new RazorEngine();

            // Compile
            var compiledTemplate = await razorEngine.CompileAsync<M>(template, builder =>
            {
                foreach (var reference in references)
                {
                    builder.AddAssemblyReference(reference);
                }
            }, cancellationToken);

            // Execute
            return await compiledTemplate.RunAsync(model);
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
