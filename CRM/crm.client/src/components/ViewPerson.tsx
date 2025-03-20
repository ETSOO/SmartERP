import { CommonPage, TabBox } from "@etsoo/materialui";
import HistoryIcon from "@mui/icons-material/History";
import ContactsIcon from "@mui/icons-material/Contacts";
import CreditCardIcon from "@mui/icons-material/CreditCard";
import ArticleIcon from "@mui/icons-material/Article";
import { app } from "../app/MyApp";
import React from "react";
import { usePageData } from "@etsoo/smarterp-core";
import { PersonViewData } from "@etsoo/smarterp-crm";
import { LinearProgress } from "@mui/material";

/**
 * View person component properties
 */
export type ViewPersonProps = {
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
  if (personId > 0) usePageData(app, labels.view, [loadData]);

  // Layout
  return (
    <CommonPage paddings={0} onRefresh={loadData}>
      {data == null ? (
        <LinearProgress />
      ) : (
        <TabBox
          root={{ marginTop: -2 }}
          tabProps={{ paddingTop: 2 }}
          tabs={[
            {
              children: <div>Item One</div>,
              label: labels.basicInfo,
              icon: <ArticleIcon />,
              iconPosition: "start"
            },
            {
              children: <div>Item Two</div>,
              label: labels.profiles,
              icon: <HistoryIcon />,
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
