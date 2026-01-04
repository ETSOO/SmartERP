import { usePageDataEmpty } from "@etsoo/smarterp-core";
import { app } from "../../../app/MyApp";
import { EditPage, InputField, OptionBool, TagList } from "@etsoo/materialui";
import { ReactUtils, useParamsEx, useRefs } from "@etsoo/react";
import { useFormik } from "formik";
import React from "react";
import Grid from "@mui/material/Grid";
import { StatusList } from "@etsoo/smarterp-core/components";
import { IdActionResult, Utils } from "@etsoo/shared";
import {
  FeatureTagKind,
  PersonInfoKind,
  SupplierCreateRQ,
  SupplierUpdateRQ
} from "@etsoo/smarterp-crm";
import { useNavigate } from "react-router-dom";
import { EntityStatus, IdentityTypeFlags } from "@etsoo/appscript";
import {
  AssignedIdDuplicateTest,
  ButtonPersonCategories,
  InfoDuplicateTest,
  NameDuplicateTest
} from "@etsoo/smarterp-crm/components";
import { AddressCreator } from "../../../components/person/AddressCreator";

export default function AddSupplier() {
  // Route
  const navigate = useNavigate();
  const { id } = useParamsEx({
    id: "number"
  });

  const isEditing = (id ?? 0) > 0;

  // Labels
  const labels = app.getLabels(
    "contact",
    "categories",
    "description",
    "isLegalPerson",
    "name",
    "nameB",
    "noChanges",
    "personBirthday",
    "personBirthdayB",
    "preferredName",
    "status",
    "tags"
  );

  // Type
  type DataType = SupplierCreateRQ;

  // State
  const [data, setData] = React.useState<DataType>({
    isLegalPerson: true,
    name: ""
  });

  // Input refs
  const refFields = [
    "assignedId",
    "birthday",
    "contact",
    "description",
    "email",
    "mobile",
    "name",
    "phone",
    "preferredName",
    "taxId"
  ] as const;
  const refs = useRefs(refFields);

  // Formik
  const formik = useFormik<DataType>({
    initialValues: data,
    enableReinitialize: true,
    validateOnChange: false,
    onSubmit: async (v) => {
      // Get updated values
      const c = { ...v };
      ReactUtils.updateRefValues(refs, c);

      // Submit
      let result: IdActionResult | undefined;
      let redirectUrl: string;
      if (id) {
        const rq: SupplierUpdateRQ = {
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

        result = await app.supplierApi.update(rq);
      } else {
        const rq: SupplierCreateRQ = {
          ...c
        };

        if (!rq.contact) {
          delete rq.mobile;
          delete rq.email;
        }

        Utils.removeEmptyValues(rq);

        redirectUrl = "./..";

        result = await app.supplierApi.create(rq);
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
    if (!id) return;
    const result = await app.supplierApi.updateRead(id);
    if (result == null) return;

    ReactUtils.updateRefs(refs, result);

    if (refs.taxId.current)
      refs.taxId.current.value =
        app.person.getInfo(result.infos, PersonInfoKind.TaxId)?.toUpperCase() ??
        "";

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
      <Grid size={{ xs: 6, sm: 3 }}>
        <InfoDuplicateTest
          infoKind={PersonInfoKind.TaxId}
          inputRef={refs.taxId}
          excludedId={id}
        />
      </Grid>
      {!isEditing && (
        <React.Fragment>
          <Grid size={{ xs: 6, sm: 3 }}>
            <InfoDuplicateTest
              infoKind={PersonInfoKind.Phone}
              inputRef={refs.phone}
              excludedId={id}
            />
          </Grid>
          <Grid size={{ xs: 12, sm: 12 }}>
            <AddressCreator
              isLegalPerson={formik.values.isLegalPerson}
              onAddressChange={(data) => {
                if (refs.name.current?.value === "" && data) {
                  ReactUtils.triggerChange(refs.name.current, data.name);
                }
                formik.setFieldValue("address", data);
              }}
            />
          </Grid>
          <Grid size={{ xs: 6, sm: 3 }}>
            <NameDuplicateTest
              fullWidth
              name="contact"
              excludedId={id}
              label={labels.contact}
              inputRef={refs.contact}
              onChange={(event) => {
                const value = event.target.value.trim();
                if (value) {
                  refs.mobile.current!.disabled = false;
                  refs.email.current!.disabled = false;
                } else {
                  refs.mobile.current!.disabled = true;
                  refs.email.current!.disabled = true;
                }
              }}
            />
          </Grid>
          <Grid size={{ xs: 6, sm: 3 }}>
            <InfoDuplicateTest
              infoKind={PersonInfoKind.Mobile}
              excludedId={id}
              inputRef={refs.mobile}
              slotProps={{ htmlInput: { disabled: true } }}
            />
          </Grid>
          <Grid size={{ xs: 6, sm: 3 }}>
            <InfoDuplicateTest
              infoKind={PersonInfoKind.Email}
              excludedId={id}
              inputRef={refs.email}
              slotProps={{ htmlInput: { disabled: true } }}
            />
          </Grid>
        </React.Fragment>
      )}
      <Grid size={{ xs: 12, sm: 12 }}>
        <ButtonPersonCategories
          fullWidth
          value={formik.values.categories ?? []}
          identityType={IdentityTypeFlags.Supplier}
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
                kind: FeatureTagKind.Supplier,
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
          label={labels.description}
          inputRef={refs.description}
          multiline
          rows={2}
        />
      </Grid>
      <Grid size={{ xs: 6, sm: 3 }}>
        <InputField
          fullWidth
          name="birthday"
          type="date"
          label={
            formik.values.isLegalPerson
              ? labels.personBirthdayB
              : labels.personBirthday
          }
          inputRef={refs.birthday}
        />
      </Grid>
      <Grid size={{ xs: 6, sm: 3 }}>
        <AssignedIdDuplicateTest fullWidth inputRef={refs.assignedId} />
      </Grid>
      <Grid size={{ xs: 6, sm: 3 }}>
        <StatusList
          inputRequired
          idValue={formik.values.status ?? EntityStatus.Normal}
          inputOnChange={formik.handleChange}
        />
      </Grid>
    </EditPage>
  );
}
