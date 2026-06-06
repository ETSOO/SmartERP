import {
  ResponsivePage,
  SearchField,
  IconButtonLink,
  MobileListItemRenderer
} from "@etsoo/materialui";
import ArticleIcon from "@mui/icons-material/Article";
import EditIcon from "@mui/icons-material/Edit";
import AddIcon from "@mui/icons-material/Add";
import React from "react";
import {
  GridCellRendererProps,
  GridDataType,
  GridDeletedCellBoxStyle,
  ScrollerListForwardRef,
  useSearchParamsEx
} from "@etsoo/react";
import { useNavigate } from "react-router-dom";
import { app } from "../../../app/MyApp";
import { usePageDataEmpty } from "@etsoo/smarterp-core";
import { AssetQueryData, Permissions } from "@etsoo/smarterp-crm";
import { DataTypes } from "@etsoo/shared";
import { DefaultUI, StatusList } from "@etsoo/smarterp-core/components";
import { BoxProps } from "@mui/material/Box";
import Fab from "@mui/material/Fab";
import { Typography } from "@mui/material";
import {
  PersonList,
  ProductList,
  SupplierList
} from "@etsoo/smarterp-crm/components";

const template = {
  keyword: "string",
  productId: "number",
  personId: "number",
  supplierId: "number",
  status: "number"
} as const satisfies DataTypes.BasicTemplate;

export default function Assets() {
  // Route
  const navigate = useNavigate();

  const { personId } = useSearchParamsEx({ personId: "number" });

  // Labels
  const labels = app.getLabels(
    "actions",
    "add",
    "balance",
    "creation",
    "edit",
    "expiry",
    "keywords",
    "product",
    "relatedTarget",
    "sn",
    "supplier",
    "times",
    "title",
    "view"
  );

  // Refs
  const ref = React.useRef<ScrollerListForwardRef<AssetQueryData>>(undefined);

  // Load data
  const reloadData = React.useCallback(() => ref.current?.reset(), []);

  const defaultCurrency = app.system.getDefaultCurrency();

  const hasEdit = app.owns(Permissions.Org.Manage);

  // Page data hook
  usePageDataEmpty(app);

  return (
    <ResponsivePage<AssetQueryData, typeof template>
      {...DefaultUI.pageProps({
        onRefresh: reloadData,
        fabButtons: (
          <React.Fragment>
            {hasEdit && (
              <Fab
                title={labels.add}
                size="medium"
                color="primary"
                onClick={() => navigate(`./add?personId=${personId ?? ""}`)}
              >
                <AddIcon />
              </Fab>
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
          label={labels.keywords}
          name="keyword"
          defaultValue={data.keyword}
        />,
        <ProductList search idValue={data.productId} />,
        <PersonList
          label={labels.relatedTarget}
          name="personId"
          search
          idValue={data.personId ?? personId}
        />,
        <SupplierList search idValue={data.supplierId} />,
        <StatusList search idValue={data.status} />
      ]}
      loadData={async (data) => {
        return await app.assetApi.query(data, {
          defaultValue: [],
          showLoading: false
        });
      }}
      columns={[
        {
          field: "owner",
          header: labels.relatedTarget
        },
        {
          field: "product",
          header: labels.product
        },
        {
          field: "sn",
          header: labels.sn,
          cellBoxStyle: GridDeletedCellBoxStyle
        },
        {
          field: "amount",
          type: GridDataType.IntMoney,
          width: 120,
          header: labels.balance,
          renderProps: (_) => app.getMoneyFormatProps(defaultCurrency)
        },
        {
          field: "times",
          type: GridDataType.Int,
          width: 100,
          header: labels.times
        },
        {
          field: "expiry",
          type: GridDataType.Date,
          width: 116,
          header: labels.expiry
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
          }: GridCellRendererProps<AssetQueryData, BoxProps>) => {
            if (data == null) return undefined;

            return (
              <React.Fragment>
                {hasEdit && (
                  <IconButtonLink
                    title={labels.edit}
                    href={`./edit/${data.id}`}
                  >
                    <EditIcon />
                  </IconButtonLink>
                )}
                {hasEdit && (
                  <IconButtonLink
                    title={labels.view}
                    href={`./view/${data.id}`}
                  >
                    <ArticleIcon />
                  </IconButtonLink>
                )}
              </React.Fragment>
            );
          }
        }
      ]}
      itemRenderer={(props) =>
        MobileListItemRenderer(props, (data) => {
          return [
            data.sn,
            app.formatDate(data.creation, "d"),
            [
              hasEdit && {
                label: labels.edit,
                icon: <EditIcon />,
                action: `./edit/${data.id}`
              },
              hasEdit && {
                label: labels.view,
                icon: <ArticleIcon />,
                action: `./view/${data.id}`
              }
            ],
            <React.Fragment>
              <Typography variant="body2">{data.owner}</Typography>
              <Typography component="div" variant="caption">
                {data.sn} - {data.product}
              </Typography>
              {data.amount != null && (
                <Typography variant="body2">
                  {labels.balance}:{" "}
                  {app.formatMoney(
                    data.amount,
                    undefined,
                    app.getMoneyFormatProps(defaultCurrency)
                  )}
                </Typography>
              )}
              {data.times != null && (
                <Typography variant="body2">
                  {labels.times}: {app.formatNumber(data.times)}
                </Typography>
              )}
              <Typography variant="body2">
                {labels.expiry}: {app.formatDate(data.expiry, "d")}
              </Typography>
            </React.Fragment>
          ];
        })
      }
    />
  );
}
