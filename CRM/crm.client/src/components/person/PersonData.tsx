import {
  ButtonLink,
  HBox,
  IconButtonLink,
  ViewContainer,
  ViewPageFieldType
} from "@etsoo/materialui";
import EditIcon from "@mui/icons-material/Edit";
import { PersonViewData } from "@etsoo/smarterp-crm";
import { app } from "../../app/MyApp";
import { GridDataType } from "@etsoo/react";
import Divider from "@mui/material/Divider";

type PersonDataProps = {
  data: PersonViewData;
  refresh: () => Promise<void>;
};

export function PersonData(props: PersonDataProps) {
  // Destruct
  const { data, refresh } = props;

  // Labels
  const labels = app.getLabels(
    "editAvatar",
    "familyName",
    "givenName",
    "logo",
    "no",
    "yes"
  );

  // Permission
  const editPermission = app.isAdminUser();

  // Layout
  return (
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
              ? [item.latinFamilyName, item.latinGivenName].join(" ")
              : undefined,
          label: "latinName"
        },
        {
          data: (item) =>
            item.userRole == null ? undefined : app.getRoleLabel(item.userRole),
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
        ...(data.privateData && Object.keys(data.privateData).length > 0
          ? ([
              {
                data: () => <Divider />,
                singleRow: true
              },
              {
                data: (item) => app.person.getGender(item.privateData?.gender),
                label: "personGender"
              },
              {
                data: (item) => app.formatDate(item.privateData?.birthday),
                label: "personBirthday"
              },
              {
                data: (item) =>
                  app.person.getMaritalStatus(item.privateData?.maritalStatus),
                label: "personMaritalStatus"
              },
              {
                data: (item) =>
                  app.person.getEducation(item.privateData?.education),
                label: "personEducation"
              },
              {
                data: (item) => app.person.getDegree(item.privateData?.degree),
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
          data: (item) => (item.isLegalPerson ? labels.yes : labels.no),
          label: "isLegalPerson"
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
      refresh={refresh}
    />
  );
}
