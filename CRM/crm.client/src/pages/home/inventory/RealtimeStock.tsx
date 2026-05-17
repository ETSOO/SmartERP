import {
  ResponsivePage,
  SearchField,
  IconButtonLink,
  MobileListItemRenderer
} from "@etsoo/materialui";
import HistoryIcon from "@mui/icons-material/History";
import WidgetsIcon from "@mui/icons-material/Widgets";
import React from "react";
import {
  GridCellRendererProps,
  GridDataType,
  ScrollerListForwardRef
} from "@etsoo/react";
import { useNavigate } from "react-router-dom";
import { app } from "../../../app/MyApp";
import { usePageDataEmpty } from "@etsoo/smarterp-core";
import { ProductScope, StockSiteQueryData } from "@etsoo/smarterp-crm";
import { DataTypes, DateUtils } from "@etsoo/shared";
import { DefaultUI } from "@etsoo/smarterp-core/components";
import { BoxProps } from "@mui/material/Box";
import { AddressList, ProductList } from "@etsoo/smarterp-crm/components";
import IconButton from "@mui/material/IconButton";
import { StockByWarehouse } from "./StockByWarehouse";

const template = {
  locationId: "number",
  productId: "number",
  refreshTimeStart: "date",
  refreshTimeEnd: "date"
} as const satisfies DataTypes.BasicTemplate;

export default function RealtimeStock() {
  // Route
  const navigate = useNavigate();

  // Labels
  const labels = app.getLabels(
    "actions",
    "history",
    "product",
    "refreshTime",
    "stockByWarehouse",
    "stockQty",
    "warehouse"
  );

  // Refs
  const ref =
    React.useRef<ScrollerListForwardRef<StockSiteQueryData>>(undefined);

  // Load data
  const reloadData = React.useCallback(() => ref.current?.reset(), []);

  const orgPersonId = app.userData?.system?.personId ?? 0;

  const locationRef = React.useRef<number>(undefined);

  function showStockByWarehouse(productId: number) {
    StockByWarehouse.show(productId, locationRef.current);
  }

  // Page data hook
  usePageDataEmpty(app);

  return (
    <ResponsivePage<StockSiteQueryData, typeof template>
      {...DefaultUI.pageProps({
        onRefresh: reloadData,
        fabButtons: undefined
      })}
      mRef={ref}
      quickAction={(data) =>
        navigate(
          `./../history/${data.productId}?locationId=${locationRef.current ?? ""}`
        )
      }
      fieldTemplate={template}
      fields={(data) => [
        <AddressList
          name="locationId"
          personId={orgPersonId}
          label={labels.warehouse}
          search
          onValueChange={(value) => {
            locationRef.current = value?.id;
          }}
        />,
        <ProductList
          search
          idValue={data.productId}
          rq={{ scope: ProductScope.Inventory }}
        />,
        <SearchField
          label={labels.refreshTime}
          name="refreshTimeStart"
          type="date"
          defaultValue={DateUtils.formatForInput(data.refreshTimeStart)}
        />,
        <SearchField
          label=""
          name="refreshTimeEnd"
          type="date"
          defaultValue={DateUtils.formatForInput(data.refreshTimeEnd)}
        />
      ]}
      loadData={(data) =>
        app.stockSiteApi.query(data, {
          defaultValue: [],
          showLoading: false
        })
      }
      columns={[
        {
          field: "productName",
          header: labels.product
        },
        {
          field: "qty",
          header: labels.stockQty,
          type: GridDataType.Number,
          width: 108
        },
        {
          width: DefaultUI.Widths.icon2,
          header: labels.actions,
          cellBoxStyle: {
            paddingTop: "6px!important",
            paddingBottom: "6px!important"
          },
          cellRenderer: ({
            data
          }: GridCellRendererProps<StockSiteQueryData, BoxProps>) => {
            if (data == null) return undefined;

            return (
              <React.Fragment>
                <IconButton
                  title={labels.stockByWarehouse}
                  onClick={() => showStockByWarehouse(data.productId)}
                >
                  <WidgetsIcon />
                </IconButton>
                <IconButtonLink
                  title={labels.history}
                  href={`./../history/${data.productId}?locationId=${locationRef.current ?? ""}`}
                >
                  <HistoryIcon />
                </IconButtonLink>
              </React.Fragment>
            );
          }
        }
      ]}
      itemRenderer={(props) =>
        MobileListItemRenderer(props, (data) => {
          return [
            `${data.productName} / ${data.qty}`,
            app.formatDate(data.refreshTime, "d"),
            [
              {
                label: labels.stockByWarehouse,
                icon: <WidgetsIcon />,
                action: () => showStockByWarehouse(data.productId)
              },
              {
                label: labels.history,
                icon: <HistoryIcon />,
                action: `./../history/${data.productId}?locationId=${locationRef.current ?? ""}`
              }
            ],
            <React.Fragment></React.Fragment>
          ];
        })
      }
    />
  );
}
