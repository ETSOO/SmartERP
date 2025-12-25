import { usePageDataEmpty } from "@etsoo/smarterp-core";
import { app } from "../../../app/MyApp";
import { EditPage, InputField, OptionBool, TagList } from "@etsoo/materialui";
import { useParamsEx, useRefs } from "@etsoo/react";
import { useFormik } from "formik";
import React from "react";
import Grid from "@mui/material/Grid";
import { StatusList } from "@etsoo/smarterp-core/components";
import { IdActionResult, Utils } from "@etsoo/shared";
import {
  CustomerCreateRQ,
  CustomerType,
  CustomerUpdateRQ,
  FeatureTagKind,
  PersonInfoKind
} from "@etsoo/smarterp-crm";
import { useNavigate } from "react-router-dom";
import { EntityStatus, IdentityTypeFlags } from "@etsoo/appscript";
import {
  ButtonPersonCategories,
  EntityDuplicateTest
} from "@etsoo/smarterp-crm/components";
import { AddressCreator } from "../../../components/person/AddressCreator";

export default function AddCustomer() {
  // Route
  const navigate = useNavigate();
  const { id = 0 } = useParamsEx({
    id: "number"
  });

  const isEditing = id > 0;

  // Labels
  const labels = app.getLabels(
    "assignedId",
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
  type DataType = CustomerCreateRQ;

  // State
  const [data, setData] = React.useState<DataType>({
    isLegalPerson:
      app.userData?.system?.mainCustomerType !== CustomerType.Individual,
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
      // Submit
      let result: IdActionResult | undefined;
      let redirectUrl: string;
      if (id > 0) {
        const rq: CustomerUpdateRQ = {
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

        result = await app.customerApi.update(rq);
      } else {
        const rq: CustomerCreateRQ = {
          ...v
        };

        if (!rq.contact) {
          delete rq.mobile;
          delete rq.email;
        }

        Utils.removeEmptyValues(rq);

        redirectUrl = "./..";

        result = await app.customerApi.create(rq);
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
    const result = await app.customerApi.updateRead(id);
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
        <EntityDuplicateTest
          fullWidth
          required
          slotProps={{ htmlInput: { maxLength: 128 } }}
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
        <EntityDuplicateTest
          infoKind={PersonInfoKind.TaxId}
          inputRef={refs.taxId}
        />
      </Grid>
      <Grid size={{ xs: 6, sm: 3 }}>
        <EntityDuplicateTest
          infoKind={PersonInfoKind.Phone}
          inputRef={refs.phone}
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 12 }}>
        <AddressCreator />
      </Grid>
      <Grid size={{ xs: 6, sm: 3 }}>
        <EntityDuplicateTest
          fullWidth
          name="contact"
          label={labels.contact}
          slotProps={{ htmlInput: { maxLength: 128 } }}
          inputRef={refs.name}
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
        <EntityDuplicateTest
          infoKind={PersonInfoKind.Mobile}
          inputRef={refs.mobile}
          slotProps={{ htmlInput: { disabled: true } }}
        />
      </Grid>
      <Grid size={{ xs: 6, sm: 3 }}>
        <EntityDuplicateTest
          infoKind={PersonInfoKind.Email}
          inputRef={refs.email}
          slotProps={{ htmlInput: { disabled: true } }}
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 12 }}>
        <ButtonPersonCategories
          fullWidth
          value={formik.values.categories ?? []}
          identityType={IdentityTypeFlags.Customer}
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
                kind: FeatureTagKind.Customer,
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
        <InputField
          fullWidth
          name="assignedId"
          slotProps={{
            htmlInput: { maxLength: 20, style: { textTransform: "uppercase" } }
          }}
          label={labels.assignedId}
          inputRef={refs.assignedId}
        />
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
