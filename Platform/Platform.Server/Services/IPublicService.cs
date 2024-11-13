using com.etsoo.ApiModel.Dto.Maps;
using com.etsoo.ApiModel.RQ.Maps;
using com.etsoo.CoreFramework.Models;
using com.etsoo.ImageUtils.Barcode;
using com.etsoo.Localization.Country;
using Platform.Server.Dto.Public;
using Platform.Server.Endpoints.Public.RQ;

namespace Platform.Server.Services
{
    public interface IPublicService : ICommonService
    {
        Task<string> CreateBarcodeAsync(BarcodeOptions rq, CancellationToken cancellationToken = default);
        Task<IEnumerable<CurrencyItem>> GetCurrenciesAsync(IEnumerable<string>? ids = null, CancellationToken cancellationToken = default);
        Task<IEnumerable<RegionData>> GetRegionsAsync(IEnumerable<string>? ids = null, CancellationToken cancellationToken = default);
        string GetPinyin(PinyinRQ rq);
        ValueTask<string> MobileQRCodeAsync(MobileQRCodeRQ rq, CancellationToken cancellationToken = default);
        Task<OrgPublicInfo> OrgInfoAsync(OrgInfoRQ rq, CancellationToken cancellationToken = default);
        Task<IEnumerable<PlaceCommon>?> QueryPlaceAsync(PlaceQueryRQ rq, CancellationToken cancellationToken = default);
    }
}