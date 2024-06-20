import { EntityStatus, UserRole } from '@etsoo/appscript';
import {
  MUGlobal,
  ResponsivePage,
  SearchField,
  ComboBox,
  IconButtonLink,
  MobileListItemRenderer,
  Switch
} from '@etsoo/materialui';
import { BoxProps, Fab, IconButton, Typography } from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import EditIcon from '@mui/icons-material/Edit';
import PersonRemoveIcon from '@mui/icons-material/PersonRemove';
import React from 'react';
import { MemberDialogs } from './MemberDialogs';
import { app } from '../../app/SmartApp';
import {
  GridCellRendererProps,
  GridDataType,
  GridDeletedCellBoxStyle,
  ScrollerListForwardRef
} from '@etsoo/react';
import { MemberQueryDto } from '../../api/dto/member/MemberQueryDto';
import { AppCache } from '../../app/AppCache';
import { useNavigate } from 'react-router-dom';

function AllMembers() {
  // Route
  const navigate = useNavigate();

  // Roles
  const roles = app.getRoles(UserRole.Founder * 2 - 1);

  const getRoleLabel = (data?: MemberQueryDto) => {
    if (data == null) return '';
    return app
      .getRoles(data.entityRole)
      .map((item) => item.label)
      .join(', ');
  };

  const deleteMember = (id: string, name: string) => {
    app.notifier.confirm(
      labels.confirmAction.format(labels.remove),
      undefined,
      async (confirmed) => {
        if (!confirmed) return;
        const result = await app.memberApi.delete(id);
        if (result == null) return;

        if (result.ok) {
          app.orgApi.sendActionMessage({
            kind: 'delete-member',
            title: app.formatAction(labels.remove, name)
          });

          reloadData();
          return;
        }

        app.alertResult(result);
      }
    );
  };

  // Edit permission
  const editPermission = app.isHRUser();

  // Labels
  const labels = app.getLabels(
    'id',
    'name',
    'organization',
    'creation',
    'actions',
    'role',
    'externalId',
    'inviteMember',
    'edit',
    'remove',
    'inactivated',
    'entityStatus',
    'confirmAction',
    'statusNormal'
  );

  // Current organization
  const organization = app.userData?.organization;

  // Refs
  const ref = React.useRef<ScrollerListForwardRef<MemberQueryDto>>();

  // Load data
  const reloadData = () => ref.current?.reset();

  const margin = MUGlobal.pagePaddings;

  React.useEffect(() => {
    // Page title
    app.setPageKey('members');
  }, []);

  return (
    <ResponsivePage<
      MemberQueryDto,
      {
        name: 'string';
        role: 'number';
        externalId: 'string';
        enabled: 'boolean';
      }
    >
      mRef={ref}
      defaultOrderBy="creation"
      cacheKey={AppCache.MemberCache}
      pageProps={{
        onRefresh: reloadData,
        fabButtons: (
          <React.Fragment>
            {editPermission && (
              <Fab
                title={labels.inviteMember}
                size="medium"
                color="primary"
                onClick={() =>
                  MemberDialogs.invite(organization!, navigate, reloadData)
                }
              >
                <AddIcon />
              </Fab>
            )}
          </React.Fragment>
        )
      }}
      fieldTemplate={{
        name: 'string',
        role: 'number',
        externalId: 'string',
        enabled: 'boolean'
      }}
      fields={(data) => [
        <SearchField
          label={labels.name}
          name="name"
          defaultValue={data.name}
          InputProps={{ sx: { width: '120px' } }}
        />,
        <ComboBox
          options={roles}
          name="role"
          label={labels.role}
          search
          idValue={data.role}
        />,
        <SearchField
          label={labels.externalId}
          name="externalId"
          defaultValue={data.externalId}
        />,
        <Switch
          label={labels.statusNormal}
          name="enabled"
          checked={data.enabled}
        />
      ]}
      loadData={async (data) => {
        return await app.memberApi.query(data, {
          defaultValue: [],
          showLoading: false
        });
      }}
      columns={[
        {
          field: 'name',
          header: labels.name,
          sortable: true,
          cellBoxStyle: GridDeletedCellBoxStyle
        },
        {
          field: 'entityRole',
          width: 180,
          header: labels.role,
          valueFormatter: ({ data }) => getRoleLabel(data),
          sortable: false
        },
        {
          field: 'externalId',
          width: 150,
          header: labels.externalId,
          sortable: true
        },
        {
          field: 'creation',
          type: GridDataType.Date,
          width: 116,
          header: labels.creation,
          sortable: true,
          sortAsc: false
        },
        {
          width: 120,
          header: labels.actions,
          cellRenderer: ({
            data,
            cellProps
          }: GridCellRendererProps<MemberQueryDto, BoxProps>) => {
            if (data == null) return undefined;

            cellProps.sx = {
              paddingTop: '6px!important',
              paddingBottom: '6px!important'
            };

            return (
              <React.Fragment>
                {editPermission && (
                  <React.Fragment>
                    <IconButtonLink
                      title={labels.edit}
                      href={`./../edit/${data.id}`}
                    >
                      <EditIcon />
                    </IconButtonLink>
                    {!data.isSelf &&
                      data.entityRole < UserRole.Founder &&
                      data.entityStatus < EntityStatus.Inactivated && (
                        <IconButton
                          title={labels.remove}
                          onClick={() => deleteMember(data.id, data.name)}
                        >
                          <PersonRemoveIcon />
                        </IconButton>
                      )}
                  </React.Fragment>
                )}
              </React.Fragment>
            );
          }
        }
      ]}
      itemSize={[116, margin]}
      innerItemRenderer={(props) =>
        MobileListItemRenderer(props, (data) => {
          return [
            data.name,
            app.formatDate(data.creation, 'd'),
            [
              editPermission && {
                label: labels.edit,
                icon: <EditIcon />,
                action: `./../edit/${data.id}`
              },
              !data.isSelf &&
                data.entityRole < UserRole.Founder && {
                  label: labels.remove,
                  icon: <PersonRemoveIcon />,
                  action: () => deleteMember(data.id, data.name)
                }
            ],
            <React.Fragment>
              <Typography variant="body2" noWrap>
                {getRoleLabel(data) +
                  (data.externalId ? ', ' + data.externalId : '')}
              </Typography>
              {data.entityStatus >= EntityStatus.Inactivated && (
                <React.Fragment>
                  <Typography variant="caption">
                    {labels.entityStatus + ': '}
                  </Typography>
                  <Typography
                    variant="caption"
                    color={(theme) => theme.palette.error.main}
                  >
                    {app.getStatusLabel(data?.entityStatus)}
                  </Typography>
                </React.Fragment>
              )}
            </React.Fragment>
          ];
        })
      }
    />
  );
}

export default AllMembers;
