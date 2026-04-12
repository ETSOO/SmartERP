import { CommonPage, DnDSortableList } from "@etsoo/materialui";
import React from "react";
import DragIndicatorIcon from "@mui/icons-material/DragIndicator";
import { PromotionListData } from "@etsoo/smarterp-crm";
import { app } from "../../../app/MyApp";
import { usePageDataEmpty } from "@etsoo/smarterp-core";
import Card from "@mui/material/Card";
import Typography from "@mui/material/Typography";
import CardContent from "@mui/material/CardContent";
import Grid from "@mui/material/Grid";
import Stack from "@mui/material/Stack";
import IconButton from "@mui/material/IconButton";

export default function SortPromotions() {
  // State
  const [items, setItems] = React.useState<PromotionListData[]>([]);

  // Labels
  const labels = app.getLabels("sortTip", "parentPromotion", "dragIndicator");

  const loadData = React.useCallback(() => {
    app.promotionApi
      .list({ isValid: true, queryPaging: 64 })
      .then((result) => setItems(result ?? []));
  }, []);

  // Page data hook
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
              <DnDSortableList<PromotionListData>
                items={items}
                labelField="title"
                onDragEnd={(items) => {
                  app.promotionApi.sort(items, {
                    // No indicator for loading
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
