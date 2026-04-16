import { CommonPage, TabBox } from "@etsoo/materialui";
import ArticleIcon from "@mui/icons-material/Article";
import ListAltIcon from "@mui/icons-material/ListAlt";
import React from "react";
import { usePageDataEmpty } from "@etsoo/smarterp-core";
import LinearProgress from "@mui/material/LinearProgress";
import { DefaultUI } from "@etsoo/smarterp-core/components";
import { app } from "../../../app/MyApp";
import { useParamsEx } from "@etsoo/react";
import { OrderViewData } from "@etsoo/smarterp-crm";
import { OrderViewUI } from "./OrderViewUI";
import { OrderLines } from "./OrderLines";

export default function ViewOrder() {
  // Route
  const { id = 0 } = useParamsEx({ id: "number" });

  // Labels
  const labels = app.getLabels("basicInfo", "orderLines", "view");

  // State
  const [data, setData] = React.useState<OrderViewData>();

  // Load data
  const loadData = React.useCallback(async () => {
    const data = await app.orderApi.read(id);
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
              children: <OrderViewUI data={data} refresh={loadData} />,
              label: labels.basicInfo,
              icon: <ArticleIcon />,
              iconPosition: "start"
            },
            {
              children: (visible) =>
                visible && <OrderLines orderId={id} refresh={loadData} />,
              label: labels.orderLines + ` (${data.lines})`,
              icon: <ListAltIcon />,
              iconPosition: "start"
            }
          ]}
        />
      )}
    </CommonPage>
  );
}
