import { CommonPage, HBox, TabBox, TabBoxPanel } from '@etsoo/materialui';
import {
  Button,
  Card,
  CardActions,
  CardContent,
  IconButton,
  List,
  ListItem,
  ListItemText,
  Typography
} from '@mui/material';
import React from 'react';
import EmailIcon from '@mui/icons-material/Email';
import PhoneIphoneIcon from '@mui/icons-material/PhoneIphone';
import DeleteIcon from '@mui/icons-material/Delete';
import FlagIcon from '@mui/icons-material/Flag';
import AddIcon from '@mui/icons-material/Add';
import { IdLabelPrimaryDto } from '@etsoo/appscript';
import { UserDialogs } from './UserDialogs';
import { app } from '../../app/SmartApp';

function PrivateData() {
  // Labels
  const labels = app.getLabels(
    'add',
    'delete',
    'defaultItem',
    'emailAddresses',
    'mobilePhones',
    'setAsDefault',
    'confirmAction'
  );

  // Create panel
  const createTab = (
    kind: string,
    label: string,
    icon: React.ReactElement,
    items: IdLabelPrimaryDto[],
    addClick: () => void
  ): TabBoxPanel => {
    // Fill items
    for (let index = items.length; index < 6; index++) {
      items.push({ id: 0, label: '\u00A0' });
    }

    // Return item
    return {
      label,
      icon,
      wrapped: true,
      children: (
        <Card sx={{ marginTop: 1 }}>
          <CardContent>
            <List disablePadding dense>
              {items.map((item, index) => (
                <ListItem
                  key={item.id === 0 ? `index${index}` : item.id}
                  disableGutters
                  disablePadding={false}
                  secondaryAction={
                    item.id > 0 ? (
                      item.isPrimary ? (
                        <Typography>{labels.defaultItem}</Typography>
                      ) : (
                        <HBox gap={0.5}>
                          <IconButton
                            edge="start"
                            size="small"
                            title={labels.setAsDefault}
                            onClick={async () => {
                              const result =
                                kind === 'Email'
                                  ? await app.userApi.emailSetAsDefault(item.id)
                                  : await app.userApi.mobileSetAsDefault(
                                      item.id
                                    );

                              if (result == null) return;

                              if (result.ok) {
                                reloadData();
                                return;
                              }

                              app.alertResult(result);
                            }}
                          >
                            <FlagIcon />
                          </IconButton>
                          <IconButton
                            edge="end"
                            size="small"
                            title={labels.delete}
                            onClick={() => {
                              app.notifier.confirm(
                                labels.confirmAction.format(labels.delete),
                                undefined,
                                async (confirmed) => {
                                  if (!confirmed) return;

                                  const result =
                                    kind === 'Email'
                                      ? await app.userApi.deleteEmail(item.id, {
                                          showLoading: false
                                        })
                                      : await app.userApi.deleteMobile(
                                          item.id,
                                          { showLoading: false }
                                        );
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
                        </HBox>
                      )
                    ) : undefined
                  }
                >
                  <ListItemText primary={item.label} />
                </ListItem>
              ))}
            </List>
          </CardContent>
          <CardActions>
            <Button
              color="primary"
              variant="outlined"
              onClick={() => addClick()}
              startIcon={<AddIcon />}
              endIcon={icon}
            >
              {labels.add}
            </Button>
          </CardActions>
        </Card>
      )
    };
  };

  // State
  const [emails, setEmails] = React.useState<IdLabelPrimaryDto[]>([]);
  const [mobiles, setMobiles] = React.useState<IdLabelPrimaryDto[]>([]);

  // Load data
  const reloadData = async () => {
    const view = await app.userApi.getPrivateData();
    if (view == null) return;

    if (view.emails) setEmails(view.emails);
    if (view.mobiles) setMobiles(view.mobiles);
  };

  React.useEffect(() => {
    // Page title
    app.setPageKey('myPrivateData');
  }, []);

  return (
    <CommonPage onUpdate={reloadData}>
      <TabBox
        tabs={[
          createTab('Email', labels.emailAddresses, <EmailIcon />, emails, () =>
            UserDialogs.addEmail(reloadData)
          ),
          createTab(
            'Mobile',
            labels.mobilePhones,
            <PhoneIphoneIcon />,
            mobiles,
            () => UserDialogs.addMobile(reloadData)
          )
        ]}
      />
    </CommonPage>
  );
}

export default PrivateData;
