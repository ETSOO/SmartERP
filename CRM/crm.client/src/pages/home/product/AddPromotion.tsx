import { usePageDataEmpty } from "@etsoo/smarterp-core";
import { app } from "../../../app/MyApp";
import {
  ComboBox,
  EditPage,
  InputField,
  IntInputField,
  MoneyInputField,
  OptionBool
} from "@etsoo/materialui";
import { ReactUtils, useParamsEx, useRefs } from "@etsoo/react";
import { useFormik } from "formik";
import React from "react";
import Grid from "@mui/material/Grid";
import { IdActionResult, NumberUtils, Utils } from "@etsoo/shared";
import {
  CustomCultureKind,
  PromotionCode,
  PromotionCodeName,
  promotionCodes,
  PromotionCreateRQ,
  PromotionUpdateRQ
} from "@etsoo/smarterp-crm";
import { useNavigate } from "react-router-dom";
import { NameCulture } from "../../../components/NameCulture";
import { CurrencyList } from "../../../components/CurrencyList";
import { StatusList } from "@etsoo/smarterp-core/components";
import { EntityStatus, IdentityTypeFlags } from "@etsoo/appscript";
import {
  ButtonPersonCategories,
  ButtonProductCategories,
  PersonsList,
  ProductsList
} from "@etsoo/smarterp-crm/components";

