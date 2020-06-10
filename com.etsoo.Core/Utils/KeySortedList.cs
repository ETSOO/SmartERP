using System.Collections.Generic;

namespace com.etsoo.Core.Utils
{
    /// <summary>
    /// Key sorted list, like { key1: [], key2: []}
    /// 键排序列表
    /// </summary>
    public class KeySortedList : SortedList<string, List<string>>
    {
        /// <summary>
        /// Add new item
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="item">Item</param>
        public void Add(string key, string item)
        {
            List<string> items;
            if (this.TryGetValue(key, out items))
            {
                items.Add(item);
            }
            else
            {
                // Call base class method otherwise will cause infinite loop
                base.Add(key, new List<string> { item });
            }
        }

        /// <summary>
        /// Add items
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="items">Items</param>
        public new void Add(string key, List<string> items)
        {
            List<string> current;
            if (this.TryGetValue(key, out current))
            {
                current.AddRange(items);
            }
            else
            {
                // Call base class method otherwise will cause infinite loop
                base.Add(key, items);
            }
        }
    }
}