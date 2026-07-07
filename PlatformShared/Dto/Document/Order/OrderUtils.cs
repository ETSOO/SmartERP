using com.etsoo.CoreFramework.Models;
using System.Globalization;
using System.Text.Json;

namespace PlatformShared.Dto.Document.Order
{
    /// <summary>
    /// Order utils
    /// 订单工具类
    /// </summary>
    public static class OrderUtils
    {
        private static readonly Dictionary<string, string> symbols = [];

        /// <summary>
        /// Get currency symbol
        /// 获取币种符号
        /// </summary>
        /// <param name="currencyCode">Currency code</param>
        /// <returns>Result</returns>
        public static string? GetCurrencySymbol(string currencyCode)
        {
            if (symbols.TryGetValue(currencyCode, out var currencySymbol))
            {
                return currencySymbol;
            }
            else
            {
                foreach (var culture in CultureInfo.GetCultures(CultureTypes.SpecificCultures))
                {
                    var region = new RegionInfo(culture.Name);
                    if (region.ISOCurrencySymbol.Equals(currencyCode, StringComparison.OrdinalIgnoreCase))
                    {
                        currencySymbol = region.CurrencySymbol;
                        symbols[currencyCode] = currencySymbol;
                        return currencySymbol;
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// Parse modifiers
        /// 解析定制选项
        /// </summary>
        /// <param name="modifiers">Modifiers</param>
        /// <param name="data">Data</param>
        /// <returns>Result</returns>
        public static CustomFieldItem[] ParseModifiers(JsonDocument? modifiers, JsonDocument? data)
        {
            if (modifiers == null || data == null)
            {
                return [];
            }

            var modifierItems = modifiers.Deserialize(ModelJsonSerializerContext.Default.IEnumerableCustomFieldData);
            if (modifierItems == null)
            {
                return [];
            }

            var dataItems = data.Deserialize(PlatformSharedContext.Default.OrderLineData);
            if (dataItems == null || dataItems.Modifiers == null)
            {
                return [];
            }

            var items = new List<CustomFieldItem>();

            foreach (var m in dataItems.Modifiers)
            {
                var modifier = modifierItems.FirstOrDefault(x => x.Name == m.Key);
                if (modifier != null)
                {
                    var type = modifier.Type;
                    var value = m.Value;

                    if (modifier.Options?.Any() is true)
                    {
                        var option = modifier.Options.FirstOrDefault(x => x.Id.ToString() == m.Value.ToString());
                        if (option != null)
                        {
                            var label = option.Label;
                            if (!string.IsNullOrEmpty(label))
                            {
                                value = label;
                            }
                        }
                    }

                    items.Add(new CustomFieldItem
                    {
                        Type = type,
                        Label = modifier.Label,
                        Name = m.Key,
                        Value = value
                    });
                }
            }

            return [..items];
        }
    }
}
