import {
  ResponsivePage,
  MobileListItemRenderer,
  NumberInputField,
  InputField,
  DataGrid,
  GridColDef,
  useGridApiRef,
  NotificationMUDataMethods,
  NotificationMUDataProps,
  HBox
} from "@etsoo/materialui";
import React from "react";
import {
  GridCellRendererProps,
  GridDataType,
  NotificationMessageType,
  ScrollerListForwardRef
} from "@etsoo/react";
import { useNavigate } from "react-router-dom";
import { app } from "../../../app/MyApp";
import { usePageDataEmpty } from "@etsoo/smarterp-core";
import {
  OrderListData,
  StockOrderItem,
  StockOrderOutRQ,
  StockQueryOrderLineData,
  StockQueryOrderLineItemData
} from "@etsoo/smarterp-crm";
import { DefaultUI } from "@etsoo/smarterp-core/components";
import { BoxProps } from "@mui/material/Box";
import Button from "@mui/material/Button";
import WidgetsIcon from "@mui/icons-material/Widgets";
import ClearIcon from "@mui/icons-material/Clear";
import IconButton from "@mui/material/IconButton";
import DoneAllIcon from "@mui/icons-material/DoneAll";
import DetailsIcon from "@mui/icons-material/Details";
import Avatar from "@mui/material/Avatar";
import { DateUtils, DomUtils, Utils } from "@etsoo/shared";
import { AddressList, CustomerList } from "@etsoo/smarterp-crm/components";
import Grid from "@mui/material/Grid";
import { LocalUtils } from "../../../app/LocalUtils";
import { StockByWarehouse } from "./StockByWarehouse";
import { Typography } from "@mui/material";
import { StockActionData } from "./StockActionData";

type StateData = {
  customerName: string;
  locationFromName: string;
  products: number;
  totalQty: number;
};

type DetailData = StockQueryOrderLineItemData & {
  order: string;
  shipQty?: number;
};

function CheckDetailsUI({
  mRef,
  productId,
  qtyPending,
  rows
}: NotificationMUDataProps & {
  productId: number;
  qtyPending: number;
  rows: DetailData[];
}) {
  // Labels
  const labels = app.getLabels(
    "deliveryQtySetError",
    "noRows",
    "order",
    "qtyDelivered",
    "shippedQty"
  );

  React.useImperativeHandle(mRef, () => ({
    getValue: (): StockOrderItem[] | undefined => {
      if (gridRef.current == null) return;

      const shipLines: StockOrderItem[] = [];

      let totalQty = 0;
      gridRef.current.getRowModels().forEach((row) => {
        const shipQty: number | undefined = row.shipQty;
        if (shipQty == null || shipQty <= 0) return;

        shipLines.push({
          productId,
          qty: shipQty,
          orderLineId: row.id
        });

        totalQty += shipQty;
      });

      if (totalQty === 0) {
        app.notifier.message(NotificationMessageType.Warning, labels.noRows);
        return;
      }

      if (totalQty > qtyPending) {
        app.notifier.message(
          NotificationMessageType.Warning,
          labels.deliveryQtySetError.format(
            totalQty.toString(),
            qtyPending.toString()
          )
        );
        return;
      }

      return shipLines;
    }
  }));

  const gridRef = useGridApiRef();

  const columns: GridColDef<DetailData>[] = [
    {
      field: "order",
      headerName: labels.order,
      editable: false,
      flex: 2
    },
    {
      field: "qtyDelivered",
      headerName: labels.qtyDelivered,
      type: "number",
      valueGetter: (_value, row) => `${row.qty - row.pendingQty} / ${row.qty}`,
      width: 118
    },
    {
      field: "shipQty",
      headerName: labels.shippedQty,
      type: "number",
      width: 100,
      editable: true
    }
  ];

  return (
    <HBox sx={{ height: 400, width: "100%" }}>
      <DataGrid
        apiRef={gridRef}
        rows={rows}
        columns={columns}
        editMode="row"
        hideFooter
        disableColumnMenu
        disableColumnSorting
        disableMultipleRowSelection
      />
    </HBox>
  );
}

