import { ButtonLink, CommonPage } from "@etsoo/materialui";
import { app } from "../../../app/MyApp";
import DeleteIcon from "@mui/icons-material/Delete";
import EmailIcon from "@mui/icons-material/Email";
import PhoneIphoneIcon from "@mui/icons-material/PhoneIphone";
import EditIcon from "@mui/icons-material/Edit";
import { UserIdentifierData } from "@etsoo/smarterp-core";
import React from "react";
import { UserIdentifierType } from "@etsoo/appscript";
import Card from "@mui/material/Card";
import CardContent from "@mui/material/CardContent";
import Grid from "@mui/material/Grid";
import CardActions from "@mui/material/CardActions";
import List from "@mui/material/List";
import ListItem from "@mui/material/ListItem";
import IconButton from "@mui/material/IconButton";
import ListItemText from "@mui/material/ListItemText";

export default function UserData() {
  // Labels
  const labels = app.getLabels(
    "addEmail",
    "addMobile",
    "confirmAction",
    "delete",
    "edit"
  );

  // User context
  const Context = app.userState.context;

  // State
  const [items, setItems] = React.useState<UserIdentifierData[]>([]);

  // Load data
  const reloadData = React.useCallback(async () => {
    const data = await app.core.userApi.allIdentifiers();
    setItems(data ?? []);
  }, []);

  // Check deletable
  const checkDeletable = React.useCallback(
    (item: UserIdentifierData) => {
      if (
        item.type === UserIdentifierType.Email ||
        item.type === UserIdentifierType.Mobile
      ) {
        return items.filter((i) => i.type === item.type).length > 1;
      }
      return true;
    },
    [items]
  );

  return (
    <CommonPage paddings={0} onRefresh={reloadData}>
      <Card>
        <CardContent>
          <Context.Consumer>
            {({ state }) => (
              <Grid container spacing={1}>
                <Grid size={{ xs: 6, md: 4 }}>{state.name}</Grid>
                {(state.familyName || state.givenName) && (
                  <Grid size={{ xs: 6, md: 4 }}>
                    {state.familyName} / {state.givenName}
                  </Grid>
                )}
                {(state.latinFamilyName || state.latinGivenName) && (
                  <Grid size={{ xs: 6, md: 4 }}>
                    {state.latinFamilyName} / {state.latinGivenName}
                  </Grid>
                )}
              </Grid>
            )}
          </Context.Consumer>
        </CardContent>
        <CardActions sx={{ justifyContent: "flex-end" }}>
          <ButtonLink
            color="primary"
            variant="outlined"
            startIcon={<EditIcon />}
            href="./edit"
          >
            {labels.edit}
          </ButtonLink>
        </CardActions>
      </Card>
      <Card sx={{ mt: 2 }}>
        <CardContent>
          <List disablePadding>
            {items.map((item, index) => (
              <ListItem
                key={item.id}
                sx={{
                  backgroundColor: (theme) =>
                    index % 2 === 0
                      ? theme.palette.grey[100]
                      : theme.palette.grey[50]
                }}
                secondaryAction={
                  checkDeletable(item) && (
                    <IconButton
                      edge="end"
                      size="small"
                      title={labels.delete}
                      onClick={() => {
                        app.notifier.confirm(
                          labels.confirmAction.format(labels.delete),
                          item.value,
                          async (confirmed) => {
                            if (!confirmed) return;

                            const result =
                              await app.core.userApi.deleteIdentifier(item.id, {
                                showLoading: false
                              });

                            if (result == null) return;

                            if (result.ok) {
                              reloadData();
                              return;
                            }

                            app.alertResult(result);
                            return false;
                          }
                        );
                      }}
                    >
                      <DeleteIcon />
                    </IconButton>
                  )
                }
              >
                <ListItemText
                  primary={item.value}
                  secondary={
                    app.core.getIdentifierTypeLabel(item.type) +
                    ", " +
                    app.formatDate(item.creation)
                  }
                />
              </ListItem>
            ))}
          </List>
        </CardContent>
        <CardActions sx={{ justifyContent: "flex-end" }}>
          <ButtonLink
            color="primary"
            variant="outlined"
            startIcon={<EmailIcon />}
            href="./addemail"
          >
            {labels.addEmail}
          </ButtonLink>
          <ButtonLink
            color="primary"
            variant="outlined"
            startIcon={<PhoneIphoneIcon />}
            href="./addmobile"
          >
            {labels.addMobile}
          </ButtonLink>
        </CardActions>
      </Card>
    </CommonPage>
  );
}
