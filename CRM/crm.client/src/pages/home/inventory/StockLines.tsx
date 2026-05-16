import {
  IconButtonLink,
  MobileListItemRenderer,
  NumberInputField,
  ResponsivePage,
  SearchField
} from "@etsoo/materialui";
import {
  GridCellRendererProps,
  GridDataType,
  ScrollerListForwardRef
} from "@etsoo/react";
import { DataTypes } from "@etsoo/shared";
import { DefaultUI } from "@etsoo/smarterp-core/components";
import { ProductScope, StockQueryLinesData } from "@etsoo/smarterp-crm";
import { app } from "../../../app/MyApp";
import { BoxProps } from "@mui/material/Box";
import React from "react";
import { ProductList } from "@etsoo/smarterp-crm/components";
import { useNavigate } from "react-router-dom";
import EditIcon from "@mui/icons-material/Edit";
import { Permissions } from "@etsoo/smarterp-crm";

const template = {
  keyword: "string",
  productId: "number",
  qtyStart: "number"
} as const satisfies DataTypes.BasicTemplate;

export type AllOrderLinesProps = {
  stockId: number;
  isDeletable: boolean;
  refresh: () => Promise<void>;
};

export function StockLines(props: AllOrderLinesProps) {
  // Route
  const navigate = useNavigate();

  // Destruct
  const { stockId, isDeletable, refresh } = props;

  // Labels
  const labels = app.getLabels(
    "actions",
    "edit",
    "keywords",
    "product",
    "price",
    "qty",
    "qtyStart",
    "view"
  );

  // Refs
  const ref =
    React.useRef<ScrollerListForwardRef<StockQueryLinesData>>(undefined);

  // Load data
  const reloadData = React.useCallback(() => ref.current?.reset(), []);

  function doEdit(data: StockQueryLinesData) {
    refresh();
  }

  return (
    <ResponsivePage<StockQueryLinesData, typeof template>
      {...DefaultUI.pageProps({
        onRefresh: reloadData,
        fabButtons: <React.Fragment></React.Fragment>
      })}
      mRef={ref}
      fieldTemplate={template}
      fields={(data) => [
        <ProductList
          search
          idValue={data.productId}
          rq={{ scope: ProductScope.Inventory }}
        />,
        <SearchField
          label={labels.keywords}
          name="keyword"
          defaultValue={data.keyword}
          minChars={2}
        />,
        <NumberInputField search name="qtyStart" label={labels.qtyStart} />
      ]}
      loadData={(data) =>
        app.stockApi.queryLines(
          {
            stockId,
            ...data,
            queryPaging: {
              batchSize: 20,
              orderBy: [{ field: "id", desc: false, unique: true }]
            }
          },
          {
            defaultValue: [],
            showLoading: false
          }
        )
      }
      columns={[
        {
          field: "productName",
          header: labels.product
        },
        {
          field: "qty",
          header: labels.qty,
          type: GridDataType.Number,
          width: 88
        },
        {
          width: DefaultUI.Widths.icon1,
          header: labels.actions,
          cellBoxStyle: {
            paddingTop: "6px!important",
            paddingBottom: "6px!important"
          },
          cellRenderer: ({
            data
          }: GridCellRendererProps<StockQueryLinesData, BoxProps>) => {
            if (data == null) return undefined;

            return (
              <React.Fragment>
                {app.owns(Permissions.Inventory.Edit) && isDeletable && (
                  <IconButtonLink
                    title={labels.edit}
                    href={`./../../editline/${data.id}`}
                  >
                    <EditIcon />
                  </IconButtonLink>
                )}
              </React.Fragment>
            );
          }
        }
      ]}
      itemRenderer={(props) =>
        MobileListItemRenderer(props, (data) => {
          return [
            data.productName,
            app.formatNumber(data.qty),
            [
              app.owns(Permissions.Inventory.Edit) &&
                isDeletable && {
                  label: labels.edit,
                  icon: <EditIcon />,
                  action: () => doEdit(data)
                }
            ],
            <React.Fragment></React.Fragment>
          ];
        })
      }
    />
  );
}
