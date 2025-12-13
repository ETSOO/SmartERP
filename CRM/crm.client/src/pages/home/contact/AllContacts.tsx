import { EntityStatus } from "@etsoo/appscript";
import {
  ResponsivePage,
  SearchField,
  IconButtonLink,
  MobileListItemRenderer,
  ButtonLink
} from "@etsoo/materialui";
import ArticleIcon from "@mui/icons-material/Article";
import CategoryIcon from "@mui/icons-material/Category";
import React from "react";
import {
  GridCellRendererProps,
  GridDataType,
  GridDeletedCellBoxStyle,
  GridMethodRef
} from "@etsoo/react";
import { useNavigate } from "react-router-dom";
import { app } from "../../../app/MyApp";
import { usePageDataEmpty } from "@etsoo/smarterp-core";
import { PersonQueryData } from "@etsoo/smarterp-crm";
import { DataTypes, Utils } from "@etsoo/shared";
import { DefaultUI, IdentityFlagsList } from "@etsoo/smarterp-core/components";
import { BoxProps } from "@mui/material/Box";
import Typography from "@mui/material/Typography";
import { Permissions } from "@etsoo/smarterp-crm";

const template = {
  keyword: "string",
  info: "string",
  identityType: "number"
} as const satisfies DataTypes.BasicTemplate;

export default function AllContacts() {
  // Route
  const navigate = useNavigate();

  // Labels
  const labels = app.getLabels(
    "actions",
    "assignedId",
    "categories",
    "contactInfo",
    "creation",
    "entityStatus",
    "identityType",
    "jobTitle",
    "name",
    "reportTo",
    "role",
    "statusNormal",
    "view"
  );

  // Refs
  const ref = React.useRef<GridMethodRef<PersonQueryData>>(undefined);

  // Load data
  const reloadData = React.useCallback(() => ref.current?.reset(), []);

  const baseIdentity = app.getPersonIdentityType();

  // Page data hook
  usePageDataEmpty(app);

  return (
    <ResponsivePage<PersonQueryData, typeof template>
      {...DefaultUI.pageProps({
        onRefresh: reloadData,
        fabButtons: (
          <React.Fragment>
            {app.owns(Permissions.Org.Manage) && (
              <ButtonLink
                href="./category"
                size="small"
                variant="outlined"
                startIcon={<CategoryIcon />}
              >
                {labels.categories}
              </ButtonLink>
            )}
          </React.Fragment>
        )
      })}
      mRef={ref}
      defaultOrderBy={[{ field: "creation", desc: true }]}
      quickAction={(data) => navigate(`./view/${data.id}`)}
      fieldTemplate={template}
      fields={(data) => [
        <SearchField
          label={labels.name}
          name="keyword"
          defaultValue={data.keyword}
        />,
        <SearchField
          label={labels.contactInfo}
          name="info"
          minChars={2}
          defaultValue={data.info}
        />,
        <IdentityFlagsList
          value={data.identityType}
          baseIdentity={baseIdentity}
          search
        />
      ]}
      loadData={async (data) => {
        return await app.personApi.query(data, {
          defaultValue: [],
          showLoading: false
        });
      }}
      columns={[
        {
          field: "identityType",
          width: 120,
          header: labels.identityType,
          valueFormatter: ({ data }) => app.person.getIdentityType(data)
        },
        {
          field: "name",
          header: labels.name,
          sortable: true,
          cellBoxStyle: GridDeletedCellBoxStyle
        },
        {
          field: "jobTitle",
          width: 120,
          header: labels.jobTitle,
          sortable: true
        },
        {
          field: "assignedId",
          width: 142,
          header: labels.assignedId
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
          width: DefaultUI.Widths.icon1,
          header: labels.actions,
          cellRenderer: ({
            data,
            cellProps
          }: GridCellRendererProps<PersonQueryData, BoxProps>) => {
            if (data == null) return undefined;

            cellProps.sx = {
              paddingTop: "6px!important",
              paddingBottom: "6px!important"
            };

            return (
              <React.Fragment>
                <IconButtonLink title={labels.view} href={`./view/${data.id}`}>
                  <ArticleIcon />
                </IconButtonLink>
              </React.Fragment>
            );
          }
        }
      ]}
      rowHeight={160}
      itemRenderer={(props) =>
        MobileListItemRenderer(props, (data) => [
          `[${app.person.getIdentityType(data)}] ${data.name}`,
          app.formatDate(data.creation, "d"),
          [
            {
              label: labels.view,
              icon: <ArticleIcon />,
              action: `./view/${data.id}`
            }
          ],
          <React.Fragment>
            <Typography variant="body2">
              {Utils.joinItems([data.jobTitle, data.assignedId], ", ")}
            </Typography>
            {data.status >= EntityStatus.Inactivated && (
              <React.Fragment>
                <Typography variant="caption">
                  {labels.entityStatus + ": "}
                </Typography>
                <Typography variant="caption" color="error">
                  {app.getStatusLabel(data?.status)}
                </Typography>
              </React.Fragment>
            )}
          </React.Fragment>
        ])
      }
    />
  );
}
