import { Tiplist, TiplistProps } from "@etsoo/materialui";
import { app } from "../app/MyApp";
import { DataTypes } from "@etsoo/shared";

type UserListDto = DataTypes.IdNameItem;

/**
 * User tiplist properties
 * 用户提示列表属性
 */
export type UserTiplistProps = Omit<
  TiplistProps<UserListDto, "id">,
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
 * User tiplist
 * 用户提示列表
 * @param props Properties
 * @returns Component
 */
export function UserTiplist(props: UserTiplistProps) {
  // Destruct
  const {
    fullWidth = true,
    label = app.get("user")!,
    maxItems = 10,
    getOptionLabel = (data) => data.name,
    name = "userId",
    search = true,
    ...rest
  } = props;

  // Layout
  return (
    <Tiplist<UserListDto>
      label={label}
      getOptionLabel={getOptionLabel}
      name={name}
      fullWidth={fullWidth}
      maxItems={maxItems}
      search={search}
      loadData={(keyword, id, maxItems) =>
        app.queryApi.userList(
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
