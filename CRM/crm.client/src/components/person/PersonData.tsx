import {
  ButtonLink,
  HBox,
  IconButtonLink,
  VBox,
  ViewContainer,
  ViewPageFieldType
} from "@etsoo/materialui";
import AddIcon from "@mui/icons-material/Add";
import DeleteIcon from "@mui/icons-material/Delete";
import EditIcon from "@mui/icons-material/Edit";
import ApartmentIcon from "@mui/icons-material/Apartment";
import { PersonViewData } from "@etsoo/smarterp-crm";
import { app } from "../../app/MyApp";
import { GridDataType } from "@etsoo/react";
import Divider from "@mui/material/Divider";
import { CoreUtils } from "@etsoo/smarterp-core";
import React from "react";
import Chip from "@mui/material/Chip";
import Button from "@mui/material/Button";
import { useNavigate } from "react-router-dom";

type PersonDataProps = {
  data: PersonViewData;
  refresh: () => Promise<void>;
};

export function PersonData(props: PersonDataProps) {
  // Destruct
  const { data, refresh } = props;

  // Route
  const navigate = useNavigate();

  // Editable
  const editable = app.ownsIdentity(data.identityType, "Edit");

  // Deletable
  const [deletable, setDeletable] = React.useState(false);

  // Labels
  const labels = app.getLabels(
    "add",
    "addresses",
    "all",
    "delete",
    "deleteConfirm",
    "edit",
    "editAvatar",
    "familyName",
    "givenName",
    "logo",
    "name",
    "nameB",
    "no",
    "yes"
  );

  React.useEffect(() => {
    app.personApi
      .isDeletable(data.id, { showLoading: false, onError: () => {} })
      .then((result) => {
        if (result == null) return;
        setDeletable(result);
      });
  }, [data.id]);

  // Layout
  return (
    <ViewContainer
      data={data}
      leftContainerLines={3}
      leftContainer={(item) => (
        <HBox justifyContent={{ xs: "center", sm: "flex-start" }}>
          {item.avatar && (
            <a href={item.avatar} target="_blank" rel="noopener noreferrer">
              <img
                src={item.avatar}
                alt={labels.logo}
                style={CoreUtils.avatarStyles(item.isLegalPerson)}
              />
            </a>
          )}
          {editable && (
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
        {
          data: "name",
          label: (item) => (item.isLegalPerson ? labels.nameB : labels.name),
          singleRow: (item) => (item.isLegalPerson ? "medium" : "default")
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
          label: "groups",
          singleRow: "large"
        },
        "assignedId",
        {
          data: (item) =>
            item.isLegalPerson
              ? app.formatDate(item.privateData?.birthday)
              : undefined,
          label: "personBirthdayB"
        },
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
          data: (item) => {
            return item.addresses?.length ? (
              <VBox flexWrap="wrap" paddingTop={0.5}>
                {item.addresses.map((a) => (
                  <HBox key={a.id} alignItems="center">
                    <Chip
                      label={a.name + " - " + a.formattedAddress}
                      size="small"
                      title={app.personAddress.getAddressKind(a.kind)}
                    />
                    {editable && (
                      <IconButtonLink
                        href={`./../../address/${item.id}?id=${a.id}`}
                        title={labels.edit}
                        size="small"
                      >
                        <EditIcon />
                      </IconButtonLink>
                    )}
                  </HBox>
                ))}
              </VBox>
            ) : editable ? (
              <React.Fragment />
            ) : undefined;
          },
          label: () => (
            <HBox gap={1} alignItems="center">
              {labels.addresses}:
              {editable && (
                <React.Fragment>
                  <ButtonLink
                    size="small"
                    variant="outlined"
                    startIcon={<AddIcon />}
                    href={`./../../address/${data.id}`}
                  >
                    {labels.add}
                  </ButtonLink>
                  <ButtonLink
                    size="small"
                    variant="outlined"
                    startIcon={<ApartmentIcon />}
                    href={`./../../addresses/${data.id}`}
                  >
                    {labels.all}
                  </ButtonLink>
                </React.Fragment>
              )}
            </HBox>
          ),
          singleRow: true
        },
        {
          data: (item) =>
            (editable || deletable) && (
              <HBox gap={1} justifyContent="center" flexWrap="wrap">
                {deletable && (
                  <Button
                    startIcon={<DeleteIcon />}
                    variant="outlined"
                    onClick={() => {
                      app.notifier.confirm(
                        labels.deleteConfirm.format(data.name),
                        undefined,
                        async (ok) => {
                          if (!ok) return;

                          const result = await app.personApi.delete(item.id);
                          if (result == null) return;

                          navigate("./../../");
                        }
                      );
                    }}
                  >
                    {labels.delete}
                  </Button>
                )}
                {editable && (
                  <ButtonLink
                    startIcon={<EditIcon />}
                    variant="outlined"
                    href={`./../../edit/${item.id}`}
                  >
                    {labels.edit}
                  </ButtonLink>
                )}
              </HBox>
            ),
          singleRow: true
        },
        ...(data.privateData &&
        !data.isLegalPerson &&
        Object.keys(data.privateData).length > 0
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