function CustomerChooserUI({
  mRef,
  data
}: NotificationMUDataProps & { data?: StockActionData }) {
  // Labels
  const labels = app.getLabels(
    "description",
    "id",
    "receivingWarehouse",
    "shippingWarehouse",
    "stockKindOrder",
    "title",
    "trackingNumber"
  );

  const orgPersonId = app.userData?.system?.personId ?? 0;

  const gridRef = useGridApiRef();

  const formRef = React.useRef<HTMLFormElement>(null);
  const titleInputRef = React.useRef<HTMLInputElement>(null);
  const customerNameRef = React.useRef<string>(undefined);
  const locationFromNameRef = React.useRef<string>(undefined);
  const locationToNameRef = React.useRef<string>(undefined);

  const [customerId, setCustomerId] = React.useState<number>();
  const [orders, setOrders] = React.useState<OrderListData[]>([]);

  React.useEffect(() => {
    if (data == null || data.orders.length === 0) {
      return;
    }

    gridRef.current?.setRowSelectionModel({
      type: "include",
      ids: new Set(data.orders)
    });
  }, [orders]);

  React.useImperativeHandle(mRef, () => ({
    getValue: (): StockActionData | undefined => {
      const form = formRef.current;
      if (form == null) return;

      // Validate form
      if (!form.reportValidity()) {
        return;
      }

      const ids = gridRef.current?.state.rowSelection.ids;
      const orders = ids ? [...ids].map(Number) : [];
      if (orders.length === 0) {
        return;
      }

      const {
        locationFromId = data?.locationFromId,
        locationToId = data?.locationToId,
        customerId = data?.personId,
        title = data?.title,
        ...rest
      } = DomUtils.dataAs(new FormData(form), {
        customerId: "number",
        locationFromId: "number",
        locationToId: "number",
        trackingNumber: "string",
        title: "string",
        description: "string"
      });

      if (
        title == null ||
        customerId == null ||
        locationFromId == null ||
        locationToId == null
      ) {
        return;
      }

      const personName = customerNameRef.current ?? data?.personName;
      const locationFromName =
        locationFromNameRef.current ?? data?.locationFromName;
      const locationToName = locationToNameRef.current ?? data?.locationToName;
      if (
        personName == null ||
        locationFromName == null ||
        locationToName == null
      ) {
        return;
      }

      return {
        personId: customerId,
        personName,
        locationFromId,
        locationToId,
        locationFromName,
        locationToName,
        orders,
        title,
        lines: data?.lines ?? [],
        ...rest
      };
    }
  }));

  const columns: GridColDef<OrderListData>[] = [
    {
      field: "id",
      headerName: `${labels.id}`,
      width: 108
    },
    {
      field: "title",
      headerName: `${labels.title}`,
      flex: 2
    }
  ];

  return (
    <form ref={formRef}>
      <Grid container spacing={2} sx={{ paddingTop: 1 }}>
        <Grid size={{ xs: 12, sm: 6 }}>
          <CustomerList
            inputRequired
            onValueChange={(value) => {
              const id = value?.id;
              setCustomerId(id);

              customerNameRef.current = value?.name;

              if (titleInputRef.current) {
                titleInputRef.current.value = value?.name
                  ? `${Utils.formatName(value.name, 6)} ${labels.stockKindOrder} ${DateUtils.format(new Date(), "yyyyMMdd")}`
                  : "";
              }

              if (id) {
                // Load Orders
                app.orderApi
                  .list(
                    {
                      customerId: id,
                      enabled: true,
                      queryPaging: {
                        currentPage: 0,
                        batchSize: 30,
                        orderBy: [{ field: "id", desc: false, unique: true }]
                      }
                    },
                    { defaultValue: [] }
                  )
                  .then((data) => {
                    if (data == null) return;
                    setOrders(data);
                  });
              } else {
                setOrders([]);
              }
            }}
            idValue={data?.personId}
            disabled={data?.personId != null}
            fullWidth
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          {customerId && (
            <AddressList
              name="locationToId"
              personId={customerId}
              label={labels.receivingWarehouse}
              inputRequired
              idValue={data?.locationToId}
              onValueChange={(value) =>
                (locationToNameRef.current = value?.name)
              }
              fullWidth
            />
          )}
        </Grid>
        <Grid size={{ xs: 12, sm: 12 }} sx={{ height: 160 }}>
          <DataGrid
            rows={orders}
            columns={columns}
            columnHeaderHeight={0}
            checkboxSelection
            disableColumnMenu
            disableColumnSorting
            disableRowSelectionOnClick
            getRowId={(row) => row.id}
            hideFooter
            apiRef={gridRef}
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <AddressList
            name="locationFromId"
            personId={orgPersonId}
            label={labels.shippingWarehouse}
            inputRequired
            idValue={data?.locationFromId}
            disabled={data?.locationFromId != null}
            onValueChange={(value) =>
              (locationFromNameRef.current = value?.name)
            }
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <InputField
            name="trackingNumber"
            label={labels.trackingNumber}
            fullWidth
            defaultValue={data?.trackingNumber ?? ""}
            slotProps={{ htmlInput: { maxLength: 20 } }}
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 12 }}>
          <InputField
            name="title"
            label={labels.title}
            inputRef={titleInputRef}
            fullWidth
            defaultValue={data?.title ?? ""}
            required
            slotProps={{ htmlInput: { maxLength: 128 } }}
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 12 }}>
          <InputField
            name="description"
            label={labels.description}
            multiline
            rows={2}
            defaultValue={data?.description ?? ""}
            fullWidth
            slotProps={{ htmlInput: { maxLength: 1280 } }}
          />
        </Grid>
      </Grid>
    </form>
  );
}

