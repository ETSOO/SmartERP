using System.Text.Json.Serialization;

namespace CRM.Server.RQ.Asset
{
    /// <summary>
    /// Asset list request data
    /// 资产列表请求数据
    /// </summary>
    [JsonDerivedType(typeof(AssetQueryRQ))]
    public record AssetListRQ : QueryIntRQ
    {

    }
}
