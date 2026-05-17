import {
  ResponsivePage,
  SearchField,
  IconButtonLink,
  MobileListItemRenderer,
  NumberInputField,
  NotificationMUDataMethods
} from "@etsoo/materialui";
import ArticleIcon from "@mui/icons-material/Article";
import React from "react";
import { GridCellRendererProps, ScrollerListForwardRef } from "@etsoo/react";
import { useNavigate } from "react-router-dom";
import { app } from "../../../app/MyApp";
import { usePageDataEmpty } from "@etsoo/smarterp-core";
import {
  StockInitRQ,
  StockItem,
  StockQueryProductData
} from "@etsoo/smarterp-crm";
import { DataTypes, NumberUtils } from "@etsoo/shared";
import { DefaultUI } from "@etsoo/smarterp-core/components";
import { BoxProps } from "@mui/material/Box";
import Typography from "@mui/material/Typography";
import { Permissions } from "@etsoo/smarterp-crm";
import {
  AddressList,
  ProductCategoryTiplist,
  ProductUnitList
} from "@etsoo/smarterp-crm/components";
import Button from "@mui/material/Button";
import { StockActionData, StockProducts, StockWindow } from "./StockWindow";
import { LocalUtils } from "../../../app/LocalUtils";

const template = {
  locationId: "number",
  name: "string",
  assignedId: "string",
  unitId: "number",
  categoryId: "number"
} as const satisfies DataTypes.BasicTemplate;

export default function InitStock() {
  // Route
  const navigate = useNavigate();

  // Labels
  const labels = app.getLabels(
    "actions",
    "add",
    "assignedId",
    "category",
    "categories",
    "confirmAction",
    "edit",
    "nextStep",
    "productName",
    "productUnit",
    "stockQty",
    "stockKindInit",
    "warehouse",
    "view"
  );

  // State
  const [products, setProducts] = React.useState<number[]>([]);

  // Refs
  const ref =
    React.useRef<ScrollerListForwardRef<StockQueryProductData>>(undefined);

  const productsRef = React.useRef<StockProducts>({});

  const locationRef = React.useRef(LocalUtils.getCurrentLocationId());

  // Load data
  const reloadData = React.useCallback(() => ref.current?.reset(), []);

  function updateQty(
    data: StockQueryProductData,
    qty: number | undefined | null
  ) {
    const id = data.id;
    if (qty == null) {
      delete productsRef.current[id];
    } else {
      productsRef.current[id] = { ...data, qty };
    }

    setProducts(Object.keys(productsRef.current).map(Number));
  }

  function complete() {
    app.notifier.data<StockActionData>(
      <StockWindow
        products={productsRef.current}
        defaultLocationToId={locationRef.current}
        toPersonId={app.userData?.system?.personId ?? 0}
        mRef={React.createRef<NotificationMUDataMethods>()}
      />,
      async (data) => {
        if (data == null) return;

        const { locationFromId, locationToId, ...rest } = data;
        if (locationToId == null) return;

        const items: StockItem[] = Object.values(productsRef.current).map(
          (p) => ({
            productId: p.id,
            qty: p.qty
          })
        );

        const rq: StockInitRQ = { ...rest, locationId: locationToId, items };

        const result = await app.stockApi.init(rq);

        if (result == null) return;

        if (result.ok) {
          return navigate("./..");
        } else {
          return app.formatResult(result);
        }
      },
      labels.stockKindInit
    );
  }

  const orgPersonId = app.userData?.system?.personId ?? 0;

  // Page data hook
  usePageDataEmpty(app);

  return (
    <ResponsivePage<StockQueryProductData, typeof template>
      {...DefaultUI.pageProps({
        onRefresh: reloadData,
        fabButtons: (
          <React.Fragment>
            {products.length > 0 && (
              <Button variant="contained" onClick={() => complete()}>
                {labels.nextStep} ({products.length})
              </Button>
            )}
          </React.Fragment>
        )
      })}
      mRef={ref}
      defaultOrderBy={[{ field: "creation", desc: true }]}
      fieldTemplate={template}
      fields={(data) => [
        <AddressList
          name="locationId"
          personId={orgPersonId}
          label={labels.warehouse}
          idValue={data.locationId ?? locationRef.current}
          onValueChange={(value) => {
            locationRef.current = value?.id;
            LocalUtils.setCurrentLocationId(locationRef.current);
          }}
          search
          sx={(theme) => ({
            "& .MuiInputLabel-root": {
              color: theme.palette.warning.main
            }
          })}
        />,
        <SearchField
          label={labels.productName}
          name="name"
          defaultValue={data.name}
        />,
        <SearchField
          label={labels.assignedId}
          name="assignedId"
          minChars={3}
          defaultValue={data.assignedId}
        />,
        <ProductCategoryTiplist
          label={labels.category}
          name="categoryId"
          search
        />,
        <ProductUnitList search value={data.unitId} />
      ]}
      loadData={(data) => {
        const locationId = data.locationId;
        if (locationId == null) return Promise.resolve([]);

        return app.stockApi.queryProduct(data, {
          defaultValue: [],
          showLoading: false
        });
      }}
      columns={[
        {
          field: "name",
          header: labels.productName,
          valueFormatter: ({ data }) =>
            data == null
              ? undefined
              : `${data.assignedId ? `${data.assignedId} - ` : ""}${data.name}`
        },
        {
          width: 148,
          header: labels.stockQty,
          cellBoxStyle: {
            paddingTop: "6px!important"
          },
          cellRenderer: ({
            data
          }: GridCellRendererProps<StockQueryProductData, BoxProps>) => {
            if (data == null) return undefined;

            const qty = productsRef.current[data.id]?.qty ?? data.qty ?? "";

            return (
              <NumberInputField
                search
                fullWidth
                defaultValue={qty}
                disabled={data.qty != null}
                onChange={(event) => {
                  const qty = NumberUtils.parse(event.target.value);
                  updateQty(data, qty);
                }}
              />
            );
          }
        },
        {
          field: "unitName",
          header: labels.productUnit,
          width: 110
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
          }: GridCellRendererProps<StockQueryProductData, BoxProps>) => {
            if (data == null) return undefined;

            return (
              <React.Fragment>
                {app.owns(Permissions.Product.View) && (
                  <IconButtonLink
                    title={labels.view}
                    href={`./../../product/view/${data.id}`}
                  >
                    <ArticleIcon />
                  </IconButtonLink>
                )}
              </React.Fragment>
            );
          }
        }
      ]}
      rowHeight={180}
      itemRenderer={(props) =>
        MobileListItemRenderer(props, (data) => {
          return [
            data.name,
            data.assignedId,
            [
              app.owns(Permissions.Product.View) && {
                label: labels.view,
                icon: <ArticleIcon />,
                action: `./../../product/view/${data.id}`
              }
            ],
            <React.Fragment>
              <Typography variant="body2">
                {data.qty} {data.unitName}
              </Typography>
            </React.Fragment>
          ];
        })
      }
    />
  );
}
