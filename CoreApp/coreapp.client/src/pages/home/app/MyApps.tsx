import {
  MUGlobal,
  ResponsivePage,
  SearchField,
  SelectEx,
  DateText,
  MobileListItemRenderer,
  MUUtils,
  IconButtonLink,
  Switch
} from "@etsoo/materialui";
import React from "react";
import AddIcon from "@mui/icons-material/Add";
import EditIcon from "@mui/icons-material/Edit";
import ArticleIcon from "@mui/icons-material/Article";
import ShoppingCartIcon from "@mui/icons-material/ShoppingCart";
import { useNavigate } from "react-router-dom";
import {
  GridCellRendererProps,
  GridDataType,
  ScrollerListForwardRef
} from "@etsoo/react";
import { DataTypes } from "@etsoo/shared";
import { AppPurchasedQueryData, usePageDataEmpty } from "@etsoo/smarterp-core";
import { app } from "../../../app/MyApp";
import { DefaultUI } from "@etsoo/smarterp-core/components";
import { AppUtils } from "../components/AppUtils";
import Fab from "@mui/material/Fab";
import { BoxProps } from "@mui/material/Box";
import IconButton from "@mui/material/IconButton";
import Typography from "@mui/material/Typography";

const template = {
  keyword: "string",
  identityType: "number",
  enabled: "boolean",
  expiryDays: "number"
} as const satisfies DataTypes.BasicTemplate;

export default function MyApps() {
  // Route
  const navigate = useNavigate();

  // Labels
  const labels = app.getLabels(
    "actions",
    "appName",
    "creation",
    "days",
    "edit",
    "expiry",
    "identityType",
    "purchase",
    "renew",
    "statusNormal",
    "view"
  );

  // Permissions
  const editPermission = app.isAdminUser();

  // Refs
  const ref = React.useRef<ScrollerListForwardRef<AppPurchasedQueryData>>();

  // Identities
  const identities = app.core.getIdentities();
  const identityLabel = React.useCallback(
    (data?: AppPurchasedQueryData) => {
      if (data == null) return "";
      return identities.find((item) => item.id === data.identityType)?.label;
    },
    [identities]
  );

  // Load data
  const reloadData = React.useCallback(() => ref.current?.reset(), []);

  const margin = MUGlobal.pagePaddings;

  // Page data hook
  usePageDataEmpty(app);

  return (
    <ResponsivePage<AppPurchasedQueryData, typeof template>
      {...DefaultUI.pageProps({
        onRefresh: reloadData,
        fabButtons: (
          <Fab
            title={labels.purchase}
            size="medium"
            color="primary"
            onClick={() =>
              navigate("./../app", {
                state: { kind: 2 }
              })
            }
          >
            <AddIcon />
          </Fab>
        )
      })}
      mRef={ref}
      defaultOrderBy={[{ field: "creation", desc: true }]}
      quickAction={(data) => navigate(`./view/${data.id}`)}
      fieldTemplate={template}
      fields={(data) => [
        <SearchField
          label={labels.appName}
          name="keyword"
          defaultValue={data.keyword}
        />,
        <SelectEx
          label={labels.identityType}
          name="identityType"
          search
          options={identities}
          value={data.identityType}
        />,
        <Switch
          label={labels.statusNormal}
          name="enabled"
          checked={data.enabled ?? false}
        />
      ]}
      loadData={async (data, lastItem) => {
        return await app.core.appApi.queryPurchased(
          MUUtils.setupPagingKeysets(data, lastItem, "id"),
          {
            defaultValue: [],
            showLoading: false
          }
        );
      }}
      columns={[
        {
          field: "identityType",
          header: labels.identityType,
          width: 120,
          valueFormatter: ({ data }) => identityLabel(data),
          sortable: true
        },
        {
          field: "name",
          header: labels.appName,
          sortable: false,
          valueFormatter: ({ data }) =>
            data ? `${app.core.getAppName(data)} / ${data.name}` : ""
        },
        {
          field: "expiry",
          type: GridDataType.Date,
          width: 116,
          header: labels.expiry,
          sortable: true,
          sortAsc: false,
          renderProps: { nearDays: 30 }
        },
        {
          field: "expiryDays",
          type: GridDataType.Int,
          header: labels.days,
          width: 72,
          sortable: false
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
          width: DefaultUI.Widths.icon3,
          header: labels.actions,
          cellRenderer: ({
            data,
            cellProps
          }: GridCellRendererProps<AppPurchasedQueryData, BoxProps>) => {
            if (data == null) return undefined;

            cellProps.sx = {
              paddingTop: "6px!important",
              paddingBottom: "6px!important"
            };

            return (
              <React.Fragment>
                <IconButton
                  title={labels.renew}
                  onClick={() => AppUtils.renewApp(data, reloadData)}
                >
                  <ShoppingCartIcon />
                </IconButton>
                {editPermission && (
                  <IconButtonLink
                    title={labels.edit}
                    href={`./edit/${data.id}`}
                  >
                    <EditIcon />
                  </IconButtonLink>
                )}
                <IconButtonLink title={labels.view} href={`./view/${data.id}`}>
                  <ArticleIcon />
                </IconButtonLink>
              </React.Fragment>
            );
          }
        }
      ]}
      itemSize={[100, margin]}
      innerItemRenderer={(props) =>
        MobileListItemRenderer(props, (data) => {
          return [
            data.name,
            identityLabel(data) + ", " + app.formatDate(data.creation, "d"),
            [
              {
                label: labels.renew,
                icon: <ShoppingCartIcon />,
                action: () => AppUtils.renewApp(data, reloadData)
              },
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
              <Typography variant="body2" component="span">
                {labels.expiry}
                {": "}
              </Typography>
              <DateText
                value={data.expiry}
                nearDays={30}
                options="d"
                {...app.getDateFormatProps()}
              />
            </React.Fragment>
          ];
        })
      }
    />
  );
}
