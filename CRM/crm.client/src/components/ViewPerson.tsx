import {
  ButtonLink,
  CommonPage,
  HBox,
  IconButtonLink,
  TabBox,
  ViewContainer,
  ViewPageFieldType
} from "@etsoo/materialui";
import HistoryIcon from "@mui/icons-material/History";
import ContactsIcon from "@mui/icons-material/Contacts";
import CreditCardIcon from "@mui/icons-material/CreditCard";
import ArticleIcon from "@mui/icons-material/Article";
import EditIcon from "@mui/icons-material/Edit";
import { app } from "../app/MyApp";
import React from "react";
import { usePageData, usePageDataEmpty } from "@etsoo/smarterp-core";
import { PersonViewData } from "@etsoo/smarterp-crm";
import { GridDataType } from "@etsoo/react";
import LinearProgress from "@mui/material/LinearProgress";
import Divider from "@mui/material/Divider";
import { Profiles } from "./Profiles";

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
    "contacts",
    "editAvatar",
    "familyName",
    "givenName",
    "logo",
    "no",
    "profiles",
    "yes",
    "view"
  );

  // Permissions
  const editPermission = app.isAdminUser();

  // State
  const [data, setData] = React.useState<PersonViewData>();

  // Load data
  const loadData = React.useCallback(async () => {
    const data = await app.personApi.read(personId);
    setData(data);
  }, [personId]);

  // Page data hook
  if (personId > 0) usePageData(app, data?.name ?? labels.view, [data]);
  else usePageDataEmpty(app);

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
              children: (
                <ViewContainer
                  data={data}
                  leftContainerLines={3}
                  leftContainer={(item) => (
                    <HBox justifyContent={{ xs: "center", sm: "flex-start" }}>
                      <img
                        src={item.avatar}
                        alt={labels.logo}
                        style={{
                          width: "160px",
                          height: "160px",
                          border: "1px solid #666"
                        }}
                      />
                      {editPermission && (
                        <IconButtonLink
                          href={`./../../avatar/${item.id}`}
                          state={item.avatar}
                          title={labels.editAvatar}
                          size="small"
                        >
                          <EditIcon />
                        </IconButtonLink>
                      )}
                    </HBox>
                  )}
                  fields={[
                    {
                      data: (item) => app.person.getIdentityType(item),
                      label: "identityType"
                    },
                    {
                      data: (item) => app.person.getTitle(item.title),
                      label: "personTitle"
                    },
                    {
                      data: (item) =>
                        item.familyName || item.givenName
                          ? [item.familyName, item.givenName].join(" / ")
                          : undefined,
                      label: labels.familyName + " / " + labels.givenName
                    },
                    "preferredName",
                    {
                      data: (item) =>
                        item.latinFamilyName || item.latinGivenName
                          ? [item.latinFamilyName, item.latinGivenName].join(
                              " "
                            )
                          : undefined,
                      label: "latinName"
                    },
                    {
                      data: (item) =>
                        item.userRole == null
                          ? undefined
                          : app.getRoleLabel(item.userRole),
                      label: "role"
                    },
                    { data: "inviterName", label: "inviter" },
                    ["expiry", GridDataType.DateTime],
                    {
                      data: (item) =>
                        item.reportToName ? (
                          <ButtonLink
                            href={`./../${item.reportTo}`}
                            size="small"
                            variant="outlined"
                          >
                            {item.reportToName}
                          </ButtonLink>
                        ) : undefined,
                      label: "reportTo"
                    },
                    "assignedId",
                    "jobTitle",
                    "regions",
                    "currencies",
                    "cultures",
                    {
                      data: "description",
                      singleRow: true
                    },
                    ...(data.privateData &&
                    Object.keys(data.privateData).length > 0
                      ? ([
                          {
                            data: () => <Divider />,
                            singleRow: true
                          },
                          {
                            data: (item) =>
                              app.person.getGender(item.privateData?.gender),
                            label: "personGender"
                          },
                          {
                            data: (item) =>
                              app.formatDate(item.privateData?.birthday),
                            label: "personBirthday"
                          },
                          {
                            data: (item) =>
                              app.person.getMaritalStatus(
                                item.privateData?.maritalStatus
                              ),
                            label: "personMaritalStatus"
                          },
                          {
                            data: (item) =>
                              app.person.getEducation(
                                item.privateData?.education
                              ),
                            label: "personEducation"
                          },
                          {
                            data: (item) =>
                              app.person.getDegree(item.privateData?.degree),
                            label: "personDegree"
                          },
                          {
                            data: (item) => item.privateData?.ethnicity,
                            label: "personEthnicity"
                          },
                          {
                            data: (item) => item.privateData?.politicalStatus,
                            label: "politicalStatus"
                          },
                          {
                            data: (item) => item.privateData?.height,
                            label: "personHeight"
                          },
                          {
                            data: (item) => item.privateData?.weight,
                            label: "personWeight"
                          }
                        ] as ViewPageFieldType<PersonViewData>[])
                      : []),
                    {
                      data: () => <Divider />,
                      singleRow: true
                    },
                    {
                      data: (item) =>
                        item.isLegalPerson ? labels.yes : labels.no,
                      label: "isLegalPerson"
                    },
                    {
                      data: (item) => (item.isOrg ? labels.yes : labels.no),
                      label: "isOrg"
                    },
                    "queryKeyword",
                    {
                      data: (item) => app.getStatusLabel(item.status),
                      label: "status"
                    },
                    ["creation", GridDataType.DateTime],
                    {
                      data: "uid",
                      singleRow: "medium"
                    }
                  ]}
                  refresh={loadData}
                />
              ),
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
