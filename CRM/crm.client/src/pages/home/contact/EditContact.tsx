import { EditPage, InputField, OptionBool, TagList } from "@etsoo/materialui";
import React from "react";
import { useFormik } from "formik";
import * as Yup from "yup";
import { DateUtils, Utils } from "@etsoo/shared";
import { EntityStatus, IdentityTypeFlags } from "@etsoo/appscript";
import { useNavigate } from "react-router-dom";
import { ReactUtils, useParamsEx, useRefs } from "@etsoo/react";
import { app } from "../../../app/MyApp";
import { usePageDataEmpty } from "@etsoo/smarterp-core";
import {
  ButtonCultures,
  ButtonCurrencies,
  ButtonIdentityTypes,
  ButtonRegions,
  StatusList,
  UserTiplist
} from "@etsoo/smarterp-core/components";
import Grid from "@mui/material/Grid";
import { PersonUpdateReadData, PersonUpdateRQ } from "@etsoo/smarterp-crm";
import Divider from "@mui/material/Divider";
import {
  AssignedIdDuplicateTest,
  ButtonEducations,
  ButtonPersonCategories,
  MaritalStatusList,
  NameDuplicateTest,
  PersonDegreeList,
  PersonGenderList,
  PersonTitleList
} from "@etsoo/smarterp-crm/components";
import InputAdornment from "@mui/material/InputAdornment";

