import { usePageDataEmpty } from "@etsoo/smarterp-core";
import { app } from "../../../app/MyApp";
import { EditPage, InputField, IntInputField } from "@etsoo/materialui";
import { ReactUtils, useRefs } from "@etsoo/react";
import { useFormik } from "formik";
import React from "react";
import Grid from "@mui/material/Grid";
import { StatusList } from "@etsoo/smarterp-core/components";
import {
  FinanceAccountCreateBulkRQ,
  FinanceAccountKind
} from "@etsoo/smarterp-crm";
import { useNavigate } from "react-router-dom";
import { EntityStatus } from "@etsoo/appscript";
import { CurrencyList } from "../../../components/CurrencyList";
import {
  AccountKindList,
  PersonList,
  ProductList
} from "@etsoo/smarterp-crm/components";
import { DataTypes, DomUtils } from "@etsoo/shared";
import Checkbox from "@mui/material/Checkbox";
import FormControlLabel from "@mui/material/FormControlLabel";

export default function BulkCreateAccounts() {
  // Route
  const navigate = useNavigate();

  // Labels
  const labels = app.getLabels(
    "account",
    "accountPrefix",
    "accountSample",
    "balance",
    "bank",
    "deleteConfirm",
    "description",
    "expiry",
    "idLength",
    "noChanges",
    "owner",
    "qty",
    "random",
    "startNumber",
    "status"
  );

  // Input refs
  const refFields = [
    "balance",
    "bank",
    "count",
    "description",
    "expiry",
    "length",
    "prefix",
    "startNumber"
  ] as const;
  const refs = useRefs(refFields);

  // Formik
  const formik = useFormik<FinanceAccountCreateBulkRQ>({
    initialValues: {
      kind: FinanceAccountKind.Prepaid,
      personId: app.userData?.system?.personId,
      bank: "",
      currency: app.currency,
      prefix: "",
      length: 6,
      startNumber: 1,
      count: 10,
      balance: 0
    },
    enableReinitialize: true,
    validateOnChange: false,
    onSubmit: async (v) => {
      const c = { ...v };
      ReactUtils.updateRefValues(refs, c);

      if (c.kind === FinanceAccountKind.Pass && !c.productId) {
        DomUtils.setFocus("productIdInput");
        return false;
      }

      if (c.bank == null || c.bank.trim() === "") {
        c.bank = (
          DataTypes.getEnumKey(FinanceAccountKind, c.kind) ?? "None"
        ).toUpperCase();
      }

      // Submit
      const result = await app.financeAccountApi.createBulk(c);
      if (result == null) return;

      if (result.ok) {
        navigate("./../accounts");
        return true;
      }

      app.alertResult(result);
    }
  });

  // Page data hook
  usePageDataEmpty(app);

  const [accountSample, setAccountSample] = React.useState("");

  const generateSample = () => {
    const prefix = refs.prefix.current?.value;
    if (!prefix) return;

    const startNumberText = refs.startNumber.current?.value ?? "1";
    if (!startNumberText) return;

    const lengthText = refs.length.current?.value ?? "";
    if (!lengthText) return;

    const startNumber = parseInt(startNumberText, 10);
    const length = parseInt(lengthText, 10);
    const sampleValue = `${prefix}${startNumber.toString().padStart(length, "0")}`;

    setAccountSample(sampleValue);
  };

  return (
    <EditPage onSubmit={formik.handleSubmit} paddings={0}>
      <Grid size={{ xs: 6, sm: 3 }}>
        <AccountKindList
          value={formik.values.kind}
          fullWidth
          required
          onChange={formik.handleChange}
        />
      </Grid>
      <Grid size={{ xs: 6, sm: 3 }}>
        <CurrencyList
          value={formik.values.currency}
          fullWidth
          required
          onChange={formik.handleChange}
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 6 }}>
        <PersonList
          label={labels.owner}
          inputRequired
          idValue={formik.values.personId}
          inputOnChange={formik.handleChange}
        />
      </Grid>
      <Grid size={{ xs: 6, sm: 3 }}>
        <InputField
          fullWidth
          required
          name="prefix"
          slotProps={{
            htmlInput: { maxLength: 20, style: { textTransform: "uppercase" } }
          }}
          label={labels.accountPrefix}
          inputRef={refs.prefix}
          onChange={generateSample}
        />
      </Grid>
      <Grid size={{ xs: 6, sm: 3 }}>
        <IntInputField
          fullWidth
          required
          name="startNumber"
          inputRef={refs.startNumber}
          label={labels.startNumber}
          min={1}
          max={99999999}
          onChange={generateSample}
        />
        <FormControlLabel
          control={<Checkbox name="random" size="small" />}
          label={labels.random}
          onChange={(_, checked) => {
            if (refs.startNumber.current != null) {
              refs.startNumber.current.value = checked ? "0" : "";
              refs.startNumber.current.disabled = checked;
            }

            if (refs.length.current != null) {
              refs.length.current.value = checked ? "40" : "";
              refs.length.current.disabled = checked;
            }
          }}
        />
      </Grid>

      <Grid size={{ xs: 6, sm: 3 }}>
        <IntInputField
          fullWidth
          required
          name="count"
          inputRef={refs.count}
          min={2}
          max={5000}
          label={labels.qty}
        />
      </Grid>
      <Grid size={{ xs: 6, sm: 3 }}>
        <IntInputField
          fullWidth
          required
          name="length"
          inputRef={refs.length}
          min={4}
          max={40}
          label={labels.idLength}
          onChange={generateSample}
        />
      </Grid>
      <Grid size={{ xs: 6, sm: 3 }}>
        <InputField
          fullWidth
          label={labels.accountSample}
          value={accountSample}
          disabled
        />
      </Grid>
      <Grid size={{ xs: 6, sm: 3 }}>
        <InputField
          fullWidth
          required
          name="balance"
          type="number"
          label={labels.balance}
          inputRef={refs.balance}
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 6 }}>
        <InputField
          fullWidth
          name="bank"
          slotProps={{ htmlInput: { maxLength: 128 } }}
          label={labels.bank}
          inputRef={refs.bank}
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 12 }}>
        <InputField
          fullWidth
          name="description"
          slotProps={{
            htmlInput: { maxLength: 128 }
          }}
          label={labels.description}
          inputRef={refs.description}
          multiline
          rows={2}
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 6 }}>
        <ProductList
          idValue={formik.values.productId}
          inputOnChange={formik.handleChange}
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
          name="expiry"
          type="datetime-local"
          label={labels.expiry}
          inputRef={refs.expiry}
        />
      </Grid>
    </EditPage>
  );
}
