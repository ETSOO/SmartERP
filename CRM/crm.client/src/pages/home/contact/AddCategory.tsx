import { usePageDataEmpty } from "@etsoo/smarterp-core";
import { app } from "../../../app/MyApp";
import { EditPage, InputField } from "@etsoo/materialui";
import { useParamsEx } from "@etsoo/react";
import { useFormik } from "formik";
import React from "react";
import Grid from "@mui/material/Grid";
import { IdActionResult, Utils } from "@etsoo/shared";
import {
  PersonCategoryCreateRQ,
  PersonCategoryUpdateRQ
} from "@etsoo/smarterp-crm";
import { useNavigate } from "react-router-dom";
import { PersonCategoryTiplist } from "@etsoo/smarterp-crm/components";
import { IdentityTypeFlags } from "@etsoo/appscript";
import { ButtonIdentityTypes } from "@etsoo/smarterp-core/components";

export default function AddCategory() {
  // Route
  const navigate = useNavigate();
  const { id = 0 } = useParamsEx({
    id: "number"
  });

  const isEditing = id > 0;

  // Labels
  const labels = app.getLabels("nameB", "noChanges", "parentCategory");

  // Type
  type DataType = PersonCategoryCreateRQ;

  // State
  const [data, setData] = React.useState<DataType>({
    identityType: IdentityTypeFlags.None,
    name: ""
  });

  // Formik
  const formik = useFormik<DataType>({
    initialValues: data,
    enableReinitialize: true,
    validateOnChange: false,
    onSubmit: async (v) => {
      // Validate
      if (v.identityType === IdentityTypeFlags.None) {
        return;
      }

      // Submit
      let result: IdActionResult | undefined;
      let redirectUrl: string;
      if (id > 0) {
        const rq: PersonCategoryUpdateRQ = {
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

        result = await app.personCategoryApi.update(rq);
      } else {
        const rq: PersonCategoryCreateRQ = {
          ...v
        };

        Utils.removeEmptyValues(rq);

        redirectUrl = "./..";

        result = await app.personCategoryApi.create(rq);
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
    const result = await app.personCategoryApi.updateRead(id);
    if (result == null) return;
    setData(result);
  }, [id]);

  // Page data hook
  usePageDataEmpty(app);

  return (
    <EditPage
      isEditing={isEditing}
      onSubmit={(event) => {
        formik.handleSubmit(event);
      }}
      onUpdate={reloadData}
      paddings={0}
    >
      <Grid size={{ xs: 12, sm: 12 }}>
        <ButtonIdentityTypes
          fullWidth
          required
          value={formik.values.identityType}
          onValueChange={(value) => formik.setFieldValue("identityType", value)}
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 6 }}>
        <PersonCategoryTiplist
          label={labels.parentCategory}
          idValue={formik.values.parentId}
          inputOnChange={formik.handleChange}
          name="parentId"
          rq={{ identityType: formik.values.identityType }}
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 12 }}>
        <InputField
          fullWidth
          required
          name="name"
          slotProps={{ htmlInput: { maxLength: 128 } }}
          label={labels.nameB}
          value={formik.values.name}
          onChange={formik.handleChange}
        />
      </Grid>
    </EditPage>
  );
}
