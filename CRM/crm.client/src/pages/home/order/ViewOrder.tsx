import { CommonPage, TabBox, TabBoxPanel } from "@etsoo/materialui";
import HistoryIcon from "@mui/icons-material/History";
import ContactsIcon from "@mui/icons-material/Contacts";
import ArticleIcon from "@mui/icons-material/Article";
import InfoIcon from "@mui/icons-material/Info";
import React from "react";
import { usePageDataEmpty } from "@etsoo/smarterp-core";
import LinearProgress from "@mui/material/LinearProgress";
import { DefaultUI } from "@etsoo/smarterp-core/components";
import { app } from "../../../app/MyApp";
import { useParamsEx } from "@etsoo/react";

export default function ViewOrder() {
  // Route
  const { id = 0 } = useParamsEx({ id: "number" });

  // Labels
  const labels = app.getLabels(
    "basicInfo",
    "contactInfo",
    "contacts",
    "profiles",
    "view"
  );

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
          root={{ marginTop: -2 }}
          tabProps={{ paddingTop: 2 }}
          tabs={[
            {
              children: <PersonData data={data} refresh={loadData} />,
              label: labels.basicInfo,
              icon: <ArticleIcon />,
              iconPosition: "start"
            },
            ...(app.ownsIdentity(data.identityType, "QueryProfile")
              ? [
                  {
                    children: (visible, index) =>
                      visible && (
                        <Profiles
                          personId={personId}
                          identityType={data.identityType}
                          index={index}
                        />
                      ),
                    label: labels.profiles,
                    icon: <HistoryIcon />,
                    iconPosition: "start"
                  } as TabBoxPanel
                ]
              : []),
            {
              children: (visible, index) =>
                visible && (
                  <ContactInfos
                    personId={personId}
                    editable={app.ownsIdentity(data.identityType, "Edit")}
                    index={index}
                  />
                ),
              label: labels.contactInfo,
              icon: <InfoIcon />,
              iconPosition: "start"
            },
            ...(app.ownsIdentity(data.identityType, "QueryContact")
              ? [
                  {
                    children: (visible, index) =>
                      visible && (
                        <PersonContacts
                          personId={personId}
                          identityType={data.identityType}
                          isLegalPerson={data.isLegalPerson}
                          index={index}
                        />
                      ),
                    label: labels.contacts,
                    icon: <ContactsIcon />,
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
