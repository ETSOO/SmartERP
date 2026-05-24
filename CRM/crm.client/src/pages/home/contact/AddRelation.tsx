import { usePageDataEmpty } from "@etsoo/smarterp-core";
import { app } from "../../../app/MyApp";
import { EditPage, InputField, TagList } from "@etsoo/materialui";
import {
  ReactUtils,
  useParamsEx,
  useRefs,
  useSearchParamsEx
} from "@etsoo/react";
import { useFormik } from "formik";
import Grid from "@mui/material/Grid";
import {
  ContactCreateRQ,
  PersonInfoKind,
  PersonRelationType
} from "@etsoo/smarterp-crm";
import { useNavigate } from "react-router-dom";
import {
  ButtonPersonCategories,
  ButtonRadioContactRelations,
  InfoDuplicateTest,
  NameDuplicateTest,
  PersonGenderList,
  PersonTitleList
} from "@etsoo/smarterp-crm/components";
import { IdentityTypeFlags } from "@etsoo/appscript";
import { Utils } from "@etsoo/shared";

export default function AddRelation() {
  // Route
  const navigate = useNavigate();
  const { id: personId = 0 } = useParamsEx({
    id: "number"
  });
  const { isLegalPerson = null, index = 3 } = useSearchParamsEx({
    isLegalPerson: "boolean",
    index: "number"
  });

  // Labels
  const labels = app.getLabels(
    "birthday",
    "description",
    "email",
    "familyName",
    "givenName",
    "jobTitle",
    "name",
    "personBirthday",
    "preferredName",
    "relationDescription",
    "tags"
  );

  // Input refs
  const refFields = [
    "birthday",
    "description",
    "email",
    "mobile",
    "phone",
    "familyName",
    "givenName",
    "jobTitle",
    "name",
    "preferredName"
  ] as const;
  const refs = useRefs(refFields);

  // Type
  type DataType = ContactCreateRQ;

  const identityType = IdentityTypeFlags.None;

  // Formik
  const formik = useFormik<DataType>({
    initialValues: {
      personId,
      relationType: PersonRelationType.Unknown,
      name: ""
    },
    enableReinitialize: true,
    validateOnChange: false,
    onSubmit: async (v) => {
      // Request data
      const rq: ContactCreateRQ = { ...v };

      // Get updated values
      ReactUtils.updateRefValues(refs, rq);

      const result = await app.personContactApi.create(rq);
      if (result == null) return;

      if (result.ok) {
        navigate(`./../../../view/${personId}?index=${index}`);
        return;
      }

      app.alertResult(result);
    }
  });

  // Page data hook
  usePageDataEmpty(app);

  return (
    <EditPage onSubmit={formik.handleSubmit} paddings={0}>
      <Grid size={{ xs: 12, sm: 6 }}>
        <ButtonRadioContactRelations
          fullWidth
          isLegalPerson={isLegalPerson}
          required
          onValueChange={(id) => formik.setFieldValue("relationType", id)}
        />
      </Grid>
      <Grid size={{ xs: 6, sm: 3 }}>
        <NameDuplicateTest
          label={labels.name}
          inputRef={refs.name}
          onChange={(event) => {
            const value = event.target.value.trim();
            if (value) {
              const nd = Utils.parseName(value);
              refs.familyName.current!.value = nd.familyName ?? "";
              refs.givenName.current!.value = nd.givenName ?? "";
            } else {
              refs.familyName.current!.value = "";
              refs.givenName.current!.value = "";
            }
          }}
          required
        />
      </Grid>
      <Grid size={{ xs: 6, sm: 3 }}>
        <InputField
          fullWidth
          name="preferredName"
          slotProps={{ htmlInput: { maxLength: 128 } }}
          label={labels.preferredName}
          inputRef={refs.preferredName}
        />
      </Grid>
      <Grid size={{ xs: 6, sm: 3 }}>
        <InfoDuplicateTest
          infoKind={PersonInfoKind.Mobile}
          inputRef={refs.mobile}
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 6 }}>
        <InfoDuplicateTest
          infoKind={PersonInfoKind.Email}
          inputRef={refs.email}
        />
      </Grid>
      <Grid size={{ xs: 6, sm: 3 }}>
        <InfoDuplicateTest
          infoKind={PersonInfoKind.Phone}
          inputRef={refs.phone}
        />
      </Grid>
      <Grid size={{ xs: 6, sm: 3 }}>
        <PersonGenderList
          fullWidth
          name="gender"
          value={formik.values.gender}
          onChange={formik.handleChange}
        />
      </Grid>
      <Grid size={{ xs: 6, sm: 3 }}>
        <InputField
          fullWidth
          name="birthday"
          type="date"
          label={labels.personBirthday}
          inputRef={refs.birthday}
        />
      </Grid>
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
      <Grid size={{ xs: 12, sm: 12 }}>
        <ButtonPersonCategories
          fullWidth
          value={formik.values.categories ?? []}
          identityType={identityType}
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
                kind: app.system.identityTypeToTagKind(identityType),
                keyword,
                queryPaging: maxItems
              },
              { showLoading: false }
            )
          }
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 12 }}>
        <InputField
          fullWidth
          name="description"
          slotProps={{
            htmlInput: { maxLength: 1280 }
          }}
          label={labels.relationDescription}
          inputRef={refs.description}
          multiline
          rows={2}
        />
      </Grid>
    </EditPage>
  );
}
