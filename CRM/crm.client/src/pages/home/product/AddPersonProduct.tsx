import { usePageDataEmpty } from "@etsoo/smarterp-core";
import { app } from "../../../app/MyApp";
import { EditPage, InputField, MoneyInputField } from "@etsoo/materialui";
import { ReactUtils, useParamsEx, useRefs } from "@etsoo/react";
import { useFormik } from "formik";
import React from "react";
import Grid from "@mui/material/Grid";
import { IActionResult, NumberUtils, Utils } from "@etsoo/shared";
import {
  PersonProductCreateRQ,
  PersonProductJsonData,
  PersonProductUpdateRQ,
  ProductReadCustomData
} from "@etsoo/smarterp-crm";
import { useNavigate } from "react-router-dom";
import { PersonList, ProductList } from "@etsoo/smarterp-crm/components";
import { IdentityTypeFlags } from "@etsoo/appscript";

export default function AddPersonProduct() {
  // Route
  const navigate = useNavigate();
  const { productId = 0, personId = 0 } = useParamsEx({
    productId: "number",
    personId: "number"
  });

  const isEditing = productId > 0;

  // Labels
  const labels = app.getLabels(
    "assignedId",
    "deleteConfirm",
    "description",
    "item",
    "nameB",
    "noChanges",
    "noData",
    "relatedTarget",
    "retailPrice"
  );

  // Input refs
  const refFields = ["assignedId"] as const;
  const refs = useRefs(refFields);

  // Type
  type DataType = PersonProductCreateRQ;

  const currencies = app.userData?.system?.currencies ?? [app.currency];
  const cultures = app.userData?.system?.cultures ?? [
    app.settings.cultures[0].name
  ];

  const [productData, setProductData] = React.useState<ProductReadCustomData>();

  // State
  const [data, setData] = React.useState<DataType>({
    productId,
    personId
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

      const form = refs.assignedId.current?.form;
      if (!form) return;

      const jsonData: PersonProductJsonData = {};

      currencies.forEach((currency) => {
        const retailPrice = NumberUtils.parse(
          form[`retailPrice${currency}`]?.value
        );
        if (retailPrice == null || isNaN(retailPrice) || retailPrice < 0)
          return;

        jsonData.prices ??= [];
        jsonData.prices.push({
          currency,
          retailPrice
        });
      });

      cultures.forEach((culture) => {
        const name = form
          .querySelector<HTMLInputElement>(`[name="name${culture}"]`)
          ?.value.trim();
        const description = form
          .querySelector<HTMLInputElement>(`[name="description${culture}"]`)
          ?.value.trim();

        if (name) {
          jsonData.cultures ??= [];
          jsonData.cultures.push({
            culture,
            name,
            description: !description ? undefined : description
          });
        }
      });

      // Submit
      let result: IActionResult | undefined;
      let redirectUrl: string;
      if (productId && personId) {
        const rq: PersonProductUpdateRQ = {
          ...c,
          jsonData: jsonData.prices || jsonData.cultures ? jsonData : undefined,
          productId,
          personId
        };

        // Changed fields
        const fields = Utils.getDataChanges(rq, data, [
          "personId",
          "productId"
        ]);
        if (fields.length === 0) {
          app.warning(labels.noChanges);
          return;
        }
        rq.changedFields = fields;

        redirectUrl = "./../../..";

        result = await app.personProductApi.update(rq);
      } else {
        if (
          c.assignedId == null &&
          jsonData.prices == null &&
          jsonData.cultures == null
        ) {
          app.warning(labels.noData);
          return;
        }

        const rq: PersonProductCreateRQ = {
          ...c,
          jsonData
        };

        Utils.removeEmptyValues(rq);

        redirectUrl = "./..";

        result = await app.personProductApi.create(rq);
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
    if (!productId || !personId) return;
    const result = await app.personProductApi.updateRead(productId, personId);
    if (result == null) return;
    ReactUtils.updateRefs(refs, result);
    setData(result);
  }, [productId, personId]);

  // Page data hook
  usePageDataEmpty(app);

  const currentProductId = formik.values.productId;
  React.useEffect(() => {
    if (currentProductId) {
      app.productApi
        .readCustom(currentProductId, { showLoading: false })
        .then((result) => {
          if (result == null) return;
          setProductData(result);
        });
    } else {
      setProductData(undefined);
    }
  }, [currentProductId]);

  return (
    <EditPage
      isEditing={isEditing}
      onDelete={() => {
        app.notifier.confirm(
          labels.deleteConfirm.format(labels.item),
          undefined,
          async (ok) => {
            if (!ok) return;

            const result = await app.personProductApi.delete(
              productId,
              personId,
              {
                showLoading: false
              }
            );
            if (result == null) return;

            if (result.ok) {
              navigate("./../../..");
              return;
            }

            app.alertResult(result);
          }
        );
      }}
      onSubmit={formik.handleSubmit}
      onUpdate={reloadData}
      paddings={0}
    >
      <Grid size={{ xs: 12, sm: 6 }}>
        <ProductList
          inputRequired
          idValue={currentProductId}
          inputOnChange={formik.handleChange}
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 6 }}>
        <PersonList
          idValue={formik.values.personId}
          inputRequired
          inputOnChange={formik.handleChange}
          label={labels.relatedTarget}
          rq={{
            identityType:
              IdentityTypeFlags.Customer | IdentityTypeFlags.Supplier
          }}
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 6 }}>
        <InputField
          name="assignedId"
          slotProps={{ htmlInput: { maxLength: 20 } }}
          fullWidth
          label={labels.assignedId}
          inputRef={refs.assignedId}
        />
      </Grid>
      {currencies.map((currency) => (
        <Grid key={currency} size={{ xs: 6, sm: 3 }}>
          <MoneyInputField
            fullWidth
            name={`retailPrice${currency}`}
            label={
              labels.retailPrice +
              ` (${app.core.publicApi.getCurrencyLabel(currency)})`
            }
            defaultValue={
              data.jsonData?.prices?.find((p) => p.currency === currency)
                ?.retailPrice ?? ""
            }
            helperText={
              productData?.prices?.find((p) => p.currency === currency)
                ?.retailPrice
            }
            symbol={NumberUtils.getCurrencySymbol(currency)}
          />
        </Grid>
      ))}
      {cultures.map((culture) => {
        const item = data.jsonData?.cultures?.find(
          (c) => c.culture === culture
        );
        return (
          <React.Fragment key={culture}>
            <Grid size={{ xs: 12, sm: 12 }}>
              <InputField
                fullWidth
                name={`name${culture}`}
                slotProps={{ htmlInput: { maxLength: 256 } }}
                label={labels.nameB + ` (${culture})`}
                defaultValue={item?.name ?? ""}
                helperText={
                  productData?.cultures?.find((n) => n.culture === culture)
                    ?.name
                }
              />
            </Grid>
            <Grid size={{ xs: 12, sm: 12 }}>
              <InputField
                fullWidth
                name={`description${culture}`}
                slotProps={{ htmlInput: { maxLength: 2560 } }}
                multiline
                minRows={2}
                label={labels.description}
                defaultValue={item?.description ?? ""}
                helperText={
                  productData?.cultures?.find((n) => n.culture === culture)
                    ?.description
                }
              />
            </Grid>
          </React.Fragment>
        );
      })}
    </EditPage>
  );
}
