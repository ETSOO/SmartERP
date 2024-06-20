import React from 'react';
import { Divider, ListItemIcon, ListItemText, MenuItem } from '@mui/material';
import ExitToAppIcon from '@mui/icons-material/ExitToApp';
import AccountCircleIcon from '@mui/icons-material/AccountCircle';
import LockIcon from '@mui/icons-material/Lock';
import AddCircleIcon from '@mui/icons-material/AddCircle';
import AccountTreeIcon from '@mui/icons-material/AccountTree';
import QrCodeIcon from '@mui/icons-material/QrCode';
import VerifiedUserIcon from '@mui/icons-material/VerifiedUser';
import {
  eventWatcher,
  SelectEx,
  UserMenu,
  UserMenuLocalProps
} from '@etsoo/materialui';
import { ExtendUtils } from '@etsoo/shared';
import { MemberDialogs } from '../main/member/MemberDialogs';
import { app } from './SmartApp';
import { useNavigate } from 'react-router-dom';
import { AppCache } from './AppCache';

const maxOrganizationItems = 16;
const oaction = 'usermenu.oaction';

export function UserMenuLocal(props: UserMenuLocalProps) {
  // Destruct
  const { organization, smDown, ...rest } = props;

  // Route
  const navigate = useNavigate();

  // Labels
  const labels = app.getLabels(
    'switchOrganization',
    'updateAvatar',
    'changePassword',
    'signout',
    'more',
    'newOrganization',
    'myInvitationCode',
    'myPrivateData'
  );

  // My invitation code
  const handleInvitationCode = () => {
    MemberDialogs.myInvitationCode();
  };

  return (
    <React.Fragment>
      {!smDown && (
        <SelectEx
          search
          autoAddBlankItem={false}
          title={labels.switchOrganization}
          sx={{ width: 280, marginRight: 1 }}
          value={organization}
          loadData={async () => {
            await ExtendUtils.sleep(100);
            return await app.orgApi.list(maxOrganizationItems);
          }}
          onLoadData={(options) => {
            if (options.length === maxOrganizationItems) {
              options.push({ id: 0, label: labels.more });
            } else {
              options.push({ id: -1, label: labels.newOrganization });
            }
          }}
          itemIconRenderer={(id) => {
            if (id === 0) {
              return <AccountTreeIcon fontSize="small" />;
            }
            if (id === -1) {
              return <AddCircleIcon fontSize="small" />;
            }
            return undefined;
          }}
          onTransitionEnd={() => eventWatcher.do(oaction)}
          onChange={(event) => {
            const id = event.target.value as number;
            if (id <= 0) {
              event.stopPropagation();
              event.preventDefault();
            }

            AppCache.switchOrg();

            eventWatcher.add({
              type: oaction,
              action: () =>
                id === 0
                  ? navigate('./organization/all')
                  : id === -1
                  ? navigate('./service/all', { state: { kind: 2 } })
                  : app.orgApi.switch(id),
              once: true
            });
          }}
        />
      )}
      <UserMenu organization={organization} {...rest}>
        {(handleMenuClose) => [
          <MenuItem key="myInvitationCode" onClick={handleInvitationCode}>
            <ListItemIcon>
              <QrCodeIcon fontSize="small" />
            </ListItemIcon>
            <ListItemText>{labels.myInvitationCode}</ListItemText>
          </MenuItem>,
          <MenuItem key="myPrivateData" href="./user/privatedata">
            <ListItemIcon>
              <VerifiedUserIcon fontSize="small" />
            </ListItemIcon>
            <ListItemText>{labels.myPrivateData}</ListItemText>
          </MenuItem>,
          <Divider key="divider1" />,
          <MenuItem key="updateAvatar" href="./user/updateavatar">
            <ListItemIcon>
              <AccountCircleIcon fontSize="small" />
            </ListItemIcon>
            <ListItemText>{labels.updateAvatar}</ListItemText>
          </MenuItem>,
          <MenuItem key="changePassword" href="./user/changepassword">
            <ListItemIcon>
              <LockIcon fontSize="small" />
            </ListItemIcon>
            <ListItemText>{labels.changePassword}</ListItemText>
          </MenuItem>,
          <Divider key="divider2" />,
          <MenuItem
            key="signout"
            onClick={async () => {
              // Sign out
              await app.signout();
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