export default function AddPromotion() {
  // Route
  const navigate = useNavigate();
  const { id } = useParamsEx({
    id: "number"
  });

  const isEditing = (id ?? 0) > 0;

  // Code
  const codeRef = React.useRef<PromotionCode>(null);

  // Culture permission
  const canManageCultures = isEditing && app.system.canManageCultures();

  // Labels
  const labels = app.getLabels(
    "coupons",
    "customerOrCategoryRequired",
    "customers",
    "discount",
    "endDate",
    "minAmount",
    "noChanges",
    "orderIndex",
    "productOrCategoryRequired",
    "products",
    "promotionCode",
    "stackable",
    "startDate",
    "suppliers",
    "title"
  );

  // Input refs
  const refFields = [
    "coupons",
    "discount",
    "minAmount",
    "orderIndex",
    "title",
    "validEnd",
    "validStart"
  ] as const;
  const refs = useRefs(refFields);

  // Type
  type DataType = PromotionCreateRQ;

  const defaultCurrency = app.system.getDefaultCurrency();

  // State
  const [data, setData] = React.useState<DataType>({
    code: 0,
    currency: defaultCurrency,
    title: "",
    minAmount: 0,
    discount: 0,
    validStart: "",
    validEnd: "",
    coupons: 0
  });

  // Formik
  const formik = useFormik<DataType>({
    initialValues: data,
    enableReinitialize: true,
    validateOnChange: false,
    onSubmit: async (v) => {
      // Get updated values
      const c = { ...v };
      ReactUtils.updateRefValues(refs, c);

      Utils.correctTypes(c, { stackable: "boolean" });

      // Validate
      const code = c.code;
      if (
        code === PromotionCodeName.PMJ ||
        code === PromotionCodeName.PMS ||
        code === PromotionCodeName.PEZ ||
        code === PromotionCodeName.PKZ ||
        code === PromotionCodeName.PJH
      ) {
        // Product or category required
        if (
          (c.productIds == null || c.productIds.length === 0) &&
          (c.productCategoryIds == null || c.productCategoryIds.length === 0)
        ) {
          app.warning(labels.productOrCategoryRequired);
          return false;
        }
      }

      if (code === PromotionCodeName.CKZ || code === PromotionCodeName.CDZ) {
        // Customer or category required
        if (
          (c.personIds == null || c.personIds.length === 0) &&
          (c.personCategoryIds == null || c.personCategoryIds.length === 0)
        ) {
          app.warning(labels.customerOrCategoryRequired);
          return false;
        }
      }

      // Submit
      let result: IdActionResult | undefined;
      let redirectUrl: string;
      if (id) {
        const rq: PromotionUpdateRQ = {
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

        result = await app.promotionApi.update(rq);
      } else {
        const rq: PromotionCreateRQ = {
          ...c
        };

        Utils.removeEmptyValues(rq);

        redirectUrl = "./..";

        result = await app.promotionApi.create(rq);
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
    const result = await app.promotionApi.updateRead(id);
    if (result == null) return;
    ReactUtils.updateRefs(refs, result);
    setData(result);
  }, [id]);

  const codes = React.useMemo(() => {
    promotionCodes.forEach(
      (item) =>
        (item.label = app.get(`promotionCode${item.label}`) ?? item.label)
    );
    return promotionCodes;
  }, []);

  const productIds = formik.values.productIds;
  const loadProductIdsValue = React.useCallback(
    () =>
      !productIds?.length
        ? Promise.resolve(undefined)
        : app.productApi.list({ ids: productIds }, { showLoading: false }),
    [productIds]
  );

  const personIds = formik.values.personIds;
  const loadPersonIdsValue = React.useCallback(
    () =>
      !personIds?.length
        ? Promise.resolve(undefined)
        : app.personApi.list({ ids: personIds }, { showLoading: false }),
    [personIds]
  );

  // Page data hook
  usePageDataEmpty(app);

  return (
    <EditPage
      isEditing={isEditing}
      onSubmit={formik.handleSubmit}
      onUpdate={reloadData}
      paddings={0}
    >
      <Grid size={{ xs: 12, sm: 6 }}>
        <ComboBox<PromotionCode>
          name="code"
          label={labels.promotionCode}
          idValue={formik.values.code}
          inputRequired
          inputOnChange={formik.handleChange}
          options={codes}
          onChange={(_event, value, reason) => {
            if (value) {
              codeRef.current = value;
              if (reason === "selectOption" && refs.title.current) {
                refs.title.current.value = value.label;
              }
            }
          }}
        />
      </Grid>
      <Grid size={{ xs: 6, sm: 3 }}>
        <CurrencyList
          value={formik.values.currency}
          onChange={formik.handleChange}
          fullWidth
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
        <MoneyInputField
          fullWidth
          name="minAmount"
          required
          symbol={NumberUtils.getCurrencySymbol(formik.values.currency)}
          label={labels.minAmount + "(m)"}
          inputRef={refs.minAmount}
        />
      </Grid>
      <Grid size={{ xs: 6, sm: 3 }}>
        <IntInputField
          fullWidth
          name="discount"
          required
          label={labels.discount + "(n)"}
          inputRef={refs.discount}
        />
      </Grid>
      <Grid size={{ xs: 6, sm: 3 }}>
        <IntInputField
          fullWidth
          name="coupons"
          label={labels.coupons}
          inputRef={refs.coupons}
        />
      </Grid>
      <Grid size={{ xs: 6, sm: 3 }}>
        <InputField
          fullWidth
          required
          name="validStart"
          type="datetime-local"
          label={labels.startDate}
          inputRef={refs.validStart}
        />
      </Grid>
      <Grid size={{ xs: 6, sm: 3 }}>
        <InputField
          fullWidth
          required
          name="validEnd"
          type="datetime-local"
          label={labels.endDate}
          inputRef={refs.validEnd}
        />
      </Grid>
      <Grid size={{ xs: 6, sm: 3 }}>
        <OptionBool
          name="stackable"
          label={labels.stackable}
          variant="outlined"
          defaultValue={formik.values.stackable}
          fullWidth
          onChange={formik.handleChange}
        />
      </Grid>
      <Grid size={{ xs: 6, sm: 3 }}>
        <IntInputField
          fullWidth
          name="orderIndex"
          label={labels.orderIndex}
          inputRef={refs.orderIndex}
          max={65000}
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 12 }}>
        <InputField
          fullWidth
          required
          name="title"
          slotProps={{ htmlInput: { maxLength: 128 } }}
          label={labels.title}
          inputRef={refs.title}
          onFocus={() => {
            if (refs.title.current) {
              let title = refs.title.current.value;
              const minAmount = refs.minAmount.current?.value ?? "";
              const discount = refs.discount.current?.value ?? "";
              title = title.replace("{m}", minAmount).replace("{n}", discount);
              refs.title.current.value = title;
            }
          }}
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 12 }}>
        <ProductsList
          label={labels.products}
          onChange={(_event, value) =>
            formik.setFieldValue(
              "productIds",
              value.map((item) => item.id)
            )
          }
          loadIdValue={loadProductIdsValue}
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 12 }}>
        <ButtonProductCategories
          fullWidth
          value={formik.values.productCategoryIds ?? []}
          onValueChange={(ids) =>
            formik.setFieldValue("productCategoryIds", ids)
          }
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 12 }}>
        <PersonsList
          label={labels.customers + " / " + labels.suppliers}
          rq={{
            identityType:
              IdentityTypeFlags.Customer | IdentityTypeFlags.Supplier
          }}
          onChange={(_event, value) =>
            formik.setFieldValue(
              "personIds",
              value.map((item) => item.id)
            )
          }
          loadIdValue={loadPersonIdsValue}
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 12 }}>
        <ButtonPersonCategories
          fullWidth
          value={formik.values.personCategoryIds ?? []}
          identityType={IdentityTypeFlags.Customer}
          onValueChange={(ids) =>
            formik.setFieldValue("personCategoryIds", ids)
          }
        />
      </Grid>
      {canManageCultures && (
        <Grid size={{ xs: 6, sm: 3 }}>
          <NameCulture id={id ?? 0} kind={CustomCultureKind.Promotion} />
        </Grid>
      )}
    </EditPage>
  );
}
