using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Business;
using com.etsoo.Utils.Actions;
using CRM.Server.Dto.Product;
using CRM.Server.RQ.Product;
using CRM.Server.Services;
using PlatformShared.Database.Models;

namespace CRM.Server.Utils
{
    /// <summary>
    /// Order BOMs
    /// 订单物料清单
    /// </summary>
    public class OrderBoms
    {
        /// <summary>
        /// Is product a bundle
        /// 产品是否为组合产品
        /// </summary>
        /// <param name="product">Sale product</param>
        /// <returns>Result</returns>
        public static bool IsBundle(QueryForSaleData product)
        {
            return product.Scope.HasFlag(ProductScope.Bundle) && product.Boms.Length > 0;
        }

        private readonly List<(QueryForSaleData Product, OrderLine Line)> _items = [];

        /// <summary>
        /// Add item
        /// 添加项目
        /// </summary>
        /// <param name="product">Sale product</param>
        /// <param name="line">Order line</param>
        /// <returns>Result</returns>
        public bool Add(QueryForSaleData product, OrderLine line)
        {
            var isBundle = IsBundle(product);

            if (isBundle)
            {
                _items.Add((product, line));
            }

            return isBundle;
        }

        /// <summary>
        /// Calculate BOMs
        /// 计算所有层级物料清单
        /// </summary>
        /// <param name="service">Order service</param>
        /// <param name="rq">Query request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async ValueTask<ActionResult> CalculateAsync(IProductService service, QueryForSaleRQ rq, CancellationToken cancellationToken = default)
        {
            if (_items.Count == 0)
            {
                return ActionResult.Success;
            }

            // Query all products
            var productIds = _items.SelectMany(i => i.Product.Boms.Select(b => b.ProductId)).Distinct().ToArray();
            var newRQ = rq with { Ids = productIds };
            var products = await service.QueryForSaleAsync(newRQ, false, cancellationToken);

            if (products.Length != productIds.Length)
            {
                return ApplicationErrors.DataOutdated.AsResult();
            }

            foreach (var (product, line) in _items)
            {
                var bomLines = new List<OrderLine>();

                foreach (var bom in product.Boms)
                {
                    var p = products.First(p => p.Id == bom.ProductId);

                    var qty = line.Qty * bom.Qty;

                    if (IsBundle(p))
                    {
                        var lineIds = p.Boms.Select(b => b.ProductId).Distinct().ToArray();
                        var lineRQ = rq with { Ids = lineIds };
                        var lineProducts = await service.QueryForSaleAsync(lineRQ, false, cancellationToken);

                        if (lineProducts.Length != lineIds.Length)
                        {
                            return ApplicationErrors.DataOutdated.AsResult(product.Name);
                        }

                        // Limit to two levels of BOMs
                        if (lineProducts.Any(IsBundle))
                        {
                            return ApplicationErrors.InvalidAction.AsResult(product.Name);
                        }

                        foreach (var lineBom in p.Boms)
                        {
                            var lineP = lineProducts.First(p => p.Id == lineBom.ProductId);
                            var lineQty = qty * lineBom.Qty;
                            bomLines.Add(CreateLine(lineP, lineQty));
                        }
                    }
                    else
                    {
                        bomLines.Add(CreateLine(p, qty));
                    }
                }

                line.BomLines = bomLines;
            }

            return ActionResult.Success;
        }

        private OrderLine CreateLine(QueryForSaleData p, decimal qty)
        {
            return new OrderLine
            {
                ProductId = p.Id,
                Title = p.Name,
                OriginalPrice = p.RetailPrice,
                CostPrice = 0,
                Price = 0,
                Qty = qty,
                AssetQty = p.AssetQty.GetValueOrDefault(),
                Amount = 0,
                Discount = 0,
                Status = EntityStatus.Normal
            };
        }
    }
}
