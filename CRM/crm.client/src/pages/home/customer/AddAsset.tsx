import { usePageDataEmpty } from "@etsoo/smarterp-core";
import { app } from "../../../app/MyApp";
import { EditPage, InputField, IntInputField } from "@etsoo/materialui";
import {
  ReactUtils,
  useParamsEx,
  useRefs,
  useSearchParamsEx
} from "@etsoo/react";
import { useFormik } from "formik";
import React from "react";
import Grid from "@mui/material/Grid";
import { IdActionResult, Utils } from "@etsoo/shared";
import { AssetCreateRQ, AssetUpdateRQ, Permissions } from "@etsoo/smarterp-crm";
import { useNavigate } from "react-router-dom";
import { StatusList } from "@etsoo/smarterp-core/components";
import { EntityStatus } from "@etsoo/appscript";
import {
  PersonList,
  ProductList,
  SupplierList
} from "@etsoo/smarterp-crm/components";
import FormControlLabel from "@mui/material/FormControlLabel";
import Checkbox from "@mui/material/Checkbox";

export default function AddAsset() {
  // Route
  const navigate = useNavigate();
  const { id } = useParamsEx({
    id: "number"
  });

  const { personId = -1 } = useSearchParamsEx({
    personId: "number"
  });

  const isEditing = (id ?? 0) > 0;

  // Labels
  const labels = app.getLabels(
    "asset",
    "deleteConfirm",
    "description",
    "expiry",
    "expiryCheck",
    "healthCheckUrl",
    "intervalMinutes",
    "noChanges",
    "noticeOwner",
    "owner",
    "sensitiveData",
    "sn"
  );

  // Input refs
  const refFields = [
    "description",
    "expiry",
    "expiryCheck",
    "healthCheckUrl",
    "intervalMinutes",
    "sensitiveData",
    "sn"
  ] as const;
  const refs = useRefs(refFields);

  // Type
  type DataType = AssetCreateRQ;

  // Setting
  const { expiryCheck, noticeOwner } = app.system.getAssetExpirationNotice();

  // State
  const [data, setData] = React.useState<DataType>({
    personId,
    productId: 0,
    sn: "",
    expiry: "",
    expiryCheck,
    data: {
      noticeOwner
    }
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
        const rq: AssetUpdateRQ = {
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

        result = await app.assetApi.update(rq);
      } else {
        const rq: AssetCreateRQ = {
          ...c
        };

        Utils.removeEmptyValues(rq);

        redirectUrl = "./..";

        result = await app.assetApi.create(rq);
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
    const result = await app.assetApi.updateRead(id);
    if (result == null) return;
    ReactUtils.updateRefs(refs, result);
    setData(result);
  }, [id]);

  // Page data hook
  usePageDataEmpty(app);

  return (
    <EditPage
      isEditing={isEditing}
      onDelete={
        app.owns(Permissions.Org.Manage) &&
        id &&
        data.status === EntityStatus.Deleted
          ? () => {
              app.notifier.confirm(
                labels.deleteConfirm.format(labels.asset),
                undefined,
                async (ok) => {
                  if (!ok) return;

                  const result = await app.assetApi.delete(id);
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
      <Grid size={{ xs: 12, sm: 6 }}>
        <PersonList
          label={labels.owner}
          inputRequired
          idValue={formik.values.personId}
          inputOnChange={formik.handleChange}
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 6 }}>
        <ProductList
          inputRequired
          idValue={formik.values.productId}
          inputOnChange={formik.handleChange}
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 6 }}>
        <SupplierList
          idValue={formik.values.supplierId}
          inputOnChange={formik.handleChange}
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 12 }}>
        <InputField
          fullWidth
          required
          name="sn"
          slotProps={{ htmlInput: { maxLength: 256 } }}
          label={labels.sn}
          inputRef={refs.sn}
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
          required
          name="expiry"
          type="datetime-local"
          label={labels.expiry}
          inputRef={refs.expiry}
        />
      </Grid>
      <Grid size={{ xs: 6, sm: 3 }}>
        <FormControlLabel
          control={
            <Checkbox
              checked={formik.values.expiryCheck ?? false}
              onChange={(e) =>
                formik.setFieldValue("expiryCheck", e.target.checked)
              }
            />
          }
          label={labels.expiryCheck}
        />
      </Grid>
      <Grid size={{ xs: 6, sm: 3 }}>
        <FormControlLabel
          control={
            <Checkbox
              checked={formik.values.data?.noticeOwner ?? false}
              onChange={(e) =>
                formik.setFieldValue("data.noticeOwner", e.target.checked)
              }
            />
          }
          label={labels.noticeOwner}
        />
      </Grid>
      <Grid size={{ xs: 6, sm: 3 }}>
        <StatusList
          inputRequired
          idValue={formik.values.status ?? EntityStatus.Normal}
          inputOnChange={formik.handleChange}
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 12 }}>
        <InputField
          fullWidth
          name="sensitiveData"
          slotProps={{ htmlInput: { maxLength: 128 } }}
          type="password"
          label={labels.sensitiveData}
          inputRef={refs.sensitiveData}
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 12 }}>
        <InputField
          fullWidth
          name="healthCheckUrl"
          slotProps={{ htmlInput: { maxLength: 1280, type: "url" } }}
          label={labels.healthCheckUrl}
          inputRef={refs.healthCheckUrl}
        />
      </Grid>
      <Grid size={{ xs: 6, sm: 3 }}>
        <IntInputField
          fullWidth
          name="data.intervalMinutes"
          label={labels.intervalMinutes}
          inputRef={refs.intervalMinutes}
        />
      </Grid>
    </EditPage>
  );
}
