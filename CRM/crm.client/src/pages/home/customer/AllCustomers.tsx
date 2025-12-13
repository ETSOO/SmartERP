import { EntityStatus } from "@etsoo/appscript";
import {
  ResponsivePage,
  SearchField,
  IconButtonLink,
  MobileListItemRenderer
} from "@etsoo/materialui";
import EditIcon from "@mui/icons-material/Edit";
import ArticleIcon from "@mui/icons-material/Article";
import React from "react";
import {
  GridCellRendererProps,
  GridDataType,
  GridDeletedCellBoxStyle,
  ScrollerListForwardRef
} from "@etsoo/react";
import { useNavigate } from "react-router-dom";
import { app } from "../../../app/MyApp";
import { usePageDataEmpty } from "@etsoo/smarterp-core";
import { PersonQueryData } from "@etsoo/smarterp-crm";
import { DataTypes } from "@etsoo/shared";
import { DefaultUI, IdentityFlagsList } from "@etsoo/smarterp-core/components";
import { BoxProps } from "@mui/material/Box";
import Typography from "@mui/material/Typography";

const template = {
  name: "string",
  identityType: "number"
} as const satisfies DataTypes.BasicTemplate;

export default function AllDepts() {
  // Route
  const navigate = useNavigate();

  // Labels
  const labels = app.getLabels(
    "actions",
    "assignedId",
    "confirmAction",
    "creation",
    "edit",
    "entityStatus",
    "identityType",
    "jobTitle",
    "personName",
    "reportTo",
    "role",
    "statusNormal",
    "view"
  );

  // Refs
  const ref = React.useRef<ScrollerListForwardRef<PersonQueryData>>(undefined);

  // Load data
  const reloadData = React.useCallback(() => ref.current?.reset(), []);

  const baseIdentity = app.getPersonIdentityType();

  // Page data hook
  usePageDataEmpty(app);

  return (
    <ResponsivePage<PersonQueryData, typeof template>
      {...DefaultUI.pageProps({
        onRefresh: reloadData,
        fabButtons: <React.Fragment></React.Fragment>
      })}
      mRef={ref}
      defaultOrderBy={[{ field: "creation", desc: true }]}
      quickAction={(data) => navigate(`./view/${data.id}`)}
      fieldTemplate={template}
      fields={(data) => [
        <SearchField
          label={labels.personName}
          name="keywords"
          defaultValue={data.name}
        />,
        <IdentityFlagsList
          value={data.identityType}
          baseIdentity={baseIdentity}
          search
        />
      ]}
      loadData={(data) =>
        app.personApi.query(data, {
          defaultValue: [],
          showLoading: false
        })
      }
      columns={[
        {
          field: "identityType",
          width: 120,
          header: labels.identityType,
          valueFormatter: ({ data }) => app.person.getIdentityType(data)
        },
        {
          field: "name",
          header: labels.personName,
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
          width: DefaultUI.Widths.icon2,
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
                <IconButtonLink title={labels.edit} href={`./edit/${data.id}`}>
                  <EditIcon />
                </IconButtonLink>
                <IconButtonLink title={labels.view} href={`./view/${data.id}`}>
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
            `[${app.person.getIdentityType(data)}] ${data.name}`,
            app.formatDate(data.creation, "d"),
            [
              {
                label: labels.edit,
                icon: <EditIcon />,
                action: `./edit/${data.id}`
              },
              {
                label: labels.view,
                icon: <ArticleIcon />,
                action: `./view/${data.id}`
              }
            ],
            <React.Fragment>
              <Typography variant="caption">{data.jobTitle}</Typography>
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
          ];
        })
      }
    />
  );
}
