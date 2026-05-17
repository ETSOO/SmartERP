import { IdentityTypeFlags } from "@etsoo/appscript";
import {
  ResponsivePage,
  SearchField,
  IconButtonLink,
  MobileListItemRenderer,
  ButtonList,
  ButtonLink,
  LinkEx,
  SelectBool,
  NumberInputField
} from "@etsoo/materialui";
import ArticleIcon from "@mui/icons-material/Article";
import LocalShippingIcon from "@mui/icons-material/LocalShipping";
import InventoryIcon from "@mui/icons-material/Inventory";
import React from "react";
import { GridCellRendererProps, ScrollerListForwardRef } from "@etsoo/react";
import { useNavigate } from "react-router-dom";
import { app } from "../../../app/MyApp";
import { usePageDataEmpty } from "@etsoo/smarterp-core";
import { Permissions, ProductScope, StockQueryData } from "@etsoo/smarterp-crm";
import { DataTypes, DateUtils } from "@etsoo/shared";
import { DefaultUI } from "@etsoo/smarterp-core/components";
import { BoxProps } from "@mui/material/Box";
import Typography from "@mui/material/Typography";
import {
  PersonList,
  ProductList,
  StockKindList
} from "@etsoo/smarterp-crm/components";

const template = {
  keyword: "string",
  kind: "number",
  personId: "number",
  trackingNumber: "string",
  inTransit: "boolean",
  productId: "number",
  totalQtyStart: "number",
  totalQtyEnd: "number",
  creationStart: "date",
  creationEnd: "date"
} as const satisfies DataTypes.BasicTemplate;

