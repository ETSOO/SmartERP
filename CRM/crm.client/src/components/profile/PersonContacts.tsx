import {
  ResponsivePage,
  SearchField,
  MobileListItemRenderer
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

const template = {
  keyword: "string"
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
};

export function PersonContacts(props: PersonContactsProps) {
  // Destruct
  const { index, identityType, personId } = props;

  // Route
  const navigate = useNavigate();

  // Labels
  const labels = app.getLabels(
    "actions",
    "add",
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

  return (
    <ResponsivePage<ContactQueryData, typeof template>
      {...DefaultUI.pageProps({
        onRefresh: reloadData,
        fabButtons: (
          <React.Fragment>
            {canAdd && (
              <Fab
                title={labels.add}
                size="medium"
                color="primary"
                onClick={() =>
                  navigate(`./../../info/${personId}?index=${index}`)
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
      fieldTemplate={template}
      fields={(data) => [
        <SearchField
          label={labels.description}
          name="keyword"
          minChars={2}
          defaultValue={data.keyword}
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

        return await app.personApi.queryContact(
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
          cellRenderer: ({
            data,
            cellProps
          }: GridCellRendererProps<ContactQueryData, BoxProps>) => {
            if (data == null) return undefined;

            cellProps.sx = {
              paddingTop: "6px!important",
              paddingBottom: "6px!important"
            };

            return (
              <React.Fragment>
                {canAdd && (
                  <IconButton title={labels.edit}>
                    <EditIcon />
                  </IconButton>
                )}
                <IconButton title={labels.view}>
                  <ArticleIcon />
                </IconButton>
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
                icon: <EditIcon />
              },
              {
                label: labels.view,
                icon: <ArticleIcon />
              }
            ],
            <React.Fragment>{data.description}</React.Fragment>
          ];
        })
      }
    />
  );
}
