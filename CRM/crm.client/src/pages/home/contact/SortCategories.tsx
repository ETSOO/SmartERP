import { CommonPage, DnDList, HBox } from "@etsoo/materialui";
import React from "react";
import DragIndicatorIcon from "@mui/icons-material/DragIndicator";
import { PersonCategoryListData } from "@etsoo/smarterp-crm";
import { app } from "../../../app/MyApp";
import { usePageDataEmpty } from "@etsoo/smarterp-core";
import { PersonCategoryTiplist } from "@etsoo/smarterp-crm/components";
import Card from "@mui/material/Card";
import Typography from "@mui/material/Typography";
import CardContent from "@mui/material/CardContent";
import Grid from "@mui/material/Grid";
import Stack from "@mui/material/Stack";
import IconButton from "@mui/material/IconButton";
import { IdentityFlagsList } from "@etsoo/smarterp-core/components";
import { useSearchParamsEx } from "@etsoo/react";
import { DataTypes } from "@etsoo/shared";
import { IdentityTypeFlags } from "@etsoo/appscript";

export default function SortCategories() {
  // Parameters
  const { identityType = -1 } = useSearchParamsEx({
    identityType: "number"
  });
  const it = DataTypes.getEnumByValue(IdentityTypeFlags, identityType);

  // State
  const [items, setItems] = React.useState<PersonCategoryListData[]>([]);

  // Labels
  const labels = app.getLabels("sortTip", "parentCategory", "dragIndicator");

  // Refs
  const refs = React.useRef<{ identityType?: number; parentId?: number }>({
    identityType: it
  });

  const loadData = React.useCallback(() => {
    const { identityType, parentId } = refs.current;
    if (identityType == null) {
      setItems([]);
    } else {
      app.personCategoryApi
        .list({ identityType, parentId: parentId ?? 0, queryPaging: 64 })
        .then((result) => setItems(result ?? []));
    }
  }, []);

  // Page data hook
  usePageDataEmpty(app);

  return (
    <CommonPage>
      <HBox marginBottom={2} gap={1} justifyContent="center">
        {it == null && (
          <IdentityFlagsList
            search
            onItemChange={(item, userAction) => {
              if (!userAction) return;
              const identityType = item?.id;
              if (identityType === refs.current.identityType) return;
              refs.current.identityType = identityType;
              loadData();
            }}
          />
        )}
        <PersonCategoryTiplist
          label={labels.parentCategory}
          name="parentId"
          search
          width={300}
          onLoadData={(rq) =>
            Object.assign(rq, { identityType: it ?? refs.current.identityType })
          }
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
              <DnDList<PersonCategoryListData>
                items={items}
                labelField="name"
                onDragEnd={(items) => {
                  app.personCategoryApi.sort(items, {
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
