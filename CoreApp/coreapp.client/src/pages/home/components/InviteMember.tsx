import { ComboBox, EmailInput, VBox } from "@etsoo/materialui";
import { app } from "../../../app/MyApp";
import { UserRole } from "@etsoo/appscript";
import { TextField, Typography } from "@mui/material";

export function InviteMember() {
  // Labels
  const labels = app.getLabels(
    "email",
    "inviteMemberMessage",
    "inviteMemberTip",
    "role"
  );

  // Roles, UserRole.User and items below it
  const roles = app.getRoles(2 * UserRole.User - 1);

  return (
    <VBox>
      <ComboBox
        options={roles}
        name="userRole"
        label={labels.role}
        idValue={8}
        inputVariant="standard"
        inputMargin="dense"
        inputRequired
      />
      <EmailInput
        autoFocus
        name="emails"
        label={labels.email + " 1"}
        required
        variant="standard"
        margin="dense"
      />
      <EmailInput
        name="emails"
        label={labels.email + " 2"}
        variant="standard"
        margin="dense"
      />
      <EmailInput
        name="emails"
        label={labels.email + " 3"}
        variant="standard"
        margin="dense"
      />
      <TextField
        autoFocus
        margin="dense"
        name="message"
        label={labels.inviteMemberMessage}
        fullWidth
        variant="standard"
        slotProps={{ htmlInput: { maxLength: 128 } }}
      />
      <Typography variant="caption">{labels.inviteMemberTip}</Typography>
    </VBox>
  );
}
