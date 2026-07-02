import { EditPage, ImagePreviewButton, InputField } from "@etsoo/materialui";
import React from "react";
import { useFormik } from "formik";
import * as Yup from "yup";
import { DomUtils, Utils } from "@etsoo/shared";
import { useNavigate } from "react-router-dom";
import { app } from "../../../app/MyApp";
import {
  usePageDataEmpty,
  UserUpdateReadDto,
  UserUpdateRQ
} from "@etsoo/smarterp-core";
import Grid from "@mui/material/Grid";
import ButtonGroup from "@mui/material/ButtonGroup";
import Button from "@mui/material/Button";

export default function EditUser() {
  // Route
  const navigate = useNavigate();

  // Labels
  const labels = app.getLabels(
    "edit",
    "familyName",
    "fullName",
    "givenName",
    "latinFamilyName",
    "latinGivenName",
    "noChanges",
    "preferredName",
    "signature"
  );

  // Form validation schema
  const validationSchema = Yup.object({
    name: Yup.string().required()
  });

  // Edit data
  const [data, setData] = React.useState<UserUpdateReadDto>({
    id: 0,
    name: ""
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

      // Name's minimum length is 2
      if (rq.name != null && rq.name.length < 2) {
        DomUtils.setFocus("name");
        return;
      }

      // Submit
      const result = await app.core.userApi.update(rq);
      if (result == null) return;

      if (result.ok) {
        await app.refreshToken();
        navigate(`./../`);
        return;
      }

      app.alertResult(result);
    }
  });

  // Load data
  const reloadData = React.useCallback(async () => {
    const data = await app.core.userApi.updateRead();
    if (data == null) return;
    setData(data);
  }, []);

  // Page data hook
  usePageDataEmpty(app);

  return (
    <EditPage isEditing onSubmit={formik.handleSubmit} onUpdate={reloadData}>
      <Grid size={{ xs: 8, md: 6, lg: 4 }}>
        <InputField
          fullWidth
          required
          name="name"
          slotProps={{ htmlInput: { maxLength: 128 } }}
          label={labels.fullName}
          value={formik.values.name ?? ""}
          onChange={formik.handleChange}
          error={formik.touched.name && Boolean(formik.errors.name)}
          helperText={formik.touched.name && formik.errors.name}
        />
      </Grid>
      <Grid size={{ xs: 4, md: 3 }}>
        <InputField
          fullWidth
          name="familyName"
          slotProps={{ htmlInput: { maxLength: 50 } }}
          label={labels.familyName}
          value={formik.values.familyName ?? ""}
          onChange={formik.handleChange}
        />
      </Grid>
      <Grid size={{ xs: 4, md: 3 }}>
        <InputField
          fullWidth
          name="givenName"
          slotProps={{ htmlInput: { maxLength: 50 } }}
          label={labels.givenName}
          value={formik.values.givenName ?? ""}
          onChange={formik.handleChange}
        />
      </Grid>
      <Grid size={{ xs: 8, md: 6, lg: 4 }}>
        <InputField
          fullWidth
          name="preferredName"
          slotProps={{ htmlInput: { maxLength: 128 } }}
          label={labels.preferredName}
          value={formik.values.preferredName ?? ""}
          onChange={formik.handleChange}
        />
      </Grid>
      <Grid size={{ xs: 4, md: 3 }}>
        <InputField
          fullWidth
          name="latinFamilyName"
          slotProps={{ htmlInput: { maxLength: 50 } }}
          label={labels.latinFamilyName}
          value={formik.values.latinFamilyName ?? ""}
          onChange={formik.handleChange}
        />
      </Grid>
      <Grid size={{ xs: 4, md: 3 }}>
        <InputField
          fullWidth
          name="latinGivenName"
          slotProps={{ htmlInput: { maxLength: 50 } }}
          label={labels.latinGivenName}
          value={formik.values.latinGivenName ?? ""}
          onChange={formik.handleChange}
        />
      </Grid>
      <Grid size={{ xs: 6, md: 3 }}>
        <ButtonGroup>
          <ImagePreviewButton size={[90, 45]} image={data.signature} />
          <Button>
            {labels.signature} ({labels.edit})
          </Button>
        </ButtonGroup>
      </Grid>
    </EditPage>
  );
}
