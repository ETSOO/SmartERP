import { usePageDataEmpty } from "@etsoo/smarterp-core";
import { app } from "../../../app/MyApp";
import { EditPage, InputField } from "@etsoo/materialui";
import {
  ReactUtils,
  useParamsEx,
  useRefs,
  useSearchParamsEx
} from "@etsoo/react";
import { useFormik } from "formik";
import React from "react";
import Grid from "@mui/material/Grid";
import { StatusList } from "@etsoo/smarterp-core/components";
import { IdActionResult, Utils } from "@etsoo/shared";
import {
  FinanceAccountCreateRQ,
  FinanceAccountKind,
  FinanceAccountUpdateRQ
} from "@etsoo/smarterp-crm";
import { useNavigate } from "react-router-dom";
import { EntityStatus } from "@etsoo/appscript";
import { CurrencyList } from "../../../components/CurrencyList";
import {
  AccountKindList,
  PersonList,
  ProductList
} from "@etsoo/smarterp-crm/components";

export default function AddAccount() {
  // Route
  const navigate = useNavigate();

  const { id = 0 } = useParamsEx({
    id: "number"
  });

  const { personId = 0 } = useSearchParamsEx({ personId: "number" });

  const isEditing = id > 0;

  // Labels
  const labels = app.getLabels(
    "accountNumber",
    "bank",
    "description",
    "expiry",
    "noChanges",
    "owner",
    "status",
    "swift"
  );

  // Input refs
  const refFields = [
    "accountNumber",
    "bank",
    "description",
    "expiry",
    "swift"
  ] as const;
  const refs = useRefs(refFields);

  // Type
  type DataType = FinanceAccountCreateRQ;

  // State
  const [data, setData] = React.useState<DataType>({
    personId,
    currency: app.currency,
    kind: FinanceAccountKind.None,
    bank: "",
    accountNumber: ""
  });

  // Formik
  const formik = useFormik<DataType>({
    initialValues: data,
    enableReinitialize: true,
    validateOnChange: false,
    onSubmit: async (v) => {
      ReactUtils.updateRefValues(refs, v);

      // Submit
      let result: IdActionResult | undefined;
      let redirectUrl: string;
      if (id > 0) {
        const rq: FinanceAccountUpdateRQ = {
          ...v,
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

        result = await app.financeAccountApi.update(rq);
      } else {
        const rq: FinanceAccountCreateRQ = {
          ...v
        };

        Utils.removeEmptyValues(rq);

        redirectUrl = "./..";

        result = await app.financeAccountApi.create(rq);
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
    if (id < 1) return;
    const result = await app.financeAccountApi.updateRead(id);
    if (result == null) return;
    ReactUtils.updateRefs(refs, result);
    setData(result);
  }, [id]);

  // Page data hook
  usePageDataEmpty(app);

  return (
    <EditPage
      isEditing={isEditing}
      onSubmit={formik.handleSubmit}
      onUpdate={reloadData}
      paddings={0}
    >
      <Grid size={{ xs: 6, sm: 3 }}>
        <AccountKindList value={formik.values.kind} fullWidth required />
      </Grid>
      <Grid size={{ xs: 6, sm: 3 }}>
        <CurrencyList value={formik.values.currency} fullWidth required />
      </Grid>
      <Grid size={{ xs: 12, sm: 6 }}>
        <PersonList
          label={labels.owner}
          inputRequired
          idValue={formik.values.personId}
          inputOnChange={formik.handleChange}
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 6 }}>
        <InputField
          fullWidth
          required
          name="bank"
          slotProps={{ htmlInput: { maxLength: 128 } }}
          label={labels.bank}
          inputRef={refs.bank}
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 6 }}>
        <InputField
          fullWidth
          required
          name="accountNumber"
          slotProps={{ htmlInput: { maxLength: 20 } }}
          label={labels.accountNumber}
          inputRef={refs.accountNumber}
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
      <Grid size={{ xs: 12, sm: 12 }}>
        <InputField
          fullWidth
          name="swift"
          slotProps={{
            htmlInput: { maxLength: 256 }
          }}
          label={labels.swift}
          inputRef={refs.swift}
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
