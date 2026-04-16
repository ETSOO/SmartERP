import {
  ResponsivePage,
  SearchField,
  MobileListItemRenderer,
  IconButtonLink
} from "@etsoo/materialui";
import EditIcon from "@mui/icons-material/Edit";
import ArticleIcon from "@mui/icons-material/Article";
import AddIcon from "@mui/icons-material/Add";
import React from "react";
import {
  GridCellRendererProps,
  GridDataType,
  GridDeletedCellBoxStyle,
  ScrollerListForwardRef
} from "@etsoo/react";
import { useNavigate } from "react-router-dom";
import { DataTypes } from "@etsoo/shared";
import { DefaultUI } from "@etsoo/smarterp-core/components";
import { BoxProps } from "@mui/material/Box";
import Fab from "@mui/material/Fab";
import { ContactQueryData } from "@etsoo/smarterp-crm";
import { app } from "../../app/MyApp";
import IconButton from "@mui/material/IconButton";
import { IdentityTypeFlags } from "@etsoo/appscript";
import { ContactRelationList } from "@etsoo/smarterp-crm/components";
import { useEditContactRelation } from "./useEditContactRelation";
import Button from "@mui/material/Button";

const template = {
  relationType: "number",
  keyword: "string",
  info: "string"
} as const satisfies DataTypes.BasicTemplate;

export type PersonContactsProps = {
  /**
   * Tab index
   */
  index: number;

  /**
   * Person ID
   */
  personId: number;

  /**
   * Identity type
   */
  identityType: IdentityTypeFlags;

  /**
   * Is legal person
   */
  isLegalPerson?: boolean | null;
};

export function PersonContacts(props: PersonContactsProps) {
  // Destruct
  const { index, identityType, isLegalPerson, personId } = props;

  // Route
  const navigate = useNavigate();

  // Labels
  const labels = app.getLabels(
    "actions",
    "add",
    "addExistingContact",
    "contactInfo",
    "creation",
    "description",
    "edit",
    "keywords",
    "personName",
    "relation",
    "view"
  );

  const canAdd = app.ownsIdentity(identityType, "AddContact");

  // Refs
  const ref = React.useRef<ScrollerListForwardRef<ContactQueryData>>(undefined);

  // Load data
  const reloadData = React.useCallback(() => ref.current?.reset(), []);

  // Edit relation
  const editRelation = useEditContactRelation(
    personId,
    isLegalPerson,
    reloadData
  );

  return (
    <ResponsivePage<ContactQueryData, typeof template>
      {...DefaultUI.pageProps({
        onRefresh: reloadData,
        fabButtons: (
          <React.Fragment>
            <Button variant="outlined" onClick={() => editRelation()}>
              {labels.addExistingContact}
            </Button>
            {canAdd && (
              <Fab
                title={labels.add}
                size="medium"
                color="primary"
                onClick={() =>
                  navigate(
                    `./../../relation/add/${personId}?isLegalPerson=${isLegalPerson}&index=${index}`
                  )
                }
              >
                <AddIcon />
              </Fab>
            )}
          </React.Fragment>
        )
      })}
      mRef={ref}
      defaultOrderBy={[{ field: "creation", desc: true }]}
      quickAction={(data) => navigate(`./../../view/${data.id}?index=0`)}
      fieldTemplate={template}
      fields={(data) => [
        <ContactRelationList search isLegalPerson={isLegalPerson} />,
        <SearchField
          label={labels.keywords}
          name="keyword"
          minChars={2}
          defaultValue={data.keyword}
        />,
        <SearchField
          label={labels.contactInfo}
          name="info"
          minChars={2}
          defaultValue={data.info}
        />
      ]}
      loadData={async (data) => {
        if (data.queryPaging.orderBy) {
          for (const f of data.queryPaging.orderBy) {
            if (f.field === "name") {
              f.field = "Contact.Name";
            }
          }
        }

        return await app.personContactApi.query(
          { personId, ...data },
          {
            defaultValue: [],
            showLoading: false
          }
        );
      }}
      columns={[
        {
          field: "relationType",
          width: 120,
          header: labels.relation,
          valueFormatter: ({ data }) =>
            app.person.getRelationType(data?.relationType)
        },
        {
          field: "name",
          header: labels.personName,
          sortable: true,
          cellBoxStyle: GridDeletedCellBoxStyle
        },
        {
          field: "description",
          header: labels.description
        },
        {
          field: "creation",
          type: GridDataType.Date,
          width: 116,
          header: labels.creation,
          sortable: true,
          sortAsc: false
        },
        {
          width: DefaultUI.Widths.icon2,
          header: labels.actions,
          cellBoxStyle: {
            paddingTop: "6px!important",
            paddingBottom: "6px!important"
          },
          cellRenderer: ({
            data
          }: GridCellRendererProps<ContactQueryData, BoxProps>) => {
            if (data == null) return undefined;

            return (
              <React.Fragment>
                {canAdd && (
                  <IconButton
                    title={labels.edit}
                    onClick={() => editRelation(data.id)}
                  >
                    <EditIcon />
                  </IconButton>
                )}
                <IconButtonLink
                  title={labels.view}
                  href={`./../../view/${data.contactId}?index=0`}
                >
                  <ArticleIcon />
                </IconButtonLink>
              </React.Fragment>
            );
          }
        }
      ]}
      itemRenderer={(props) =>
        MobileListItemRenderer(props, (data) => {
          return [
            `[${app.person.getRelationType(data.relationType)}] ${data.name}`,
            app.formatDate(data.creation, "d"),
            [
              canAdd && {
                label: labels.edit,
                icon: <EditIcon />,
                action: () => editRelation(data.id)
              },
              {
                label: labels.view,
                icon: <ArticleIcon />,
                action: `./../../view/${data.id}?index=0`
              }
            ],
            <React.Fragment>{data.description}</React.Fragment>
          ];
        })
      }
    />
  );
}
