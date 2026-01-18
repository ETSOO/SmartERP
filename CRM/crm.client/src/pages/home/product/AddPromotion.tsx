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
  promotionCodes,
  PromotionCreateRQ,
  PromotionUpdateRQ
} from "@etsoo/smarterp-crm";
import { useNavigate } from "react-router-dom";
import { Permissions } from "@etsoo/smarterp-crm";
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
  const canManageCultures =
    isEditing &&
    app.owns(Permissions.Org.Manage) &&
    (app.userData?.system?.cultures.length ?? 0) > 1;

  // Labels
  const labels = app.getLabels(
    "coupons",
    "customers",
    "discount",
    "endDate",
    "minAmount",
    "noChanges",
    "products",
    "promotionCode",
    "stackable",
    "startDate",
    "title"
  );

  // Input refs
  const refFields = [
    "coupons",
    "discount",
    "minAmount",
    "title",
    "validEnd",
    "validStart"
  ] as const;
  const refs = useRefs(refFields);

  // Type
  type DataType = PromotionCreateRQ;

  // State
  const [data, setData] = React.useState<DataType>({
    code: 0,
    currency: app.currency,
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

  // Page data hook
  usePageDataEmpty(app);

  return (
    <EditPage
      isEditing={isEditing}
      onSubmit={(event) => {
        formik.handleSubmit(event);
      }}
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
      <Grid size={{ xs: 12, sm: 6 }}>
        <CurrencyList
          value={formik.values.currency}
          onChange={formik.handleChange}
          fullWidth
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
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 12 }}>
        <ProductsList
          label={labels.products}
          onChange={(_event, value) =>
            formik.setFieldValue("productIds", value)
          }
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
          label={labels.customers}
          rq={{ identityType: IdentityTypeFlags.Customer }}
          onChange={(_event, value) => formik.setFieldValue("personIds", value)}
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
          required
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
        <StatusList
          inputRequired
          idValue={formik.values.status ?? EntityStatus.Normal}
          inputOnChange={formik.handleChange}
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
