import { DataGrid, GridColDef, VBox } from "@etsoo/materialui";
import { app } from "../../../app/MyApp";
import { StockSiteViewProductData } from "@etsoo/smarterp-crm";
import { NotificationMessageType } from "@etsoo/react";

export namespace StockByWarehouse {
  export function show(productId: number, locationId?: number) {
    app.stockSiteApi.viewProduct(productId).then((data) => {
      if (data == null) return;

      const labels = app.getLabels("noRows", "qty", "refreshTime", "warehouse");

      const columns: GridColDef<StockSiteViewProductData>[] = [
        {
          field: "locationName",
          headerName: labels.warehouse,
          flex: 2,
          cellClassName: (params) => {
            if (params.row.locationId === locationId) {
              return "warehouse-highlight";
            } else {
              return "";
            }
          }
        },
        {
          field: "qty",
          headerName: labels.qty,
          type: "number",
          width: 110
        },
        {
          field: "refreshTime",
          headerName: labels.refreshTime,
          width: 110,
          valueFormatter: (value) => app.formatDate(value)
        }
      ];

      app.notifier.alert(
        <VBox sx={{ height: 400, width: "100%" }}>
          <DataGrid
            rows={data}
            columns={columns}
            hideFooter
            disableColumnMenu
            disableColumnSorting
            disableMultipleRowSelection
            localeText={{ noRowsLabel: labels.noRows }}
            sx={{
              "& .warehouse-highlight": {
                fontWeight: "bold"
              }
            }}
          />
        </VBox>,
        undefined,
        NotificationMessageType.Info,
        { fullScreen: app.smDown }
      );
    });
  }
}
