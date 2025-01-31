using com.etsoo.Web;

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
        /// <returns>Content</returns>
        public static Task<string> BuildTemplateAsync<M>(string file, M model) where M : class
        {
            // File should not starts with '/'
            var filePath = Path.Combine(AppContext.BaseDirectory, file);
            return RazorUtils.RenderAsync(filePath, model);
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
            return $"{localTime.ToString()} ({data.TZ.StandardName})";
        }
    }
}
