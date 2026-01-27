import { EditPage, NumberInputField, OptionBool } from "@etsoo/materialui";
import { app } from "../../../app/MyApp";
import React from "react";
import { SystemSettings, UpdateSettingsRQ } from "@etsoo/smarterp-crm";
import { usePageDataEmpty } from "@etsoo/smarterp-core";
import Grid from "@mui/material/Grid";
import { CustomerTypeList } from "@etsoo/smarterp-crm/components";
import { useFormik } from "formik";
import {
  ButtonCultures,
  ButtonCurrencies
} from "@etsoo/smarterp-core/components";
import { Utils } from "@etsoo/shared";
import { useNavigate } from "react-router-dom";

export default function UpdateSettings() {
  // Route
  const navigate = useNavigate();

  // Labels
  const labels = app.getLabels(
    "currencies",
    "cultures",
    "defaultTaxRate",
    "edit",
    "hasInventory",
    "mainCustomerType",
    "noChanges",
    "supplier",
    "supplierCurrencies"
  );

  // State
  const [settings, setSettings] = React.useState<SystemSettings>();

  // Formik
  const formik = useFormik({
    initialValues: settings ?? ({} as SystemSettings),
    enableReinitialize: true,
    validateOnChange: false,
    onSubmit: async (values) => {
      // Request data
      const rq: UpdateSettingsRQ = { ...values };

      rq.hasInventory ??= false;

      // Correct for types safety
      Utils.correctTypes(rq, {
        hasInventory: "boolean"
      });

      if (settings) {
        const fields = Utils.getDataChanges(rq, settings);
        if (fields.length === 0) {
          app.warning(labels.noChanges);
          return;
        }
        rq.changedFields = fields;
      }

      const result = await app.systemApi.updateSettings(rq);
      if (result == null) return;

      if (result.ok) {
        // Refresh token to get the updated avatar
        await app.refreshToken({ showLoading: true });
        navigate("./..");
        return;
      }

      app.alertResult(result);
    }
  });

  // Load data
  const loadData = React.useCallback(async () => {
    const data = await app.systemApi.readSettings();
    if (data == null) return;
    setSettings(data);
  }, []);

  usePageDataEmpty(app);

  return (
    <EditPage
      onUpdate={loadData}
      onSubmit={(event) => {
        formik.handleSubmit(event);
      }}
    >
      <Grid size={{ xs: 12, sm: 6 }}>
        <CustomerTypeList
          name="mainCustomerType"
          label={labels.mainCustomerType}
          required
          fullWidth
          value={formik.values.mainCustomerType}
          onChange={formik.handleChange}
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 6 }}>
        <OptionBool
          name="hasInventory"
          label={labels.hasInventory}
          required
          fullWidth
          defaultValue={formik.values.hasInventory}
          onChange={formik.handleChange}
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 12 }}>
        <ButtonCurrencies
          fullWidth
          required
          value={formik.values.currencies}
          onValueChange={(ids) => formik.setFieldValue("currencies", ids)}
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 12 }}>
        <ButtonCurrencies
          label={labels.supplierCurrencies}
          fullWidth
          value={formik.values.supplierCurrencies}
          onValueChange={(ids) =>
            formik.setFieldValue("supplierCurrencies", ids)
          }
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 12 }}>
        <ButtonCultures
          fullWidth
          required
          value={formik.values.cultures}
          onValueChange={(ids) => formik.setFieldValue("cultures", ids)}
        />
      </Grid>
      <Grid size={{ xs: 6, sm: 3 }}>
        <NumberInputField
          fullWidth
          name="taxRate"
          max={99}
          step={0.01}
          label={labels.defaultTaxRate}
          value={formik.values.taxRate ?? ""}
          onChange={formik.handleChange}
        />
      </Grid>
    </EditPage>
  );
}
