using com.etsoo.CoreFramework.Business;

namespace PlatformShared.Dto
{
    /// <summary>
    /// Identity type data base
    /// 识别类型数据基础
    /// </summary>
    public record IdentityTypeDataBase
    {
        /// <summary>
        /// Name
        /// 名称
        /// </summary>
        public required string Name { get; init; }

        /// <summary>
        /// Identity type
        /// 识别类型
        /// </summary>
        public IdentityTypeFlags IdentityType { get; init; }
    }

    /// <summary>
    /// Identity type data
    /// 识别类型数据
    /// </summary>
    public record IdentityTypeData : IdentityTypeDataBase
    {
        /// <summary>
        /// Owner
        /// 所有者
        /// </summary>
        public IdentityTypeDataBase? Owner { get; init; }
    }
}
