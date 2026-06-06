import React from "react";
import { InputField } from "@etsoo/materialui";
import { ButtonRadioRegions } from "@etsoo/smarterp-core/components";
import Grid from "@mui/material/Grid";
import { app } from "../../app/MyApp";
import { ReactUtils, useRefs } from "@etsoo/react";
import InputAdornment from "@mui/material/InputAdornment";
import IconButton from "@mui/material/IconButton";
import SpellcheckIcon from "@mui/icons-material/Spellcheck";
import { AddressCreateRQ, AddressKind } from "@etsoo/smarterp-crm";
import useMediaQuery from "@mui/material/useMediaQuery";
import { useTheme } from "@mui/material/styles";
import { PlaceCommon } from "@etsoo/smarterp-core";
import { MapApiProvider } from "@etsoo/appscript";
import { DataTypes, DomUtils } from "@etsoo/shared";
import { AddressKindList } from "@etsoo/smarterp-crm/components";

const refFields = [
  "personId",
  "placeId",
  "provider",
  "kind",
  "rawAddress",
  "name",
  "region",
  "state",
  "city",
  "district",
  "postalCode",
  "route",
  "street",
  "formattedAddress"
] as const;

export type AddressPageProps = {
  /**
   * Initial data
   */
  data?: AddressCreateRQ;

  /**
   * Is legal person
   */
  isLegalPerson: boolean;
};

function getAddressKind(isLegalPerson: boolean) {
  return isLegalPerson ? AddressKind.Office : AddressKind.Home;
}

export function createEmptyAddress(
  isLegalPerson: boolean,
  personId: number = 0
): AddressCreateRQ {
  return {
    personId,
    kind: getAddressKind(isLegalPerson),
    provider: MapApiProvider.Google,
    name: "",
    region: "",
    state: "",
    city: "",
    formattedAddress: ""
  };
}

export function createAddressFromForm(
  form: HTMLFormElement
): AddressCreateRQ | undefined {
  // Form data
  const {
    personId,
    placeId,
    provider,
    kind = "1",
    name,
    region,
    state,
    city,
    district,
    postalCode,
    route,
    street,
    formattedAddress
  } = DomUtils.dataAs(
    new FormData(form),
    Object.fromEntries(refFields.map((key) => [key, "string"]))
  );

  const providerValue =
    DataTypes.getEnumByValue(MapApiProvider, provider as any) ??
    MapApiProvider.Google;

  const kindValue = DataTypes.getEnumByValue(
    AddressKind,
    parseInt(kind) as any
  );

  if (!kindValue) {
    form.querySelector<HTMLInputElement>('input[name="kindInput"]')?.focus();
    return;
  }

  if (!name) {
    form.querySelector<HTMLInputElement>('input[name="name"]')?.focus();
    return;
  }

  if (!region) {
    form.querySelector<HTMLInputElement>('input[name="region"]')?.focus();
    return;
  }

  if (!state) {
    form.querySelector<HTMLInputElement>('input[name="state"]')?.focus();
    return;
  }

  if (!city) {
    form.querySelector<HTMLInputElement>('input[name="city"]')?.focus();
    return;
  }

  if (!formattedAddress) {
    form
      .querySelector<HTMLInputElement>('input[name="formattedAddress"]')
      ?.focus();
    return;
  }

  return {
    personId: Number(personId || "0"),
    placeId: placeId ?? "",
    provider: providerValue,
    kind: kindValue,
    name,
    region,
    state,
    city,
    district,
    postalCode,
    route,
    street,
    formattedAddress
  };
}

export function enrichPlace(
  provider: MapApiProvider,
  place: PlaceCommon,
  isLegalPerson: boolean
): AddressCreateRQ {
  return {
    personId: 0,
    provider,
    kind: getAddressKind(isLegalPerson),
    placeId: place.placeId,
    name: place.name,
    region: place.region ?? "",
    state: place.state ?? "",
    city: place.city ?? "",
    district: place.district,
    route: place.route,
    street: place.street,
    postalCode: place.postalCode,
    formattedAddress: place.formattedAddress,
    location: place.location
  };
}

export function AddressPage(props: AddressPageProps) {
  // Destruct
  const { isLegalPerson, data = createEmptyAddress(isLegalPerson) } = props;

  // Labels
  const labels = app.getLabels(
    "addressCity",
    "addressDistrict",
    "addressFormatted",
    "addressPostalCode",
    "addressRaw",
    "addressRawTip",
    "addressRoute",
    "addressState",
    "nameB",
    "addressStreet",
    "noChanges",
    "parseAddress",
    "status",
    "type"
  );

  // Input refs
  const refs = useRefs(refFields);

  const theme = useTheme();
  const isLG = useMediaQuery(theme.breakpoints.up("md"));
  const rows = isLG ? 1 : 2;

  const [formData, setFormData] = React.useState<AddressCreateRQ>(data);

  React.useEffect(() => {
    setFormData(data);
  }, [data.placeId]);

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
        region: formData.region,
        pageSize: 3
      },
      { onError: () => false }
    );

    if (items != null && items.length > 0) {
      const addr = items[0];
      const place = enrichPlace(provider, addr, isLegalPerson);
      setFormData(place);
    }
  };

  React.useEffect(() => {
    ReactUtils.updateRefs(refs, formData);
  }, [formData.placeId]);

  return (
    <>
      <input type="hidden" name="personId" ref={refs.personId} />
      <input type="hidden" name="placeId" ref={refs.placeId} />
      <input type="hidden" name="provider" ref={refs.provider} />
      <Grid size={{ xs: 12, sm: 12 }}>
        <InputField
          name="raw_address"
          label={labels.addressRaw}
          multiline
          rows={rows}
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
          value={formData.region}
          onValueChange={(value) => (formData.region = value)}
        />
      </Grid>
      <Grid size={{ xs: 6, sm: 3 }}>
        <AddressKindList idValue={formData.kind} inputRequired />
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
          rows={rows}
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
          rows={rows}
          slotProps={{ htmlInput: { maxLength: 128 } }}
          fullWidth
          inputRef={refs.street}
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 12 }}>
        <InputField
          name="formattedAddress"
          label={labels.addressFormatted}
          multiline
          rows={rows}
          slotProps={{ htmlInput: { maxLength: 256 } }}
          fullWidth
          required
          inputRef={refs.formattedAddress}
        />
      </Grid>
    </>
  );
}
