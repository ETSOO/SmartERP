import {
  MUGlobal,
  ResponsivePage,
  SearchField,
  SelectEx,
  DateText,
  MobileListItemRenderer,
  TooltipClick
} from '@etsoo/materialui';
import { BoxProps, Button, Fab, IconButton, Typography } from '@mui/material';
import React from 'react';
import AddIcon from '@mui/icons-material/Add';
import EditIcon from '@mui/icons-material/Edit';
import HelpIcon from '@mui/icons-material/Help';
import ShoppingCartIcon from '@mui/icons-material/ShoppingCart';
import KeyIcon from '@mui/icons-material/Key';
import { ProductPurchasedDto } from '../../api/dto/product/ProductPurchasedDto';
import { app } from '../../app/SmartApp';
import { useNavigate } from 'react-router-dom';
import {
  GridCellRendererProps,
  GridDataType,
  NotificationMessageType,
  ScrollerListForwardRef
} from '@etsoo/react';
import { DateUtils } from '@etsoo/shared';
import { ServiceDialogs } from './ServiceDialogs';
import { AppCache } from '../../app/AppCache';

function MyServices() {
  // Route
  const navigate = useNavigate();

  // Labels
  const labels = app.getLabels(
    'serviceName',
    'creation',
    'actions',
    'serviceIdentity',
    'productUnit',
    'price',
    'purchase',
    'expiry',
    'renew',
    'customName',
    'serviceHelp',
    'apiKey',
    'apiKeyTip',
    'copy',
    'completeTip'
  );

  // Refs
  const ref = React.useRef<ScrollerListForwardRef<ProductPurchasedDto>>();

  // Identities
  const identities = app.getIdentities();
  const identityLabel = React.useCallback(
    (data?: ProductPurchasedDto) => {
      if (data == null) return '';
      return identities.find((item) => item.id === data.serviceIdentity)?.label;
    },
    [identities]
  );

  // Product units
  const productUnitLabel = (data?: ProductPurchasedDto) => {
    if (data == null) return '';
    return app.publicApi.getUnitLabel(data.productUnit);
  };

  // Load data
  const reloadData = async () => {
    ref.current?.reset();
  };

  const setCustomName = (data: ProductPurchasedDto) => {
    app.notifier.prompt(
      `[${data.name}] ${labels.customName}`,
      async (name) => {
        if (name == null) return;

        const result = await app.productApi.setCustomName(data.id, name, {
          showLoading: false
        });
        if (result == null) return;

        if (result.ok) {
          reloadData();
          return;
        }

        app.alertResult(result);
      },
      undefined,
      {
        inputProps: {
          type: 'input',
          defaultValue: data.customName,
          required: false
        }
      }
    );
  };

  const createApiKey = (data: ProductPurchasedDto) => {
    app.notifier.confirm(
      labels.apiKeyTip,
      `${data.customName ?? data.name} - ${labels.apiKey}`,
      async (ok) => {
        if (!ok) return;
        const result = await app.productApi.createApiKey(data.id, {
          showLoading: false
        });
        if (result == null) return;
        if (!result.ok) {
          app.alertResult(result);
          return;
        }
        const id = result.data?.id;
        if (!id) {
          return;
        }

        app.notifier
          .alert(
            <React.Fragment>
              <Typography component="span">{id}</Typography>
              <TooltipClick title={labels.completeTip.format(labels.copy)}>
                {(openTooltip) => (
                  <Button
                    variant="outlined"
                    size="small"
                    onClick={() => {
                      navigator.clipboard?.writeText(id);
                      openTooltip();
                    }}
                  >
                    {labels.copy}
                  </Button>
                )}
              </TooltipClick>
            </React.Fragment>,
            undefined,
            NotificationMessageType.Success
          )
          .dismiss(180);
      }
    );
  };

  const margin = MUGlobal.pagePaddings;

  React.useEffect(() => {
    // Page title
    app.setPageKey('servicesPurchased');
  }, []);

  return (
    <ResponsivePage<
      ProductPurchasedDto,
      { name: 'string'; serviceIdentity: 'number' }
    >
      mRef={ref}
      defaultOrderBy="creation"
      cacheKey={AppCache.MyServiceCache}
      pageProps={{
        onRefresh: reloadData,
        fabButtons: (
          <Fab
            title={labels.purchase}
            size="medium"
            color="primary"
            onClick={() =>
              navigate('./../all', {
                state: { kind: 2 }
              })
            }
          >
            <AddIcon />
          </Fab>
        )
      }}
      fieldTemplate={{ name: 'string', serviceIdentity: 'number' }}
      fields={(data) => [
        <SearchField
          label={labels.serviceName}
          name="name"
          defaultValue={data.name}
        />,
        <SelectEx
          label={labels.serviceIdentity}
          name="serviceIdentity"
          search
          options={identities}
          value={data.serviceIdentity}
        />
      ]}
      loadData={async (data) => {
        return await app.productApi.queryPurchased(data, {
          defaultValue: [],
          showLoading: false
        });
      }}
      columns={[
        {
          field: 'serviceIdentity',
          header: labels.serviceIdentity,
          width: 120,
          valueFormatter: ({ data }) => identityLabel(data),
          sortable: true
        },
        {
          field: 'name',
          header: labels.serviceName,
          sortable: false,
          valueFormatter: ({ data }) => data?.customName ?? data?.name
        },
        {
          field: 'price',
          width: 120,
          header: labels.price,
          sortable: false,
          valueFormatter: ({ data }) =>
            data == null
              ? ''
              : `${app.formatMoney(data.price, true)} / ${productUnitLabel(
                  data
                )}`
        },
        {
          field: 'expiry',
          type: GridDataType.Date,
          width: 116,
          header: labels.expiry,
          sortable: true,
          sortAsc: false,
          renderProps: { nearDays: 30 }
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
          width: 192,
          header: labels.actions,
          cellRenderer: ({
            data,
            cellProps
          }: GridCellRendererProps<ProductPurchasedDto, BoxProps>) => {
            if (data == null) return undefined;

            cellProps.sx = {
              paddingTop: '6px!important',
              paddingBottom: '6px!important'
            };

            return (
              <React.Fragment>
                <IconButton
                  title={labels.renew}
                  onClick={() => ServiceDialogs.renew(data, reloadData)}
                >
                  <ShoppingCartIcon />
                </IconButton>
                <IconButton
                  title={labels.customName}
                  onClick={() => setCustomName(data)}
                >
                  <EditIcon />
                </IconButton>
                {!DateUtils.isExpired(data.expiry) && (
                  <IconButton
                    title={labels.apiKey}
                    onClick={() => createApiKey(data)}
                  >
                    <KeyIcon />
                  </IconButton>
                )}
                {data.helpUrl && (
                  <IconButton
                    title={labels.serviceHelp}
                    href={data.helpUrl}
                    target="_blank"
                  >
                    <HelpIcon />
                  </IconButton>
                )}
              </React.Fragment>
            );
          }
        }
      ]}
      itemSize={[100, margin]}
      innerItemRenderer={(props) =>
        MobileListItemRenderer(props, (data) => {
          return [
            data.customName ?? data.name,
            identityLabel(data) + ', ' + app.formatDate(data.creation, 'd'),
            [
              {
                label: labels.renew,
                icon: <ShoppingCartIcon />,
                action: () => ServiceDialogs.renew(data, reloadData)
              },
              {
                label: labels.customName,
                icon: <EditIcon />,
                action: () => setCustomName(data)
              },
              !DateUtils.isExpired(data.expiry) && {
                label: labels.apiKey,
                icon: <KeyIcon />,
                action: () => createApiKey(data)
              },
              data.helpUrl != null && {
                label: labels.serviceHelp,
                icon: <HelpIcon />,
                action: data.helpUrl
              }
            ],
            <React.Fragment>
              <Typography variant="body2" component="span">
                {labels.expiry}
                {': '}
              </Typography>
              <DateText
                value={data.expiry}
                nearDays={30}
                options="d"
                {...app.getDateFormatProps()}
              />
            </React.Fragment>
          ];
        })
      }
    />
  );
}

export default MyServices;
