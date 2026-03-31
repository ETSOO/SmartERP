import { PromotionCodeData } from "./PromotionCodeData";
import { PromotionItem } from "./PromotionItem";

/**
 * Order line for promotion calculation
 * 促销计算的订单行
 */
export type PromotionOrderLine = {
  /**
   * Real sale price
   * 实际销售价格
   */
  price: number;

  /**
   * Quantity
   * 数量
   */
  qty: number;

  /**
   * Current price for promotion calculation, it will be modified by promotion code calculate function
   * 当前促销计算价格，促销代码的计算函数会修改这个价格以供后续促销计算使用
   */
  currentPrice?: number | undefined;
};

/**
 * Promotion code calculation result
 * 促销代码计算结果
 */
export type PromotionCodeCalculation = {
  /**
   * Promotion id
   * 促销编号
   */
  id: number;

  /**
   * Promotion title
   * 促销标题
   */
  title: string;

  /**
   * Discount amount
   * 折扣金额
   */
  amount: number;
};

/**
 * Promotion code definition
 * 促销代码定义，如果是面向产品和产品类目的促销，满足的金额是本行的金额；如果是面向订单的促销，满足的金额是订单的总金额
 * calculate函数会修改订单行的currentPrice以供后续促销计算使用
 */
export type PromotionCode = {
  id: number;
  label: string;
  calculate: (
    p: PromotionItem,
    sale: PromotionOrderLine | null | undefined,
    amount: number
  ) => PromotionCodeCalculation | undefined;
  validate: (data: PromotionCodeData) => string | void;
};

/**
 * Promotion code name
 * 促销代码名称
 */
export enum PromotionCodeName {
  OMJ = 1, // 订单满{m}减{n}
  PMJ = 6, // 产品满{m}减{n}
  PMS = 2, // 产品买{n}送一
  PEZ = 3, // 产品第二件{n}折
  PKZ = 7, // 产品{n}折
  PJH = 8, // 产品{n}件以上{m}优惠价
  CKZ = 4, // 客户{n}折
  CDZ = 5 // {t}会员{n}折
}

