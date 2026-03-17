import { usePageDataEmpty } from "@etsoo/smarterp-core";
import { app } from "../../../app/MyApp";
import { CustomAttributeArea, EditPage, InputField } from "@etsoo/materialui";
import {
  ReactUtils,
  useParamsEx,
  useRefs,
  useSearchParamsEx
} from "@etsoo/react";
import { useFormik } from "formik";
import React from "react";
import Grid from "@mui/material/Grid";
import { DataTypes, IdActionResult, Utils } from "@etsoo/shared";
import {
  CustomCultureKind,
  PersonCategoryCreateRQ,
  PersonCategoryUpdateRQ
} from "@etsoo/smarterp-crm";
import { useNavigate } from "react-router-dom";
import {
  PersonCategoryAssignedIdDuplicateTest,
  PersonCategoryTiplist
} from "@etsoo/smarterp-crm/components";
import { IdentityTypeFlags } from "@etsoo/appscript";
import { ButtonIdentityTypes } from "@etsoo/smarterp-core/components";
import { NameCulture } from "../../../components/NameCulture";

export default function AddCategory() {
  // Route
  const navigate = useNavigate();
  const { id } = useParamsEx({
    id: "number"
  });

  const { identityType = -1 } = useSearchParamsEx({
    identityType: "number"
  });
  const it = DataTypes.getEnumByValue(IdentityTypeFlags, identityType);

  const isEditing = (id ?? 0) > 0;

  // Culture permission
  const canManageCultures = isEditing && app.system.canManageCultures();

  // Labels
  const labels = app.getLabels("nameB", "noChanges", "parentCategory");

  // Input refs
  const refFields = ["assignedId", "attributes", "name"] as const;
  const refs = useRefs(refFields);

  // Type
  type DataType = PersonCategoryCreateRQ;

  // State
  const [data, setData] = React.useState<DataType>({
    identityType: it ?? IdentityTypeFlags.None,
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

      // Get updated values
      const c = { ...v };
      ReactUtils.updateRefValues(refs, c);

      if (!!c.attributes && typeof c.attributes === "string") {
        c.attributes = JSON.parse(c.attributes);
      }

      // Submit
      let result: IdActionResult | undefined;
      let redirectUrl: string;
      if (id) {
        const rq: PersonCategoryUpdateRQ = {
          ...c,
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
          ...c
        };

        Utils.removeEmptyValues(rq);

        redirectUrl = "./..";

        result = await app.personCategoryApi.create(rq);
      }

      if (result == null) return;

      if (result.ok) {
        navigate(redirectUrl + "?identityType=" + v.identityType);
        return;
      }

      app.alertResult(result);
    }
  });

  // Load data
  const reloadData = React.useCallback(async () => {
    if (!id) return;
    const result = await app.personCategoryApi.updateRead(id);
    if (result == null) return;
    ReactUtils.updateRefs(refs, result);
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
      <Grid size={{ xs: 12, sm: 6 }}>
        <PersonCategoryAssignedIdDuplicateTest
          fullWidth
          inputRef={refs.assignedId}
          excludedId={id}
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 12 }}>
        <InputField
          fullWidth
          required
          name="name"
          slotProps={{ htmlInput: { maxLength: 128 } }}
          label={labels.nameB}
          inputRef={refs.name}
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 12 }}>
        <CustomAttributeArea name="attributes" inputRef={refs.attributes} />
      </Grid>
      {canManageCultures && (
        <Grid size={{ xs: 6, sm: 3 }}>
          <NameCulture id={id ?? 0} kind={CustomCultureKind.PersonCategory} />
        </Grid>
      )}
    </EditPage>
  );
}
