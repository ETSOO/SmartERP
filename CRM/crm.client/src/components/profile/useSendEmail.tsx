import React from "react";
import { app } from "../../app/MyApp";
import Grid from "@mui/material/Grid";
import TextField from "@mui/material/TextField";
import { Checkbox, FormControlLabel, Typography } from "@mui/material";
import { DomUtils } from "@etsoo/shared";
import { PersonChooser } from "@etsoo/smarterp-crm/components";

type SendEmailProps = {
  personId: number;
};

function SendEmail(props: SendEmailProps) {
  // Desturture
  const { personId } = props;

  // Labels
  const labels = app.getLabels(
    "includeAttachments",
    "includeComments",
    "message"
  );

  // Layout
  return (
    <Grid container spacing={1}>
      <PersonChooser name="persons" personId={personId} />
      <Grid size={{ xs: 12, sm: 6 }}>
        <FormControlLabel
          control={<Checkbox name="includeComments" />}
          label={labels.includeComments}
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 6 }}>
        <FormControlLabel
          control={<Checkbox name="includeAttachments" />}
          label={labels.includeAttachments}
        />
      </Grid>
      <Grid size={12}>
        <Typography variant="caption">{labels.message}:</Typography>
        <TextField name="message" multiline rows={2} fullWidth />
      </Grid>
    </Grid>
  );
}

export function useSendEmail(id: number, personId: number) {
  // Labels
  const labels = app.getLabels("noChanges", "sendEmail");

  return React.useCallback(() => {
    app.showInputDialog({
      title: labels.sendEmail,
      message: "",
      callback: async (form) => {
        // Cancelled
        if (form == null) return;

        // Form data
        const data = DomUtils.dataAs(new FormData(form), {
          includeComments: "boolean",
          includeAttachments: "boolean",
          message: "string",
          persons: "number[]"
        });

        if (data.persons == null || data.persons.length === 0) {
          return false;
        }

        const result = await app.core.orgApi.sendProfileEmail(
          { id, persons: [], ...data },
          { showLoading: false }
        );
        if (result == null) {
          return false;
        }

        if (!result.ok) {
          return app.formatResult(result);
        }
      },
      inputs: <SendEmail personId={personId} />,
      fullScreen: app.smDown
    });
  }, [id, personId]);
}
