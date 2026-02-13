using System.Text.Json.Serialization;

namespace PlatformShared.Dto
{
    /// <summary>
    /// Category item with parents
    /// 类目项及其父项
    /// </summary>
    public record CategoryItemWithParents : CategoryItem
    {
        /// <summary>
        /// Parent Ids
        /// 所有父类编号
        /// </summary>
        [JsonIgnore]
        public IEnumerable<int>? ParentIds { get; set; }
    }
}