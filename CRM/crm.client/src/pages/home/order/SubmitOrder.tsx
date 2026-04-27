import { usePageDataEmpty } from "@etsoo/smarterp-core";
import { app } from "../../../app/MyApp";
import {
  EditPage,
  InputField,
  MoneyInputField,
  NumberInputField,
  TagList
} from "@etsoo/materialui";
import { ReactUtils, useParamsEx, useRefs } from "@etsoo/react";
import { useFormik } from "formik";
import React from "react";
import Grid from "@mui/material/Grid";
import { StatusList } from "@etsoo/smarterp-core/components";
import { DateUtils, IdActionResult, NumberUtils, Utils } from "@etsoo/shared";
import {
  FeatureTagKind,
  OrderCreateRQ,
  OrderLineRQ,
  OrderUpdateRQ,
  PromotionSaleItemBase
} from "@etsoo/smarterp-crm";
import { useNavigate } from "react-router-dom";
import { EntityStatus } from "@etsoo/appscript";
import {
  AddressList,
  ContactTiplist,
  CustomerList,
  OrderAssignedIdDuplicateTest,
  OrderDeliveryTiplist,
  OrderPaymentTiplist,
  OrderSourceIdDuplicateTest,
  OrderTitleDuplicateTest,
  UserTiplist
} from "@etsoo/smarterp-crm/components";
import { LocalUtils } from "../../../app/LocalUtils";
import { CurrencyList } from "../../../components/CurrencyList";
import { CultureList } from "../../../components/CultureList";

type OrderData = {
  customerId: number;
  currency: string;
  symbol?: string;
  culture: string;
  discount: number;
  lines: number;
  items: number;
  amount: number;
  hasPromotion: boolean;

  linesArray: LocalUtils.OrderLine[];
  promotions?: PromotionSaleItemBase[];
};

