import {
  CustomFieldUI,
  HBox,
  InputField,
  MenuButton,
  MoneyInputField,
  NotificationMUDataMethods,
  NotificationMUDataProps,
  NumberInputField,
  VBox
} from "@etsoo/materialui";
import { app } from "../../../app/MyApp";
import {
  OrderLineCreateRQ,
  OrderUtils,
  ProductListData,
  PromotionItem,
  QueryForSaleData
} from "@etsoo/smarterp-crm";
import React from "react";
import { ProductList } from "@etsoo/smarterp-crm/components";
import { DomUtils, NumberUtils } from "@etsoo/shared";
import IconButton from "@mui/material/IconButton";
import Badge from "@mui/material/Badge";
import { Divider, Typography } from "@mui/material";
import { CustomFieldRef } from "@etsoo/appscript";

function OrderLineUI({
  mRef,
  data
}: NotificationMUDataProps & {
  data: OrderUIUtils.AddOrderLineType;
}) {
  // Labels
  const labels = app.getLabels(
    "amount",
    "description",
    "price",
    "qty",
    "promotions",
    "title"
  );

  // Form ref
  const formRef = React.useRef<HTMLFormElement>(null);
  const titleRef = React.useRef<HTMLInputElement>(null);

  // State
  const [price, setPrice] = React.useState<number>();
  const [qty, setQty] = React.useState<number>();
  const [sale, setSale] = React.useState<QueryForSaleData>();

  const promotions = React.useMemo(() => {
    if (sale == null || qty == null || qty <= 0 || price == null || price < 0)
      return [];

    return OrderUtils.calculatePromotions(sale.promotions, undefined, {
      qty,
      price
    });
  }, [sale, qty, price]);

  const symbol = NumberUtils.getCurrencySymbol(data.currency);

  React.useImperativeHandle(mRef, () => ({
    getValue: (): OrderLineCreateRQ | undefined => {
      if (formRef.current == null) return undefined;

      if (!formRef.current.reportValidity()) {
        return undefined;
      }

      const { productId, title, description } = DomUtils.dataAs(
        new FormData(formRef.current),
        {
          productId: "number",
          title: "string",
          description: "string"
        }
      );

      if (
        productId == null ||
        title == null ||
        price == null ||
        price < 0 ||
        qty == null ||
        qty <= 0
      ) {
        DomUtils.setFocus("qty", formRef.current);
        return;
      }

      const rq: OrderLineCreateRQ = {
        orderId: data.orderId,
        productId,
        title,
        price,
        qty,
        promotions,
        description
      };

      if (modifiersRef.current) {
        const modifiers = modifiersRef.current.getValue();
        rq.data ??= {};
        rq.data.modifiers = modifiers;
      }

      return rq;
    }
  }));

  const changeProduct = (value: ProductListData | null) => {
    if (titleRef.current) {
      titleRef.current.value = value?.name ?? "";
    }

    if (value != null) {
      app.productApi
        .queryForSale(
          {
            customerId: data.customerId,
            currency: data.currency,
            ids: [value.id]
          },
          { showLoading: false }
        )
        .then((products) => {
          if (products == null || products.length === 0) return;

          const sale = products[0];
          setSale(sale);

          const price = Math.min(
            sale.retailPrice,
            sale.promotionPrice ?? Number.MAX_SAFE_INTEGER,
            sale.customerRetailPrice ?? Number.MAX_SAFE_INTEGER
          );

          setPrice(price);
          setQty(sale.minQty ?? 1);
        });
    }
  };

  const titleFormatter = (item: PromotionItem) => {
    const p = promotions.find((p) => p.id === item.id);
    return (
      item.title +
      (p != null
        ? ` (${app.formatMoney(p.amount, undefined, { currency: data.currency })})`
        : "")
    );
  };

  const customFields = sale?.modifiers ?? [];
  const modifiersRef =
    React.useRef<CustomFieldRef<Record<string, unknown>>>(null);

  return (
    <form ref={formRef}>
      <VBox spacing={2} sx={{ paddingTop: 1 }}>
        <ProductList fullWidth inputRequired onValueChange={changeProduct} />
        <InputField
          fullWidth
          name="title"
          slotProps={{
            htmlInput: { maxLength: 128 }
          }}
          label={labels.title}
          inputRef={titleRef}
          required
        />
        <HBox spacing={1}>
          <MoneyInputField
            fullWidth
            name="price"
            label={labels.price}
            symbol={symbol}
            value={price ?? ""}
            onChange={(input) =>
              setPrice(NumberUtils.parse(input.target.value))
            }
            helperText={
              labels.amount +
              ": " +
              app.formatMoney(
                price != null && qty != null ? price * qty : 0,
                undefined,
                {
                  currency: data.currency
                }
              )
            }
            required
          />
          <NumberInputField
            fullWidth
            name="qty"
            label={labels.qty}
            min={sale?.minQty}
            max={sale?.capQty}
            step={sale?.stepQty}
            value={qty ?? ""}
            onChange={(input) => setQty(NumberUtils.parse(input.target.value))}
            helperText={
              sale != null &&
              sale.promotions.length > 0 && (
                <HBox spacing={1}>
                  <MenuButton<PromotionItem>
                    items={sale.promotions}
                    labelField={titleFormatter}
                    button={(clickHandler) => {
                      return (
                        <IconButton
                          onClick={clickHandler}
                          size="small"
                          sx={{ marginLeft: 1.5 }}
                          title={[
                            labels.promotions,
                            ...sale.promotions.map(titleFormatter)
                          ].join("\n")}
                        >
                          <Badge
                            badgeContent={sale.promotions.length}
                            color="secondary"
                          />
                        </IconButton>
                      );
                    }}
                  />
                  <Typography variant="caption">
                    {app.formatMoney(
                      promotions.reduce((sum, p) => sum + p.amount, 0),
                      undefined,
                      { currency: data.currency }
                    )}
                  </Typography>
                </HBox>
              )
            }
            required
          />
        </HBox>
        <InputField
          fullWidth
          name="description"
          slotProps={{
            htmlInput: { maxLength: 1280 }
          }}
          label={labels.description}
          multiline
          rows={2}
        />
        {customFields.length > 0 && (
          <React.Fragment>
            <Divider />
            <CustomFieldUI fields={customFields} mref={modifiersRef} />
          </React.Fragment>
        )}
      </VBox>
    </form>
  );
}

export namespace OrderUIUtils {
  /**
   * Add order line type
   */
  export type AddOrderLineType = {
    customerId: number;
    orderId: number;
    currency: string;
  };

  /**
   * Add order line
   * @param data Data
   * @param refresh Refresh
   */
  export function addOrderLine(
    data: AddOrderLineType,
    refresh: () => Promise<void> | void
  ) {
    app.notifier.data<OrderLineCreateRQ>(
      <OrderLineUI
        data={data}
        mRef={React.createRef<NotificationMUDataMethods>()}
      />,
      async (data) => {
        if (data == null) return;

        const result = await app.orderLineApi.create(data);
        if (result == null) return;

        if (result.ok) {
          await refresh();
          return;
        } else {
          return app.formatResult(result);
        }
      },
      app.get("addOrderLine")
    );
  }
}
