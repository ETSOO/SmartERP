import { ListType } from "@etsoo/shared";
import { app } from "../../../app/MyApp";
import React from "react";
import { InputField, MaskInput, OptionGroup, VBox } from "@etsoo/materialui";
import { BusinessTax } from "@etsoo/appscript";
import { OrgTiplist } from "@etsoo/smarterp-core/components";

/**
 * Buy kind
 */
export const enum BuyKind {
  ExistingOrg = 1,
  NewOrg = 2
}

export type BuyAppProps = {
  /**
   * Kind
   */
  kind?: BuyKind;
};

export function BuyApp(props: BuyAppProps) {
  // Destruct
  const { kind: initKind } = props;

  // State
  const [kind, setKind] = React.useState(initKind);

  React.useEffect(() => setKind(initKind), [initKind]);

  // Current region
  const region = app.settings.currentRegion;

  // Tax
  const tax = region == null ? undefined : BusinessTax.getById(region.id);

  // Labels
  const labels = app.getLabels("existingOrg", "newOrg", "org", "orgName");

  // Options
  const options: ListType[] = [
    { id: 1, label: labels.existingOrg },
    { id: 2, label: labels.newOrg }
  ];

  // Layout
  return (
    <VBox gap={1} width="100%" paddingTop={1}>
      <OptionGroup
        name="kind"
        options={options}
        row
        defaultValue={kind}
        onValueChange={(value) => {
          if (value == null || Array.isArray(value)) return;
          setKind(value);
        }}
      />
      {kind === 1 && (
        <React.Fragment>
          <OrgTiplist idValue={app.userData?.organization} />
          <VBox height="60px" />
        </React.Fragment>
      )}
      {kind === 2 && (
        <React.Fragment>
          <InputField
            autoFocus
            margin="dense"
            name="name"
            label={labels.orgName}
            fullWidth
            variant="standard"
            required
            slotProps={{ htmlInput: { maxLength: 128 } }}
          />
          <MaskInput
            mask={{ mask: tax?.mask ?? "" }}
            margin="dense"
            name="pin"
            label={app.get(tax?.labelKey ?? "taxId")}
            fullWidth
            variant="standard"
            helperText={app.get((tax?.labelKey ?? "taxId") + "Help")}
            slotProps={{
              htmlInput: {
                maxLength: 20,
                style: { textTransform: "uppercase" }
              }
            }}
          />
        </React.Fragment>
      )}
    </VBox>
  );
}
