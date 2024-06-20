import { ListItemButton, ListItemIcon, ListItemText } from '@mui/material';
import HistoryIcon from '@mui/icons-material/History';
import HomeIcon from '@mui/icons-material/Home';
import AccountTreeIcon from '@mui/icons-material/AccountTree';
import PeopleIcon from '@mui/icons-material/People';
import AppsIcon from '@mui/icons-material/Apps';
import PaidIcon from '@mui/icons-material/Paid';
import React from 'react';
import { app } from './SmartApp';
import { useLocation } from 'react-router-dom';
import { LeftDrawer, LeftDrawerProps, MUGlobal } from '@etsoo/materialui';

export function LeftDrawerLocal(props: LeftDrawerProps) {
  // Destruct
  const { organization } = props;

  // Location
  // Reload when location changes
  const pathname = useLocation().pathname.replace('/home/', './');

  const getMenuItem = React.useCallback(
    (href: string) => MUGlobal.getMenuItem(pathname, href),
    [pathname]
  );

  // Labels
  const labels = app.getLabels(
    'etsoo',
    'smartERP',
    'hideMenu',
    'menuHome',
    'menuLoginHistory',
    'organizations',
    'members',
    'servicesPurchased',
    'servicesAll'
  );

  return (
    <LeftDrawer {...props}>
      <ListItemButton {...getMenuItem('./')}>
        <ListItemIcon>
          <HomeIcon />
        </ListItemIcon>
        <ListItemText primary={labels.menuHome} />
      </ListItemButton>
      {organization && (
        <React.Fragment>
          <ListItemButton {...getMenuItem('./organization/all')}>
            <ListItemIcon>
              <AccountTreeIcon />
            </ListItemIcon>
            <ListItemText primary={labels.organizations} />
          </ListItemButton>
          <ListItemButton {...getMenuItem('./member/all')}>
            <ListItemIcon>
              <PeopleIcon />
            </ListItemIcon>
            <ListItemText primary={labels.members} />
          </ListItemButton>
          {app.isFinanceUser() && (
            <ListItemButton {...getMenuItem('./service/my')}>
              <ListItemIcon>
                <PaidIcon />
              </ListItemIcon>
              <ListItemText primary={labels.servicesPurchased} />
            </ListItemButton>
          )}
        </React.Fragment>
      )}
      <ListItemButton {...getMenuItem('./service/all')}>
        <ListItemIcon>
          <AppsIcon />
        </ListItemIcon>
        <ListItemText primary={labels.servicesAll} />
      </ListItemButton>
      <ListItemButton {...getMenuItem('./user/loginhistory')}>
        <ListItemIcon>
          <HistoryIcon />
        </ListItemIcon>
        <ListItemText primary={labels.menuLoginHistory} />
      </ListItemButton>
    </LeftDrawer>
  );
}
