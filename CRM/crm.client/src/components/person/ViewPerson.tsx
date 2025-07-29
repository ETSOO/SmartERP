import { CommonPage, TabBox } from "@etsoo/materialui";
import HistoryIcon from "@mui/icons-material/History";
import ContactsIcon from "@mui/icons-material/Contacts";
import CreditCardIcon from "@mui/icons-material/CreditCard";
import ArticleIcon from "@mui/icons-material/Article";
import InfoIcon from "@mui/icons-material/Info";
import { app } from "../../app/MyApp";
import React from "react";
import { usePageDataEmpty } from "@etsoo/smarterp-core";
import { PersonViewData } from "@etsoo/smarterp-crm";
import LinearProgress from "@mui/material/LinearProgress";
import { Profiles } from "../profile/Profiles";
import { DefaultUI } from "@etsoo/smarterp-core/components";
import { PersonData } from "./PersonData";
import { ContactInfos } from "../profile/ContactInfos";

/**
 * View person component properties
 */
export type ViewPersonProps = {
  /**
   * Person ID
   */
  personId: number;
};

/**
 * View person component
 * @param props Props
 * @returns Component
 */
export function ViewPerson(props: ViewPersonProps) {
  // Destruct
  const { personId } = props;

  // Labels
  const labels = app.getLabels(
    "assets",
    "basicInfo",
    "contactInfo",
    "contacts",
    "profiles",
    "view"
  );

  // State
  const [data, setData] = React.useState<PersonViewData>();

  // Load data
  const loadData = React.useCallback(async () => {
    const data = await app.personApi.read(personId);
    setData(data);
  }, [personId]);

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
            {
              children: (visible) =>
                visible && <Profiles personId={personId} />,
              label: labels.profiles,
              icon: <HistoryIcon />,
              iconPosition: "start"
            },
            {
              children: (visible) =>
                visible && <ContactInfos personId={personId} />,
              label: labels.contactInfo,
              icon: <InfoIcon />,
              iconPosition: "start"
            },
            {
              children: <div>Item Three</div>,
              label: labels.contacts,
              icon: <ContactsIcon />,
              iconPosition: "start"
            },
            {
              children: <div>Item Four</div>,
              label: labels.assets,
              icon: <CreditCardIcon />,
              iconPosition: "start"
            }
          ]}
        />
      )}
    </CommonPage>
  );
}
