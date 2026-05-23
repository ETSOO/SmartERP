import {
  MobileListItemRenderer,
  NumberInputField,
  ResponsivePage,
  SearchField,
  SelectEx,
  VBox
} from "@etsoo/materialui";
import {
  GridCellRendererProps,
  GridDataType,
  ScrollerListForwardRef
} from "@etsoo/react";
import { DataTypes, DomUtils, IdActionResult } from "@etsoo/shared";
import { DefaultUI } from "@etsoo/smarterp-core/components";
import {
  OrderKind,
  ProductScope,
  StockKind,
  StockQueryLineData,
  StockQueryOrderLineData,
  StockQueryOrderLineRQ
} from "@etsoo/smarterp-crm";
import { app } from "../../../app/MyApp";
import { BoxProps } from "@mui/material/Box";
import React from "react";
import { OrderAllList, ProductList } from "@etsoo/smarterp-crm/components";
import AddIcon from "@mui/icons-material/Add";
import EditIcon from "@mui/icons-material/Edit";
import { Permissions } from "@etsoo/smarterp-crm";
import Fab from "@mui/material/Fab";
import IconButton from "@mui/material/IconButton";
import { Typography } from "@mui/material";

const template = {
  keyword: "string",
  productId: "number",
  qtyStart: "number"
} as const satisfies DataTypes.BasicTemplate;

type QtyData = {
  id?: number;
  max: number;
  step?: number;
  title?: string;
};

export type AllOrderLinesProps = {
  stockId: number;
  personId: number;
  locationId: number;
  kind: StockKind;
  isDeletable: boolean;
  refresh: () => Promise<void>;
};

function StockLineUI({
  kind,
  stockId,
  personId,
  locationId,
  data
}: {
  kind: StockKind;
  stockId: number;
  personId: number;
  locationId: number;
  data?: StockQueryLineData;
}) {
  // Labels
  const labels = app.getLabels("order", "orderLines", "po", "qty");

  const orderKind = kind === StockKind.Order ? OrderKind.Order : OrderKind.PO;

  const [lines, setLines] = React.useState<StockQueryOrderLineData[]>();
  const [qtyData, setQtyData] = React.useState<QtyData>();

  React.useEffect(() => {
    if (data == null) return;

    app.stockApi.readLine(data.id).then((line) => {
      if (line == null) return;

      const max = Math.abs(line.qty) + (line.pendingQty ?? 0);

      setQtyData({
        max,
        step: line.stepQty,
        title: `${max} / ${line.orderQty}`
      });
    });
  }, [data]);

  return (
    <React.Fragment>
      <input type="hidden" name="id" value={qtyData?.id ?? data?.id ?? ""} />

      <VBox spacing={2} sx={{ paddingTop: 1 }}>
        {data == null && (
          <OrderAllList
            label={kind === StockKind.Order ? labels.order : labels.po}
            rq={{ kind: orderKind, personId, enabled: true }}
            inputRequired
            onValueChange={async (od) => {
              if (od == null) return;

              const rq: StockQueryOrderLineRQ = {
                personId,
                locationId,
                stockId,
                orders: [od.id]
              };

              const result = await app.stockApi.queryOrderLines(rq);
              if (result == null) return;

              setLines(result);
            }}
          />
        )}
        {data == null && lines != null && (
          <SelectEx
            fullWidth
            required
            options={lines}
            label={labels.orderLines}
            labelField={(data) =>
              `${data.name}, ${data.pendingQty} / ${data.orderQty} ${data.unitName} `
            }
            onItemChange={(item) => {
              if (item == null) {
                setQtyData(undefined);
              } else {
                const max =
                  kind === StockKind.Order
                    ? Math.min(item.pendingQty, item.stockQty)
                    : item.pendingQty;

                setQtyData({
                  id: item.lines[0].id,
                  max,
                  step: item.stepQty
                });
              }
            }}
          />
        )}
        {data != null && (
          <Typography>
            {data.productName} {qtyData?.title}
          </Typography>
        )}
        <NumberInputField
          label={labels.qty}
          name="qty"
          required
          fullWidth
          defaultValue={data == null ? "" : Math.abs(data.qty)}
          max={qtyData?.max ?? 0}
          step={qtyData?.step ?? 1}
        />
      </VBox>
    </React.Fragment>
  );
}

export function StockLines(props: AllOrderLinesProps) {
  // Destruct
  const { stockId, personId, kind, locationId, isDeletable, refresh } = props;

  // Labels
  const labels = app.getLabels(
    "actions",
    "add",
    "edit",
    "keywords",
    "noChanges",
    "product",
    "price",
    "qty",
    "qtyStart",
    "view"
  );

  // Refs
  const ref =
    React.useRef<ScrollerListForwardRef<StockQueryLineData>>(undefined);

  // Load data
  const reloadData = React.useCallback(() => ref.current?.reset(), []);

  const isValidKind = kind === StockKind.Order || kind === StockKind.PO;
  const isEditable =
    app.owns(Permissions.Inventory.Edit) && isDeletable && isValidKind;

  function doEdit(data?: StockQueryLineData) {
    app.showInputDialog({
      title: data == null ? labels.add : labels.edit,
      message: "",
      callback: async (form) => {
        // Cancelled
        if (form == null) return;

        if (!form.reportValidity()) {
          return false;
        }

        // Form data
        const { id, qty } = DomUtils.dataAs(new FormData(form), {
          id: "number",
          qty: "number"
        });

        if (qty == null || (data != null && qty === Math.abs(data.qty))) {
          DomUtils.setFocus("qty", form);
          return labels.noChanges;
        }

        let result: IdActionResult | undefined;

        if (data == null) {
          result = await app.stockApi.createLine({
            stockId,
            orderLineId: id ?? 0,
            qty
          });
        } else {
          result = await app.stockApi.updateLine({ id: data.id, qty });
        }

        if (result == null) return;

        if (result.ok) {
          reloadData();
          refresh();
          return true;
        }

        return app.formatResult(result);
      },
      inputs: (
        <StockLineUI
          kind={kind}
          stockId={stockId}
          personId={personId}
          locationId={locationId}
          data={data}
        />
      )
    });
  }

  return (
    <ResponsivePage<StockQueryLineData, typeof template>
      {...DefaultUI.pageProps({
        onRefresh: reloadData,
        fabButtons: (
          <React.Fragment>
            {isEditable && (
              <Fab
                title={labels.add}
                size="medium"
                color="primary"
                onClick={() => doEdit()}
              >
                <AddIcon />
              </Fab>
            )}
          </React.Fragment>
        )
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
          header: labels.product,
          valueFormatter: ({ data }) =>
            data
              ? data.productName +
                (data.orderLineId ? ` (${data.orderLineId})` : "")
              : ""
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
          }: GridCellRendererProps<StockQueryLineData, BoxProps>) => {
            if (data == null) return undefined;

            return (
              <React.Fragment>
                {isEditable && (
                  <IconButton title={labels.edit} onClick={() => doEdit(data)}>
                    <EditIcon />
                  </IconButton>
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
              isEditable &&
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
