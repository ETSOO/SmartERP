import { PersonProfileAttachmentItem } from "@etsoo/smarterp-crm";
import { app } from "../../app/MyApp";
import { MoreFab } from "@etsoo/materialui";
import InputAdornment from "@mui/material/InputAdornment";

type AttachmentActionsProps = {
  file: PersonProfileAttachmentItem;
  callback: () => void;
};

export function MoreAttachmentActions({
  file,
  callback
}: AttachmentActionsProps) {
  const labels = app.getLabels("actions", "delete", "edit", "fileName");
  return (
    <MoreFab
      color="inherit"
      size="small"
      iconButton
      title={labels.actions}
      anchorOrigin={{
        vertical: "bottom",
        horizontal: "right"
      }}
      transformOrigin={{
        vertical: "top",
        horizontal: "right"
      }}
      actions={[
        {
          label: labels.edit,
          action: () => {
            app.notifier.prompt(
              labels.fileName,
              async (description) => {
                if (!description || description === file.description)
                  return false;

                const result = await app.profileApi.updateAttachment(
                  { id: file.id, description },
                  { showLoading: false }
                );

                if (result == null) return false;

                if (result.ok) {
                  callback();
                } else {
                  return app.formatResult(result);
                }
              },
              labels.edit,
              {
                inputProps: {
                  type: "input",
                  defaultValue: file.description,
                  required: true,
                  slotProps: {
                    input: {
                      endAdornment: (
                        <InputAdornment position="end">
                          {file.extension}
                        </InputAdornment>
                      )
                    },
                    htmlInput: { maxLength: 128 }
                  }
                }
              }
            );
          }
        },
        { label: "-" },
        {
          label: labels.delete,
          action: async () => {
            const result = await app.profileApi.deleteAttachment(file.id);
            if (result == null) return;

            if (result.ok) {
              callback();
            } else {
              app.alertResult(result);
            }
          }
        }
      ]}
    />
  );
}
