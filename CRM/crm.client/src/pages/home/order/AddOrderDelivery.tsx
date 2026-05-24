import { usePageDataEmpty } from "@etsoo/smarterp-core";
import { app } from "../../../app/MyApp";
import { EditPage, InputField, OptionBool } from "@etsoo/materialui";
import { ReactUtils, useParamsEx, useRefs } from "@etsoo/react";
import { useFormik } from "formik";
import React from "react";
import Grid from "@mui/material/Grid";
import { IdActionResult, Utils } from "@etsoo/shared";
import { useNavigate } from "react-router-dom";
import {
  OrderDeliveryCreateRQ,
  OrderDeliveryKind,
  OrderDeliveryUpdateRQ
} from "@etsoo/smarterp-crm";
import { OrderDeliveryKindList } from "@etsoo/smarterp-crm/components";
import { useIsOrder } from "./useIsOrder";

export default function AddOrderDelivery() {
  const navigate = useNavigate();
  const { id } = useParamsEx({ id: "number" });
  const isEditing = (id ?? 0) > 0;

  const isOrder = useIsOrder();

  const labels = app.getLabels("enabled", "noChanges", "title");

  const refFields = ["title"] as const;
  const refs = useRefs(refFields);

  type DataType = OrderDeliveryCreateRQ;

  const [data, setData] = React.useState<DataType>({
    kind: 0 as OrderDeliveryKind,
    title: "",
    isValid: true,
    isOrder
  });

  const formik = useFormik<DataType>({
    initialValues: data,
    enableReinitialize: true,
    validateOnChange: false,
    onSubmit: async (values) => {
      const current = { ...values };
      ReactUtils.updateRefValues(refs, current);
      Utils.correctTypes(current, { isValid: "boolean" });

      let result: IdActionResult | undefined;
      let redirectUrl: string;

      if (id) {
        const rq: OrderDeliveryUpdateRQ = { ...current, id };
        const fields = Utils.getDataChanges(rq, data);

        if (fields.length === 0) {
          app.warning(labels.noChanges);
          return;
        }

        rq.changedFields = fields;
        redirectUrl = "./../..";
        result = await app.orderDeliveryApi.update(rq);
      } else {
        const rq: OrderDeliveryCreateRQ = { ...current };
        Utils.removeEmptyValues(rq);
        redirectUrl = "./..";
        result = await app.orderDeliveryApi.create(rq);
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
    const result = await app.orderDeliveryApi.updateRead(id);
    if (result == null) return;
    ReactUtils.updateRefs(refs, result);
    setData(result);
  }, [id]);

  usePageDataEmpty(app);

  return (
    <EditPage
      isEditing={isEditing}
      onSubmit={formik.handleSubmit}
      onUpdate={reloadData}
      paddings={0}
    >
      <Grid size={{ xs: 12, sm: 6 }}>
        <OrderDeliveryKindList
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
