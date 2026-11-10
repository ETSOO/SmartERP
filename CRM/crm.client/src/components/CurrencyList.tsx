import { SelectEx, SelectExProps } from "@etsoo/materialui";
import { app } from "../app/MyApp";
import { CurrencyItem } from "@etsoo/smarterp-core";

/**
 * Currency list component
 * @param props Props
 * @returns Component
 */
export function CurrencyList(
  props: Omit<SelectExProps<CurrencyItem>, "options" | "loadData"> & {
    filterCurrencies?: (currencies: string[]) => string[];
  }
) {
  // Destruct
  const {
    filterCurrencies = (currencies) => currencies,
    label = app.get("currency"),
    labelField = "name",
    name = "currency",
    ...rest
  } = props;

  // Currencies
  const currencies = filterCurrencies(app.userData?.system?.currencies || []);

  // Layout
  return (
    <SelectEx
      label={label}
      labelField={labelField}
      loadData={() =>
        app.core.publicApi.getCurrencies(currencies, { showLoading: false })
      }
      name={name}
      {...rest}
    />
  );
}
