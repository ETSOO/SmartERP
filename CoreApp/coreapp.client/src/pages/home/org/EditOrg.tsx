import { EditPage, InputField, MaskInput } from "@etsoo/materialui";
import { Grid2 } from "@mui/material";
import React from "react";
import { useFormik } from "formik";
import * as Yup from "yup";
import { DataTypes, Utils } from "@etsoo/shared";
import { BusinessTax } from "@etsoo/appscript";
import { useNavigate } from "react-router-dom";
import { useParamsEx } from "@etsoo/react";
import { app } from "../../../app/MyApp";
import { OrgUpdateReadDto, OrgUpdateRQ } from "@etsoo/smarterp-core";
import { OrgTiplist } from "@etsoo/smarterp-core/components";
import { PageDataContext } from "@etsoo/toolpad";

export default function EditOrg() {
  // Route
  const navigate = useNavigate();
  const { id } = useParamsEx({ id: "number" });

  // Page data
  const { dispatch } = React.useContext(PageDataContext);

  // Data type
  type DataType = OrgUpdateRQ;

  // Labels
  const labels = app.getLabels(
    "edit",
    "noChanges",
    "organizationName",
    "tradeAs",
    "brand",
    "parentOrg"
  );

  // Form validation schema
  const validationSchema = Yup.object({
    name: Yup.string().required()
  });

  // Edit data
  const [data, setData] = React.useState<DataType>({ id: 0 });

  // Formik
  // https://formik.org/docs/examples/with-material-ui
  // https://firxworx.com/blog/coding/react/integrating-formik-with-react-material-ui-and-typescript/
  const formik = useFormik<DataType>({
    initialValues: data,
    enableReinitialize: true,
    validationSchema: validationSchema,
    onSubmit: async (values) => {
      // Edit only
      if (values.id == null) return;

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
      //rq.changedFields = fields;
      rq.changedFields = [];

      // Submit
      const result = await app.core.orgApi.update({ id: 1 });
      if (result == null) return;

      if (result.ok) {
        navigate("./../../all", {
          state: { id }
        });
        return;
      }

      app.alertResult(result);
    }
  });

  // Load data
  const reloadData = async () => {
    if (id == null) return;
    const data = await app.core.orgApi.updateRead(id);
    if (data == null) return;
    setData(data);
  };

  // Tax
  const tax = BusinessTax.getById(app.region);

  React.useEffect(() => {
    // Page title
    dispatch({ page: labels.edit });

    return () => {
      app.pageExit();
    };
  }, []);

  return (
    <EditPage
      onSubmit={(event) => {
        formik.handleSubmit(event);
      }}
      onUpdate={reloadData}
    >
      <Grid2 size={{ xs: 12, sm: 6 }}>
        <InputField
          fullWidth
          required
          name="name"
          inputProps={{ maxLength: 128 }}
          label={labels.organizationName}
          value={formik.values.name ?? ""}
          onChange={formik.handleChange}
          error={formik.touched.name && Boolean(formik.errors.name)}
          helperText={formik.touched.name && formik.errors.name}
        />
      </Grid2>
      <Grid2 size={{ xs: 12, sm: 6 }}>
        <MaskInput
          mask={{ mask: tax?.mask ?? "" }}
          name="pin"
          label={app.get(tax?.labelKey ?? "taxId")}
          fullWidth
          inputProps={{
            maxLength: 20,
            style: { textTransform: "uppercase" }
          }}
          value={formik.values.pin ?? ""}
          onChange={formik.handleChange}
          error={formik.touched.pin && Boolean(formik.errors.pin)}
          helperText={formik.touched.pin && formik.errors.pin}
        />
      </Grid2>
      <Grid2 size={{ xs: 12, sm: 6 }}>
        <InputField
          fullWidth
          name="brand"
          inputProps={{ maxLength: 30 }}
          label={labels.brand}
          value={formik.values.brand ?? ""}
          onChange={formik.handleChange}
        />
      </Grid2>
      <Grid2 size={{ xs: 12, sm: 6 }}>
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
      </Grid2>
    </EditPage>
  );
}