export default function AllInventory() {
  // Route
  const navigate = useNavigate();

  // Labels
  const labels = app.getLabels(
    "actions",
    "confirmAction",
    "creation",
    "description",
    "edit",
    "inTransit",
    "items",
    "keywords",
    "realtimeInventory",
    "receiptTime",
    "receivingWarehouse",
    "relatedTarget",
    "shippingWarehouse",
    "statusNormal",
    "stockKindAssembly",
    "stockKindLoss",
    "stockKindInit",
    "stockKindOrder",
    "stockKindPO",
    "stockKindStockTaking",
    "stockKindStockTransfer",
    "title",
    "totalQty",
    "trackingNumber",
    "view"
  );

  // Refs
  const ref = React.useRef<ScrollerListForwardRef<StockQueryData>>(undefined);

  // Load data
  const reloadData = React.useCallback(() => ref.current?.reset(), []);

  const hasManage = app.owns(Permissions.Inventory.Manage);

  // Page data hook
  usePageDataEmpty(app);

  return (
    <ResponsivePage<StockQueryData, typeof template>
      {...DefaultUI.pageProps({
        onRefresh: reloadData,
        fabButtons: hasManage ? (
          <React.Fragment>
            <ButtonLink
              href={`./realtime`}
              size="small"
              variant="outlined"
              startIcon={<InventoryIcon />}
            >
              {labels.realtimeInventory}
            </ButtonLink>
            <ButtonLink
              href={`./order`}
              size="small"
              variant="outlined"
              startIcon={<LocalShippingIcon />}
            >
              {labels.stockKindOrder}
            </ButtonLink>
            <ButtonList
              variant="contained"
              items={[
                {
                  label: labels.stockKindPO,
                  action: () => navigate(`./po`)
                },
                {
                  label: labels.stockKindAssembly,
                  action: () => navigate(`./assembly`)
                },
                { label: "-" },
                {
                  label: labels.stockKindStockTransfer,
                  action: () => navigate(`./transfer`)
                },
                {
                  label: labels.stockKindLoss,
                  action: () => navigate(`./loss`)
                },
                {
                  label: labels.stockKindStockTaking,
                  action: () => navigate(`./take`)
                },
                { label: "-" },
                {
                  label: labels.stockKindInit,
                  action: () => navigate(`./init`)
                }
              ]}
            />
          </React.Fragment>
        ) : undefined
      })}
      mRef={ref}
      defaultOrderBy={[{ field: "creation", desc: true }]}
      quickAction={(data) => navigate(`./view/${data.id}`)}
      fieldTemplate={template}
      fields={(data) => [
        <SearchField
          label={labels.keywords}
          name="keyword"
          defaultValue={data.keyword}
          minChars={3}
        />,
        <StockKindList search value={data.kind} />,
        <PersonList
          search
          rq={{
            identityType:
              IdentityTypeFlags.Customer |
              IdentityTypeFlags.Supplier |
              IdentityTypeFlags.Org
          }}
          label={labels.relatedTarget}
          idValue={data.personId}
        />,
        <SearchField
          label={labels.trackingNumber}
          name="trackingNumber"
          defaultValue={data.trackingNumber}
          minChars={3}
        />,
        <SelectBool
          search
          name="inTransit"
          label={labels.inTransit}
          value={data.inTransit}
        />,
        <ProductList
          search
          idValue={data.productId}
          rq={{ scope: ProductScope.Inventory }}
        />,
        <NumberInputField
          search
          name="totalQtyStart"
          label={labels.totalQty}
          defaultValue={data.totalQtyStart}
        />,
        <NumberInputField
          search
          name="totalQtyEnd"
          label=""
          defaultValue={data.totalQtyEnd}
        />,
        <SearchField
          label={labels.creation}
          name="creationStart"
          type="date"
          defaultValue={DateUtils.formatForInput(data.creationStart)}
        />,
        <SearchField
          label=""
          name="creationEnd"
          type="date"
          defaultValue={DateUtils.formatForInput(data.creationEnd)}
        />
      ]}
      loadData={(data) =>
        app.stockApi.query(data, {
          defaultValue: [],
          showLoading: false
        })
      }
      columns={[
        {
          width: 250,
          header: labels.title,
          cellBoxStyle: {
            paddingTop: "10px!important",
            paddingBottom: "10px!important"
          },
          cellRenderer: ({
            data
          }: GridCellRendererProps<StockQueryData, BoxProps>) => {
            if (data == null) return undefined;

            return (
              <React.Fragment>
                <Typography variant="body2">
                  [{app.stock.getKind(data.kind)}] {data.title}
                </Typography>
                <Typography variant="caption" color="text.secondary">
                  {data.personName}{" "}
                  <LinkEx to={`./../contact/view/${data.personId}`}>
                    {labels.view}
                  </LinkEx>
                </Typography>
              </React.Fragment>
            );
          }
        },
        {
          width: 108,
          header: labels.items,
          align: "right",
          cellBoxStyle: {
            paddingTop: "10px!important",
            paddingBottom: "10px!important"
          },
          cellRenderer: ({
            data
          }: GridCellRendererProps<StockQueryData, BoxProps>) => {
            if (data == null) return undefined;

            return (
              <React.Fragment>
                <Typography variant="body2">
                  {app.formatNumber(data.totalLines)}
                </Typography>
                <Typography variant="caption" color="text.secondary">
                  {app.formatNumber(data.totalQty)}
                </Typography>
              </React.Fragment>
            );
          }
        },
        {
          width: 160,
          header: labels.receivingWarehouse,
          cellBoxStyle: {
            paddingTop: "10px!important",
            paddingBottom: "10px!important"
          },
          cellRenderer: ({
            data
          }: GridCellRendererProps<StockQueryData, BoxProps>) => {
            if (data == null) return undefined;

            return (
              <React.Fragment>
                <Typography variant="body2">{data.locationFrom}</Typography>
                {data.locationToId != data.locationFromId && (
                  <Typography
                    variant="caption"
                    color="text.secondary"
                    title={labels.shippingWarehouse}
                  >
                    {data.locationTo}
                  </Typography>
                )}
              </React.Fragment>
            );
          }
        },
        {
          header: labels.description,
          cellBoxStyle: {
            paddingTop: "10px!important",
            paddingBottom: "10px!important"
          },
          cellRenderer: ({
            data
          }: GridCellRendererProps<StockQueryData, BoxProps>) => {
            if (data == null) return undefined;

            return (
              <React.Fragment>
                <Typography variant="body2">{data.description}</Typography>
                <Typography
                  variant="caption"
                  color="text.secondary"
                  title={labels.trackingNumber}
                >
                  {data.trackingNumber}
                </Typography>
              </React.Fragment>
            );
          }
        },
        {
          width: 116,
          header: labels.receiptTime,
          cellBoxStyle: {
            paddingTop: "10px!important",
            paddingBottom: "10px!important"
          },
          cellRenderer: ({
            data
          }: GridCellRendererProps<StockQueryData, BoxProps>) => {
            if (data == null) return undefined;

            return (
              <React.Fragment>
                <Typography variant="body2">
                  {app.formatDate(data.receiptTime) ?? "-"}
                </Typography>
                <Typography
                  variant="caption"
                  color="text.secondary"
                  title={labels.creation}
                >
                  {app.formatDate(data.creation)}
                </Typography>
              </React.Fragment>
            );
          }
        },
        {
          width: DefaultUI.Widths.icon1,
          header: labels.actions,
          cellBoxStyle: {
            paddingTop: "6px!important",
            paddingBottom: "6px!important"
          },
          cellRenderer: ({
            data
          }: GridCellRendererProps<StockQueryData, BoxProps>) => {
            if (data == null) return undefined;

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
      rowHeight={[64, 220]}
      itemRenderer={(props) =>
        MobileListItemRenderer(props, (data) => {
          return [
            `[${app.stock.getKind(data.kind)}] ${data.title}`,
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
                {data.personName}{" "}
                <LinkEx to={`./../contact/view/${data.personId}`}>
                  {labels.view}
                </LinkEx>
              </Typography>
              <Typography variant="body2">
                {labels.items}: {app.formatNumber(data.totalLines)} /{" "}
                {app.formatNumber(data.totalQty)}
              </Typography>
              <Typography variant="body2">
                {labels.receiptTime}: {app.formatDate(data.receiptTime) ?? "-"}{" "}
                / {app.formatDate(data.creation)}
              </Typography>
            </React.Fragment>
          ];
        })
      }
    />
  );
}
