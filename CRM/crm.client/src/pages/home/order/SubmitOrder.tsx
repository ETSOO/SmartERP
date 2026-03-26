import { usePageDataEmpty } from "@etsoo/smarterp-core";
import { app } from "../../../app/MyApp";
import {
  CustomFieldUI,
  EditPage,
  InputField,
  OptionBool,
  TagList
} from "@etsoo/materialui";
import { ReactUtils, useParamsEx, useRefs } from "@etsoo/react";
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
import {
  CustomFieldData,
  CustomFieldRef,
  EntityStatus,
  IdentityTypeFlags
} from "@etsoo/appscript";
import {
  AssignedIdDuplicateTest,
  ButtonPersonCategories,
  InfoDuplicateTest,
  NameDuplicateTest
} from "@etsoo/smarterp-crm/components";
import { AddressCreator } from "../../../components/person/AddressCreator";
import { Divider } from "@mui/material";

export default function SubmitOrder() {
  // Route
  const navigate = useNavigate();
  const { id } = useParamsEx({
    id: "number"
  });

  const isEditing = (id ?? 0) > 0;

  // Labels
  const labels = app.getLabels("noChanges", "status", "title");

  // Type
  type DataType = OrderCreateRQ;

  // State
  const [data, setData] = React.useState<DataType>({
    isLegalPerson:
      app.userData?.system?.mainCustomerType !== CustomerType.Individual,
    name: ""
  });

  // Input refs
  const refFields = ["title"] as const;
  const refs = useRefs(refFields);

  // Formik
  const formik = useFormik<DataType>({
    initialValues: data,
    enableReinitialize: true,
    validateOnChange: false,
    onSubmit: async (v) => {
      // Get updated values
      const c = structuredClone(v);
      ReactUtils.updateRefValues(refs, c);

      // Submit
      let result: IdActionResult | undefined;
      let redirectUrl: string;
      if (id) {
        const rq: CustomerUpdateRQ = {
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

        result = await app.customerApi.update(rq);
      } else {
        const rq: CustomerCreateRQ = {
          ...c
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
    if (!id) return;
    const result = await app.customerApi.updateRead(id);
    if (result == null) return;

    ReactUtils.updateRefs(refs, result);

    if (refs.taxId.current)
      refs.taxId.current.value =
        app.person.getInfo(result.infos, PersonInfoKind.TaxId)?.toUpperCase() ??
        "";

    setData(result);

    if (result.categories && result.categories.length > 0) {
      const attributes = await app.personCategoryApi.getAttributes(
        result.categories
      );
      if (attributes == null) return;
      setCustomFields(attributes);
    }
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
