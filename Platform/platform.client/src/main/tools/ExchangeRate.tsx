import React from 'react';
import { app } from '../../app/SmartApp';
import { LinearProgress } from '@mui/material';
import { CommonPage, LineChart, OptionGroup } from '@etsoo/materialui';
import { ColorUtils, DataTypes, ListType1 } from '@etsoo/shared';
import { useDelayedExecutor } from '@etsoo/react';
import { ExchangeRateHistoryDto } from '@etsoo/appscript';

const storageName = 'exchange-rate-currencies';

function ExchangeRate() {
  // Labels
  const labels = app.getLabels('monthLabel', 'exchangeRateTip');

  // Months
  const months: DataTypes.IdLabelItem[] = [1, 3, 6, 12].map((m) => ({
    id: m,
    label: labels.monthLabel.format(m.toString()) ?? m.toString()
  }));

  // Storage
  const storedCurrencies = app.storage.getPersistedData<string[]>(storageName);

  // States
  const [options, setOptions] = React.useState<ListType1[]>([]);
  const [items, setItems] = React.useState<ExchangeRateHistoryDto[]>();

  const rq = React.useRef<{ currencies?: string[]; months?: number }>({});

  const isMounted = React.useRef(true);
  const delayed = useDelayedExecutor(() => {
    const { currencies, months } = rq.current;
    if (currencies == null) return;
    app.publicApi
      .exchangeRateHistory(currencies, months)
      .then((result) => setItems(result ?? []));
  }, 480);

  const parseData = () => {
    const currencies = rq.current.currencies;
    if (currencies == null || currencies.length === 0 || items == null)
      return {
        datasets: []
      };

    // colors
    const colors = ColorUtils.getColors('#e21821', 128);

    // Datasets
    const datasets = currencies.map((c, i) => {
      const option = options.find((o) => o.id === c)!;
      return {
        backgroundColor: colors[i],
        borderColor: colors[i],
        label: getCurrencyLabel(option),
        data: items
          .filter((item) => item.id === c)
          .map((item) => item.exchangeRate)
      };
    });

    return {
      datasets,
      labels: items
        ?.filter((item) => item.id === currencies[0])
        .map((item) => item.updateTime)
    };
  };

  const getCurrencyLabel = (option: ListType1) =>
    `${option.label}(${option.id})`;

  if (storedCurrencies) rq.current.currencies = storedCurrencies;

  React.useEffect(() => {
    if (rq.current.currencies == null) return;
    delayed.call();
  }, [delayed]);

  React.useEffect(() => {
    // Page title
    app.setPageKey('exchangeRate');

    // Get currencies
    app.publicApi.currencies().then((result) => {
      if (result == null) return;
      // Remove CNY
      result.splice(
        result.findIndex((item) => item.id === 'CNY'),
        1
      );
      setOptions(result);
    });

    return () => {
      isMounted.current = false;
      app.pageExit();
      delayed.clear();
    };
  }, [delayed]);

  return (
    <CommonPage>
      <OptionGroup
        name="currencies"
        options={options}
        defaultValue={storedCurrencies}
        multiple
        row
        getOptionLabel={getCurrencyLabel}
        onValueChange={(values) => {
          if (Array.isArray(values)) {
            rq.current.currencies = values;
            delayed.call();
            app.storage.setPersistedData(storageName, values);
          }
        }}
      />
      <OptionGroup
        name="months"
        options={months}
        defaultValue={1}
        row
        onValueChange={(value) => {
          if (typeof value === 'number') {
            rq.current.months = value;
            delayed.call();
          }
        }}
      />
      {items == null ? (
        rq.current.currencies == null ? (
          <React.Fragment />
        ) : (
          <LinearProgress />
        )
      ) : (
        (() => {
          return (
            <LineChart data={parseData()} title={labels.exchangeRateTip} />
          );
        })()
      )}
    </CommonPage>
  );
}

export default ExchangeRate;
