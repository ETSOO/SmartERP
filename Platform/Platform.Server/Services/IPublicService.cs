using com.etsoo.ImageUtils.Barcode;
using Platform.Server.Endpoints.Public.RQ;

namespace Platform.Server.Services
{
    public interface IPublicService
    {
        ValueTask<string> MobileQRCodeAsync(MobileQRCodeRQ rq, CancellationToken cancellationToken = default);
        Task<string> QRCodeAsync(BarcodeOptions rq, CancellationToken cancellationToken = default);
    }
}