import { ComboBox, EditPage, InputField } from "@etsoo/materialui";
import React from "react";
import { useFormik } from "formik";
import * as Yup from "yup";
import { DateUtils, Utils } from "@etsoo/shared";
import { EntityStatus, UserRole } from "@etsoo/appscript";
import { useNavigate } from "react-router-dom";
import { useParamsEx } from "@etsoo/react";
import { app } from "../../../app/MyApp";
import { usePageDataEmpty } from "@etsoo/smarterp-core";
import { UserTiplist } from "@etsoo/smarterp-core/components";
import Grid from "@mui/material/Grid";
import { UserUpdateReadData, UserUpdateRQ } from "@etsoo/smarterp-crm";
import { ButtonDepts, ButtonGroups } from "@etsoo/smarterp-crm/components";

export default function EditUser() {
  // Route
  const navigate = useNavigate();
  const { id = 0 } = useParamsEx({ id: "number" });

  // Labels
  const labels = app.getLabels(
    "assignedId",
    "expiry",
    "fullName",
    "name",
    "noChanges",
    "reportTo",
    "role",
    "status"
  );

  // Form validation schema
  const validationSchema = Yup.object({
    name: Yup.string().required()
  });

  // Edit data
  const [data, setData] = React.useState<UserUpdateReadData>({
    id,
    name: "",
    userRole: UserRole.User,
    status: EntityStatus.Normal
  });

  // Formik
  // https://formik.org/docs/examples/with-material-ui
  // https://firxworx.com/blog/coding/react/integrating-formik-with-react-material-ui-and-typescript/
  const formik = useFormik<UserUpdateRQ>({
    initialValues: data,
    enableReinitialize: true,
    validationSchema: validationSchema,
    onSubmit: async (values) => {
      // Request data
      const rq = { ...values };

      // Changed fields
      const fields = Utils.getDataChanges(rq, data);

      if (fields.length === 0) {
        app.warning(labels.noChanges);
        return;
      }
      rq.changedFields = fields;

      // Submit
      const result = await app.userApi.update(rq);
      if (result == null) return;

      if (result.ok) {
        navigate(`./../../view/${id}`);
        return;
      }

      app.alertResult(result);
    }
  });

  // Load data
  const reloadData = React.useCallback(async () => {
    const data = await app.userApi.updateRead(id);
    if (data == null) return;
    setData(data);
  }, [id]);

  // Page data hook
  usePageDataEmpty(app);

  return (
    <EditPage
      isEditing
      onSubmit={(event) => {
        formik.handleSubmit(event);
      }}
      onUpdate={reloadData}
    >
      <Grid size={{ xs: 12, sm: 6 }}>
        <InputField
          fullWidth
          name="name"
          slotProps={{ htmlInput: { maxLength: 128 } }}
          label={labels.name}
          value={formik.values.name ?? ""}
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 6 }}>
        <InputField
          fullWidth
          name="assignedId"
          slotProps={{
            htmlInput: { maxLength: 20, style: { textTransform: "uppercase" } }
          }}
          label={labels.assignedId}
          value={formik.values.assignedId ?? ""}
          onChange={formik.handleChange}
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 12 }}>
        <ButtonDepts
          fullWidth
          ids={formik.values.depts}
          onValueChange={(ids) => formik.setFieldValue("depts", ids)}
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 12 }}>
        <ButtonGroups
          fullWidth
          ids={formik.values.groups}
          onValueChange={(ids) => formik.setFieldValue("groups", ids)}
        />
      </Grid>
      <Grid size={{ xs: 6, sm: 3 }}>
        <UserTiplist
          label={labels.reportTo}
          idValue={formik.values.reportTo}
          rq={{ enabled: true, excludedIds: [id] }}
          onChange={(_event, value) =>
            // Set null instead of undefined to avoid remove the property causing
            // Utils.getDataChanges ignore the field
            formik.setFieldValue("reportTo", value?.id ?? null)
          }
        />
      </Grid>
      <Grid size={{ xs: 6, sm: 3 }}>
        <ComboBox
          name="userRole"
          label={labels.role}
          inputRequired
          idValue={formik.values.userRole}
          inputOnChange={formik.handleChange}
          options={app.getRoles()}
        />
      </Grid>
      <Grid size={{ xs: 6, sm: 3 }}>
        <InputField
          fullWidth
          name="expiry"
          type="date"
          label={labels.expiry}
          value={DateUtils.formatForInput(formik.values.expiry) ?? ""}
          onChange={formik.handleChange}
        />
      </Grid>
      <Grid size={{ xs: 6, sm: 3 }}>
        <ComboBox
          name="status"
          label={labels.status}
          inputRequired
          idValue={formik.values.status}
          inputOnChange={formik.handleChange}
          options={app.getStatusList()}
        />
      </Grid>
    </EditPage>
  );
}
