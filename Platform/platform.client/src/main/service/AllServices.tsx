import {
  MUGlobal,
  ResponsivePage,
  SearchField,
  HBox,
  MoneyText,
  SelectEx,
  MobileListItemRenderer
} from '@etsoo/materialui';
import { BoxProps, Button, Typography } from '@mui/material';
import ShoppingCartIcon from '@mui/icons-material/ShoppingCart';
import React from 'react';
import { ServiceDialogs } from './ServiceDialogs';
import { app } from '../../app/SmartApp';
import { useLocation, useNavigate } from 'react-router-dom';
import { DomUtils } from '@etsoo/shared';
import {
  GridCellRendererProps,
  GridDataType,
  ScrollerListForwardRef
} from '@etsoo/react';
import { ProductQueryDto } from '../../api/dto/product/ProductQueryDto';
import { AppCache } from '../../app/AppCache';

function AllServices() {
  // Route
  const navigate = useNavigate();
  const location = useLocation();
  const { kind = 1 } = DomUtils.dataAs(location.state, { kind: 'number' });

  // Labels
  const labels = app.getLabels(
    'serviceName',
    'creation',
    'actions',
    'serviceIdentity',
    'productUnit',
    'price',
    'purchase'
  );

  // Refs
  const ref = React.useRef<ScrollerListForwardRef<ProductQueryDto>>();

  // Identities
  const identities = app.getIdentities();
  const identityLabel = React.useCallback(
    (data?: ProductQueryDto) => {
      if (data == null) return '';
      return identities.find((item) => item.id === data.serviceIdentity)?.label;
    },
    [identities]
  );

  // Product units
  const productUnitLabel = (data?: ProductQueryDto) => {
    if (data == null) return '';
    return app.publicApi.getUnitLabel(data.productUnit);
  };

  // Load data
  const reloadData = () => ref.current?.reset();

  const margin = MUGlobal.pagePaddings;

  React.useEffect(() => {
    // Page title
    app.setPageKey('servicesAll');
  }, []);

  return (
    <ResponsivePage<
      ProductQueryDto,
      { name: 'string'; serviceIdentity: 'number' }
    >
      mRef={ref}
      defaultOrderBy="creation"
      cacheKey={AppCache.ServiceCache}
      pageProps={{ onRefresh: reloadData }}
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
      loadData={(data) =>
        app.productApi.query(data, {
          defaultValue: [],
          showLoading: false
        })
      }
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
          sortable: false
        },
        {
          field: 'price',
          type: GridDataType.IntMoney,
          width: 120,
          header: labels.price,
          sortable: false,
          renderProps: app.getMoneyFormatProps()
        },
        {
          field: 'productUnit',
          width: 100,
          header: labels.productUnit,
          valueFormatter: ({ data }) => productUnitLabel(data),
          sortable: false
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
          align: 'center',
          cellRenderer: ({
            data,
            cellProps
          }: GridCellRendererProps<ProductQueryDto, BoxProps>) => {
            if (data == null) return undefined;

            cellProps.sx = {
              paddingTop: '10px!important',
              paddingBottom: '9px!important'
            };

            return (
              <Button
                variant="outlined"
                size="small"
                startIcon={<ShoppingCartIcon />}
                onClick={() => ServiceDialogs.buy(data, kind, navigate)}
              >
                {labels.purchase}
              </Button>
            );
          }
        }
      ]}
      itemSize={[134, margin]}
      innerItemRenderer={(props) =>
        MobileListItemRenderer(props, (data) => {
          return [
            data.name,
            identityLabel(data) + ', ' + app.formatDate(data.creation, 'd'),
            [],
            <React.Fragment>
              <HBox>
                <MoneyText
                  value={data.price}
                  currency={app.currency}
                  isInteger
                />
                <Typography>
                  &nbsp;/&nbsp;
                  {productUnitLabel(data)}
                </Typography>
              </HBox>
              <Button
                variant="outlined"
                fullWidth
                startIcon={<ShoppingCartIcon />}
                onClick={() => ServiceDialogs.buy(data, kind, navigate)}
              >
                {labels.purchase}
              </Button>
            </React.Fragment>
          ];
        })
      }
    />
  );
}

export default AllServices;
