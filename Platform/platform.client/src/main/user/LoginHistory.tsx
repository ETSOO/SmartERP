import {
  DialogButton,
  MUGlobal,
  SearchField,
  ResponsivePage,
  MobileListItemRenderer,
  SelectBool,
  Tiplist
} from '@etsoo/materialui';
import { DateUtils } from '@etsoo/shared';
import { BoxProps, Typography } from '@mui/material';
import InfoIcon from '@mui/icons-material/Info';
import React from 'react';
import { LoginHistoryDto } from '../../api/dto/user/LoginHistoryDto';
import { app } from '../../app/SmartApp';
import {
  GridCellRendererProps,
  GridDataType,
  ScrollerListForwardRef
} from '@etsoo/react';

function LoginHistory() {
  // Labels
  const labels = app.getLabels(
    'device',
    'successLogin',
    'no',
    'yes',
    'creation',
    'startDate',
    'endDate',
    'language',
    'success',
    'description',
    'actions'
  );

  // Refs
  const ref = React.useRef<ScrollerListForwardRef<LoginHistoryDto>>();

  // Load data
  const reloadData = () => ref.current?.reset();

  const margin = MUGlobal.pagePaddings;
  const creationEndRef = React.useRef<HTMLInputElement>();

  React.useEffect(() => {
    // Page title
    app.setPageKey('menuLoginHistory');
  }, []);

  return (
    <ResponsivePage<
      LoginHistoryDto,
      {
        deviceId: 'number';
        success: 'boolean';
        creationStart: 'string' | 'date';
        creationEnd: 'string' | 'date';
      }
    >
      mRef={ref}
      defaultOrderBy="creation"
      defaultOrderByAsc={false}
      cacheKey="search-history-cache"
      pageProps={{ onRefresh: reloadData }}
      fieldTemplate={{
        deviceId: 'number',
        success: 'boolean',
        creationStart: 'date',
        creationEnd: 'date'
      }}
      fields={(data) => [
        <Tiplist
          label={labels.device}
          name="deviceId"
          search
          loadData={(keyword, id) =>
            app.userApi.deviceList(
              { id, keyword },
              { defaultValue: [], showLoading: false }
            )
          }
          idValue={data.deviceId}
        />,
        <SelectBool
          label={labels.successLogin}
          name="success"
          value={`${data.success}`}
        />,
        <SearchField
          label={labels.startDate}
          name="creationStart"
          type="date"
          onChange={(event: React.ChangeEvent<HTMLInputElement>) => {
            if (creationEndRef.current == null) return;
            const date = DateUtils.formatForInput(
              event.currentTarget.valueAsDate
            );
            if (date) creationEndRef.current.min = date;
          }}
          inputProps={{ max: DateUtils.formatForInput(new Date()) }}
          defaultValue={DateUtils.formatForInput(data.creationStart)}
        />,
        <SearchField
          label={labels.endDate}
          name="creationEnd"
          type="date"
          inputRef={creationEndRef}
          inputProps={{
            max: DateUtils.formatForInput(new Date())
          }}
          defaultValue={DateUtils.formatForInput(data.creationEnd)}
        />
      ]}
      loadData={async (data) =>
        app.userApi.loginHistory(data, { defaultValue: [], showLoading: false })
      }
      columns={[
        {
          field: 'creation',
          type: GridDataType.DateTime,
          width: 164,
          header: labels.creation,
          sortable: true,
          sortAsc: false,
          renderProps: app.getDateFormatProps()
        },
        { field: 'deviceName', header: labels.device },
        {
          field: 'language',
          width: 90,
          header: labels.language,
          sortable: false
        },
        {
          field: 'success',
          width: 90,
          type: GridDataType.Boolean,
          header: labels.success,
          sortable: false
        },
        {
          field: 'reason',
          width: 150,
          header: labels.description
        },
        {
          width: 80,
          header: labels.actions,
          align: 'center',
          cellRenderer: ({
            data,
            cellProps
          }: GridCellRendererProps<LoginHistoryDto, BoxProps>) => {
            if (data == null) return undefined;

            cellProps.sx = {
              paddingTop: '6px!important',
              paddingBottom: '6px!important'
            };

            return (
              <DialogButton
                content={JSON.stringify(data, undefined, 2)}
                contentPre
                disableScrollLock
                maxWidth="xs"
                size="small"
                icon={<InfoIcon />}
              >
                JSON data
              </DialogButton>
            );
          }
        }
      ]}
      itemSize={[134, margin]}
      innerItemRenderer={(props) =>
        MobileListItemRenderer(props, (data) => {
          return [
            data.deviceName,
            app.formatDate(data.creation, 'ds'),
            <DialogButton
              content={JSON.stringify(data, undefined, 2)}
              contentPre
              disableScrollLock
              maxWidth="xs"
              size="small"
              icon={<InfoIcon />}
            >
              JSON data
            </DialogButton>,
            <React.Fragment>
              <Typography variant="caption" noWrap>
                {[data.region, data.language, data.timezone].join(', ')}
              </Typography>
              <Typography
                variant="body2"
                noWrap
                color={data.success ? 'green' : 'red'}
              >
                {data.success ? 'Success' : 'Failed: ' + data.reason}
              </Typography>
            </React.Fragment>
          ];
        })
      }
    />
  );
}

export default LoginHistory;
