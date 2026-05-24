import { EditPage, InputField, MaskInput } from "@etsoo/materialui";
import React from "react";
import { useFormik } from "formik";
import * as Yup from "yup";
import { Utils } from "@etsoo/shared";
import { BusinessTax, EntityStatus } from "@etsoo/appscript";
import { useNavigate } from "react-router-dom";
import { useParamsEx } from "@etsoo/react";
import { app } from "../../../app/MyApp";
import {
  OrgUpdateReadDto,
  OrgUpdateRQ,
  usePageDataEmpty
} from "@etsoo/smarterp-core";
import { OrgTiplist, StatusList } from "@etsoo/smarterp-core/components";
import Grid from "@mui/material/Grid";

export default function EditOrg() {
  // Route
  const navigate = useNavigate();
  const { id = 0 } = useParamsEx({ id: "number" });

  // Permissions
  const deletePermission = app.isAdminUser();

  // Labels
  const labels = app.getLabels(
    "brand",
    "deleteConfirm",
    "noChanges",
    "orgName",
    "parentOrg",
    "status"
  );

  // Form validation schema
  const validationSchema = Yup.object({
    name: Yup.string().required()
  });

  // Edit data
  const [data, setData] = React.useState<OrgUpdateReadDto>({
    id,
    name: "",
    status: EntityStatus.Normal
  });

  // Formik
  // https://formik.org/docs/examples/with-material-ui
  // https://firxworx.com/blog/coding/react/integrating-formik-with-react-material-ui-and-typescript/
  const formik = useFormik<OrgUpdateRQ>({
    initialValues: data,
    enableReinitialize: true,
    validationSchema: validationSchema,
    onSubmit: async (values) => {
      // Request data
      const rq = { ...values };

      // Format identifier
      rq.pin = Utils.removeNonLetters(rq.pin);

      // Changed fields
      const fields = Utils.getDataChanges(rq, data);
      if (fields.length === 0) {
        app.warning(labels.noChanges);
        return;
      }
      rq.changedFields = fields;

      // Submit
      const result = await app.core.orgApi.update(rq);
      if (result == null) return;

      if (result.ok) {
        navigate(`./../../my/${id}`);
        return;
      }

      app.alertResult(result);
    }
  });

  // Load data
  const reloadData = React.useCallback(async () => {
    const data = await app.core.orgApi.updateRead(id);
    if (data == null) return;
    setData(data);
  }, [id]);

  // Tax
  const tax = BusinessTax.getById(app.region);

  // Page data hook
  usePageDataEmpty(app);

  return (
    <EditPage
      isEditing
      onSubmit={formik.handleSubmit}
      onUpdate={reloadData}
      onDelete={
        data.status === EntityStatus.Deleted && deletePermission
          ? () => {
              app.notifier.confirm(
                labels.deleteConfirm.format(` [${data.name}] `),
                undefined,
                async (ok) => {
                  const id = formik.values.id;
                  if (!ok || id == null) return;

                  const result = await app.core.orgApi.delete(id, {
                    showLoading: false
                  });
                  if (result == null) return;

                  if (result.ok) {
                    navigate("./../../my");
                    return;
                  }

                  app.alertResult(result);
                }
              );
            }
          : undefined
      }
    >
      <Grid size={{ xs: 12, sm: 6 }}>
        <InputField
          fullWidth
          required
          name="name"
          slotProps={{ htmlInput: { maxLength: 128 } }}
          label={labels.orgName}
          value={formik.values.name ?? ""}
          onChange={formik.handleChange}
          error={formik.touched.name && Boolean(formik.errors.name)}
          helperText={formik.touched.name && formik.errors.name}
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 6 }}>
        <InputField
          fullWidth
          name="brand"
          slotProps={{ htmlInput: { maxLength: 30 } }}
          label={labels.brand}
          value={formik.values.brand ?? ""}
          onChange={formik.handleChange}
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 6 }}>
        <MaskInput
          mask={{ mask: tax?.mask ?? "" }}
          name="pin"
          label={app.get(tax?.labelKey ?? "taxId")}
          fullWidth
          slotProps={{
            htmlInput: { maxLength: 20, style: { textTransform: "uppercase" } }
          }}
          value={formik.values.pin ?? ""}
          onChange={formik.handleChange}
          error={formik.touched.pin && Boolean(formik.errors.pin)}
          helperText={formik.touched.pin && formik.errors.pin}
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 6 }}>
        <OrgTiplist
          label={labels.parentOrg}
          name="parentId"
          idValue={formik.values.parentId}
          inputOnChange={formik.handleChange}
          inputError={
            formik.touched.parentId && Boolean(formik.errors.parentId)
          }
          inputHelperText={formik.touched.parentId && formik.errors.parentId}
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
    </EditPage>
  );
}
