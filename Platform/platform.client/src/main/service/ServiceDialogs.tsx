import { BusinessTax, IActionResult, ProductUnit } from '@etsoo/appscript';
import {
  ComboBox,
  InputField,
  MaskInput,
  OptionGroup,
  VBox
} from '@etsoo/materialui';
import { DomUtils, ListType } from '@etsoo/shared';
import { Button, InputAdornment, TextField, Typography } from '@mui/material';
import React from 'react';
import HelpIcon from '@mui/icons-material/Help';
import { app } from '../../app/SmartApp';
import { NavigateFunction } from 'react-router-dom';
import { ProductQueryDto } from '../../api/dto/product/ProductQueryDto';
import { ProductPurchasedDto } from '../../api/dto/product/ProductPurchasedDto';
import { AppCache } from '../../app/AppCache';

interface ServiceBuyProps {
  /**
   * Kind
   */
  kind?: number;
}

function ServiceBuy(props: ServiceBuyProps) {
  // Labels
  const labels = app.getLabels(
    'organization',
    'newOrganization',
    'existingOrganization',
    'organizationName',
    'region'
  );

  // Current region
  const region = app.settings.currentRegion;

  // Tax
  const tax = region == null ? undefined : BusinessTax.getById(region.id);

  // Options
  const options: ListType[] = [
    { id: 1, label: labels.existingOrganization },
    { id: 2, label: labels.newOrganization }
  ];

  // Destruct
  const { kind: initKind = 1 } = props;

  // State
  const [kind, setKind] = React.useState(initKind);

  // Layout
  return (
    <VBox gap={1} width="100%" paddingTop={1}>
      <OptionGroup
        name="kind"
        options={options}
        row
        defaultValue={kind}
        onValueChange={(value) => {
          if (value == null || Array.isArray(value)) return;
          setKind(value);
        }}
      />
      {kind === 1 && (
        <React.Fragment>
          <ComboBox
            label={labels.organization}
            name="organization"
            fullWidth
            inputVariant="standard"
            inputMargin="dense"
            inputRequired
            loadData={() => app.orgApi.list(100)}
          />
          <VBox height="60px" />
        </React.Fragment>
      )}
      {kind === 2 && (
        <React.Fragment>
          <InputField
            autoFocus
            margin="dense"
            name="name"
            label={labels.organizationName}
            fullWidth
            variant="standard"
            required
            inputProps={{ maxLength: 128 }}
          />
          <MaskInput
            mask={{ mask: tax?.mask ?? '' }}
            margin="dense"
            name="identifier"
            label={app.get(tax?.labelKey ?? 'taxId')}
            fullWidth
            variant="standard"
            helperText={app.get((tax?.labelKey ?? 'taxId') + 'Help')}
            inputProps={{
              maxLength: 20,
              style: { textTransform: 'uppercase' }
            }}
          />
        </React.Fragment>
      )}
    </VBox>
  );
}

/**
 * Service dialogs
 */
export namespace ServiceDialogs {
  /**
   * Buy service
   * @param data Product data
   * @param kind Kind, 1 = for existing organization, 2 = for new organization
   * @param navigate Navigate function
   */
  export function buy(
    data: ProductQueryDto,
    kind: number,
    navigate: NavigateFunction
  ) {
    // Labels
    const labels = app.getLabels(
      'purchase',
      'operationSucceeded',
      'serviceHelp',
      'underDevelopment'
    );

    // Message
    const message = (
      <React.Fragment>
        <Typography component="span">{data.name}</Typography>
        <Typography component="span" fontWeight="bolder">
          , {formatPrice(data)}
        </Typography>
        {data.entityStatus === 199 && (
          <React.Fragment>
            <br />
            <Typography variant="caption" color="warning">
              {labels.underDevelopment + '...'}
            </Typography>
          </React.Fragment>
        )}
        {data.helpUrl && (
          <React.Fragment>
            <br />
            <Button
              href={data.helpUrl}
              target="_blank"
              startIcon={<HelpIcon />}
            >
              {labels.serviceHelp}
            </Button>
          </React.Fragment>
        )}
      </React.Fragment>
    );

    app.showInputDialog({
      title: labels.purchase,
      message,
      callback: async (form) => {
        // Cancelled
        if (form == null) {
          return;
        }

        // Form data
        const formData = DomUtils.dataAs(new FormData(form), {
          kind: 'number',
          organization: 'number',
          name: 'string',
          identifier: 'string'
        });

        let result: IActionResult | undefined;

        if (formData.kind === 1) {
          // Check organization
          if (formData.organization == null) {
            DomUtils.setFocus('organizationInput', form);
            return false;
          }

          result = await app.productApi.buy(data.id, formData.organization, {
            showLoading: false
          });
        } else {
          // Check name
          if (formData.name == null) {
            DomUtils.setFocus('name', form);
            return false;
          }

          result = await app.productApi.buyNew(
            {
              id: data.id,
              region: app.region,
              name: formData.name,
              identifier: formData.identifier,
              deviceId: app.deviceId
            },
            { showLoading: false }
          );
        }

        if (result == null) return false;

        if (result.ok) {
          // Clear cache
          AppCache.removeServiceCache();
          AppCache.removeMyServiceCache();

          if (formData.kind === 2) {
            AppCache.removeOrgCache();
          }

          // Refresh token in silence
          app.refreshToken({ showLoading: false });

          // Succeed
          app.notifier.succeed(labels.operationSucceeded, undefined, () => {
            // New organization created
            const url =
              formData.kind === 1 ? './../my' : './../../organization/all';
            navigate(url);
          });

          return;
        }

        app.alertResult(result);
        return false;
      },
      inputs: <ServiceBuy kind={kind} />,
      fullScreen: app.smDown
    });
  }

  function formatPrice(data: { price: number; productUnit: ProductUnit }) {
    return (
      app.formatMoney(data.price, true) +
      ' / ' +
      app.publicApi.getUnitLabel(data.productUnit)
    );
  }

  export function renew(data: ProductPurchasedDto, callback?: () => void) {
    // Labels
    const labels = app.getLabels('renew', 'qty');

    app.showInputDialog({
      title: labels.renew,
      message: (
        <React.Fragment>
          <Typography component="span">{data.name}</Typography>
          <Typography component="span" fontWeight="bolder">
            , {formatPrice(data)}
          </Typography>
        </React.Fragment>
      ),
      callback: async (form) => {
        // Cancelled
        if (form == null) {
          return;
        }

        // Form data
        const { qty } = DomUtils.dataAs(new FormData(form), {
          qty: 'number'
        });
        if (qty == null || qty < 1) {
          DomUtils.setFocus('qty', form);
          return false;
        }

        const result = await app.productApi.renew(
          { id: data.id, qty },
          { showLoading: false }
        );

        if (result == null) return;

        if (result.ok) {
          app.orgApi.sendActionMessage({
            kind: 'renew-service',
            title: app.formatAction(labels.renew, data.name)
          });

          if (callback) callback();
          return;
        }

        app.alertResult(result);
        return false;
      },
      inputs: (
        <VBox gap={1} width="100%" paddingTop={1}>
          <TextField
            name="qty"
            margin="dense"
            variant="standard"
            label={labels.qty}
            defaultValue={1}
            required
            type="number"
            inputProps={{ max: 100, min: 1, step: 1, inputMode: 'numeric' }}
            InputProps={{
              endAdornment: (
                <InputAdornment position="end">
                  {app.publicApi.getUnitLabel(data.productUnit)}
                </InputAdornment>
              )
            }}
          />
        </VBox>
      ),
      fullScreen: app.smDown
    });
  }
}
