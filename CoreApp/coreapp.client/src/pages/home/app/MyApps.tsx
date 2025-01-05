import {
  MUGlobal,
  ResponsivePage,
  SearchField,
  SelectEx,
  DateText,
  MobileListItemRenderer,
  TooltipClick
} from "@etsoo/materialui";
import { BoxProps, Button, Fab, IconButton, Typography } from "@mui/material";
import React from "react";
import AddIcon from "@mui/icons-material/Add";
import EditIcon from "@mui/icons-material/Edit";
import HelpIcon from "@mui/icons-material/Help";
import ShoppingCartIcon from "@mui/icons-material/ShoppingCart";
import KeyIcon from "@mui/icons-material/Key";
import { useNavigate } from "react-router-dom";
import {
  GridCellRendererProps,
  GridDataType,
  ScrollerListForwardRef
} from "@etsoo/react";
import { DataTypes, DateUtils } from "@etsoo/shared";
import { AppPurchasedQueryData } from "@etsoo/smarterp-core";
import { app } from "../../../app/MyApp";

const template = {
  name: "string",
  identityType: "number"
} as const satisfies DataTypes.BasicTemplate;

export default function MyApps() {
  // Route
  const navigate = useNavigate();

  // Labels
  const labels = app.getLabels(
    "appName",
    "creation",
    "actions",
    "identityType",
    "productUnit",
    "price",
    "purchase",
    "expiry",
    "renew",
    "customName",
    "serviceHelp",
    "apiKey",
    "apiKeyTip",
    "copy",
    "completeTip"
  );

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
  const reloadData = async () => {
    ref.current?.reset();
  };

  const setCustomName = (data: AppPurchasedQueryData) => {
    app.notifier.prompt(
      `[${data.name}] ${labels.customName}`,
      async (name) => {
        if (name == null) return;

        /*
        const result = await app.productApi.setCustomName(data.id, name, {
          showLoading: false
        });
        if (result == null) return;

        if (result.ok) {
          reloadData();
          return;
        }

        app.alertResult(result);
        */
      },
      undefined,
      {
        inputProps: {
          type: "input",
          defaultValue: data.name,
          required: false
        }
      }
    );
  };

  const createApiKey = (data: AppPurchasedQueryData) => {};

  const margin = MUGlobal.pagePaddings;

  React.useEffect(() => {
    // Page title
    app.setPageKey("servicesPurchased");
  }, []);

  return (
    <ResponsivePage<AppPurchasedQueryData, typeof template>
      adjustHeight={24}
      mRef={ref}
      defaultOrderBy={[{ field: "creation", desc: true }]}
      pageProps={{
        onRefresh: reloadData,
        paddings: 0,
        fabButtons: (
          <Fab
            title={labels.purchase}
            size="medium"
            color="primary"
            onClick={() =>
              navigate("./../all", {
                state: { kind: 2 }
              })
            }
          >
            <AddIcon />
          </Fab>
        )
      }}
      fieldTemplate={template}
      fields={(data) => [
        <SearchField
          label={labels.appName}
          name="name"
          defaultValue={data.name}
        />,
        <SelectEx
          label={labels.identityType}
          name="identityType"
          search
          options={identities}
          value={data.identityType}
        />
      ]}
      loadData={async (data) => {
        return await app.core.appApi.queryPurchased(data, {
          defaultValue: [],
          showLoading: false
        });
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
          valueFormatter: ({ data }) => data?.name
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
          field: "creation",
          type: GridDataType.Date,
          width: 116,
          header: labels.creation,
          sortable: true,
          sortAsc: false
        },
        {
          width: 192,
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
                <IconButton title={labels.renew} onClick={() => {}}>
                  <ShoppingCartIcon />
                </IconButton>
                <IconButton
                  title={labels.customName}
                  onClick={() => setCustomName(data)}
                >
                  <EditIcon />
                </IconButton>
                {!DateUtils.isExpired(data.expiry) && (
                  <IconButton
                    title={labels.apiKey}
                    onClick={() => createApiKey(data)}
                  >
                    <KeyIcon />
                  </IconButton>
                )}
                {data.helpUrl && (
                  <IconButton
                    title={labels.serviceHelp}
                    href={data.helpUrl}
                    target="_blank"
                  >
                    <HelpIcon />
                  </IconButton>
                )}
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
                action: () => {}
              },
              {
                label: labels.customName,
                icon: <EditIcon />,
                action: () => setCustomName(data)
              },
              !DateUtils.isExpired(data.expiry) && {
                label: labels.apiKey,
                icon: <KeyIcon />,
                action: () => createApiKey(data)
              },
              data.helpUrl != null && {
                label: labels.serviceHelp,
                icon: <HelpIcon />,
                action: data.helpUrl
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
