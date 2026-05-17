import React from "react";
import {
  DataGrid,
  GridColDef,
  InputField,
  NotificationMUDataProps,
  VBox
} from "@etsoo/materialui";
import { StockQueryProductData } from "@etsoo/smarterp-crm";
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
      description?: string;
      locationFromId?: number;
      locationToId?: number;
    }
  | undefined;

export function StockWindow({
  products,
  fromPersonId,
  toPersonId,
  defaultLocationFromId,
  defaultLocationToId,
  mRef
}: NotificationMUDataProps & {
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
    "stockKindInit",
    "title"
  );

  const title = `${labels.stockKindInit} ${DateUtils.format(new Date(), "yyyyMMdd")}`;

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
        description
      } = DomUtils.dataAs(new FormData(form), {
        locationFromId: "number",
        locationToId: "number",
        title: "string",
        description: "string"
      });

      if (title == null) {
        return;
      }

      if (locationFromId == null && locationToId == null) {
        return;
      }

      return { locationFromId, locationToId, title, description };
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
      width: 110
    },
    {
      field: "unitName",
      headerName: labels.productUnit,
      width: 110
    }
  ];

  return (
    <form ref={formRef}>
      <VBox spacing={2} sx={{ paddingTop: 1 }}>
        {fromPersonId && (
          <AddressList
            name="locationFromId"
            personId={fromPersonId}
            label={labels.shippingWarehouse}
            inputRequired
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
          slotProps={{ htmlInput: { maxLength: 128 } }}
        />
        <InputField
          name="description"
          label={labels.description}
          multiline
          rows={1}
          fullWidth
          slotProps={{ htmlInput: { maxLength: 1280 } }}
        />
        <VBox sx={{ height: 250, width: "100%" }}>
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
