import TranslateIcon from "@mui/icons-material/Translate";
import { Button } from "@mui/material";
import { app } from "../app/MyApp";
import { InputField, VBox } from "@etsoo/materialui";
import React from "react";
import { CultureList } from "./CultureList";
import { CustomCultureKind } from "@etsoo/smarterp-crm";
import { DomUtils } from "@etsoo/shared";

function CultureInput(props: NameCulturePros) {
  // Destruct
  const { kind, id, isProduct = kind === CustomCultureKind.Product } = props;

  // Label
  const labels = app.getLabels("description", "title");

  // Refs
  const titleRef = React.useRef<HTMLInputElement>(null);
  const descriptionRef = React.useRef<HTMLInputElement>(null);

  // Maxlength
  const maxLength = isProduct ? 512 : 256;

  // Layout
  return (
    <VBox spacing={1} sx={{ paddingTop: 1 }}>
      <CultureList
        fullWidth
        onItemChange={(item) => {
          const titleInput = titleRef.current;
          if (titleInput == null) return;

          titleInput.disabled = true;
          titleInput.value = "";

          if (descriptionRef.current) descriptionRef.current.value = "";

          if (item) {
            app.systemApi
              .readCulture({ id, culture: item.id, kind })
              .then((data) => {
                titleInput.disabled = false;

                if (data == null) return;

                titleInput.value = data.title;

                if (descriptionRef.current)
                  descriptionRef.current.value = data.description || "";
              });
          }
        }}
        required
      />
      <InputField
        fullWidth
        name="title"
        slotProps={{
          htmlInput: { maxLength, disabled: true }
        }}
        label={labels.title}
        inputRef={titleRef}
        multiline
        required
        rows={2}
      />
      {isProduct && (
        <InputField
          fullWidth
          name="description"
          slotProps={{
            htmlInput: { maxLength: 10 * maxLength }
          }}
          label={labels.description}
          inputRef={descriptionRef}
          multiline
          rows={3}
        />
      )}
    </VBox>
  );
}

export type NameCulturePros = {
  /**
   * Id
   * 编号
   */
  id: number;

  /**
   * Is product
   * 是否为产品
   */
  isProduct?: boolean;

  /**
   * Kind
   * 类型
   */
  kind: CustomCultureKind;

  /**
   * Success handler
   */
  onSuccess?: () => void;
};

export function NameCulture(props: NameCulturePros) {
  // Label
  const labels = app.getLabels("culture", "cultures");

  function showUI() {
    app.showInputDialog({
      title: labels.culture,
      message: "",
      callback: async (form) => {
        // Cancelled
        if (form == null) return;

        // Validate form
        if (!form.reportValidity()) {
          return false;
        }

        // Form data
        const { culture, title, description } = DomUtils.dataAs(
          new FormData(form),
          {
            culture: "string",
            title: "string",
            description: "string"
          }
        );

        if (!culture) {
          DomUtils.setFocus("culture", form);
          return false;
        }

        if (!title) {
          DomUtils.setFocus("title", form);
          return false;
        }

        const result = await app.systemApi.updateCulture({
          id: props.id,
          kind: props.kind,
          culture,
          title,
          description
        });

        if (result == null) return;

        if (result.ok) {
          props.onSuccess?.();
          return true;
        } else {
          return app.formatResult(result);
        }
      },
      inputs: <CultureInput {...props} />
    });
  }

  return (
    <Button
      startIcon={<TranslateIcon />}
      variant="outlined"
      onClick={() => showUI()}
    >
      {labels.cultures}
    </Button>
  );
}
