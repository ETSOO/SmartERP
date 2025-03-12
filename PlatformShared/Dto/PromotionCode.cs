using com.etsoo.Utils;
using System.Text.Json;

namespace PlatformShared.Dto
{
    /// <summary>
    /// Promotion code converter
    /// 促销码转化器
    /// </summary>
    public class PromotionCodeConverter : EnumerationConverter<PromotionCode, short>
    {
        public PromotionCodeConverter() : base()
        {
            // Only for initialization, otherwise PromotionCode not referenced
            if (PromotionCode.PMS.Value > 0) return;
        }
    }

    /// <summary>
    /// Promotion sale item
    /// 销售促销项目
    /// </summary>
    public record PromotionSaleItem
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Title
        /// 标题
        /// </summary>
        public string Title { get; set; } = default!;

        /// <summary>
        /// Amount
        /// 金额
        /// </summary>
        public decimal Amount { get; set; }
    }

    /// <summary>
    /// Data format
    /// 数据格式
    /// </summary>
    public abstract class PromotionCode : Enumeration<short>
    {
        /// <summary>
        /// 订单满{m}减{n}
        /// </summary>
        public static readonly PromotionCodeOMJ OMJ = new();

        /// <summary>
        /// 产品满{m}减{n}
        /// </summary>
        public static readonly PromotionCodePMJ PMJ = new();

        /// <summary>
        /// 产品买{n}送一
        /// </summary>
        public static readonly PromotionCodePMS PMS = new();

        /// <summary>
        /// 产品第二件{n}折
        /// </summary>
        public static readonly PromotionCodePEZ PEZ = new();

        /// <summary>
        /// 产品{n}折
        /// </summary>
        public static readonly PromotionCodePKZ PKZ = new();

        /// <summary>
        /// 产品{n}件以上{m}优惠价
        /// </summary>
        public static readonly PromotionCodePJH PJH = new();

        /// <summary>
        /// {t}会员{n}折
        /// </summary>
        public static readonly PromotionCodeCDZ CDZ = new();

        /// <summary>
        /// 客户{n}折
        /// </summary>
        public static readonly PromotionCodeCKZ CKZ = new();

        protected PromotionCode(short value, string name) : base(value, name)
        {
        }

        public abstract PromotionSaleItem? Calculate(PromotionItem p, IPromotionCodeLine? sale, decimal amount);

        public abstract string? Check(PromotionCodeData data);

        public override void WriteJson(Utf8JsonWriter writer)
        {
            writer.WriteNumberValue(Value);
        }

        /// <summary>
        /// 订单满{m}减{n}
        /// </summary>
        public class PromotionCodeOMJ : PromotionCode
        {
            public PromotionCodeOMJ() : base(1, "OMJ")
            {
            }

            public override PromotionSaleItem? Calculate(PromotionItem p, IPromotionCodeLine? sale, decimal amount)
            {
                var minAmount = p.MinAmount;

                if (minAmount < 1)
                {
                    return null;
                }

                var times = (int)Math.Floor(amount / minAmount);
                if (times > 0)
                {
                    return new PromotionSaleItem
                    {
                        Id = p.Id,
                        Title = p.Title,
                        Amount = times * p.Discount
                    };
                }

                return null;
            }

            public override string? Check(PromotionCodeData data)
            {
                if (data.MinAmount < 1)
                    return "minAmount";
                if (
                  data.Discount < 1 ||
                  data.Discount >= data.MinAmount
                )
                    return "discount";

                return null;
            }
        }

        /// <summary>
        /// 产品满{m}减{n}
        /// </summary>
        public class PromotionCodePMJ : PromotionCode
        {
            public PromotionCodePMJ() : base(6, "PMJ")
            {
            }

            public override PromotionSaleItem? Calculate(PromotionItem p, IPromotionCodeLine? sale, decimal amount)
            {
                if (sale == null) return null;

                var minAmount = p.MinAmount;

                var currentPrice = sale.CurrentPrice ?? sale.Price;
                var qty = sale.Qty;
                var newAmount = currentPrice * qty;

                if (minAmount > 0 && newAmount < minAmount)
                {
                    return null;
                }

                var times = (int)Math.Floor(newAmount / minAmount);
                if (times > 0)
                {
                    var newDiscount = times * p.Discount;
                    sale.CurrentPrice = (int)Math.Round((newAmount - newDiscount) / qty);
                    return new PromotionSaleItem
                    {
                        Id = p.Id,
                        Title = p.Title,
                        Amount = times * p.Discount
                    };
                }

                return null;
            }

            public override string? Check(PromotionCodeData data)
            {
                if (data.MinAmount < 1)
                    return "minAmount";
                if (data.ProductId == null && data.ProductCategoryId == null)
                    return "categoryIdInput";
                if (
                  data.Discount < 1 ||
                  data.Discount >= data.MinAmount
                )
                    return "discount";

                return null;
            }
        }

        /// <summary>
        /// 产品买{n}送一
        /// </summary>
        public class PromotionCodePMS : PromotionCode
        {
            public PromotionCodePMS() : base(2, "PMS")
            {
            }

            public override PromotionSaleItem? Calculate(PromotionItem p, IPromotionCodeLine? sale, decimal amount)
            {
                if (sale == null) return null;

                var minAmount = p.MinAmount;

                if (minAmount > 0 && amount < minAmount)
                {
                    return null;
                }

                var qty = sale.Qty;
                var times = (int)Math.Floor(qty / (p.Discount + 1));
                if (times > 0)
                {
                    var currentPrice = sale.CurrentPrice ?? sale.Price;
                    var newDiscount = times * currentPrice;
                    sale.CurrentPrice = (int)Math.Round((currentPrice * qty - newDiscount) / qty);
                    return new PromotionSaleItem
                    {
                        Id = p.Id,
                        Title = p.Title,
                        Amount = newDiscount
                    };
                }

                return null;
            }

            public override string? Check(PromotionCodeData data)
            {
                if (data.ProductId == null && data.ProductCategoryId == null)
                    return "categoryIdInput";
                if (data.Discount < 1) return "discount";
                return null;
            }
        }

        /// <summary>
        /// 产品第二件{n}折
        /// </summary>
        public class PromotionCodePEZ : PromotionCode
        {
            public PromotionCodePEZ() : base(3, "PEZ")
            {
            }

            public override PromotionSaleItem? Calculate(PromotionItem p, IPromotionCodeLine? sale, decimal amount)
            {
                if (sale == null) return null;

                var minAmount = p.MinAmount;

                if (minAmount > 0 && amount < minAmount)
                {
                    return null;
                }

                var times = (int)Math.Floor(sale.Qty / 2);
                if (times > 0)
                {
                    var currentPrice = sale.CurrentPrice ?? sale.Price;
                    var qty = sale.Qty;

                    var newPrice = Math.Round(currentPrice * p.Discount) / 100;
                    var pAmount = times * (currentPrice - newPrice);

                    sale.CurrentPrice = Math.Round(currentPrice * qty - pAmount) / qty;

                    return new PromotionSaleItem
                    {
                        Id = p.Id,
                        Title = p.Title,
                        Amount = pAmount
                    };
                }

                return null;
            }

            public override string? Check(PromotionCodeData data)
            {
                if (data.ProductId == null && data.ProductCategoryId == null)
                    return "categoryIdInput";
                if (data.Discount < 10 || data.Discount >= 100)
                    return "discount";
                return null;
            }
        }

        /// <summary>
        /// 产品{n}折
        /// </summary>
        public class PromotionCodePKZ : PromotionCode
        {
            public PromotionCodePKZ() : base(7, "PKZ")
            {
            }

            public override PromotionSaleItem? Calculate(PromotionItem p, IPromotionCodeLine? sale, decimal amount)
            {
                if (sale == null) return null;

                var minAmount = p.MinAmount;

                if (minAmount > 0 && amount < minAmount)
                {
                    return null;
                }

                var currentPrice = sale.CurrentPrice ?? sale.Price;
                var newPrice = Math.Round(currentPrice * p.Discount) / 100;
                sale.CurrentPrice = newPrice;

                return new PromotionSaleItem
                {
                    Id = p.Id,
                    Title = p.Title,
                    Amount = Math.Round(sale.Qty * (currentPrice - newPrice))
                };
            }

            public override string? Check(PromotionCodeData data)
            {
                if (data.MinAmount < 0)
                    return "minAmount";
                if (data.ProductId == null && data.ProductCategoryId == null)
                    return "categoryIdInput";
                if (
                  data.Discount < 1 ||
                  data.Discount >= 100
                )
                    return "discount";

                return null;
            }
        }

        /// <summary>
        /// 产品{n}件以上{m}优惠价
        /// </summary>
        public class PromotionCodePJH : PromotionCode
        {
            public PromotionCodePJH() : base(8, "PJH")
            {
            }

            public override PromotionSaleItem? Calculate(PromotionItem p, IPromotionCodeLine? sale, decimal amount)
            {
                if (sale == null) return null;

                var qty = sale.Qty;
                var currentPrice = sale.CurrentPrice ?? sale.Price;

                var minAmount = p.MinAmount;
                var discount = p.Discount;

                if (discount < 2 || minAmount < 0.01M || qty < discount || currentPrice <= minAmount) return null;

                sale.CurrentPrice = minAmount;

                return new PromotionSaleItem
                {
                    Id = p.Id,
                    Title = p.Title,
                    Amount = Math.Round(qty * (currentPrice - minAmount))
                };
            }

            public override string? Check(PromotionCodeData data)
            {
                if (data.MinAmount < 0.01M)
                    return "minAmount";
                if (data.ProductId == null && data.ProductCategoryId == null)
                    return "categoryIdInput";
                if (data.Discount < 2)
                    return "discount";

                return null;
            }
        }

        /// <summary>
        /// 客户{n}折
        /// </summary>
        public class PromotionCodeCKZ : PromotionCode
        {
            public PromotionCodeCKZ() : base(4, "CKZ")
            {
            }

            public override PromotionSaleItem? Calculate(PromotionItem p, IPromotionCodeLine? sale, decimal amount)
            {
                var minAmount = p.MinAmount;

                if (minAmount > 0 && amount < minAmount)
                {
                    return null;
                }

                var discount = p.Discount;

                decimal newAmount;
                if (sale == null)
                {
                    newAmount = Math.Round(amount * (100 - discount)) / 100;
                }
                else
                {
                    var currentPrice = sale.CurrentPrice ?? sale.Price;
                    var newPrice = Math.Round(currentPrice * discount) / 100;
                    sale.CurrentPrice = newPrice;
                    newAmount = Math.Round(sale.Qty * (currentPrice - newPrice));
                }

                return new PromotionSaleItem
                {
                    Id = p.Id,
                    Title = p.Title,
                    Amount = newAmount
                };
            }

            public override string? Check(PromotionCodeData data)
            {
                if (data.PersonId == null) return "customerIdInput";
                if (data.Discount < 10 || data.Discount >= 100)
                    return "discount";
                return null;
            }
        }

        /// <summary>
        /// {t}会员{n}折
        /// </summary>
        public class PromotionCodeCDZ : PromotionCode
        {
            public PromotionCodeCDZ() : base(5, "CDZ")
            {
            }

            public override PromotionSaleItem? Calculate(PromotionItem p, IPromotionCodeLine? sale, decimal amount)
            {
                var minAmount = p.MinAmount;

                if (minAmount > 0 && amount < minAmount)
                {
                    return null;
                }

                var discount = p.Discount;

                decimal newAmount;
                if (sale == null)
                {
                    newAmount = Math.Round(amount * (100 - discount)) / 100;
                }
                else
                {
                    var currentPrice = sale.CurrentPrice ?? sale.Price;
                    var newPrice = Math.Round(currentPrice * discount) / 100;
                    sale.CurrentPrice = newPrice;
                    newAmount = Math.Round(sale.Qty * (currentPrice - newPrice));
                }

                return new PromotionSaleItem
                {
                    Id = p.Id,
                    Title = p.Title,
                    Amount = newAmount
                };
            }

            public override string? Check(PromotionCodeData data)
            {
                if (data.PersonCategoryId == null) return "customerKindInput";
                if (data.Discount < 10 || data.Discount >= 100)
                    return "discount";
                return null;
            }
        }
    }
}
