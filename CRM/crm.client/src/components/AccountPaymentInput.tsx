import React from "react";
import { MoneyInputField, MoneyInputFieldProps } from "@etsoo/materialui";
import IconButton from "@mui/material/IconButton";
import InputAdornment from "@mui/material/InputAdornment";
import DoneAllIcon from "@mui/icons-material/DoneAll";
import { app } from "../app/MyApp";
import { ReactUtils, useCombinedRefs } from "@etsoo/react";

export type AccountPaymentInputProps = MoneyInputFieldProps & {
  onFullAmount:
    | number
    | ((
        input: HTMLInputElement,
        e: React.MouseEvent<HTMLButtonElement, MouseEvent>
      ) => void);
};

export function AccountPaymentInput(props: AccountPaymentInputProps) {
  // Destruct
  const {
    fullWidth = true,
    inputRef,
    onChange,
    onChangeDelay,
    onFullAmount,
    slotProps = {},
    ...rest
  } = props;

  slotProps.input ??= {};

  const fullAmountLabel = app.get("fullAmount");

  const fullAmountHandler =
    typeof onFullAmount === "number"
      ? (
          sourceInput: HTMLInputElement,
          _e: React.MouseEvent<HTMLButtonElement, MouseEvent>
        ) => {
          const form = sourceInput.form;
          if (form == null) return;

          let total = 0;
          const elements = form.elements;
          for (let i = 0; i < elements.length; i++) {
            const element = elements[i];
            if (
              element instanceof HTMLInputElement &&
              element.value !== "" &&
              element.name.startsWith("account")
            ) {
              total += Number(element.value) || 0;
            }
          }

          const max = parseInt(sourceInput.max);
          const balance = onFullAmount - total;
          const balanceText =
            balance > max ? max.toString() : balance.toString();

          ReactUtils.triggerChange(
            sourceInput,
            sourceInput.value === "" ? balanceText : ""
          );
        }
      : onFullAmount;

  const localRef = React.useRef<HTMLInputElement>(null);

  Object.assign(slotProps.input, {
    endAdornment: (
      <InputAdornment position="end">
        <IconButton
          title={fullAmountLabel}
          onClick={(e) => fullAmountHandler(localRef.current!, e)}
        >
          <DoneAllIcon />
        </IconButton>
      </InputAdornment>
    )
  });

  // Layout
  return (
    <MoneyInputField
      fullWidth={fullWidth}
      onChange={onChange}
      onChangeDelay={onChangeDelay}
      slotProps={slotProps}
      inputRef={useCombinedRefs(localRef, inputRef)}
      {...rest}
    />
  );
}
