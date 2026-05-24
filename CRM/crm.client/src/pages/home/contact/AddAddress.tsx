import { usePageDataEmpty } from "@etsoo/smarterp-core";
import { app } from "../../../app/MyApp";
import { EditPage, InputField } from "@etsoo/materialui";
import {
  ReactUtils,
  useParamsEx,
  useRefs,
  useSearchParamsEx
} from "@etsoo/react";
import { useFormik } from "formik";
import React from "react";
import Grid from "@mui/material/Grid";
import {
  AddressCreateRQ,
  AddressKind,
  AddressUpdateRQ,
  CustomerType
} from "@etsoo/smarterp-crm";
import { useNavigate } from "react-router-dom";
import { ButtonRadioRegions } from "@etsoo/smarterp-core/components";
import SpellcheckIcon from "@mui/icons-material/Spellcheck";
import InputAdornment from "@mui/material/InputAdornment";
import IconButton from "@mui/material/IconButton";
import { IdActionResult, Utils } from "@etsoo/shared";
import { MapApiProvider } from "@etsoo/appscript";
import {
  AddressDuplicateTest,
  AddressKindList,
  AddressList
} from "@etsoo/smarterp-crm/components";

export default function AddAddress() {
  // Route
  const navigate = useNavigate();
  const { id: personId = 0 } = useParamsEx({
    id: "number"
  });
  const { id = 0 } = useSearchParamsEx({ id: "number" });

  const isEditing = id > 0;

  // Labels
  const labels = app.getLabels(
    "addressCity",
    "addressDistrict",
    "addressPostalCode",
    "addressRaw",
    "addressRawTip",
    "addressRoute",
    "addressState",
    "nameB",
    "addressStreet",
    "mainAddress",
    "noChanges",
    "parseAddress",
    "status",
    "type"
  );

  // Input refs
  const refFields = [
    "rawAddress",
    "name",
    "state",
    "city",
    "district",
    "postalCode",
    "route",
    "street",
    "formattedAddress"
  ] as const;
  const refs = useRefs(refFields);

  // Type
  type DataType = AddressCreateRQ;

  // Address kind
  const kind =
    app.userData?.system?.mainCustomerType === CustomerType.Individual
      ? AddressKind.Home
      : AddressKind.Office;

  // State
  const [data, setData] = React.useState<DataType>({
    personId,
    provider: MapApiProvider.Google,
    name: "",
    region: "",
    state: "",
    city: "",
    formattedAddress: "",
    kind
  });

  // Formik
  const formik = useFormik<DataType>({
    initialValues: { ...data },
    enableReinitialize: true,
    validateOnChange: false,
    onSubmit: async (v) => {
      // Get updated values
      ReactUtils.updateRefValues(refs, v);

      // Submit
      let result: IdActionResult | undefined;
      let redirectUrl: string = `./../../view/${personId}`;
      if (id > 0) {
        const rq: AddressUpdateRQ = {
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

        result = await app.personAddressApi.update(rq);
      } else {
        const rq: AddressCreateRQ = {
          ...v
        };

        Utils.removeEmptyValues(rq);

        result = await app.personAddressApi.create(rq);
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
    const result = await app.personAddressApi.updateRead(id);
    if (result == null) return;

    ReactUtils.updateRefs(refs, result);

    setData({ ...result, personId });
  }, [id]);

  const parseAddress = async () => {
    const input = refs.rawAddress.current;
    if (!input) return;

    const rawAddress = input.value.trim();
    if (!rawAddress) {
      input.focus();
      return;
    }

    let [provider, items] = await app.core.publicApi.queryPlace(
      {
        query: rawAddress,
        language: app.culture,
        region: formik.values.region
      },
      { onError: () => false }
    );

    if (items != null && items.length > 0) {
      const addr = items[0];
      formik.setFieldValue("provider", provider);
      formik.setFieldValue("region", addr.region);
      ReactUtils.updateRefs(refs, addr);
    }
  };

  // Page data hook
  usePageDataEmpty(app);

  return (
    <EditPage
      isEditing={isEditing}
      onSubmit={formik.handleSubmit}
      onUpdate={reloadData}
      paddings={0}
    >
      <Grid size={{ xs: 12, sm: 12 }}>
        <InputField
          name="raw_address"
          label={labels.addressRaw}
          multiline
          rows={2}
          helperText={labels.addressRawTip}
          fullWidth
          inputRef={refs.rawAddress}
          slotProps={{
            htmlInput: { maxLength: 256 },
            input: {
              endAdornment: (
                <InputAdornment position="end">
                  <IconButton
                    title={labels.parseAddress}
                    onClick={() => parseAddress()}
                  >
                    <SpellcheckIcon />
                  </IconButton>
                </InputAdornment>
              )
            }
          }}
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 9 }}>
        <ButtonRadioRegions
          fullWidth
          required
          value={formik.values.region}
          onValueChange={(value) => {
            formik.setFieldValue("region", value);
          }}
        />
      </Grid>
      <Grid size={{ xs: 6, sm: 3 }}>
        <AddressKindList
          idValue={formik.values.kind}
          inputOnChange={formik.handleChange}
          inputRequired
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 12 }}>
        <InputField
          name="name"
          label={labels.nameB}
          slotProps={{ htmlInput: { maxLength: 128 } }}
          required
          fullWidth
          inputRef={refs.name}
        />
      </Grid>
      <Grid size={{ xs: 6, sm: 3 }}>
        <InputField
          name="state"
          label={labels.addressState}
          slotProps={{ htmlInput: { maxLength: 50 } }}
          required
          fullWidth
          inputRef={refs.state}
        />
      </Grid>
      <Grid size={{ xs: 6, sm: 3 }}>
        <InputField
          name="city"
          label={labels.addressCity}
          slotProps={{ htmlInput: { maxLength: 50 } }}
          required
          fullWidth
          inputRef={refs.city}
        />
      </Grid>
      <Grid size={{ xs: 6, sm: 3 }}>
        <InputField
          name="district"
          label={labels.addressDistrict}
          slotProps={{ htmlInput: { maxLength: 50 } }}
          fullWidth
          inputRef={refs.district}
        />
      </Grid>
      <Grid size={{ xs: 6, sm: 3 }}>
        <InputField
          name="postalCode"
          label={labels.addressPostalCode}
          slotProps={{ htmlInput: { maxLength: 10 } }}
          fullWidth
          inputRef={refs.postalCode}
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 12 }}>
        <InputField
          name="route"
          label={labels.addressRoute}
          multiline
          rows={2}
          slotProps={{ htmlInput: { maxLength: 128 } }}
          fullWidth
          inputRef={refs.route}
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 12 }}>
        <InputField
          name="street"
          label={labels.addressStreet}
          multiline
          rows={2}
          slotProps={{ htmlInput: { maxLength: 128 } }}
          fullWidth
          inputRef={refs.street}
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 12 }}>
        <AddressDuplicateTest
          multiline
          rows={2}
          fullWidth
          required
          inputRef={refs.formattedAddress}
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 6 }}>
        <AddressList
          personId={personId}
          name="parentId"
          label={labels.mainAddress}
          rq={{ excludedIds: id > 0 ? [id] : undefined, isLocation: false }}
          idValue={formik.values.parentId}
          inputOnChange={formik.handleChange}
        />
      </Grid>
    </EditPage>
  );
}
