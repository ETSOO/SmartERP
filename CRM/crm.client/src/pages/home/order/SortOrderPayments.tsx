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
import { OrderPaymentListData } from "@etsoo/smarterp-crm";

export default function SortOrderPayments() {
  const [items, setItems] = React.useState<OrderPaymentListData[]>([]);

  const labels = app.getLabels("sortTip", "dragIndicator");

  const loadData = React.useCallback(() => {
    app.orderPaymentApi
      .list({ isValid: true, queryPaging: 64 })
      .then((result) => setItems(result ?? []));
  }, []);

  usePageDataEmpty(app);

  React.useEffect(() => loadData(), []);

  return (
    <CommonPage>
      {items.length > 0 && (
        <Card>
          <Typography
            variant="caption"
            display="block"
            sx={{ paddingLeft: 2, paddingTop: 2, paddingRight: 2 }}
          >
            * {labels.sortTip}
          </Typography>
          <CardContent>
            <Grid container spacing={0}>
              <DnDSortableList<OrderPaymentListData>
                items={items}
                labelField="title"
                onDragEnd={(items) => {
                  app.orderPaymentApi.sort(items, {
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
                    display="flex"
                    justifyContent="space-between"
                    alignItems="center"
                    ref={ref}
                    style={style}
                  >
                    <Stack direction="row" alignItems="center">
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
