import { useParamsEx } from "@etsoo/react";
import { HBox, VBox, ViewPage } from "@etsoo/materialui";
import { app } from "../../../app/MyApp";
import { usePageDataEmpty } from "@etsoo/smarterp-core";
import React from "react";
import Typography from "@mui/material/Typography";
import { GroupQueryItemsData, GroupViewData } from "@etsoo/smarterp-crm";

export default function ViewOrderLine() {
  // Route
  const { id = 0 } = useParamsEx({ id: "number" });

  // State
  const [items, setItems] = React.useState<GroupQueryItemsData[]>([]);

  // Load data
  const loadData = React.useCallback(async () => {
    const results = await Promise.all([
      app.groupApi.read(id),
      app.groupApi.queryItems()
    ]);
    const viewData = results[0];
    const items = results[1];
    if (items) {
      setItems(items);
    }

    return viewData;
  }, [id]);

  // Page data hook
  usePageDataEmpty(app);

  return (
    <ViewPage<GroupViewData>
      paddings={0}
      titleBar={(item) => (
        <HBox
          sx={{
            justifyContent: "center",
            alignItems: "center",
            marginBottom: 2
          }}
        >
          <Typography
            variant="subtitle2"
            sx={{ textAlign: "center", paddingRight: 2 }}
          >
            {item.name}
          </Typography>
        </HBox>
      )}
      fields={[
        {
          data: (item) => app.getRoleLabel(item.roles),
          label: "role",
          singleRow: true,
          horizontal: true
        }
      ]}
      loadData={loadData}
    ></ViewPage>
  );
}