export default function OrderStock() {
  // Route
  const navigate = useNavigate();

  // Labels
  const labels = app.getLabels(
    "chooseCustomer",
    "clear",
    "confirmAction",
    "deliveredQty",
    "details",
    "nextStep",
    "orderQty",
    "productName",
    "products",
    "productUnit",
    "shipAll",
    "shippedQty",
    "stockQty",
    "stockByWarehouse",
    "submit",
    "totalQty"
  );

  // State
  const [stateData, setStateData] = React.useState<StateData>();

  // Refs
  const ref =
    React.useRef<ScrollerListForwardRef<StockQueryOrderLineData>>(undefined);

  const stockDataRef = React.useRef<StockActionData>(
    app.storage.getPersistedData<StockActionData>(
      LocalUtils.STOCK_ORDER_DATA_KEY
    )
  );

  // Load data
  const reloadData = React.useCallback(() => ref.current?.reset(), []);

  // Page data hook
  usePageDataEmpty(app);

  function cacheData(data: StockActionData) {
    app.storage.setPersistedData(LocalUtils.STOCK_ORDER_DATA_KEY, data);
    doStateData(data);
  }

  function chooseCustomer() {
    app.notifier.data<StockActionData>(
      <CustomerChooserUI
        data={stockDataRef.current}
        mRef={React.createRef<NotificationMUDataMethods>()}
      />,
      async (data) => {
        if (data == null) {
          if (stockDataRef.current == null) {
            navigate(-1);
          }
          return;
        }
        stockDataRef.current = data;
        cacheData(data);
        reloadData();
      },
      labels.chooseCustomer,
      {
        fullScreen: app.smDown,
        buttons: (n, _callback, base) => (
          <React.Fragment>
            <Button
              startIcon={<ClearIcon />}
              variant="outlined"
              onClick={() => {
                app.notifier.confirm(
                  labels.confirmAction.format(labels.clear),
                  undefined,
                  (result) => {
                    if (result) {
                      n.dismiss();

                      stockDataRef.current = undefined;
                      app.storage.setPersistedData(
                        LocalUtils.STOCK_ORDER_DATA_KEY,
                        null
                      );
                      reloadData();
                      setStateData(undefined);

                      chooseCustomer();
                    }
                  }
                );
              }}
            >
              {labels.clear}
            </Button>
            {base()}
          </React.Fragment>
        )
      }
    );
  }

  function doStateData(data: StockActionData) {
    const products = new Set(data.lines.map((l) => l.productId)).size;
    const totalQty = data.lines.reduce((sum, line) => sum + line.qty, 0);

    setStateData({
      customerName: data.personName,
      locationFromName: data.locationFromName,
      products,
      totalQty
    });
  }

  async function loadData() {
    const data = stockDataRef.current;
    if (data == null) {
      return [];
    }

    const { personId, locationFromId: locationId, orders } = data;

    const lines = await app.stockApi.queryOrderLines(
      { personId, locationId, orders },
      {
        defaultValue: [],
        showLoading: false
      }
    );

    if (lines == null) return [];
    else return lines;
  }

  const ordersRef = React.useRef<OrderListData[]>([]);

  function combineData(
    data: StockQueryOrderLineData,
    orders: OrderListData[],
    lines: StockOrderItem[]
  ) {
    return data.lines.map<DetailData>((line) => {
      const order =
        orders.find((o) => o.id === line.orderId)?.title ??
        line.orderId.toString();
      const s = lines.find((l) => l.orderLineId === line.id);

      return {
        ...line,
        order,
        shipQty: s?.qty
      };
    });
  }

  function updateLines(
    data: StockActionData,
    productId: number,
    lines: StockOrderItem[]
  ) {
    // Remove old lines for the product and add new lines
    data.lines = [
      ...data.lines.filter((l) => l.productId !== productId),
      ...lines
    ];
  }

  async function checkDetails(
    data: StockQueryOrderLineData,
    callback: (qty?: number) => void
  ) {
    if (stockDataRef.current == null) return;

    if (ordersRef.current.length === 0) {
      const orders = await app.orderApi.list(
        { ids: stockDataRef.current?.orders },
        { showLoading: false, defaultValue: [] }
      );
      if (orders == null) return;
      ordersRef.current = orders;
    }

    const rows = combineData(
      data,
      ordersRef.current,
      stockDataRef.current.lines
    );

    app.notifier.data<StockOrderItem[] | undefined>(
      <CheckDetailsUI
        productId={data.id}
        qtyPending={data.pendingQty}
        rows={rows}
        mRef={React.createRef<NotificationMUDataMethods>()}
      />,
      async (lines) => {
        if (lines == null || stockDataRef.current == null) return;
        updateLines(stockDataRef.current, data.id, lines);
        const newQty = lines.reduce((sum, line) => sum + line.qty, 0);
        callback(newQty);
        cacheData(stockDataRef.current);
      },
      labels.details,
      {
        fullScreen: app.smDown
      }
    );
  }

  function getQty(id: number): number | undefined {
    if (stockDataRef.current == null) return undefined;

    const lines = stockDataRef.current.lines.filter((l) => l.productId === id);
    if (lines.length === 0) return undefined;

    return lines.reduce((sum, line) => sum + line.qty, 0);
  }

  function doShip(data: StockQueryOrderLineData, qty: number | undefined) {
    if (stockDataRef.current == null) return;

    if (qty == null || qty <= 0) {
      // remove all lines
      stockDataRef.current.lines = stockDataRef.current.lines.filter(
        (l) => !data.lines.some((dl) => dl.id == l.orderLineId)
      );
    } else {
      const newLines: StockOrderItem[] = [];

      const lines = data.lines;

      // First check if there is a line that can fully match the qty
      // 首先检查是否有完全匹配的行
      const fullMatch = lines.find((l) => l.pendingQty === qty);
      if (fullMatch) {
        newLines.push({
          productId: data.id,
          qty,
          orderLineId: fullMatch.id
        });
      } else {
        for (let i = 0; i < lines.length; i++) {
          const line = lines[i];

          const shipQty = qty <= line.pendingQty ? qty : line.pendingQty;

          qty -= shipQty;

          newLines.push({
            productId: data.id,
            qty: shipQty,
            orderLineId: line.id
          });

          if (qty > 0) {
            // If there is still qty to ship, try to find a line with the same pendingQty to ship together
            // 向后检查是否有完全匹配
            for (let j = i + 1; j < data.lines.length; j++) {
              const next = data.lines[j];

              if (next.pendingQty === qty) {
                newLines.push({
                  productId: data.id,
                  qty,
                  orderLineId: next.id
                });

                qty = 0;
                break;
              }
            }
          }

          if (qty <= 0) {
            break;
          }
        }
      }
      updateLines(stockDataRef.current, data.id, newLines);
    }

    cacheData(stockDataRef.current);
  }

  async function doShipAll() {
    if (stockDataRef.current == null) return;

    const products = await loadData();

    const lines = products.flatMap((p) =>
      p.lines.map<StockOrderItem>((l) => ({
        productId: p.id,
        qty: Math.min(l.pendingQty, p.stockQty),
        orderLineId: l.id
      }))
    );

    stockDataRef.current.lines = lines;

    cacheData(stockDataRef.current);

    reloadData();
  }

  function doSubmit() {
    const data = stockDataRef.current;
    if (data == null) return;

    const { products, totalQty } = data.lines.reduce(
      (prev, line) => {
        return {
          products: [...prev.products, line.productId],
          totalQty: prev.totalQty + line.qty
        };
      },
      {
        products: [] as number[],
        totalQty: 0
      }
    );

    const title = `${labels.products}: ${new Set(products).size}, ${labels.totalQty}: ${totalQty}. ${labels.confirmAction.format(labels.submit)}`;

    app.notifier.confirm(title, undefined, async (yes) => {
      if (!yes) return;

      const rq: StockOrderOutRQ = {
        customerId: data.personId,
        locationFromId: data.locationFromId,
        locationToId: data.locationToId,
        title: data.title,
        description: data.description,
        trackingNumber: data.trackingNumber,
        orders: data.orders,
        items: data.lines
      };

      const result = await app.stockApi.orderOut(rq);
      if (result == null) return;

      if (result.ok) {
        // Clear cache
        app.storage.setPersistedData(LocalUtils.STOCK_ORDER_DATA_KEY, null);

        navigate("./..");
        return;
      }

      app.alertResult(result);
    });
  }

  React.useEffect(() => {
    if (stockDataRef.current == null) {
      chooseCustomer();
    } else {
      // Auto load data
      doStateData(stockDataRef.current);
    }
  }, []);

  return (
    <ResponsivePage<StockQueryOrderLineData, never>
      {...DefaultUI.pageProps({
        onRefresh: reloadData,
        fabButtons: stateData ? (
          <React.Fragment>
            {stateData.products > 0 && (
              <Button variant="contained" onClick={() => doSubmit()}>
                {labels.nextStep} ({stateData.products} / {stateData.totalQty})
              </Button>
            )}
            {stateData.locationFromName}
            {" => "}
            <IconButton
              size="small"
              title={stateData.customerName}
              onClick={() => chooseCustomer()}
            >
              <Avatar>{stateData.customerName}</Avatar>
            </IconButton>
            <Button
              variant="outlined"
              onClick={doShipAll}
              startIcon={<DoneAllIcon />}
            >
              {labels.shipAll}
            </Button>
          </React.Fragment>
        ) : undefined
      })}
      mRef={ref}
      fields={[]}
      loadData={async () => {
        const lines = await loadData();

        if (stockDataRef.current != null) {
          const lineIds = lines.flatMap((l) => l.lines.map((li) => li.id));
          const stockLines = stockDataRef.current.lines.filter((l) =>
            lineIds.includes(l.orderLineId)
          );

          if (stockDataRef.current.lines.length !== stockLines.length) {
            stockDataRef.current.lines = stockLines;
            cacheData(stockDataRef.current);
          }
        }

        return lines;
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
          field: "stockQty",
          header: labels.stockQty,
          type: GridDataType.Number,
          width: 108
        },
        {
          header: "",
          width: 48,
          cellBoxStyle: {
            paddingTop: "6px!important",
            paddingLeft: "0px!important",
            paddingRight: "0px!important"
          },
          cellRenderer: ({
            data
          }: GridCellRendererProps<StockQueryOrderLineData, BoxProps>) => {
            if (data == null) return undefined;
            return (
              <IconButton
                title={labels.stockByWarehouse}
                onClick={() =>
                  StockByWarehouse.show(
                    data.id,
                    stockDataRef.current?.locationToId
                  )
                }
              >
                {<WidgetsIcon />}
              </IconButton>
            );
          }
        },
        {
          field: "orderQty",
          header: labels.orderQty,
          type: GridDataType.Number,
          width: 108
        },
        {
          header: labels.deliveredQty,
          type: GridDataType.Number,
          valueFormatter: ({ data }) =>
            data == null ? undefined : data.orderQty - data.pendingQty,
          width: 108
        },
        {
          width: 188,
          header: labels.shippedQty,
          cellBoxStyle: {
            paddingTop: "6px!important"
          },
          cellRenderer: ({
            data
          }: GridCellRendererProps<StockQueryOrderLineData, BoxProps>) => {
            if (data == null) return undefined;

            const qty = getQty(data.id);
            const qtyInputRef = React.createRef<HTMLInputElement>();
            const lineLength = data.lines.length;

            return (
              <HBox spacing={0.5} sx={{ alignItems: "center" }}>
                <NumberInputField
                  search
                  fullWidth
                  inputRef={qtyInputRef}
                  step={data.stepQty ?? 1}
                  max={Math.min(data.pendingQty, data.stockQty)}
                  defaultValue={qty ?? ""}
                  onNumberChange={(value) => {
                    doShip(data, value);
                  }}
                />
                {lineLength > 0 && (
                  <IconButton
                    size="small"
                    title={`${labels.details} (${lineLength})`}
                    onClick={() =>
                      checkDetails(data, (qty) => {
                        if (qtyInputRef.current && qty != null) {
                          qtyInputRef.current.value = qty.toString();
                        }
                      })
                    }
                  >
                    <DetailsIcon />
                  </IconButton>
                )}
              </HBox>
            );
          }
        },
        {
          field: "unitName",
          header: labels.productUnit,
          width: 110
        }
      ]}
      rowHeight={180}
      itemRenderer={(props) =>
        MobileListItemRenderer(props, (data) => {
          const qty = getQty(data.id);
          const qtyInputRef = React.createRef<HTMLInputElement>();
          const lineLength = data.lines.length;

          return [
            data.name,
            data.assignedId,
            [
              {
                label: labels.stockByWarehouse,
                icon: <WidgetsIcon />,
                action: () =>
                  StockByWarehouse.show(
                    data.id,
                    stockDataRef.current?.locationToId
                  )
              }
            ],
            <React.Fragment>
              <Typography variant="body2">
                {labels.deliveredQty}: {data.orderQty - data.pendingQty} /{" "}
                {data.orderQty} {data.unitName}
              </Typography>
              <HBox spacing={0.5} sx={{ paddingTop: 1, alignItems: "center" }}>
                <NumberInputField
                  search
                  fullWidth
                  inputRef={qtyInputRef}
                  step={data.stepQty ?? 1}
                  max={Math.min(data.pendingQty, data.stockQty)}
                  defaultValue={qty ?? ""}
                  onNumberChange={(value) => {
                    doShip(data, value);
                  }}
                />
                {lineLength > 0 && (
                  <IconButton
                    size="small"
                    title={`${labels.details} (${lineLength})`}
                    onClick={() =>
                      checkDetails(data, (qty) => {
                        if (qtyInputRef.current && qty != null) {
                          qtyInputRef.current.value = qty.toString();
                        }
                      })
                    }
                  >
                    <DetailsIcon />
                  </IconButton>
                )}
              </HBox>
            </React.Fragment>
          ];
        })
      }
    />
  );
}
