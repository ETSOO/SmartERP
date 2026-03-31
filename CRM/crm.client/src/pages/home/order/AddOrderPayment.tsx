import { usePageDataEmpty } from "@etsoo/smarterp-core";
import { app } from "../../../app/MyApp";
import { EditPage, InputField, OptionBool } from "@etsoo/materialui";
import { ReactUtils, useParamsEx, useRefs } from "@etsoo/react";
import { useFormik } from "formik";
import React from "react";
import Grid from "@mui/material/Grid";
import { IdActionResult, Utils } from "@etsoo/shared";
import { useNavigate } from "react-router-dom";
import { OrderPaymentKindList } from "@etsoo/smarterp-crm/components";
import {
  OrderPaymentCreateRQ,
  OrderPaymentKind,
  OrderPaymentUpdateRQ
} from "@etsoo/smarterp-crm";

export default function AddOrderPayment() {
  const navigate = useNavigate();
  const { id } = useParamsEx({ id: "number" });
  const isEditing = (id ?? 0) > 0;

  const labels = app.getLabels("enabled", "noChanges", "title");

  const refFields = ["title"] as const;
  const refs = useRefs(refFields);

  type DataType = OrderPaymentCreateRQ;

  const [data, setData] = React.useState<DataType>({
    kind: 0 as OrderPaymentKind,
    title: "",
    isValid: true
  });

  const formik = useFormik<DataType>({
    initialValues: data,
    enableReinitialize: true,
    validateOnChange: false,
    onSubmit: async (v) => {
      const c = { ...v };
      ReactUtils.updateRefValues(refs, c);
      Utils.correctTypes(c, { isValid: "boolean" });

      let result: IdActionResult | undefined;
      let redirectUrl: string;

      if (id) {
        const rq: OrderPaymentUpdateRQ = { ...c, id };
        const fields = Utils.getDataChanges(rq, data);
        if (fields.length === 0) {
          app.warning(labels.noChanges);
          return;
        }
        rq.changedFields = fields;
        redirectUrl = "./../..";
        result = await app.orderPaymentApi.update(rq);
      } else {
        const rq: OrderPaymentCreateRQ = { ...c };
        Utils.removeEmptyValues(rq);
        redirectUrl = "./..";
        result = await app.orderPaymentApi.create(rq);
      }

      if (result == null) return;
      if (result.ok) {
        navigate(redirectUrl);
        return;
      }
      app.alertResult(result);
    }
  });

  const reloadData = React.useCallback(async () => {
    if (!id) return;
    const result = await app.orderPaymentApi.updateRead(id);
    if (result == null) return;
    ReactUtils.updateRefs(refs, result);
    setData(result);
  }, [id]);

  usePageDataEmpty(app);

  return (
    <EditPage
      isEditing={isEditing}
      onSubmit={(event) => formik.handleSubmit(event)}
      onUpdate={reloadData}
      paddings={0}
    >
      <Grid size={{ xs: 12, sm: 6 }}>
        <OrderPaymentKindList
          required
          onItemChange={(item, userAction) => {
            if (refs.title.current && !isEditing && item != null)
              refs.title.current.value = item.label;

            if (userAction) formik.setFieldValue("kind", item?.id);
          }}
          value={data.kind}
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 6 }}>
        <OptionBool
          name="isValid"
          label={labels.enabled}
          variant="outlined"
          defaultValue={formik.values.isValid}
          fullWidth
          onChange={formik.handleChange}
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
    </EditPage>
  );
}
