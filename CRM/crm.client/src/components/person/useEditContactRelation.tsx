import { DomUtils, IdActionResult, Utils } from "@etsoo/shared";
import { app } from "../../app/MyApp";
import {
  ContactRelationAddRQ,
  ContactRelationUpdateReadData,
  ContactRelationUpdateRQ
} from "@etsoo/smarterp-crm";
import React from "react";
import { InputField, VBox } from "@etsoo/materialui";
import {
  ButtonRadioContactRelations,
  PersonList
} from "@etsoo/smarterp-crm/components";
import Button from "@mui/material/Button";
import DeleteIcon from "@mui/icons-material/Delete";

export function useEditContactRelation(
  personId: number,
  isLegalPerson: boolean | null | undefined,
  onSuccess: () => void
) {
  // Labels
  const labels = app.getLabels(
    "contact",
    "delete",
    "deleteConfirm",
    "description",
    "edit",
    "noChanges",
    "relatedTarget"
  );

  // Delete relation
  const deleteRelation = (id: number, onDeleted: () => void) => {
    app.notifier.confirm(
      labels.deleteConfirm.format(labels.contact),
      undefined,
      async (ok) => {
        if (!ok) return;

        const result = await app.personContactApi.delete(id);
        if (result == null) return;

        if (result.ok) {
          onDeleted();
        } else {
          return app.formatResult(result);
        }
      }
    );
  };

  function doRelation(id?: number, data?: ContactRelationUpdateReadData) {
    // Show
    const dialog = app.showInputDialog({
      title: labels.edit,
      message: data && (
        <Button
          startIcon={<DeleteIcon />}
          variant="outlined"
          onClick={() =>
            deleteRelation(data.id, () => {
              onSuccess();
              dialog.dismiss();
            })
          }
        >
          {labels.delete}
        </Button>
      ),
      callback: async (form) => {
        // Cancelled
        if (form == null) return;

        if (!form.reportValidity()) {
          return false;
        }

        // Form data
        const { contactId, relationType, ...rest } = DomUtils.dataAs(
          new FormData(form),
          {
            relationType: "number",
            contactId: "number",
            description: "string"
          }
        );

        let result: IdActionResult<number> | undefined;

        if (id && data) {
          // Changed fields
          const rq: ContactRelationUpdateRQ = {
            id,
            contactId,
            relationType,
            ...rest
          };

          const fields = Utils.getDataChanges(rq, data);
          if (fields.length === 0) {
            return labels.noChanges;
          }

          rq.changedFields = fields;

          // Update
          result = await app.personContactApi.updateRelation(rq);
        } else {
          if (contactId == null || relationType == null) {
            return false;
          }

          const rq: ContactRelationAddRQ = {
            personId,
            contactId,
            relationType,
            ...rest
          };

          // Create
          result = await app.personContactApi.add(rq);
        }

        if (result == null) return;

        if (result.ok) {
          onSuccess();
          return;
        }

        app.alertResult(result);
      },
      inputs: (
        <VBox spacing={2} sx={{ marginTop: 1 }}>
          <ButtonRadioContactRelations
            fullWidth
            isLegalPerson={isLegalPerson}
            value={data?.relationType}
            required
          />
          <PersonList
            name="contactId"
            label={labels.relatedTarget}
            idValue={data?.contactId}
            inputRequired
          />
          <InputField
            label={labels.description}
            name="description"
            rows={2}
            multiline
            defaultValue={data?.description}
          />
        </VBox>
      ),
      fullScreen: app.smDown
    });
  }

  return React.useCallback(
    (id?: number) => {
      if (id) {
        app.personContactApi.updateRelationRead(id).then((result) => {
          if (result == null) return;
          doRelation(id, result);
        });
      } else {
        doRelation(id);
      }
    },
    [personId]
  );
}
