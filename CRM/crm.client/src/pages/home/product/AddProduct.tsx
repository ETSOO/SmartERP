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
import { IdActionResult, Utils } from "@etsoo/shared";
import {
  FeatureTagKind,
  ProductCreateRQ,
  ProductInventoryWay,
  ProductScope,
  ProductUpdateRQ,
  ProductUsage
} from "@etsoo/smarterp-crm";
import { useNavigate } from "react-router-dom";
import { BusinessUtils, EntityStatus } from "@etsoo/appscript";
import {
  ButtonProductCategories,
  ProductAssignedIdDuplicateTest,
  ProductInventoryWayList,
  ProductNameDuplicateTest,
  ProductScopeList,
  ProductUnitList,
  ProductUsageList
} from "@etsoo/smarterp-crm/components";

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
    "description",
    "minQty",
    "noChanges",
    "productName",
    "promotionPrice",
    "queryKeyword",
    "retailPrice",
    "status",
    "stepQty",
    "tags"
  );

  // Type
  type DataType = ProductCreateRQ & { isAsset?: boolean };

  const defaultInventoryWay = app.userData?.system?.hasInventory
    ? ProductInventoryWay.Simple
    : ProductInventoryWay.None;

  const defaultCurrency = app.userData?.system?.currencies[0] ?? "CNY";

  // State
  const [data, setData] = React.useState<DataType>({
    name: "",
    unitId: 1,
    scope: ProductScope.Public,
    usage: ProductUsage.FinishedProduct,
    inventoryWay: defaultInventoryWay,
    price: {
      currency: defaultCurrency,
      retailPrice: 9999
    }
  });

  // Input refs
  const refFields = [
    "assetQty",
    "assignedId",
    "capQty",
    "channelPrice",
    "costPrice",
    "description",
    "minQty",
    "name",
    "queryKeyword",
    "promotionPrice",
    "retailPrice",
    "stepQty"
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

      if (c.isAsset && (c.assetQty == null || c.assetQty < 0)) {
        refs.assetQty.current?.focus();
        return;
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

  // Load data
  const reloadData = React.useCallback(async () => {
    if (!id) return;

    const result = await app.productApi.updateRead(id);
    if (result == null) return;

    ReactUtils.updateRefs(refs, result);
    setData({ ...result, isAsset: result.assetQty != null });
  }, [id]);

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

              formik.setFieldValue(
                "inventoryWay",
                isAsset ? ProductInventoryWay.None : defaultInventoryWay
              );
            }

            formik.setFieldValue("isAsset", isAsset);
            if (refs.assetQty.current && !isAsset)
              refs.assetQty.current.value = "";
          }}
        />
      </Grid>
      <Grid size={{ xs: 6, sm: 3 }}>
        <ProductScopeList
          fullWidth
          value={formik.values.scope}
          onChange={formik.handleChange}
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
        <ProductInventoryWayList
          fullWidth
          value={formik.values.inventoryWay}
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
        />
      </Grid>
      <Grid size={{ xs: 6, sm: 3 }}>
        <MoneyInputField
          fullWidth
          name="price.promotionPrice"
          label={labels.promotionPrice}
          inputRef={refs.promotionPrice}
        />
      </Grid>
      <Grid size={{ xs: 6, sm: 3 }}>
        <MoneyInputField
          fullWidth
          name="price.channelPrice"
          label={labels.channelPrice}
          inputRef={refs.channelPrice}
        />
      </Grid>
      <Grid size={{ xs: 6, sm: 3 }}>
        <MoneyInputField
          fullWidth
          name="price.costPrice"
          label={labels.costPrice}
          inputRef={refs.costPrice}
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 12 }}>
        <ButtonProductCategories
          fullWidth
          value={formik.values.categories ?? []}
          onValueChange={(ids) => formik.setFieldValue("categories", ids)}
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
      <Grid size={{ xs: 6, sm: 3 }}>
        <ProductAssignedIdDuplicateTest fullWidth inputRef={refs.assignedId} />
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
    </EditPage>
  );
}
