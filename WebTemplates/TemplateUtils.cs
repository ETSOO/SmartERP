using PlatformShared.Dto;
using RazorEngineCore;
using System.Collections.Concurrent;
using System.Reflection;

namespace WebTemplates
{
    /// <summary>
    /// Template utilities
    /// 模板工具类
    /// </summary>
    public static class TemplateUtils
    {
        private static readonly ConcurrentDictionary<string, RazorEngineCompiledTemplate<object>?> _cache = new();

        private static readonly Dictionary<string, string[]> SupportedCultures = new()
        {
            ["zh-Hans"] = ["zh-CN"],
            ["zh-Hant"] = ["zh-TW", "zh-HK"]
        };

        /// <summary>
        /// Action notice template
        /// 操作通知模板
        /// </summary>
        private const string ActionNoticeTemplate = "Action/EmailActionNotice_{culture}.cshtml";

        /// <summary>
        /// Build template
        /// 编译模板
        /// </summary>
        /// <param name="template">Template</param>
        /// <param name="model">Model data</param>
        /// <returns>Result</returns>
        public static Task<string> BuildAsync(string template, object model)
        {
            var obj = Get(template) ?? throw new Exception($"Template not found: {template}");
            return obj.RunAsync(model);
        }

        /// <summary>
        /// Build action notice template
        /// 编译操作通知模板
        /// </summary>
        /// <param name="culture">Culture</param>
        /// <param name="data">Model data</param>
        /// <returns>Result</returns>
        public static Task<string> BuildActionNoticeAsync(string culture, ActionNoticeData data)
        {
            var template = FormatCulture(ActionNoticeTemplate, culture);
            return BuildAsync(template, data);
        }

        /// <summary>
        /// Format culture
        /// 格式化文化
        /// </summary>
        /// <param name="template">Template</param>
        /// <param name="culture">Culture</param>
        /// <returns>Result</returns>
        public static string FormatCulture(string template, string culture)
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
        /// Get template
        /// 获取模板
        /// </summary>
        /// <param name="template">Template</param>
        /// <returns>Result</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static RazorEngineCompiledTemplate<object>? Get(string template)
        {
            return _cache.GetOrAdd(template, k =>
            {
                var assembly = Assembly.GetExecutingAssembly();

                var names = assembly.GetManifestResourceNames();
                var resourceName = $"WebTemplates.Templates.{template.Replace('/', '.')}.bin";
                if (!names.Contains(resourceName))
                {
                    return null;
                }

                using var stream = assembly.GetManifestResourceStream(resourceName);
                if (stream == null)
                {
                    return null;
                }

                return RazorEngineCompiledTemplate<object>.LoadFromStream(stream);
            });
        }
    }
}
