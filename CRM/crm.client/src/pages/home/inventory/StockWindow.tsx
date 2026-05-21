import React from "react";
import {
  DataGrid,
  GridColDef,
  InputField,
  NotificationMUDataProps,
  VBox
} from "@etsoo/materialui";
import { StockKind, StockQueryProductData } from "@etsoo/smarterp-crm";
import { app } from "../../../app/MyApp";
import { AddressList } from "@etsoo/smarterp-crm/components";
import { DateUtils, DomUtils } from "@etsoo/shared";

export type StockProducts = Record<
  number,
  Required<Pick<StockQueryProductData, "qty">> & StockQueryProductData
>;

export type StockActionData =
  | {
      title: string;
      trackingNumber?: string;
      description?: string;
      locationFromId?: number;
      locationToId?: number;
    }
  | undefined;

export function StockWindow({
  kind,
  products,
  fromPersonId,
  toPersonId,
  defaultLocationFromId,
  defaultLocationToId,
  mRef
}: NotificationMUDataProps & {
  kind: StockKind;
  products: StockProducts;
  fromPersonId?: number;
  toPersonId?: number;
  defaultLocationFromId?: number;
  defaultLocationToId?: number;
}) {
  // Labels
  const labels = app.getLabels(
    "description",
    "product",
    "products",
    "productUnit",
    "qty",
    "receivingWarehouse",
    "shippingWarehouse",
    "title",
    "trackingNumber"
  );

  const title = `${app.stock.getKind(kind)} ${DateUtils.format(new Date(), "yyyyMMdd")}`;

  const rows = Object.values(products);

  const totalRows = rows.length;
  const totalQty = rows.reduce((sum, row) => sum + row.qty, 0);

  const formRef = React.useRef<HTMLFormElement>(null);

  React.useImperativeHandle(mRef, () => ({
    getValue: (): StockActionData => {
      const form = formRef.current;
      if (form == null) return;

      // Validate form
      if (!form.reportValidity()) {
        return;
      }

      const {
        locationFromId = defaultLocationFromId,
        locationToId = defaultLocationToId,
        title,
        trackingNumber,
        description
      } = DomUtils.dataAs(new FormData(form), {
        locationFromId: "number",
        locationToId: "number",
        title: "string",
        trackingNumber: "string",
        description: "string"
      });

      if (title == null) {
        return;
      }

      if (locationFromId == null && locationToId == null) {
        return;
      } else if (locationToId === locationFromId) {
        DomUtils.setFocus("locationToIdInput", form);
        return;
      }

      return {
        locationFromId,
        locationToId,
        title,
        trackingNumber,
        description
      };
    }
  }));

  const columns: GridColDef<StockQueryProductData>[] = [
    {
      field: "name",
      headerName: `${labels.product} (${totalRows} / ${totalQty})`,
      flex: 2
    },
    {
      field: "qty",
      headerName: labels.qty,
      type: "number",
      width: 88
    },
    {
      field: "unitName",
      headerName: labels.productUnit,
      width: 102
    }
  ];

  const gridHeight = fromPersonId != null && toPersonId != null ? 200 : 250;

  return (
    <form ref={formRef}>
      <VBox spacing={2} sx={{ paddingTop: 1 }}>
        {fromPersonId && (
          <AddressList
            name="locationFromId"
            personId={fromPersonId}
            label={labels.shippingWarehouse}
            inputRequired
            size="small"
            idValue={defaultLocationFromId}
            disabled={defaultLocationFromId != null}
          />
        )}
        {toPersonId && (
          <AddressList
            name="locationToId"
            personId={toPersonId}
            label={labels.receivingWarehouse}
            inputRequired
            size="small"
            idValue={defaultLocationToId}
            disabled={defaultLocationToId != null}
          />
        )}
        <InputField
          name="title"
          label={labels.title}
          fullWidth
          required
          defaultValue={title}
          size="small"
          slotProps={{ htmlInput: { maxLength: 128 } }}
        />
        {kind == StockKind.StockTransfer && (
          <InputField
            name="trackingNumber"
            label={labels.trackingNumber}
            fullWidth
            size="small"
            slotProps={{ htmlInput: { maxLength: 20 } }}
          />
        )}
        <InputField
          name="description"
          label={labels.description}
          multiline
          rows={1}
          fullWidth
          size="small"
          slotProps={{ htmlInput: { maxLength: 1280 } }}
        />
        <VBox sx={{ height: gridHeight, width: "100%" }}>
          <DataGrid
            rows={rows}
            columns={columns}
            hideFooter
            disableColumnMenu
            disableColumnSorting
            disableMultipleRowSelection
            getRowId={(row) => row.id}
          />
        </VBox>
      </VBox>
    </form>
  );
}
