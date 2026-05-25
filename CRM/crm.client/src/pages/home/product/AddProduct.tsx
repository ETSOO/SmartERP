import { usePageDataEmpty } from "@etsoo/smarterp-core";
import { app } from "../../../app/MyApp";
import {
  CustomAttributeArea,
  CustomFieldUI,
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
import { IdActionResult, NumberUtils, Utils } from "@etsoo/shared";
import {
  FeatureTagKind,
  Permissions,
  ProductCreateRQ,
  ProductScope,
  ProductUpdateRQ,
  ProductUsage
} from "@etsoo/smarterp-crm";
import { useNavigate } from "react-router-dom";
import {
  BusinessUtils,
  CustomFieldData,
  CustomFieldRef,
  EntityStatus
} from "@etsoo/appscript";
import {
  ButtonProductCategories,
  ProductAssignedIdDuplicateTest,
  ProductButtonScopes,
  ProductNameDuplicateTest,
  ProductUnitList,
  ProductUsageList
} from "@etsoo/smarterp-crm/components";
import Divider from "@mui/material/Divider";

export default function AddProduct() {
  // Route
  const navigate = useNavigate();
  const { id } = useParamsEx({
    id: "number"
  });

  const isEditing = (id ?? 0) > 0;

  // Labels
  const labels = app.getLabels(
    "assetQty",
    "capQty",
    "categories",
    "channelPrice",
    "costPrice",
    "days",
    "defaultTaxRate",
    "deleteConfirm",
    "description",
    "introductionUrl",
    "minQty",
    "modifiers",
    "noChanges",
    "product",
    "productName",
    "promotionPrice",
    "queryKeyword",
    "retailPrice",
    "status",
    "stepQty",
    "tags",
    "taxRate",
    "validity"
  );

  // Type
  type DataType = ProductCreateRQ & { isAsset?: boolean };

  const defaultCurrency = app.system.getDefaultCurrency();
  const defaultTaxRate = app.userData?.system?.taxRate;

  // State
  const [data, setData] = React.useState<DataType>({
    name: "",
    unitId: 1,
    scope: ProductScope.InternalSale | ProductScope.PublicSale,
    usage: ProductUsage.FinishedProduct,
    price: {
      currency: defaultCurrency,
      retailPrice: 9999
    }
  });

  const [customFields, setCustomFields] = React.useState<CustomFieldData[]>([]);
  const attributesRef =
    React.useRef<CustomFieldRef<Record<string, unknown>>>(null);

  // Input refs
  const refFields = [
    "assetQty",
    "assignedId",
    "capQty",
    "channelPrice",
    "costPrice",
    "description",
    "introductionUrl",
    "minQty",
    "modifiers",
    "name",
    "queryKeyword",
    "promotionPrice",
    "retailPrice",
    "stepQty",
    "taxRate",
    "validity"
  ] as const;
  const refs = useRefs(refFields);

  // Formik
  const formik = useFormik<DataType>({
    initialValues: data,
    enableReinitialize: true,
    validateOnChange: false,
    onSubmit: async (v) => {
      // Get updated values
      const c = structuredClone(v);
      ReactUtils.updateRefValues(refs, c);

      if (c.price) {
        if (Utils.isEmptyObject(c.price)) {
          delete c.price;
        } else {
          c.price.currency = c.price.currency ?? defaultCurrency;
        }
      }

      if (c.isAsset && (c.assetQty == null || c.assetQty < 0)) {
        refs.assetQty.current?.focus();
        return;
      }

      if (!!c.modifiers && typeof c.modifiers === "string") {
        c.modifiers = JSON.parse(c.modifiers);
      }

      if (attributesRef.current) {
        const attributes = attributesRef.current.getValue();
        c.data ??= {};
        c.data.attributes = attributes;
      }

      // Submit
      let result: IdActionResult | undefined;
      let redirectUrl: string;
      if (id) {
        const rq: ProductUpdateRQ = {
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

        if (rq.modifiers == null) {
          delete rq.modifiers;
        }

        redirectUrl = "./../..";

        result = await app.productApi.update(rq);
      } else {
        const rq: ProductCreateRQ = {
          ...c
        };

        Utils.removeEmptyValues(rq);

        redirectUrl = "./..";

        result = await app.productApi.create(rq);
      }

      if (result == null) return;

      if (result.ok) {
        navigate(redirectUrl);
        return;
      }

      app.alertResult(result);
    }
  });

  const currency = formik.values.price?.currency ?? defaultCurrency;
  const symbol = NumberUtils.getCurrencySymbol(currency);

  // Load data
  const reloadData = React.useCallback(async () => {
    if (!id) return;

    const result = await app.productApi.updateRead(id);
    if (result == null) return;

    ReactUtils.updateRefs(refs, result);
    setData({ ...result, isAsset: result.assetQty != null });

    if (result.categories && result.categories.length > 0) {
      const attributes = await app.productCategoryApi.getAttributes(
        result.categories
      );
      if (attributes == null) return;
      setCustomFields(attributes);
    }
  }, [id]);

  // Page data hook
  usePageDataEmpty(app);

  return (
    <EditPage
      isEditing={isEditing}
      onDelete={
        app.owns(Permissions.Product.Delete) && id
          ? () => {
              app.notifier.confirm(
                labels.deleteConfirm.format(labels.product),
                undefined,
                async (ok) => {
                  if (!ok) return;

                  const result = await app.productApi.delete(id);
                  if (result == null) return;

                  if (result.ok) {
                    navigate(`./../../`);
                    return;
                  }

                  app.alertResult(result);
                }
              );
            }
          : undefined
      }
      onSubmit={formik.handleSubmit}
      onUpdate={reloadData}
      paddings={0}
    >
      <Grid size={{ xs: 12, sm: 12 }}>
        <ProductNameDuplicateTest
          fullWidth
          required
          excludedId={id}
          inputRef={refs.name}
        />
      </Grid>
      <Grid size={{ xs: 6, sm: 3 }}>
        <ProductUnitList
          fullWidth
          value={formik.values.unitId}
          required
          onItemChange={(item, userAction) => {
            if (!userAction) return;

            formik.setFieldValue("unitId", item?.id);

            let isAsset = false;
            if (item) {
              isAsset = BusinessUtils.isAssetUnit(item.baseUnit);
            }

            formik.setFieldValue("isAsset", isAsset);
            if (refs.assetQty.current && !isAsset)
              refs.assetQty.current.value = "";
          }}
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 9, xl: 6 }}>
        <ProductButtonScopes
          fullWidth
          value={formik.values.scope}
          onValueChange={(v) => formik.setFieldValue("scope", v)}
        />
      </Grid>
      <Grid size={{ xs: 6, sm: 3 }}>
        <ProductUsageList
          fullWidth
          value={formik.values.usage}
          onChange={formik.handleChange}
        />
      </Grid>
      <Grid size={{ xs: 6, sm: 3 }}>
        <NumberInputField
          fullWidth
          name="minQty"
          label={labels.minQty}
          max={9999}
          step={0.01}
          inputRef={refs.minQty}
        />
      </Grid>
      <Grid size={{ xs: 6, sm: 3 }}>
        <NumberInputField
          fullWidth
          name="stepQty"
          max={9999}
          step={0.01}
          label={labels.stepQty}
          inputRef={refs.stepQty}
        />
      </Grid>
      <Grid size={{ xs: 6, sm: 3 }}>
        <NumberInputField
          fullWidth
          name="capQty"
          min={1}
          max={99999999}
          label={labels.capQty}
          inputRef={refs.capQty}
        />
      </Grid>
      <Grid size={{ xs: 6, sm: 3 }}>
        <NumberInputField
          fullWidth
          name="assetQty"
          label={labels.assetQty}
          inputRef={refs.assetQty}
          disabled={!formik.values.isAsset}
        />
      </Grid>
      <Grid size={{ xs: 6, sm: 3 }}>
        <MoneyInputField
          fullWidth
          name="price.retailPrice"
          label={
            labels.retailPrice +
            ` (${app.core.publicApi.getCurrencyLabel(
              formik.values.price?.currency ?? defaultCurrency
            )})`
          }
          inputRef={refs.retailPrice}
          symbol={symbol}
        />
      </Grid>
      <Grid size={{ xs: 6, sm: 3 }}>
        <MoneyInputField
          fullWidth
          name="price.promotionPrice"
          label={labels.promotionPrice}
          inputRef={refs.promotionPrice}
          symbol={symbol}
        />
      </Grid>
      <Grid size={{ xs: 6, sm: 3 }}>
        <MoneyInputField
          fullWidth
          name="price.channelPrice"
          label={labels.channelPrice}
          inputRef={refs.channelPrice}
          symbol={symbol}
        />
      </Grid>
      <Grid size={{ xs: 6, sm: 3 }}>
        <MoneyInputField
          fullWidth
          name="price.costPrice"
          label={labels.costPrice}
          inputRef={refs.costPrice}
          symbol={symbol}
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 12 }}>
        <ButtonProductCategories
          fullWidth
          value={formik.values.categories ?? []}
          onValueChange={(ids) => {
            formik.setFieldValue("categories", ids);
            app.productCategoryApi.getAttributes(ids).then((result) => {
              setCustomFields(result ?? []);
            });
          }}
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
                kind: FeatureTagKind.Product,
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
            htmlInput: { maxLength: 2560 }
          }}
          label={labels.description}
          inputRef={refs.description}
          multiline
          rows={2}
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 12 }}>
        <CustomAttributeArea
          label={labels.modifiers}
          name="modifiers"
          inputRef={refs.modifiers}
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 12 }}>
        <InputField
          fullWidth
          name="introductionUrl"
          slotProps={{
            htmlInput: { maxLength: 256 }
          }}
          type="url"
          label={labels.introductionUrl}
          inputRef={refs.introductionUrl}
        />
      </Grid>
      <Grid size={{ xs: 6, sm: 3 }}>
        <ProductAssignedIdDuplicateTest
          fullWidth
          excludedId={id}
          inputRef={refs.assignedId}
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
        <InputField
          fullWidth
          name="queryKeyword"
          slotProps={{ htmlInput: { maxLength: 30 } }}
          label={labels.queryKeyword}
          inputRef={refs.queryKeyword}
        />
      </Grid>
      <Grid size={{ xs: 6, sm: 3 }}>
        <NumberInputField
          fullWidth
          name="validity"
          label={`${labels.validity} (${labels.days})`}
          inputRef={refs.validity}
        />
      </Grid>
      {defaultTaxRate && (
        <Grid size={{ xs: 6, sm: 3 }}>
          <NumberInputField
            fullWidth
            name="taxRate"
            max={99}
            step={0.01}
            label={labels.taxRate}
            inputRef={refs.taxRate}
            helperText={labels.defaultTaxRate + `: ${defaultTaxRate}`}
          />
        </Grid>
      )}
      {customFields.length > 0 && (
        <React.Fragment>
          <Grid size={{ xs: 12, sm: 12 }}>
            <Divider />
          </Grid>
          <CustomFieldUI
            fields={customFields}
            mref={attributesRef}
            initialValue={data.data?.attributes as Record<string, unknown>}
          />
        </React.Fragment>
      )}
    </EditPage>
  );
}