export const promotionCodes: PromotionCode[] = [
  {
    id: PromotionCodeName.OMJ,
    label: "OMJ", // 订单满{m}减{n}
    calculate: (
      p: PromotionItem,
      _sale: PromotionOrderLine | null | undefined,
      amount: number
    ) => {
      const { id, title, minAmount, discount } = p;
      if (minAmount < 1) return undefined;

      const times = Math.floor(amount / minAmount);
      if (times > 0) {
        return {
          id,
          title,
          amount: times * discount
        };
      }
      return undefined;
    },
    validate: (data: PromotionCodeData) => {
      if (data.minAmount == null || data.minAmount < 1) return "MinAmount";
      if (
        data.discount == null ||
        data.discount < 1 ||
        data.discount >= data.minAmount
      )
        return "Discount";
    }
  },
  {
    id: PromotionCodeName.PMJ,
    label: "PMJ", // 产品满{m}减{n}
    calculate: (
      p: PromotionItem,
      sale: PromotionOrderLine | null | undefined,
      amount: number
    ) => {
      if (sale == null) return undefined;
      const { id, title, minAmount, discount } = p;
      if (minAmount > 0 && amount < minAmount) return undefined;

      const { price, currentPrice = price, qty } = sale;
      const newAmount = currentPrice * qty;
      const times = Math.floor(newAmount / minAmount);
      if (times > 0) {
        const newDiscount = times * discount;
        sale.currentPrice = Math.round((newAmount - newDiscount) / qty);
        return {
          id,
          title,
          amount: newDiscount
        };
      }
      return undefined;
    },
    validate: (data: PromotionCodeData) => {
      if (data.minAmount == null || data.minAmount < 1) return "MinAmount";
      if (!data.productIds?.length && !data.productCategoryIds?.length)
        return "Products";
      if (
        data.discount == null ||
        data.discount < 1 ||
        data.discount >= data.minAmount
      )
        return "Discount";
    }
  },
  {
    id: PromotionCodeName.PMS,
    label: "PMS", // 产品买{n}送一
    calculate: (
      p: PromotionItem,
      sale: PromotionOrderLine | null | undefined,
      amount: number
    ) => {
      if (sale == null) return undefined;
      const { id, title, minAmount, discount } = p;
      if (minAmount > 0 && amount < minAmount) return undefined;

      const { qty, price, currentPrice = price } = sale;
      const times = Math.floor(qty / (discount + 1));
      if (times > 0) {
        const newDiscount = times * currentPrice;
        sale.currentPrice = Math.round(
          (currentPrice * qty - newDiscount) / qty
        );
        return {
          id,
          title,
          amount: newDiscount
        };
      }

      return undefined;
    },
    validate: (data: PromotionCodeData) => {
      if (!data.productIds?.length && !data.productCategoryIds?.length)
        return "Products";
      if (data.discount == null || data.discount < 1) return "Discount";
    }
  },
  {
    id: PromotionCodeName.PEZ,
    label: "PEZ", // 产品第二件{n}折
    calculate: (
      p: PromotionItem,
      sale: PromotionOrderLine | null | undefined,
      amount: number
    ) => {
      if (sale == null) return undefined;
      const { id, title, minAmount, discount } = p;
      if (minAmount > 0 && amount < minAmount) return undefined;

      const { qty, price, currentPrice = price } = sale;
      const times = Math.floor(qty / 2);
      if (times > 0) {
        const secondPrice = Math.round(currentPrice * (100 - discount)) / 100;
        const amount = times * secondPrice;

        // Average price
        const averagePrice = Math.round((currentPrice * qty - amount) / qty);
        sale.currentPrice = averagePrice;

        return {
          id,
          title,
          amount
        };
      }

      return undefined;
    },
    validate: (data: PromotionCodeData) => {
      if (!data.productIds?.length && !data.productCategoryIds?.length)
        return "Products";
      if (data.discount == null || data.discount < 10 || data.discount >= 100)
        return "Discount";
    }
  },
  {
    id: PromotionCodeName.PKZ,
    label: "PKZ", // 产品{n}折
    calculate: (
      p: PromotionItem,
      sale: PromotionOrderLine | null | undefined,
      amount: number
    ) => {
      if (sale == null) return undefined;
      const { id, title, minAmount, discount } = p;
      if (minAmount > 0 && amount < minAmount) return undefined;

      const { qty, price, currentPrice = price } = sale;
      const newPrice = Math.round(currentPrice * discount) / 100;
      sale.currentPrice = newPrice;

      return {
        id,
        title,
        amount: Math.round((currentPrice - newPrice) * qty)
      };
    },
    validate: (data: PromotionCodeData) => {
      if (!data.productIds?.length && !data.productCategoryIds?.length)
        return "Products";
      if (data.discount == null || data.discount < 1 || data.discount >= 100)
        return "Discount";
    }
  },
  {
    id: PromotionCodeName.PJH,
    label: "PJH", // 产品{n}件以上{m}优惠价
    calculate: (
      p: PromotionItem,
      sale: PromotionOrderLine | null | undefined,
      _amount: number
    ) => {
      if (sale == null) return undefined;
      const { id, title, minAmount, discount } = p;
      if (discount < 2 || minAmount < 0.01) return undefined;

      const { qty, price, currentPrice = price } = sale;

      if (qty < discount || currentPrice <= minAmount) return undefined;

      sale.currentPrice = minAmount;

      return {
        id,
        title,
        amount: Math.round((currentPrice - minAmount) * qty)
      };
    },
    validate: (data: PromotionCodeData) => {
      if (!data.productIds?.length && !data.productCategoryIds?.length)
        return "Products";
      if (data.minAmount == null || data.minAmount < 0.01) return "MinAmount";
      if (data.discount == null || data.discount < 2) return "Discount";
    }
  },
  {
    id: PromotionCodeName.CKZ,
    label: "CKZ", // 客户{n}折
    calculate: (
      p: PromotionItem,
      sale: PromotionOrderLine | null | undefined,
      amount: number
    ) => {
      const { id, title, minAmount, discount } = p;
      if (minAmount > 0 && amount < minAmount) return undefined;

      let newAmount: number;
      if (sale == null) {
        newAmount = Math.round(amount * (100 - discount)) / 100;
      } else {
        const { qty, price, currentPrice = price } = sale;
        const newPrice = Math.round(currentPrice * discount) / 100;
        sale.currentPrice = newPrice;
        newAmount = Math.round(qty * (currentPrice - newPrice));
      }

      return {
        id,
        title,
        amount: newAmount
      };
    },
    validate: (data: PromotionCodeData) => {
      if (!data.personCategoryIds?.length) return "PersonCategories";
      if (data.discount == null || data.discount < 10 || data.discount >= 100)
        return "Discount";
    }
  },
  {
    id: PromotionCodeName.CDZ,
    label: "CDZ", // {t}会员{n}折
    calculate: (
      p: PromotionItem,
      sale: PromotionOrderLine | null | undefined,
      amount: number
    ) => {
      const { id, title, minAmount, discount } = p;
      if (minAmount > 0 && amount < minAmount) return undefined;

      let newAmount: number;
      if (sale == null) {
        newAmount = Math.round(amount * (100 - discount)) / 100;
      } else {
        const { qty, price, currentPrice = price } = sale;
        const newPrice = Math.round(currentPrice * discount) / 100;
        sale.currentPrice = newPrice;
        newAmount = Math.round(qty * (currentPrice - newPrice));
      }

      return {
        id,
        title,
        amount: newAmount
      };
    },
    validate: (data: PromotionCodeData) => {
      if (!data.personCategoryIds?.length) return "PersonCategories";
      if (data.discount == null || data.discount < 10 || data.discount >= 100)
        return "Discount";
    }
  }
];
