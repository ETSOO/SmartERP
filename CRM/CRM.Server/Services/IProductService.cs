using com.etsoo.CoreFramework.Models;
using com.etsoo.Utils.Actions;
using CRM.Server.Dto.Product;
using CRM.Server.RQ.Product;
using PlatformShared.Dto;
using System.Buffers;

namespace CRM.Server.Services
{
    public interface IProductService
    {
        IEnumerable<PromotionSaleItem> CalculatePromotions(IEnumerable<PromotionItem> promotions, decimal amount, IPromotionCodeLine? sale = null);
        Task<IActionResult> CreateAsync(ProductCreateRQ rq, CancellationToken cancellationToken = default);
        ValueTask<ProductDuplicateTestData[]?> DuplicateTestAsync(ProductDuplicateTestRQ rq, CancellationToken cancellationToken = default);
        decimal GetSalePrice(QueryForSaleData product);
        Task ListAsync(ProductListRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
        Task<ProductQueryData[]> QueryAsync(ProductQueryRQ rq, CancellationToken cancellationToken = default);
        Task<QueryForSaleData[]> QueryForSaleAsync(QueryForSaleRQ rq, CancellationToken cancellationToken = default);
        Task<ProductUnitItem[]> QueryUnitAsync(CancellationToken cancellationToken = default);
        Task<ProductViewData?> ReadAsync(int id, CancellationToken cancellationToken = default);
        Task<ProductReadCustomData?> ReadCustomAsync(int id, CancellationToken cancellationToken = default);
        Task<ProductPriceItem?> ReadPriceAsync(int id, string currency, CancellationToken cancellationToken = default);
        Task<IActionResult> UpdateAsync(ProductUpdateRQ rq, CancellationToken cancellationToken = default);
        Task<IActionResult> UpdateLogoAsync(ProductUpdateLogoRQ rq, CancellationToken cancellationToken = default);
        Task<AppActionData?> UploadLogoActionAsync(int id, CancellationToken cancellationToken = default);
        Task<ProductUpdateReadData?> UpdateReadAsync(int id, CancellationToken cancellationToken = default);
        Task<int> UpdateUnitAsync(ProductUnitUpdateRQ rq, CancellationToken cancellationToken = default);
        Task<IActionResult> UpdatePriceAsync(int id, ProductPriceItem item, CancellationToken cancellationToken = default);
        (IEnumerable<PromotionSaleItem>? saleItems, IActionResult result) ValidatePromotions(IEnumerable<PromotionSaleItemBase>? items, IEnumerable<PromotionItem> promotions, decimal amount, IPromotionCodeLine? sale = null);
        IActionResult? ValidateQty(QueryForSaleData product, decimal qty);
    }
}