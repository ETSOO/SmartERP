import { CommonPage, DnDSortableList } from "@etsoo/materialui";
import React from "react";
import DragIndicatorIcon from "@mui/icons-material/DragIndicator";
import { app } from "../../../app/MyApp";
import { usePageDataEmpty } from "@etsoo/smarterp-core";
import Card from "@mui/material/Card";
import Typography from "@mui/material/Typography";
import CardContent from "@mui/material/CardContent";
import Grid from "@mui/material/Grid";
import Stack from "@mui/material/Stack";
import IconButton from "@mui/material/IconButton";
import { OrderDeliveryListData } from "@etsoo/smarterp-crm";
import { useIsOrder } from "./useIsOrder";

export default function SortOrderDeliveries() {
  const [items, setItems] = React.useState<OrderDeliveryListData[]>([]);

  const isOrder = useIsOrder();

  const labels = app.getLabels("sortTip", "dragIndicator");

  const loadData = React.useCallback(() => {
    app.orderDeliveryApi
      .list({ isValid: true, isOrder, queryPaging: 64 })
      .then((result) => setItems(result ?? []));
  }, [isOrder]);

  usePageDataEmpty(app);

  React.useEffect(() => loadData(), []);

  return (
    <CommonPage>
      {items.length > 0 && (
        <Card>
          <Typography
            variant="caption"
            component="div"
            sx={{ paddingLeft: 2, paddingTop: 2, paddingRight: 2 }}
          >
            * {labels.sortTip}
          </Typography>
          <CardContent>
            <Grid container spacing={0}>
              <DnDSortableList<OrderDeliveryListData>
                items={items}
                labelField="title"
                onDragEnd={(items) => {
                  app.orderDeliveryApi.sort(items, {
                    showLoading: false
                  });
                }}
                itemRenderer={(
                  data,
                  style,
                  { sortable: { index }, ref, handleRef }
                ) => (
                  <Grid
                    size={{ xs: 12, md: 6, xl: 3 }}
                    ref={ref}
                    style={{
                      display: "flex",
                      justifyContent: "space-between",
                      alignItems: "center",
                      ...style
                    }}
                  >
                    <Stack direction="row" sx={{ alignItems: "center" }}>
                      <IconButton
                        style={{ cursor: "move" }}
                        size="small"
                        title={labels.dragIndicator}
                        ref={handleRef}
                      >
                        <DragIndicatorIcon />
                      </IconButton>
                      <Typography>
                        {index + 1}. {data.title}
                      </Typography>
                    </Stack>
                  </Grid>
                )}
              ></DnDSortableList>
            </Grid>
          </CardContent>
        </Card>
      )}
    </CommonPage>
  );
}
