import { Tiplist, TiplistProps } from "@etsoo/materialui";
import { OrgListDto } from "../api/dto/query/OrgListDto";
import { app } from "../app/MyApp";

/**
 * Organization tiplist properties
 * 机构提示列表属性
 */
export type OrgTiplistProps = Omit<
  TiplistProps<OrgListDto, "id">,
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
 * Organization tiplist
 * 机构提示列表
 * @param props Properties
 * @returns Component
 */
export function OrgTiplist(props: OrgTiplistProps) {
  // Destruct
  const {
    fullWidth = true,
    label = app.get("org")!,
    maxItems = 10,
    getOptionLabel = (data) => data.name + "(" + data.pin + ")",
    name = "orgId",
    search = true,
    ...rest
  } = props;

  // Layout
  return (
    <Tiplist<OrgListDto>
      label={label}
      getOptionLabel={getOptionLabel}
      name={name}
      fullWidth={fullWidth}
      maxItems={maxItems}
      search={search}
      loadData={(keyword, id, maxItems) =>
        app.queryApi.orgList(
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
