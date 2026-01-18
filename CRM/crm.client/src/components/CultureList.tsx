import { SelectEx, SelectExProps } from "@etsoo/materialui";
import { app } from "../app/MyApp";
import { CultureItem } from "@etsoo/appscript";

/**
 * Culture list component
 * @param props Props
 * @returns Component
 */
export function CultureList(
  props: Omit<SelectExProps<CultureItem>, "options" | "loadData">
) {
  // Destruct
  const {
    label = app.get("culture"),
    labelField = "name",
    name = "culture",
    ...rest
  } = props;

  // Cultures
  const cultures = app.userData?.system?.cultures || [];

  // Layout
  return (
    <SelectEx
      label={label}
      labelField={labelField}
      loadData={() =>
        app.core.publicApi.getCultures(cultures, { showLoading: false })
      }
      name={name}
      {...rest}
    />
  );
}
