import {
  CustomFieldUI,
  EditPage,
  InputField,
  MoneyInputField,
  NumberInputField
} from "@etsoo/materialui";
import React from "react";
import { useFormik } from "formik";
import { NumberUtils, Utils } from "@etsoo/shared";
import { useNavigate } from "react-router-dom";
import { ReactUtils, useParamsEx, useRefs } from "@etsoo/react";
import { app } from "../../../app/MyApp";
import { usePageDataEmpty } from "@etsoo/smarterp-core";
import { StatusList } from "@etsoo/smarterp-core/components";
import Grid from "@mui/material/Grid";
import { POLineUpdateRQ } from "@etsoo/smarterp-crm";
import { SupplierList, UserTiplist } from "@etsoo/smarterp-crm/components";
import { CustomFieldData, CustomFieldRef } from "@etsoo/appscript";
import Divider from "@mui/material/Divider";

type POData = {
  id: number;
  symbol?: string;
  isDeletable: boolean;
};

export default function EditPOLine() {
  // Route
  const navigate = useNavigate();
  const { id = 0 } = useParamsEx({ id: "number" });

  // Labels
  const labels = app.getLabels(
    "costPrice",
    "deleteConfirm",
    "description",
    "endTime",
    "noChanges",
    "poLine",
    "poLineStartTime",
    "originalPrice",
    "price",
    "qty",
    "status",
    "title"
  );

  const modifiersRef =
    React.useRef<CustomFieldRef<Record<string, unknown>>>(null);

  // Edit data
  const [data, setData] = React.useState<POLineUpdateRQ>({ id });
  const [po, setPO] = React.useState<POData>();
  const [customFields, setCustomFields] = React.useState<CustomFieldData[]>([]);

  // Formik
  // https://formik.org/docs/examples/with-material-ui
  // https://firxworx.com/blog/coding/react/integrating-formik-with-react-material-ui-and-typescript/
  const formik = useFormik<POLineUpdateRQ>({
    initialValues: data,
    enableReinitialize: true,
    onSubmit: async (values) => {
      // Request data
      const rq: POLineUpdateRQ = structuredClone(values);

      ReactUtils.updateRefValues(refs, rq);

      if (modifiersRef.current) {
        const modifiers = modifiersRef.current.getValue();
        rq.data ??= {};
        rq.data.modifiers = modifiers;
      }

      // Changed fields
      const fields = Utils.getDataChanges(rq, data);

      if (fields.length === 0) {
        app.warning(labels.noChanges);
        return;
      }
      rq.changedFields = fields;

      // Submit
      const result = await app.poLineApi.update(rq);
      if (result == null) return;

      if (result.ok) {
        navigate(`./../../viewline/${id}`);
        return;
      }

      app.alertResult(result);
    }
  });

  // Load data
  const reloadData = React.useCallback(async () => {
    const result = await app.poLineApi.updateRead(id);
    if (result == null) return;

    ReactUtils.updateRefs(refs, result);

    if (result.modifiers) {
      setCustomFields(result.modifiers);
    }

    setData(result);
    setPO({
      id: result.poId,
      symbol: NumberUtils.getCurrencySymbol(result.currency),
      isDeletable: result.isDeletable
    });
  }, [id]);

  // Input refs
  const refFields = [
    "costPrice",
    "description",
    "endTime",
    "originalPrice",
    "price",
    "qty",
    "startTime",
    "title"
  ] as const;
  const refs = useRefs(refFields);

  // Page data hook
  usePageDataEmpty(app);

  return (
    <EditPage
      isEditing
      onSubmit={(event) => {
        formik.handleSubmit(event);
      }}
      onUpdate={reloadData}
      onDelete={
        po?.isDeletable
          ? () => {
              app.notifier.confirm(
                labels.deleteConfirm.format(labels.poLine),
                undefined,
                async (ok) => {
                  if (!ok) return;

                  const result = await app.poLineApi.delete(id);
                  if (result == null) return;

                  if (result.ok) {
                    navigate(`./../../view/${po.id}`);
                    return;
                  }

                  app.alertResult(result);
                }
              );
            }
          : undefined
      }
      paddings={0}
    >
      <Grid size={{ xs: 12, sm: 12 }}>
        <InputField
          fullWidth
          name="title"
          slotProps={{
            htmlInput: { maxLength: 128 }
          }}
          label={labels.title}
          inputRef={refs.title}
        />
      </Grid>
      <Grid size={{ xs: 6, sm: 3 }}>
        <MoneyInputField
          fullWidth
          name="price"
          label={labels.price}
          inputRef={refs.price}
          symbol={po?.symbol}
        />
      </Grid>
      <Grid size={{ xs: 6, sm: 3 }}>
        <NumberInputField
          fullWidth
          name="qty"
          label={labels.qty}
          inputRef={refs.qty}
        />
      </Grid>
      <Grid size={{ xs: 6, sm: 3 }}>
        <MoneyInputField
          fullWidth
          name="originalPrice"
          label={labels.originalPrice}
          inputRef={refs.originalPrice}
          symbol={po?.symbol}
        />
      </Grid>
      <Grid size={{ xs: 6, sm: 3 }}>
        <MoneyInputField
          fullWidth
          name="costPrice"
          label={labels.costPrice}
          inputRef={refs.costPrice}
          symbol={po?.symbol}
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
        <UserTiplist
          idValue={data.userId}
          inputOnChange={formik.handleChange}
        />
      </Grid>
      <Grid size={{ xs: 6, sm: 3 }}>
        <InputField
          fullWidth
          name="startTime"
          type="datetime-local"
          label={labels.poLineStartTime}
          inputRef={refs.startTime}
        />
      </Grid>
      <Grid size={{ xs: 6, sm: 3 }}>
        <InputField
          fullWidth
          name="endTime"
          type="datetime-local"
          label={labels.endTime}
          inputRef={refs.endTime}
        />
      </Grid>
      <Grid size={{ xs: 6, sm: 3 }}>
        <StatusList
          fullWidth
          inputRequired
          idValue={formik.values.status}
          inputOnChange={formik.handleChange}
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 6 }}>
        <SupplierList
          idValue={data.supplierId}
          inputOnChange={formik.handleChange}
        />
      </Grid>
      {customFields.length > 0 && (
        <React.Fragment>
          <Grid size={{ xs: 12, sm: 12 }}>
            <Divider />
          </Grid>
          <CustomFieldUI
            fields={customFields}
            mref={modifiersRef}
            initialValue={data.data?.modifiers as Record<string, unknown>}
          />
        </React.Fragment>
      )}
    </EditPage>
  );
}
