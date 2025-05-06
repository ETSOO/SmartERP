using com.etsoo.ApiModel.Dto.Maps;
using com.etsoo.ApiModel.Dto.SmartERP;
using com.etsoo.ApiModel.RQ.Maps;
using com.etsoo.ApiModel.RQ.SmartERP;
using com.etsoo.ImageUtils.Barcode;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Serialization.Country;
using Platform.Server.Dto.Member;
using Platform.Server.Dto.Public;
using Platform.Server.Endpoints.Public.RQ;

namespace Platform.Server.Services
{
    public interface IPublicService : ICommonService
    {
        Task<IActionResult> AcceptInvitationAsync(AcceptInvitationRQ rq, CancellationToken cancellationToken = default);
        Task<string> CreateBarcodeAsync(BarcodeOptions rq, CancellationToken cancellationToken = default);
        Task<IEnumerable<CultureItem>> GetCulturesAsync(IEnumerable<string> ids, CancellationToken cancellationToken = default);
        Task<IEnumerable<CurrencyItem>> GetCurrenciesAsync(IEnumerable<string>? ids = null, CancellationToken cancellationToken = default);
        Task<IEnumerable<CustomResourceData>> GetCustomResourcesAsync(string culture, CancellationToken cancellationToken = default);
        Task<IEnumerable<RegionItem>> GetRegionsAsync(IEnumerable<string>? ids = null, CancellationToken cancellationToken = default);
        string GetPinyin(PinyinRQ rq);
        ValueTask<string> MobileQRCodeAsync(MobileQRCodeRQ rq, CancellationToken cancellationToken = default);
        Task<OrgPublicInfo> OrgInfoAsync(OrgInfoRQ rq, CancellationToken cancellationToken = default);
        Task<IEnumerable<PlaceCommon>?> QueryPlaceAsync(PlaceQueryRQ rq, CancellationToken cancellationToken = default);
        ChinaPinData? ParseChinaPin(string pin);
        Task<MemberInvitationData?> ReadInvitationAsync(Guid id, CancellationToken cancellationToken = default);
    }
}