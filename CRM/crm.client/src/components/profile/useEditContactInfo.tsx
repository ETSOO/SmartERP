import { DataTypes, DomUtils, Utils } from "@etsoo/shared";
import { app } from "../../app/MyApp";
import {
  PersonInfoKind,
  PersonInfoQueryData,
  PersonInfoUpdateRQ
} from "@etsoo/smarterp-crm";
import React from "react";
import { HBox, InputField, SelectBool, VBox } from "@etsoo/materialui";
import { InfoKindList } from "@etsoo/smarterp-crm/components";

export function useEditContactInfo(personId: number, onSuccess: () => void) {
  // Labels
  const labels = app.getLabels(
    "defaultItem",
    "description",
    "edit",
    "identifier",
    "noChanges",
    "subscribed"
  );

  return React.useCallback(
    (data: PersonInfoQueryData) => {
      // Show
      app.showInputDialog({
        title: labels.edit,
        message: "",
        callback: async (form) => {
          // Cancelled
          if (form == null) return;

          // Form data
          const { kind, ...rest } = DomUtils.dataAs(new FormData(form), {
            kind: "number",
            identifier: "string",
            description: "string",
            isDefault: "boolean",
            subscribed: "boolean"
          });

          const infoKind =
            (kind
              ? DataTypes.getEnumByValue(PersonInfoKind, kind)
              : undefined) ?? data.kind;

          // Changed fields
          const rq: PersonInfoUpdateRQ = {
            id: data.id,
            kind: infoKind,
            ...rest
          };

          const fields = Utils.getDataChanges(rq, data);
          if (fields.length === 0) {
            return labels.noChanges;
          }

          rq.kind ??= data.kind;
          rq.changedFields = fields;

          // Update
          const result = await app.personApi.updateInfo(rq);
          if (result == null) return;

          if (result.ok) {
            onSuccess();
            return;
          }

          app.alertResult(result);
        },
        inputs: (
          <VBox gap={2} marginTop={1}>
            <HBox gap={1}>
              <InfoKindList value={data.kind} />
              <SelectBool
                name="isDefault"
                label={labels.defaultItem}
                value={data.isDefault ?? true}
                search={false}
              />
            </HBox>
            <InputField
              label={labels.identifier}
              name="identifier"
              defaultValue={data.identifier}
            />
            <InputField
              label={labels.description}
              name="description"
              rows={2}
              multiline
              defaultValue={data.description}
            />
            <HBox gap={1}>
              <SelectBool
                name="subscribed"
                label={labels.subscribed}
                value={data.subscribed}
                autoAddBlankItem
                search={false}
              />
            </HBox>
          </VBox>
        ),
        fullScreen: app.smDown
      });
    },
    [personId]
  );
}
