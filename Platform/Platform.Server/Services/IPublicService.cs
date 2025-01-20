using com.etsoo.ApiModel.Dto.Maps;
using com.etsoo.ApiModel.RQ.Maps;
using com.etsoo.CoreFramework.Models;
using com.etsoo.ImageUtils.Barcode;
using com.etsoo.Localization.Country;
using com.etsoo.Utils.Actions;
using Platform.Server.Dto.Member;
using Platform.Server.Dto.Public;
using Platform.Server.Endpoints.Public.RQ;

namespace Platform.Server.Services
{
    public interface IPublicService : ICommonService
    {
        Task<IActionResult> AcceptInvitationAsync(AcceptInvitationRQ rq, CancellationToken cancellationToken = default);
        Task<string> CreateBarcodeAsync(BarcodeOptions rq, CancellationToken cancellationToken = default);
        Task<IEnumerable<CurrencyItem>> GetCurrenciesAsync(IEnumerable<string>? ids = null, CancellationToken cancellationToken = default);
        Task<IEnumerable<RegionData>> GetRegionsAsync(IEnumerable<string>? ids = null, CancellationToken cancellationToken = default);
        string GetPinyin(PinyinRQ rq);
        ValueTask<string> MobileQRCodeAsync(MobileQRCodeRQ rq, CancellationToken cancellationToken = default);
        Task<OrgPublicInfo> OrgInfoAsync(OrgInfoRQ rq, CancellationToken cancellationToken = default);
        Task<IEnumerable<PlaceCommon>?> QueryPlaceAsync(PlaceQueryRQ rq, CancellationToken cancellationToken = default);
        Task<MemberInvitationData?> ReadInvitationAsync(Guid id, CancellationToken cancellationToken = default);
    }
}