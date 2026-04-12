import { CommonPage, TabBox, TabBoxPanel } from "@etsoo/materialui";
import HistoryIcon from "@mui/icons-material/History";
import ContactsIcon from "@mui/icons-material/Contacts";
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
import { PersonContacts } from "./PersonContacts";

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
          root={{ sx: { marginTop: -2 } }}
          tabProps={{ sx: { paddingTop: 2 } }}
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
