import { CommonPage, TabBox } from "@etsoo/materialui";
import { AccountTransactions } from "./AccountTransactions";
import ArticleIcon from "@mui/icons-material/Article";
import ListAltIcon from "@mui/icons-material/ListAlt";
import React from "react";
import { usePageDataEmpty } from "@etsoo/smarterp-core";
import LinearProgress from "@mui/material/LinearProgress";
import { DefaultUI } from "@etsoo/smarterp-core/components";
import { app } from "../../../app/MyApp";
import { useParamsEx } from "@etsoo/react";
import { FinanceAccountViewData } from "@etsoo/smarterp-crm";
import { ViewAccountUI } from "./ViewAccountUI";

export default function ViewAccount() {
  // Route
  const { id = 0 } = useParamsEx({ id: "number" });

  // Labels
  const labels = app.getLabels("basicInfo", "transactions", "view");

  // State
  const [data, setData] = React.useState<FinanceAccountViewData>();

  // Load data
  const loadData = React.useCallback(async () => {
    const data = await app.financeAccountApi.read(id);
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
              children: <ViewAccountUI data={data} refresh={loadData} />,
              label: labels.basicInfo,
              icon: <ArticleIcon />,
              iconPosition: "start"
            },
            {
              children: (visible) =>
                visible && (
                  <AccountTransactions accountId={id} refresh={loadData} />
                ),
              label: labels.transactions,
              icon: <ListAltIcon />,
              iconPosition: "start"
            }
          ]}
        />
      )}
    </CommonPage>
  );
}
