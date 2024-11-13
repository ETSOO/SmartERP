import React from "react";
import { Divider, ListItemIcon, ListItemText, MenuItem } from "@mui/material";
import ExitToAppIcon from "@mui/icons-material/ExitToApp";
import { UserMenu, UserMenuLocalProps } from "@etsoo/materialui";
import { app } from "./MyApp";

export function UserMenuLocal(props: UserMenuLocalProps) {
  // Destruct
  const { organization, smDown, ...rest } = props;

  // Labels
  const labels = app.getLabels(
    "switchOrganization",
    "updateAvatar",
    "changePassword",
    "signout",
    "more",
    "newOrganization",
    "myInvitationCode",
    "myPrivateData",
    "signoutSuccess"
  );

  return (
    <React.Fragment>
      <UserMenu organization={organization} {...rest}>
        {(handleMenuClose) => [
          <Divider key="divider2" />,
          <MenuItem
            key="signout"
            onClick={async () => {
              // Sign out
              await app.signout(() => {
                app.notifier.alert(
                  labels.signoutSuccess,
                  undefined,
                  undefined,
                  {
                    fullScreen: true,
                    primaryButton: false
                  }
                );
                return false;
              });
              handleMenuClose();
            }}
          >
            <ListItemIcon>
              <ExitToAppIcon fontSize="small" />
            </ListItemIcon>
            <ListItemText>{labels.signout}</ListItemText>
          </MenuItem>
        ]}
      </UserMenu>
    </React.Fragment>
  );
}