export default function EditContact() {
  // Route
  const navigate = useNavigate();
  const { id = 0 } = useParamsEx({ id: "number" });

  // Labels
  const labels = app.getLabels(
    "cultures",
    "currencies",
    "description",
    "expiry",
    "familyName",
    "fullName",
    "givenName",
    "isLegalPerson",
    "jobTitle",
    "latinFamilyName",
    "latinGivenName",
    "name",
    "nameB",
    "noChanges",
    "personBirthday",
    "personBirthdayB",
    "personEthnicity",
    "personHeight",
    "personWeight",
    "personTitle",
    "politicalStatus",
    "preferredName",
    "queryKeyword",
    "reportTo",
    "role",
    "status",
    "tags",
    "unitCM",
    "unitKILOGRAM"
  );

  // Form validation schema
  const validationSchema = Yup.object({
    name: Yup.string().required()
  });

  // Edit data
  const [data, setData] = React.useState<PersonUpdateReadData>({
    id,
    identityType: IdentityTypeFlags.None,
    isLegalPerson: false,
    name: "",
    status: EntityStatus.Normal
  });

  // Input refs
  const refFields = [
    "assignedId",
    "birthday",
    "description",
    "ethnicity",
    "expiry",
    "familyName",
    "givenName",
    "height",
    "latinFamilyName",
    "latinGivenName",
    "jobTitle",
    "name",
    "politicalStatus",
    "preferredName",
    "queryKeyword",
    "weight"
  ] as const;
  const refs = useRefs(refFields);

  // Clone data to avoid reference change
  const { privateData, ...rest } = data;

  // Formik
  // https://formik.org/docs/examples/with-material-ui
  // https://firxworx.com/blog/coding/react/integrating-formik-with-react-material-ui-and-typescript/
  const formik = useFormik<PersonUpdateReadData>({
    initialValues: { privateData: { ...privateData }, ...rest },
    enableReinitialize: true,
    validationSchema: validationSchema,
    onSubmit: async (values) => {
      // Request data
      const rq: PersonUpdateRQ = { ...values };

      // Get updated values
      ReactUtils.updateRefValues(refs, rq);

      // Changed fields
      const fields = Utils.getDataChanges(rq, data);
      if (fields.length === 0) {
        app.warning(labels.noChanges);
        return;
      }
      rq.changedFields = fields;

      // Private data
      if (
        rq.privateData &&
        privateData &&
        rq.changedFields.includes("privateData")
      ) {
        rq.privateData.changedFields = Utils.getDataChanges(
          rq.privateData,
          privateData
        );
      }

      // Update
      const result = await app.personApi.update(rq);

      if (result == null) return;

      if (result.ok) {
        navigate("./../..");
        return;
      }

      app.alertResult(result);
    }
  });

  // Load data
  const reloadData = React.useCallback(async () => {
    const data = await app.personApi.updateRead(id);
    if (data == null) return;
    ReactUtils.updateRefs(refs, data);
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
      paddings={0}
    >
      <Grid size={{ xs: 12, sm: 6 }}>
        <ButtonIdentityTypes
          fullWidth
          value={formik.values.identityType}
          onValueChange={(value) => formik.setFieldValue("identityType", value)}
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 6 }}>
        <OptionBool
          fullWidth
          name="isLegalPerson"
          label={labels.isLegalPerson}
          defaultValue={formik.values.isLegalPerson}
          onValueChange={(value) =>
            formik.setFieldValue("isLegalPerson", value)
          }
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 6 }}>
        <NameDuplicateTest
          fullWidth
          required
          excludedId={id}
          label={formik.values.isLegalPerson ? labels.nameB : labels.name}
          inputRef={refs.name}
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 6 }}>
        <InputField
          fullWidth
          name="preferredName"
          slotProps={{ htmlInput: { maxLength: 128 } }}
          label={labels.preferredName}
          inputRef={refs.preferredName}
        />
      </Grid>
      {formik.values.isLegalPerson && (
        <Grid size={{ xs: 6, sm: 3 }}>
          <InputField
            fullWidth
            name="privateData.birthday"
            type="date"
            label={labels.personBirthdayB}
            inputRef={refs.birthday}
            ref={() => {
              const input = refs.birthday.current;
              if (input) {
                const birthday = DateUtils.formatForInput(
                  data.privateData?.birthday
                );
                if (birthday) {
                  input.value = birthday;
                }
              }
            }}
          />
        </Grid>
      )}
      {!formik.values.isLegalPerson && (
        <React.Fragment>
          <Grid size={{ xs: 6, sm: 3 }}>
            <InputField
              fullWidth
              name="jobTitle"
              slotProps={{ htmlInput: { maxLength: 50 } }}
              label={labels.jobTitle}
              inputRef={refs.jobTitle}
            />
          </Grid>
          <Grid size={{ xs: 6, sm: 3 }}>
            <PersonTitleList
              fullWidth
              value={formik.values.title}
              onChange={formik.handleChange}
            />
          </Grid>
          <Grid size={{ xs: 6, sm: 3 }}>
            <InputField
              fullWidth
              name="familyName"
              slotProps={{ htmlInput: { maxLength: 50 } }}
              label={labels.familyName}
              inputRef={refs.familyName}
            />
          </Grid>
          <Grid size={{ xs: 6, sm: 3 }}>
            <InputField
              fullWidth
              name="givenName"
              slotProps={{ htmlInput: { maxLength: 50 } }}
              label={labels.givenName}
              inputRef={refs.givenName}
            />
          </Grid>
          <Grid size={{ xs: 6, sm: 3 }}>
            <InputField
              fullWidth
              name="latinFamilyName"
              slotProps={{ htmlInput: { maxLength: 50 } }}
              label={labels.latinFamilyName}
              inputRef={refs.latinFamilyName}
            />
          </Grid>
          <Grid size={{ xs: 6, sm: 3 }}>
            <InputField
              fullWidth
              name="latinGivenName"
              slotProps={{ htmlInput: { maxLength: 50 } }}
              label={labels.givenName}
              inputRef={refs.latinGivenName}
            />
          </Grid>
        </React.Fragment>
      )}
      <Grid size={{ xs: 12, sm: 12 }}>
        <ButtonPersonCategories
          fullWidth
          value={formik.values.categories ?? []}
          identityType={formik.values.identityType}
          onValueChange={(ids) => formik.setFieldValue("categories", ids)}
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 12 }}>
        <TagList
          value={formik.values.tags ?? []}
          disableCloseOnSelect
          openOnFocus
          onChange={(_event, value) => formik.setFieldValue("tags", value)}
          label={labels.tags}
          loadData={(keyword, maxItems) =>
            app.tagApi.list(
              {
                kind: app.system.identityTypeToTagKind(
                  formik.values.identityType
                ),
                keyword,
                queryPaging: maxItems
              },
              { showLoading: false }
            )
          }
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 12 }}>
        <ButtonRegions
          fullWidth
          value={formik.values.regions ?? []}
          onValueChange={(ids) => formik.setFieldValue("regions", ids)}
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 12 }}>
        <ButtonCurrencies
          fullWidth
          value={formik.values.currencies ?? []}
          onValueChange={(ids) => formik.setFieldValue("currencies", ids)}
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 12 }}>
        <ButtonCultures
          fullWidth
          value={formik.values.cultures ?? []}
          onValueChange={(ids) => formik.setFieldValue("cultures", ids)}
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
        <AssignedIdDuplicateTest fullWidth inputRef={refs.assignedId} />
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
        <InputField
          fullWidth
          name="queryKeyword"
          slotProps={{ htmlInput: { maxLength: 30 } }}
          label={labels.queryKeyword}
          inputRef={refs.queryKeyword}
        />
      </Grid>
      <Grid size={{ xs: 6, sm: 3 }}>
        <InputField
          fullWidth
          name="expiry"
          type="date"
          label={labels.expiry}
          inputRef={refs.expiry}
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
      {!formik.values.isLegalPerson && (
        <React.Fragment>
          <Grid size={{ xs: 12, sm: 12 }}>
            <Divider />
          </Grid>
          <Grid size={{ xs: 6, sm: 3 }}>
            <PersonGenderList
              fullWidth
              name="privateData.gender"
              value={formik.values.privateData?.gender}
              onChange={formik.handleChange}
            />
          </Grid>
          <Grid size={{ xs: 6, sm: 3 }}>
            <InputField
              fullWidth
              name="privateData.birthday"
              type="date"
              label={labels.personBirthday}
              inputRef={refs.birthday}
            />
          </Grid>
          <Grid size={{ xs: 12, sm: 6 }}>
            <ButtonEducations
              fullWidth
              value={formik.values.privateData?.education}
              onValueChange={(value) =>
                formik.setFieldValue("privateData.education", value)
              }
            />
          </Grid>
          <Grid size={{ xs: 6, sm: 3 }}>
            <PersonDegreeList
              fullWidth
              name="privateData.degree"
              value={formik.values.privateData?.degree}
              onChange={formik.handleChange}
            />
          </Grid>
          <Grid size={{ xs: 6, sm: 3 }}>
            <MaritalStatusList
              fullWidth
              name="privateData.maritalStatus"
              value={formik.values.privateData?.maritalStatus}
              onChange={formik.handleChange}
            />
          </Grid>
          <Grid size={{ xs: 6, sm: 3 }}>
            <InputField
              fullWidth
              name="privateData.ethnicity"
              slotProps={{ htmlInput: { maxLength: 50 } }}
              label={labels.personEthnicity}
              inputRef={refs.ethnicity}
            />
          </Grid>
          <Grid size={{ xs: 6, sm: 3 }}>
            <InputField
              fullWidth
              name="privateData.politicalStatus"
              slotProps={{ htmlInput: { maxLength: 50 } }}
              label={labels.politicalStatus}
              inputRef={refs.politicalStatus}
            />
          </Grid>
          <Grid size={{ xs: 6, sm: 3 }}>
            <InputField
              fullWidth
              name="privateData.height"
              type="integer"
              label={labels.personHeight}
              inputMode="numeric"
              inputRef={refs.height}
              slotProps={{
                input: {
                  endAdornment: (
                    <InputAdornment position="end">
                      {labels.unitCM}
                    </InputAdornment>
                  )
                }
              }}
            />
          </Grid>
          <Grid size={{ xs: 6, sm: 3 }}>
            <InputField
              fullWidth
              name="privateData.weight"
              type="decimal"
              label={labels.personWeight}
              inputMode="decimal"
              inputRef={refs.weight}
              slotProps={{
                input: {
                  endAdornment: (
                    <InputAdornment position="end">
                      {labels.unitKILOGRAM}
                    </InputAdornment>
                  )
                }
              }}
            />
          </Grid>
        </React.Fragment>
      )}
    </EditPage>
  );
}
