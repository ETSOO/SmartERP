import { CommonPage, TabBox, TabBoxPanel } from "@etsoo/materialui";
import ArticleIcon from "@mui/icons-material/Article";
import ListAltIcon from "@mui/icons-material/ListAlt";
import HistoryIcon from "@mui/icons-material/History";
import React from "react";
import { usePageDataEmpty } from "@etsoo/smarterp-core";
import LinearProgress from "@mui/material/LinearProgress";
import { DefaultUI } from "@etsoo/smarterp-core/components";
import { app } from "../../../app/MyApp";
import { useParamsEx } from "@etsoo/react";
import { POViewData } from "@etsoo/smarterp-crm";
import { POViewUI } from "./POViewUI";
import { POLines } from "./POLines";
import { IdentityTypeFlags } from "@etsoo/appscript";
import { Profiles } from "../../../components/profile/Profiles";

export default function ViewPO() {
  // Route
  const { id = 0 } = useParamsEx({ id: "number" });

  // Labels
  const labels = app.getLabels("basicInfo", "poLines", "profiles", "view");

  // State
  const [data, setData] = React.useState<POViewData>();

  // Load data
  const loadData = React.useCallback(async () => {
    const data = await app.poApi.read(id);
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
              children: <POViewUI data={data} refresh={loadData} />,
              label: labels.basicInfo,
              icon: <ArticleIcon />,
              iconPosition: "start"
            },
            {
              children: (visible) =>
                visible && (
                  <POLines
                    poId={id}
                    poStatus={data.status}
                    currency={data.currency}
                    supplierId={data.supplierId}
                    refresh={loadData}
                  />
                ),
              label: labels.poLines + ` (${data.lines})`,
              icon: <ListAltIcon />,
              iconPosition: "start"
            },
            ...(app.ownsIdentity(IdentityTypeFlags.Supplier, "QueryProfile")
              ? [
                  {
                    children: (visible, index) =>
                      visible && (
                        <Profiles
                          personId={data.supplierId}
                          identityType={IdentityTypeFlags.Supplier}
                          orderId={id}
                          index={index}
                        />
                      ),
                    label: labels.profiles,
                    icon: <HistoryIcon />,
                    iconPosition: "start"
                  } as TabBoxPanel
                ]
              : [])
          ]}
        />
      )}
    </CommonPage>
  );
}
