import { useParamsEx } from "@etsoo/react";
import { HBox, VBox, ViewPage } from "@etsoo/materialui";
import { app } from "../../../app/MyApp";
import { usePageDataEmpty } from "@etsoo/smarterp-core";
import React from "react";
import Typography from "@mui/material/Typography";
import { GroupQueryItemsData, GroupViewData } from "@etsoo/smarterp-crm";
import Divider from "@mui/material/Divider";
import FormControlLabel from "@mui/material/FormControlLabel";
import Checkbox from "@mui/material/Checkbox";
import Grid from "@mui/material/Grid";

type AllItemsProps = {
  items: GroupQueryItemsData[];
  includedItems: number[];
};

function AllItems(props: AllItemsProps) {
  const { items, includedItems } = props;

  const getLabel = React.useCallback((name: string) => {
    let label = app.get(name.formatInitial(false));
    return label;
  }, []);

  return (
    <VBox spacing={0.5}>
      {app.system.getModules().map((m) => {
        // Module items
        const moduleItems = items
          .filter((item) => item.module === m.id)
          .sort((a, b) => a.id - b.id);

        let all: boolean = false;

        return (
          <React.Fragment key={m.id}>
            <Typography>{m.label}</Typography>
            <Divider />
            <Grid container>
              {moduleItems.map((item, index) => {
                let isIncluded = includedItems.includes(item.id);
                if (isIncluded) {
                  if (index === 0) {
                    all = true;
                  }
                } else if (index > 0 && all) {
                  isIncluded = true;
                }
                return (
                  <Grid
                    size={{ xs: 6, sm: 4, md: 3, lg: 2, xl: 1 }}
                    key={item.id}
                  >
                    <FormControlLabel
                      control={
                        <Checkbox
                          disabled
                          value={item.id}
                          defaultChecked={isIncluded}
                        />
                      }
                      label={getLabel(item.name)}
                    />
                  </Grid>
                );
              })}
            </Grid>
          </React.Fragment>
        );
      })}
    </VBox>
  );
}

export default function ViewGroup() {
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
        },
        {
          data: (item) => <AllItems items={items} includedItems={item.items} />,
          singleRow: true
        }
      ]}
      loadData={loadData}
    ></ViewPage>
  );
}
