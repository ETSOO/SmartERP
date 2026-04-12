import React from "react";
import { HBox, MUGlobal } from "@etsoo/materialui";
import {
  AddressTiplist,
  AddressTiplistProps
} from "@etsoo/smarterp-core/components";
import { Grid, IconButton } from "@mui/material";
import EditIcon from "@mui/icons-material/Edit";
import { app } from "../../app/MyApp";
import { AddressPage, createAddressFromForm, enrichPlace } from "./AddressPage";
import { AddressCreateRQ } from "@etsoo/smarterp-crm";

export type AddressCreatorProps = AddressTiplistProps & {
  /**
   * Is legal person
   */
  isLegalPerson: boolean;

  /**
   * Address change handler
   * @param address Address data
   */
  onAddressChange: (address?: AddressCreateRQ) => void;
};

export function AddressCreator(props: AddressCreatorProps) {
  // Destruct
  const { isLegalPerson, onAddressChange, sx, ...rest } = props;

  // Labels
  const labels = app.getLabels("address", "edit");

  // Data
  const addressRef = React.useRef<AddressCreateRQ | undefined>(undefined);

  function updateAddress(address?: AddressCreateRQ) {
    addressRef.current = address;
    onAddressChange(address);
  }

  const edit = () => {
    app.showInputDialog({
      title: labels.address,
      message: "",
      callback: async (form) => {
        if (form == null) return;

        const data = createAddressFromForm(form);
        if (data == null) return false;

        updateAddress(data);

        return true;
      },
      inputs: (
        <Grid
          container
          spacing={MUGlobal.pagePaddings}
          sx={{ justifyContent: "left", paddingTop: 1 }}
        >
          <AddressPage
            isLegalPerson={isLegalPerson}
            data={addressRef.current}
          />
        </Grid>
      ),
      fullScreen: true
    });
  };

  return (
    <HBox spacing={0.5} sx={{ alignItems: "center" }}>
      <AddressTiplist
        {...rest}
        sx={{ flex: 2, ...sx }}
        onValueChange={(value, provider) => {
          if (value == null) {
            updateAddress(undefined);
          } else {
            updateAddress(enrichPlace(provider, value, isLegalPerson));
          }
        }}
      />
      <IconButton
        title={labels.edit}
        sx={{ flex: 0, mt: 1 }}
        onClick={() => edit()}
      >
        <EditIcon />
      </IconButton>
    </HBox>
  );
}
