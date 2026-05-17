import {
  ResponsivePage,
  SearchField,
  IconButtonLink,
  MobileListItemRenderer,
  NumberInputField
} from "@etsoo/materialui";
import ArticleIcon from "@mui/icons-material/Article";
import React from "react";
import {
  GridCellRendererProps,
  GridDataType,
  ScrollerListForwardRef,
  useParamsEx,
  useSearchParamsEx
} from "@etsoo/react";
import { app } from "../../../app/MyApp";
import { usePageDataEmpty } from "@etsoo/smarterp-core";
import { ProductScope, StockQueryProductLineData } from "@etsoo/smarterp-crm";
import { DataTypes, DateUtils } from "@etsoo/shared";
import { DefaultUI } from "@etsoo/smarterp-core/components";
import { BoxProps } from "@mui/material/Box";
import { AddressList, ProductList } from "@etsoo/smarterp-crm/components";

const template = {
  productId: "number",
  locationId: "number",
  qtyStart: "number",
  qtyEnd: "number",
  creationStart: "date",
  creationEnd: "date"
} as const satisfies DataTypes.BasicTemplate;

export default function StockHistory() {
  // Route
  const { productId } = useParamsEx({
    productId: "number"
  });

  const { locationId } = useSearchParamsEx({
    locationId: "number"
  });

  // Labels
  const labels = app.getLabels(
    "actions",
    "creation",
    "inventory",
    "qty",
    "stockQty",
    "warehouse"
  );

  // Refs
  const ref =
    React.useRef<ScrollerListForwardRef<StockQueryProductLineData>>(undefined);

  // Load data
  const reloadData = React.useCallback(() => ref.current?.reset(), []);

  const orgPersonId = app.userData?.system?.personId ?? 0;

  // Page data hook
  usePageDataEmpty(app);

  return (
    <ResponsivePage<StockQueryProductLineData, typeof template>
      {...DefaultUI.pageProps({
        onRefresh: reloadData,
        fabButtons: undefined
      })}
      mRef={ref}
      fieldTemplate={template}
      fields={(data) => [
        <ProductList
          search
          idValue={data.productId ?? productId}
          rq={{ scope: ProductScope.Inventory }}
          sx={(theme) => ({
            "& .MuiInputLabel-root": {
              color: theme.palette.warning.main
            }
          })}
        />,
        <AddressList
          name="locationId"
          personId={orgPersonId}
          idValue={locationId ?? data.locationId}
          label={labels.warehouse}
          search
        />,
        <NumberInputField
          search
          name="qtyStart"
          label={labels.qty}
          defaultValue={data.qtyStart}
        />,
        <NumberInputField
          search
          name="qtyEnd"
          label=""
          defaultValue={data.qtyEnd}
        />,
        <SearchField
          label={labels.creation}
          name="creationStart"
          type="date"
          defaultValue={DateUtils.formatForInput(data.creationStart)}
        />,
        <SearchField
          label=""
          name="creationEnd"
          type="date"
          defaultValue={DateUtils.formatForInput(data.creationEnd)}
        />
      ]}
      loadData={(data) => {
        const { productId, ...rest } = data;
        if (productId == null) return Promise.resolve([]);

        return app.stockApi.queryProductLines(
          { productId, ...rest },
          {
            defaultValue: [],
            showLoading: false
          }
        );
      }}
      columns={[
        {
          field: "creation",
          header: labels.creation,
          width: 116,
          type: GridDataType.Date
        },
        {
          field: "title",
          header: labels.inventory
        },
        {
          field: "qty",
          header: labels.qty,
          type: GridDataType.Number,
          width: 108
        },
        {
          field: "locationName",
          header: labels.warehouse,
          width: 180
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
          }: GridCellRendererProps<StockQueryProductLineData, BoxProps>) => {
            if (data == null) return undefined;

            return (
              <React.Fragment>
                <IconButtonLink
                  title={labels.inventory}
                  href={`./../../view/${data.stockId}`}
                >
                  <ArticleIcon />
                </IconButtonLink>
              </React.Fragment>
            );
          }
        }
      ]}
      itemRenderer={(props) =>
        MobileListItemRenderer(props, (data) => {
          return [
            `${data.title} / ${data.qty}`,
            app.formatDate(data.creation, "d"),
            [
              {
                label: labels.inventory,
                icon: <ArticleIcon />,
                action: `./../../view/${data.stockId}`
              }
            ],
            <React.Fragment></React.Fragment>
          ];
        })
      }
    />
  );
}
