import { CommonPage, TabBox } from "@etsoo/materialui";
import ArticleIcon from "@mui/icons-material/Article";
import ListAltIcon from "@mui/icons-material/ListAlt";
import React from "react";
import { usePageDataEmpty } from "@etsoo/smarterp-core";
import LinearProgress from "@mui/material/LinearProgress";
import { DefaultUI } from "@etsoo/smarterp-core/components";
import { app } from "../../../app/MyApp";
import { useParamsEx } from "@etsoo/react";
import { StockViewUI } from "./StockViewUI";
import { StockLines } from "./StockLines";
import { StockKind, StockViewData } from "@etsoo/smarterp-crm";

export default function ViewStock() {
  // Route
  const { id = 0 } = useParamsEx({ id: "number" });

  // Labels
  const labels = app.getLabels("basicInfo", "stockLines", "view");

  // State
  const [data, setData] = React.useState<StockViewData>();

  // Load data
  const loadData = React.useCallback(async () => {
    const data = await app.stockApi.read(id);
    setData(data);
  }, [id]);

  // Page data hook
  usePageDataEmpty(app);

  // Layout
  return (
    <CommonPage paddings={0} onRefresh={loadData}>
      {data == null ? (
        <LinearProgress />
      ) : (
        <TabBox
          {...DefaultUI.tabsProps(app.smDown)}
          root={{ sx: { marginTop: -2 } }}
          tabProps={{ sx: { paddingTop: 2 } }}
          tabs={[
            {
              children: <StockViewUI data={data} refresh={loadData} />,
              label: labels.basicInfo,
              icon: <ArticleIcon />,
              iconPosition: "start"
            },
            {
              children: (visible) =>
                visible && (
                  <StockLines
                    stockId={id}
                    kind={data.kind}
                    personId={data.personId}
                    locationId={
                      data.kind === StockKind.Order
                        ? data.locationFromId
                        : data.locationToId
                    }
                    isDeletable={data.isDeletable}
                    refresh={loadData}
                  />
                ),
              label: labels.stockLines + ` (${data.totalLines})`,
              icon: <ListAltIcon />,
              iconPosition: "start"
            }
          ]}
        />
      )}
    </CommonPage>
  );
}
