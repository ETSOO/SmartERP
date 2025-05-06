import {
  MUGlobal,
  ResponsivePage,
  SearchField,
  MobileListItemRenderer,
  MUUtils
} from "@etsoo/materialui";
import ShoppingCartIcon from "@mui/icons-material/ShoppingCart";
import HelpCenterIcon from "@mui/icons-material/HelpCenter";
import OpenInBrowserIcon from "@mui/icons-material/OpenInBrowser";
import React from "react";
import { DataTypes, DomUtils } from "@etsoo/shared";
import { GridCellRendererProps, ScrollerListForwardRef } from "@etsoo/react";
import { app } from "../../../app/MyApp";
import { AppQueryData, AppUrl, usePageDataEmpty } from "@etsoo/smarterp-core";
import { AppUtils } from "../components/AppUtils";
import { useLocation, useNavigate } from "react-router-dom";
import { BuyKind } from "../components/BuyApp";
import { DefaultUI, IdentityTypeList } from "@etsoo/smarterp-core/components";
import { BoxProps } from "@mui/material/Box";
import Button from "@mui/material/Button";
import IconButton from "@mui/material/IconButton";
import Typography from "@mui/material/Typography";

const template = {
  keyword: "string",
  identityType: "number"
} as const satisfies DataTypes.BasicTemplate;

function getWebUrl(urls: AppUrl[]): string | undefined {
  return urls[0]?.web;
}

function getHelpUrl(urls: AppUrl[]): string | undefined {
  return urls[0]?.help;
}

export default function AllApps() {
  // Route
  const navigate = useNavigate();
  const location = useLocation();
  const { kind = 1 } = DomUtils.dataAs(location.state, { kind: "number" });
  const kindEnum = kind as BuyKind;

  // Labels
  const labels = app.getLabels(
    "actions",
    "appHelpUrl",
    "appName",
    "appWebUrl",
    "buy",
    "identityType",
    "statusNormal"
  );

  // Refs
  const ref = React.useRef<ScrollerListForwardRef<AppQueryData>>();

  // Load data
  const reloadData = React.useCallback(() => ref.current?.reset(), []);

  const margin = MUGlobal.pagePaddings;

  // Page data hook
  usePageDataEmpty(app);

  return (
    <ResponsivePage<AppQueryData, typeof template>
      {...DefaultUI.pageProps({
        onRefresh: reloadData
      })}
      mRef={ref}
      defaultOrderBy={[{ field: "creation", desc: true }]}
      fieldTemplate={template}
      fields={(data) => [
        <SearchField
          label={labels.appName}
          name="keyword"
          defaultValue={data.keyword}
        />,
        <IdentityTypeList search value={data.identityType} />
      ]}
      loadData={(data, lastItem) =>
        app.core.appApi.query(
          MUUtils.setupPagingKeysets(data, lastItem, "id"),
          {
            defaultValue: [],
            showLoading: false
          }
        )
      }
      columns={[
        {
          field: "identityType",
          header: labels.identityType,
          width: 120,
          valueFormatter: ({ data }) =>
            app.core.getIdentityLabel(data?.identityType),
          sortable: true
        },
        {
          field: "name",
          header: labels.appName,
          sortable: false
        },
        {
          field: "urls",
          header: labels.appWebUrl,
          sortable: false,
          valueFormatter: ({ data }) =>
            data == null ? undefined : getWebUrl(data.urls)
        },
        {
          width: DefaultUI.Widths.icon4,
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

            const webUrl = getWebUrl(data.urls);
            const helpUrl = getHelpUrl(data.urls);

            return (
              <React.Fragment>
                <Button
                  variant="outlined"
                  size="small"
                  startIcon={<ShoppingCartIcon />}
                  onClick={() => AppUtils.buyApp(data, kindEnum, navigate)}
                >
                  {labels.buy}
                </Button>
                {webUrl && (
                  <IconButton
                    onClick={() => window.open(webUrl, "_blank")}
                    title={labels.appWebUrl}
                  >
                    <OpenInBrowserIcon />
                  </IconButton>
                )}
                {helpUrl && (
                  <IconButton
                    onClick={() => window.open(helpUrl, "_blank")}
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
          const webUrl = getWebUrl(data.urls);
          const helpUrl = getHelpUrl(data.urls);
          return [
            data.name,
            app.core.getIdentityLabel(data.identityType),
            [
              webUrl != null && {
                label: labels.appWebUrl,
                icon: <OpenInBrowserIcon />,
                action: () => {
                  window.open(webUrl, "_blank");
                }
              },
              helpUrl != null && {
                label: labels.appHelpUrl,
                icon: <HelpCenterIcon />,
                action: () => {
                  window.open(helpUrl, "_blank");
                }
              }
            ],
            <React.Fragment>
              <Typography variant="body2">{webUrl}</Typography>
              <Button
                variant="outlined"
                fullWidth
                startIcon={<ShoppingCartIcon />}
                onClick={() => AppUtils.buyApp(data, kindEnum, navigate)}
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
