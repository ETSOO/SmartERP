import { CommonPage, DnDList, HBox } from "@etsoo/materialui";
import React from "react";
import DragIndicatorIcon from "@mui/icons-material/DragIndicator";
import { ProductCategoryListData } from "@etsoo/smarterp-crm";
import { app } from "../../../app/MyApp";
import { usePageDataEmpty } from "@etsoo/smarterp-core";
import { ProductCategoryTiplist } from "@etsoo/smarterp-crm/components";
import Card from "@mui/material/Card";
import Typography from "@mui/material/Typography";
import CardContent from "@mui/material/CardContent";
import Grid from "@mui/material/Grid";
import Stack from "@mui/material/Stack";
import IconButton from "@mui/material/IconButton";

export default function SortProductCategories() {
  // State
  const [items, setItems] = React.useState<ProductCategoryListData[]>([]);

  // Labels
  const labels = app.getLabels("sortTip", "parentCategory", "dragIndicator");

  // Refs
  const refs = React.useRef<{ parentId?: number }>({});

  const loadData = React.useCallback(() => {
    const { parentId } = refs.current;
    app.productCategoryApi
      .list({ parentId: parentId ?? 0, queryPaging: 64 })
      .then((result) => setItems(result ?? []));
  }, []);

  // Page data hook
  usePageDataEmpty(app);

  React.useEffect(() => loadData(), []);

  return (
    <CommonPage>
      <HBox marginBottom={2} gap={1} justifyContent="center">
        <ProductCategoryTiplist
          label={labels.parentCategory}
          name="parentId"
          search
          width={300}
          onValueChange={(value) => {
            const parentId = value?.id;
            if (parentId === refs.current.parentId) return;
            refs.current.parentId = parentId;
            loadData();
          }}
        />
      </HBox>
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
              <DnDList<ProductCategoryListData>
                items={items}
                labelField="name"
                onDragEnd={(items) => {
                  app.productCategoryApi.sort(items, {
                    // No indicator for loading
                    showLoading: false
                  });
                }}
                itemRenderer={(item, index, nodeRef, actionNodeRef) => (
                  <Grid
                    size={{ xs: 12, md: 6, xl: 3 }}
                    display="flex"
                    justifyContent="space-between"
                    alignItems="center"
                    {...nodeRef}
                  >
                    <Stack direction="row" alignItems="center">
                      <IconButton
                        style={{ cursor: "move" }}
                        size="small"
                        title={labels.dragIndicator}
                        {...actionNodeRef}
                      >
                        <DragIndicatorIcon />
                      </IconButton>
                      <Typography>
                        {index + 1}. {item.name}
                      </Typography>
                    </Stack>
                  </Grid>
                )}
              ></DnDList>
            </Grid>
          </CardContent>
        </Card>
      )}
    </CommonPage>
  );
}
