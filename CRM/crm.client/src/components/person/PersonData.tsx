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
import { CoreUtils } from "@etsoo/smarterp-core";

type PersonDataProps = {
  data: PersonViewData;
  refresh: () => Promise<void>;
};

export function PersonData(props: PersonDataProps) {
  // Destruct
  const { data, refresh } = props;

  // Labels
  const labels = app.getLabels(
    "edit",
    "editAvatar",
    "familyName",
    "givenName",
    "logo",
    "no",
    "yes"
  );

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
            style={CoreUtils.avatarStyles(item.isLegalPerson)}
          />
          <IconButtonLink
            href={`./../../avatar/${item.id}`}
            state={item.avatar}
            title={labels.editAvatar}
            size="small"
          >
            <EditIcon />
          </IconButtonLink>
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
        {
          data: (item) =>
            item.contactOwners != null && item.contactOwners.length > 0 ? (
              <HBox gap={0.5} flexWrap="wrap">
                {item.contactOwners.map((o) => (
                  <ButtonLink
                    key={o.id}
                    href={`./../${o.id}`}
                    size="small"
                    variant="outlined"
                  >
                    {o.title}
                  </ButtonLink>
                ))}
              </HBox>
            ) : undefined,
          label: "groups"
        },
        "assignedId",
        "jobTitle",
        {
          data: (item) =>
            item.categories?.map((c) => c.names.join(" -> ")).join(", "),
          label: "categories",
          singleRow: "medium",
          horizontal: true
        },
        {
          data: "tags",
          singleRow: "medium",
          horizontal: true
        },
        {
          data: "description",
          singleRow: true,
          horizontal: true
        },
        "regions",
        "currencies",
        "cultures",
        {
          data: (item) =>
            item.editable && (
              <HBox gap={1} justifyContent="center" flexWrap="wrap">
                <ButtonLink
                  startIcon={<EditIcon />}
                  variant="outlined"
                  href={`./../../edit/${item.id}`}
                >
                  {labels.edit}
                </ButtonLink>
              </HBox>
            ),
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
