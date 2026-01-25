import { useParamsEx, useRefs, useSearchParamsEx } from "@etsoo/react";
import { useNavigate } from "react-router-dom";
import { app } from "../../../app/MyApp";
import { usePageDataEmpty } from "@etsoo/smarterp-core";
import { EditPage } from "@etsoo/materialui";
import Grid from "@mui/material/Grid";
import {
  InfoDuplicateTest,
  InfoKindList
} from "@etsoo/smarterp-crm/components";
import {
  PersonInfoCreateRQ,
  PersonInfoItem,
  PersonInfoKind
} from "@etsoo/smarterp-crm";
import { DataTypes, DomUtils, NumberUtils } from "@etsoo/shared";
import { RefObject } from "react";

export default function AddContactInfo() {
  // Route
  const navigate = useNavigate();
  const { id: personId = 0 } = useParamsEx({
    id: "number"
  });
  const { index = 0 } = useSearchParamsEx({ index: "number" });

  // Labels
  const labels = app.getLabels("identifier", "noData");

  // Input refs
  const refFields = [
    "identifier1",
    "identifier2",
    "identifier3",
    "identifier4",
    "identifier5"
  ] as const;
  const refs = useRefs(refFields);

  // Kind change handler
  const handleKindChange = (
    event:
      | React.ChangeEvent<HTMLInputElement>
      | (Event & { target: { value: unknown; name: string } })
  ) => {
    const { name, value } = event.target;
    const inputRef =
      refs[name.replace("kind", "identifier") as (typeof refFields)[number]];

    const input = inputRef?.current;
    if (input == null) return;

    switch (value) {
      case PersonInfoKind.Email:
        input.type = "email";
        input.inputMode = "email";
        break;
      case PersonInfoKind.Phone:
      case PersonInfoKind.Mobile:
        input.type = "tel";
        input.inputMode = "tel";
        break;
      case PersonInfoKind.QQ:
        input.type = "number";
        input.inputMode = "numeric";
        break;
      case PersonInfoKind.WeChat:
        input.type = "text";
        input.inputMode = "text";
        break;
      default:
        input.type = "url";
        input.inputMode = "url";
        break;
    }

    input.disabled = false;
  };

  function getInfoKind(ref: RefObject<HTMLInputElement | null>) {
    const input = ref?.current;
    if (input == null || input.form == null) return PersonInfoKind.Email;

    const kindField = input.name.replace("identifier", "kind");

    const kindValue = (
      input.form.elements.namedItem(kindField) as HTMLInputElement
    ).value;

    return (
      DataTypes.getEnumByValue(PersonInfoKind, kindValue) ??
      PersonInfoKind.Email
    );
  }

  // Page data hook
  usePageDataEmpty(app);

  return (
    <EditPage
      isEditing={false}
      onSubmit={async (event) => {
        event.preventDefault();

        // Form data
        const formData = new FormData(event.currentTarget);

        // Check all inputs
        const isValid = event.currentTarget.reportValidity();
        if (!isValid) return;

        const items: PersonInfoItem[] = [];
        refFields.forEach((field) => {
          const inputRef = refs[field];
          const input = inputRef?.current;
          if (input == null) return;

          const kindField = field.replace("identifier", "kind");
          const kindValue = NumberUtils.parse(
            formData.get(kindField)?.toString()
          );
          if (!kindValue) {
            DomUtils.setFocus(kindField);
            return;
          }

          const kind = kindValue as PersonInfoKind;

          items.push({
            kind,
            identifier: input.value
          });
        });

        if (items.length === 0) {
          app.warning(labels.noData);
          return;
        }

        const rq: PersonInfoCreateRQ = { personId, items };

        const result = await app.personInfoApi.create(rq);
        if (result == null) return;

        if (result.ok) {
          navigate(`./../../view/${personId}?index=${index}`);
          return;
        }

        app.alertResult(result);
      }}
      paddings={0}
    >
      <Grid size={{ xs: 12, sm: 3 }}>
        <InfoKindList name="kind1" fullWidth onChange={handleKindChange} />
      </Grid>
      <Grid size={{ xs: 12, sm: 9 }}>
        <InfoDuplicateTest
          inputRef={refs.identifier1}
          infoKind={() => getInfoKind(refs.identifier1)}
          name="identifier1"
          slotProps={{ htmlInput: { disabled: true, maxLength: 256 } }}
          label={labels.identifier}
          fullWidth
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 3 }}>
        <InfoKindList name="kind2" fullWidth onChange={handleKindChange} />
      </Grid>
      <Grid size={{ xs: 12, sm: 9 }}>
        <InfoDuplicateTest
          inputRef={refs.identifier2}
          infoKind={() => getInfoKind(refs.identifier2)}
          name="identifier2"
          slotProps={{ htmlInput: { disabled: true, maxLength: 256 } }}
          label={labels.identifier}
          fullWidth
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 3 }}>
        <InfoKindList name="kind3" fullWidth onChange={handleKindChange} />
      </Grid>
      <Grid size={{ xs: 12, sm: 9 }}>
        <InfoDuplicateTest
          inputRef={refs.identifier3}
          infoKind={() => getInfoKind(refs.identifier3)}
          name="identifier3"
          slotProps={{ htmlInput: { disabled: true, maxLength: 256 } }}
          label={labels.identifier}
          fullWidth
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 3 }}>
        <InfoKindList name="kind4" fullWidth onChange={handleKindChange} />
      </Grid>
      <Grid size={{ xs: 12, sm: 9 }}>
        <InfoDuplicateTest
          inputRef={refs.identifier4}
          infoKind={() => getInfoKind(refs.identifier4)}
          name="identifier4"
          slotProps={{ htmlInput: { disabled: true, maxLength: 256 } }}
          label={labels.identifier}
          fullWidth
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 3 }}>
        <InfoKindList name="kind5" fullWidth onChange={handleKindChange} />
      </Grid>
      <Grid size={{ xs: 12, sm: 9 }}>
        <InfoDuplicateTest
          inputRef={refs.identifier5}
          infoKind={() => getInfoKind(refs.identifier5)}
          name="identifier5"
          slotProps={{ htmlInput: { disabled: true, maxLength: 256 } }}
          label={labels.identifier}
          fullWidth
        />
      </Grid>
    </EditPage>
  );
}
