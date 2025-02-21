import { Tiplist, TiplistProps } from "@etsoo/materialui";
import { app } from "../app/MyApp";
import { DataTypes } from "@etsoo/shared";

type AppListDto = DataTypes.IdNameItem;

/**
 * App tiplist properties
 * 应用提示列表属性
 */
export type AppTiplistProps = Omit<
  TiplistProps<AppListDto, "id">,
  "loadData" | "label" | "name"
> & {
  /**
   * Label
   */
  label?: string;

  /**
   * Name
   */
  name?: string;
};

/**
 * App tiplist
 * 应用提示列表
 * @param props Properties
 * @returns Component
 */
export function AppTiplist(props: AppTiplistProps) {
  // Destruct
  const {
    fullWidth = true,
    label = app.get("app")!,
    maxItems = 10,
    getOptionLabel = (data) => `${data.id}.${data.name}`,
    name = "appId",
    search = true,
    ...rest
  } = props;

  // Layout
  return (
    <Tiplist<AppListDto>
      label={label}
      getOptionLabel={getOptionLabel}
      name={name}
      fullWidth={fullWidth}
      maxItems={maxItems}
      search={search}
      loadData={(keyword, id, maxItems) =>
        app.queryApi.appList(
          {
            keyword,
            id,
            queryPaging: {
              batchSize: maxItems
            }
          },
          { showLoading: false, defaultValue: [] }
        )
      }
      {...rest}
    />
  );
}
