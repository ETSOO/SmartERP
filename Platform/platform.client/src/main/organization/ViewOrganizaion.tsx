import { app } from '../../app/SmartApp';
import { GridDataType, useParamsEx } from '@etsoo/react';
import { BusinessTax } from '@etsoo/appscript';
import React from 'react';
import { ButtonLink, HBox, IconButtonLink, ViewPage } from '@etsoo/materialui';
import { Divider, Grid, Paper, Typography } from '@mui/material';
import EditIcon from '@mui/icons-material/Edit';
import AddIcon from '@mui/icons-material/Add';
import { OrgApiViewDto, OrgViewSetData } from '../../api/dto/org/OrgViewSet';

function ViewOrganiztion() {
  // Permissions
  const editPermission = app.isAdminUser();

  // Route
  const { id = 0 } = useParamsEx({ id: 'number' });

  // View data
  const [apis, setApis] = React.useState<OrgApiViewDto[]>();

  // Labels
  const labels = app.getLabels('edit', 'logo', 'addApi');

  // Tax
  const tax = BusinessTax.getById(app.region);

  React.useEffect(() => {
    // Page title
    app.setPageKey('viewOrganiztion');
  }, []);

  return (
    <ViewPage<OrgViewSetData>
      fields={[
        {
          data: (item) => (
            <HBox justifyContent="center" alignItems="center">
              <Typography
                variant="subtitle2"
                textAlign="center"
                paddingRight={2}
              >
                {item.name}
              </Typography>
              {editPermission && (
                <IconButtonLink
                  href={`./../../edit/${item.id}`}
                  title={labels.edit}
                  size="small"
                >
                  <EditIcon />
                </IconButtonLink>
              )}
            </HBox>
          ),
          singleRow: true
        },
        {
          data: (item) => (
            <HBox>
              <img
                src={item.avatar}
                alt={labels.logo}
                style={{
                  width: '160px',
                  height: '80px',
                  border: '1px solid #666'
                }}
              />
              <IconButtonLink
                href={`./../../avatar/${item.id}`}
                state={item.avatar}
                title={labels.edit}
                size="small"
              >
                <EditIcon />
              </IconButtonLink>
            </HBox>
          ),
          singleRow: false
        },
        {
          data: 'tradeAs',
          label: 'tradeAs',
          singleRow: false
        },
        {
          data: 'brand',
          label: 'brand',
          singleRow: false
        },
        {
          data: 'identifier',
          label: app.get(tax?.labelKey ?? 'taxId'),
          singleRow: false
        },
        {
          data: (item) =>
            item.parentName ? (
              <ButtonLink
                href={`./../${item.parentId}`}
                size="small"
                variant="outlined"
              >
                {item.parentName}
              </ButtonLink>
            ) : undefined,
          label: 'parentOrg'
        },
        ['expiry', GridDataType.DateTime],
        {
          data: (item) => app.getStatusLabel(item.entityStatus),
          label: 'status'
        },
        ['creation', GridDataType.DateTime]
      ]}
      loadData={async () => {
        const result = await app.orgApi.readUI(id);
        if (result == null) return;
        setApis(result.apis);
        return result.data;
      }}
    >
      {(data) => (
        <Grid container spacing={1}>
          <Grid item xs={12} sm={12} textAlign="right">
            <ButtonLink
              href={`./../../addapi?org=${data.id}`}
              state={data}
              variant="contained"
              startIcon={<AddIcon />}
            >
              {labels.addApi}
            </ButtonLink>
          </Grid>
          <Grid item xs={12} sm={12}>
            <Divider />
          </Grid>
          {apis?.map((c) => {
            return (
              <Grid item xs={6} sm={4} lg={3} xl={2} key={c.id}>
                <Paper sx={{ padding: 2 }}>
                  <HBox alignItems="center">
                    <Typography
                      variant="subtitle2"
                      fontWeight="bold"
                      flex={1}
                      sx={{
                        textDecoration: c.enabled ? undefined : 'line-through'
                      }}
                    >
                      {c.title}
                    </Typography>
                    <IconButtonLink
                      title={labels.edit}
                      size="small"
                      href={`./../../editapi/${c.id}?org=${data.id}`}
                    >
                      <EditIcon />
                    </IconButtonLink>
                  </HBox>
                  <Typography variant="body2">{c.appId}</Typography>
                  <Typography variant="body2">
                    {app.formatDate(c.creation)}
                  </Typography>
                </Paper>
              </Grid>
            );
          })}
        </Grid>
      )}
    </ViewPage>
  );
}

export default ViewOrganiztion;