export default function SubmitOrder() {
  // Route
  const navigate = useNavigate();
  const { id } = useParamsEx({
    id: "number"
  });

  const isEditing = (id ?? 0) > 0;

  // Labels
  const labels = app.getLabels(
    "amount",
    "deliveryAddress",
    "deliveryInstruction",
    "description",
    "discount",
    "endDate",
    "items",
    "noChanges",
    "orderLines",
    "orderSource",
    "paymentInstruction",
    "startDate",
    "status",
    "tags",
    "taxAmount"
  );

  // Type
  type DataType = OrderUpdateRQ;

  // State
  const [data, setData] = React.useState<DataType>({
    id: 0,
    userId: app.userData?.userPersonId
  });
  const [orderData, setOrderData] = React.useState<OrderData>();

  // Input refs
  const refFields = [
    "assignedId",
    "deliveryInstruction",
    "description",
    "endDate",
    "paymentInstruction",
    "source",
    "sourceId",
    "startDate",
    "taxAmount",
    "title"
  ] as const;
  const refs = useRefs(refFields);

  // Formik
  const formik = useFormik<DataType>({
    initialValues: data,
    enableReinitialize: true,
    validateOnChange: false,
    onSubmit: async (v) => {
      if (orderData == null) return;

      // Get updated values
      const c = structuredClone(v);

      ReactUtils.updateRefValues(refs, c);

      // Submit
      let result: IdActionResult | undefined;
      let redirectUrl: string;
      if (id) {
        const rq: OrderUpdateRQ = {
          ...c,
          id
        };

        // Changed fields
        const fields = Utils.getDataChanges(rq, data);
        if (fields.length === 0) {
          app.warning(labels.noChanges);
          return;
        }
        rq.changedFields = fields;

        redirectUrl = "./../..";

        result = await app.orderApi.update(rq);
      } else {
        const lines: OrderLineRQ[] = orderData.linesArray.map(
          ({
            productId,
            qty,
            price,
            title,
            description,
            data,
            promotions
          }) => ({
            productId,
            qty,
            price,
            title,
            description,
            data,
            promotions
          })
        );

        const rq: OrderCreateRQ = {
          ...c,
          customerId: orderData.customerId,
          currency: orderData.currency,
          culture: orderData.culture,
          amount: orderData.amount,
          promotions: orderData.promotions,
          lines
        };

        Utils.removeEmptyValues(rq);

        redirectUrl = "./..";

        result = await app.orderApi.create(rq);

        if (result?.ok) {
          // Clear local storage
          LocalUtils.clearOrderData();
        }
      }

      if (result == null) return;

      if (result.ok) {
        navigate(redirectUrl);
        return;
      }

      app.alertResult(result);
    }
  });

  // Load data
  const reloadData = React.useCallback(async () => {
    if (!id) return;
    const result = await app.orderApi.updateRead(id);
    if (result == null) return;

    ReactUtils.updateRefs(refs, result);

    const discount = result.discount + result.lineDiscount;

    setOrderData({
      customerId: result.customerId,
      currency: result.currency,
      symbol: NumberUtils.getCurrencySymbol(result.currency),
      culture: result.culture,
      discount: discount,
      lines: result.lines,
      items: result.items,
      amount: result.amount,
      hasPromotion: discount > 0,

      linesArray: []
    });

    setData(result);
  }, [id]);

  // Page data hook
  usePageDataEmpty(app);

  React.useEffect(() => {
    if (isEditing) return;

    // Local storage
    const customer =
      app.storage.getPersistedObject<LocalUtils.CustomerQueryData>(
        LocalUtils.ORDER_CUSTOMER_DATA_KEY
      );

    // Order lines
    const pLines = app.storage.getPersistedObject<LocalUtils.OrderLine[]>(
      LocalUtils.ORDER_LINES_DATA_KEY
    );

    // Order promotions
    const pPromotions = app.storage.getPersistedObject<PromotionSaleItemBase[]>(
      LocalUtils.ORDER_PROMOTIONS_DATA_KEY
    );

    if (
      !customer ||
      !customer.customerId ||
      !customer.culture ||
      pLines == null ||
      pPromotions == null
    ) {
      navigate("./../add");
      return;
    }

    const lineDiscount = pLines.reduce((sum, line) => {
      if (line.promotions == null) return sum;
      return sum + line.promotions.reduce((s, p) => s + p.amount, 0);
    }, 0);

    const orderDiscount = pPromotions.reduce(
      (sum, item) => sum + item.amount,
      0
    );

    const discount = orderDiscount + lineDiscount;

    // Order line amount = line amount - line promotion amount
    const amount =
      pLines.reduce((sum, line) => sum + line.amount, 0) - orderDiscount;

    setOrderData({
      customerId: customer.customerId,
      currency: customer.currency,
      symbol: NumberUtils.getCurrencySymbol(customer.currency),
      culture: customer.culture,
      discount,
      lines: pLines.length,
      items: pLines.reduce((sum, line) => sum + line.qty, 0),
      amount,
      hasPromotion: discount > 0,

      linesArray: pLines,
      promotions: pPromotions
    });
  }, [isEditing]);

  return (
    <EditPage
      isEditing={isEditing}
      onSubmit={(event) => {
        formik.handleSubmit(event);
      }}
      onUpdate={reloadData}
      paddings={0}
    >
      <Grid size={{ xs: 12, sm: 9 }}>
        <CustomerList
          idValue={orderData?.customerId}
          disabled={orderData?.hasPromotion}
          inputOnChange={formik.handleChange}
          onValueChange={(item) => {
            if (isEditing) return;

            const input = refs.title.current;
            if (input == null) return;

            input.value =
              item == null
                ? ""
                : `${Utils.formatName(item.name, 6)} ${DateUtils.format(new Date(), "yyyyMMdd")}`;
          }}
        />
      </Grid>
      <Grid size={{ xs: 6, sm: 3 }}>
        <CurrencyList value={orderData?.currency} fullWidth disabled />
      </Grid>
      <Grid size={{ xs: 6, sm: 3 }}>
        <NumberInputField
          fullWidth
          label={labels.orderLines}
          defaultValue={orderData?.lines}
          disabled
        />
      </Grid>
      <Grid size={{ xs: 6, sm: 3 }}>
        <NumberInputField
          fullWidth
          label={labels.items}
          defaultValue={orderData?.items}
          disabled
        />
      </Grid>
      <Grid size={{ xs: 6, sm: 3 }}>
        <MoneyInputField
          fullWidth
          label={labels.discount}
          defaultValue={orderData?.discount}
          symbol={orderData?.symbol}
          disabled
        />
      </Grid>
      <Grid size={{ xs: 6, sm: 3 }}>
        <MoneyInputField
          fullWidth
          label={labels.amount}
          defaultValue={orderData?.amount}
          symbol={orderData?.symbol}
          disabled
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 9 }}>
        <OrderTitleDuplicateTest
          fullWidth
          required
          isOrder
          excludedId={id}
          inputRef={refs.title}
        />
      </Grid>
      <Grid size={{ xs: 6, sm: 3 }}>
        <CultureList
          value={orderData?.culture}
          fullWidth
          onChange={formik.handleChange}
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 9 }}>
        <AddressList
          personId={orderData?.customerId ?? 0}
          label={labels.deliveryAddress}
          idValue={data.addressId}
          inputOnChange={formik.handleChange}
        />
      </Grid>
      <Grid size={{ xs: 6, sm: 3 }}>
        <ContactTiplist
          personId={orderData?.customerId ?? 0}
          idValue={data.contactId}
          inputOnChange={formik.handleChange}
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 6 }}>
        <OrderDeliveryTiplist
          idValue={data.deliveryId}
          inputOnChange={formik.handleChange}
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 6 }}>
        <InputField
          fullWidth
          name="deliveryInstruction"
          slotProps={{
            htmlInput: { maxLength: 512 }
          }}
          label={labels.deliveryInstruction}
          inputRef={refs.deliveryInstruction}
          multiline
          rows={2}
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 6 }}>
        <OrderPaymentTiplist
          idValue={data.paymentId}
          inputOnChange={formik.handleChange}
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 6 }}>
        <InputField
          fullWidth
          name="paymentInstruction"
          slotProps={{
            htmlInput: { maxLength: 512 }
          }}
          label={labels.paymentInstruction}
          inputRef={refs.paymentInstruction}
          multiline
          rows={2}
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 12 }}>
        <TagList
          value={formik.values.tags ?? []}
          disableCloseOnSelect
          openOnFocus
          onChange={(_event, value) => formik.setFieldValue("tags", value)}
          label={labels.tags}
          loadData={(keyword, maxItems) =>
            app.tagApi.list(
              {
                kind: FeatureTagKind.Order,
                keyword,
                queryPaging: maxItems
              },
              { showLoading: false }
            )
          }
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 12 }}>
        <InputField
          fullWidth
          name="description"
          slotProps={{
            htmlInput: { maxLength: 1280 }
          }}
          label={labels.description}
          inputRef={refs.description}
          multiline
          rows={2}
        />
      </Grid>
      <Grid size={{ xs: 6, sm: 3 }}>
        <InputField
          fullWidth
          name="startDate"
          type="datetime-local"
          label={labels.startDate}
          inputRef={refs.startDate}
        />
      </Grid>
      <Grid size={{ xs: 6, sm: 3 }}>
        <InputField
          fullWidth
          name="endDate"
          type="datetime-local"
          label={labels.endDate}
          inputRef={refs.endDate}
        />
      </Grid>
      <Grid size={{ xs: 6, sm: 3 }}>
        <InputField
          fullWidth
          name="source"
          slotProps={{
            htmlInput: { maxLength: 20 }
          }}
          label={labels.orderSource}
          inputRef={refs.source}
        />
      </Grid>
      <Grid size={{ xs: 6, sm: 3 }}>
        <OrderSourceIdDuplicateTest
          excludedId={id}
          isOrder
          fullWidth
          inputRef={refs.sourceId}
        />
      </Grid>
      <Grid size={{ xs: 6, sm: 3 }}>
        <OrderAssignedIdDuplicateTest
          excludedId={id}
          isOrder
          fullWidth
          inputRef={refs.assignedId}
        />
      </Grid>
      <Grid size={{ xs: 6, sm: 3 }}>
        <MoneyInputField
          fullWidth
          name="taxAmount"
          label={labels.taxAmount}
          inputRef={refs.taxAmount}
          symbol={orderData?.symbol}
        />
      </Grid>
      <Grid size={{ xs: 6, sm: 3 }}>
        <StatusList
          inputRequired
          idValue={formik.values.status ?? EntityStatus.Normal}
          inputOnChange={formik.handleChange}
        />
      </Grid>
      <Grid size={{ xs: 6, sm: 3 }}>
        <UserTiplist
          idValue={data.userId}
          inputOnChange={formik.handleChange}
        />
      </Grid>
    </EditPage>
  );
}
