import {
  MUGlobal,
  ResponsivePage,
  SearchField,
  SelectEx,
  MobileListItemRenderer
} from "@etsoo/materialui";
import { BoxProps, Button, IconButton, Typography } from "@mui/material";
import ShoppingCartIcon from "@mui/icons-material/ShoppingCart";
import HelpCenterIcon from "@mui/icons-material/HelpCenter";
import OpenInBrowserIcon from "@mui/icons-material/OpenInBrowser";
import React from "react";
import { DataTypes } from "@etsoo/shared";
import { GridCellRendererProps, ScrollerListForwardRef } from "@etsoo/react";
import { app } from "../../../app/MyApp";
import { AppQueryData } from "@etsoo/smarterp-core";
import { AppUtils } from "./components/AppUtils";
import { useNavigate } from "react-router-dom";
import { BuyKind } from "./components/BuyApp";

const template = {
  name: "string",
  identityType: "number"
} as const satisfies DataTypes.BasicTemplate;

export default function AllApps() {
  // Route
  const navigate = useNavigate();

  // Labels
  const labels = app.getLabels(
    "actions",
    "appHelpUrl",
    "appName",
    "appWebUrl",
    "buy",
    "identityType"
  );

  // Refs
  const ref = React.useRef<ScrollerListForwardRef<AppQueryData>>();

  // Identities
  const identities = app.core.getIdentities();
  const identityLabel = React.useCallback(
    (data?: AppQueryData) => {
      if (data == null) return "";
      return identities.find((item) => item.id === data.identityType)?.label;
    },
    [identities]
  );

  // Load data
  const reloadData = () => ref.current?.reset();

  const margin = MUGlobal.pagePaddings;

  React.useEffect(() => {
    // Page title
    app.setPageKey("servicesAll");
  }, []);

  return (
    <ResponsivePage<AppQueryData, typeof template>
      adjustHeight={24}
      mRef={ref}
      defaultOrderBy={[{ field: "creation", desc: true }]}
      pageProps={{ onRefresh: reloadData, paddings: 0 }}
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
      loadData={(data) =>
        app.core.appApi.query(data, {
          defaultValue: [],
          showLoading: false
        })
      }
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
          sortable: false
        },
        {
          field: "webUrl",
          header: labels.appWebUrl,
          sortable: false
        },
        {
          width: 190,
          header: labels.actions,
          align: "center",
          cellRenderer: ({
            data,
            cellProps
          }: GridCellRendererProps<AppQueryData, BoxProps>) => {
            if (data == null) return undefined;

            cellProps.sx = {
              paddingTop: "10px!important",
              paddingBottom: "9px!important"
            };

            return (
              <React.Fragment>
                <Button
                  variant="outlined"
                  size="small"
                  startIcon={<ShoppingCartIcon />}
                  onClick={() =>
                    AppUtils.buyApp(data, BuyKind.ExistingOrg, navigate)
                  }
                >
                  {labels.buy}
                </Button>
                <IconButton
                  onClick={() => window.open(data.webUrl, "_blank")}
                  title={labels.appWebUrl}
                >
                  <OpenInBrowserIcon />
                </IconButton>
                {data.helpUrl && (
                  <IconButton
                    onClick={() => window.open(data.helpUrl, "_blank")}
                    title={labels.appHelpUrl}
                  >
                    <HelpCenterIcon />
                  </IconButton>
                )}
              </React.Fragment>
            );
          }
        }
      ]}
      itemSize={[134, margin]}
      innerItemRenderer={(props) =>
        MobileListItemRenderer(props, (data) => {
          return [
            data.name,
            identityLabel(data),
            [
              {
                label: labels.appWebUrl,
                icon: <OpenInBrowserIcon />,
                action: () => {
                  window.open(data.webUrl, "_blank");
                }
              },
              data.helpUrl != null && {
                label: labels.appHelpUrl,
                icon: <HelpCenterIcon />,
                action: () => {
                  window.open(data.helpUrl, "_blank");
                }
              }
            ],
            <React.Fragment>
              <Typography variant="body2">{data.webUrl}</Typography>
              <Button
                variant="outlined"
                fullWidth
                startIcon={<ShoppingCartIcon />}
                onClick={() =>
                  AppUtils.buyApp(data, BuyKind.ExistingOrg, navigate)
                }
              >
                {labels.buy}
              </Button>
            </React.Fragment>
          ];
        })
      }
    />
  );
}
