import { EntityStatus } from "@etsoo/appscript";
import {
  ResponsivePage,
  SearchField,
  IconButtonLink,
  MobileListItemRenderer,
  ButtonLink,
  MoneyText
} from "@etsoo/materialui";
import AddIcon from "@mui/icons-material/Add";
import CategoryIcon from "@mui/icons-material/Category";
import CelebrationIcon from "@mui/icons-material/Celebration";
import EditIcon from "@mui/icons-material/Edit";
import ArticleIcon from "@mui/icons-material/Article";
import FlagIcon from "@mui/icons-material/Flag";
import PriceCheckIcon from "@mui/icons-material/PriceCheck";
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
import { ProductQueryData } from "@etsoo/smarterp-crm";
import { DataTypes } from "@etsoo/shared";
import { DefaultUI, StatusList } from "@etsoo/smarterp-core/components";
import { BoxProps } from "@mui/material/Box";
import Typography from "@mui/material/Typography";
import Fab from "@mui/material/Fab";
import { Permissions } from "@etsoo/smarterp-crm";
import {
  ProductCategoryTiplist,
  ProductScopeList,
  ProductUnitList
} from "@etsoo/smarterp-crm/components";

const template = {
  name: "string",
  assignedId: "string",
  status: "number",
  unitId: "number",
  categoryId: "number",
  scope: "number"
} as const satisfies DataTypes.BasicTemplate;

export default function AllProducts() {
  // Route
  const navigate = useNavigate();

  // Labels
  const labels = app.getLabels(
    "actions",
    "add",
    "assignedId",
    "category",
    "categories",
    "confirmAction",
    "edit",
    "entityStatus",
    "personProducts",
    "productName",
    "productUnit",
    "productUnits",
    "promotionPrice",
    "promotions",
    "retailPrice",
    "view"
  );

  // Refs
  const ref = React.useRef<ScrollerListForwardRef<ProductQueryData>>(undefined);

  // Load data
  const reloadData = React.useCallback(() => ref.current?.reset(), []);

  // Page data hook
  usePageDataEmpty(app);

  return (
    <ResponsivePage<ProductQueryData, typeof template>
      {...DefaultUI.pageProps({
        onRefresh: reloadData,
        fabButtons: (
          <React.Fragment>
            {app.owns(Permissions.Product.Manage) && (
              <React.Fragment>
                <ButtonLink
                  href={`./promotion`}
                  size="small"
                  variant="outlined"
                  startIcon={<CelebrationIcon />}
                >
                  {labels.promotions}
                </ButtonLink>
                <ButtonLink
                  href={`./category`}
                  size="small"
                  variant="outlined"
                  startIcon={<CategoryIcon />}
                >
                  {labels.categories}
                </ButtonLink>
                <ButtonLink
                  href={`./unit`}
                  size="small"
                  variant="outlined"
                  startIcon={<FlagIcon />}
                >
                  {labels.productUnits}
                </ButtonLink>
              </React.Fragment>
            )}
            {app.owns(Permissions.Product.Manage) && (
              <Fab
                title={labels.personProducts}
                size="small"
                color="secondary"
                onClick={() => navigate("./personProduct")}
              >
                <PriceCheckIcon />
              </Fab>
            )}
            {app.owns(Permissions.Product.Add) && (
              <Fab
                title={labels.add}
                size="medium"
                color="primary"
                onClick={() => navigate("./add")}
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
          label={labels.productName}
          name="name"
          defaultValue={data.name}
        />,
        <SearchField
          label={labels.assignedId}
          name="assignedId"
          minChars={3}
          defaultValue={data.assignedId}
        />,
        <ProductCategoryTiplist
          label={labels.category}
          name="categoryId"
          search
        />,
        <ProductUnitList search value={data.unitId} />,
        <ProductScopeList search value={data.scope} />,
        <StatusList search idValue={data.status} />
      ]}
      loadData={(data) =>
        app.productApi.query(data, {
          defaultValue: [],
          showLoading: false
        })
      }
      columns={[
        {
          field: "name",
          header: labels.productName,
          valueFormatter: ({ data }) =>
            data == null
              ? undefined
              : `${data.assignedId ? `${data.assignedId} - ` : ""}${data.name}`,
          sortable: true,
          cellBoxStyle: GridDeletedCellBoxStyle
        },
        {
          field: "retailPrice",
          type: GridDataType.Money,
          width: 120,
          header: labels.retailPrice,
          renderProps: (data) => app.getMoneyFormatProps(data?.currency)
        },
        {
          field: "promotionPrice",
          type: GridDataType.Money,
          width: 120,
          header: labels.promotionPrice,
          renderProps: app.getMoneyFormatProps()
        },
        {
          field: "unitName",
          header: labels.productUnit,
          width: 110,
          valueFormatter: ({ data }) => {
            if (data == null) return undefined;
            if (data.assetQty) return `${data.unitName} (${data.assetQty})`;
            else return data.unitName;
          }
        },
        {
          field: "categories",
          header: labels.categories,
          width: 200,
          valueFormatter: ({ data }) =>
            data?.categories?.map((c) => c.names.join(" -> ")).join(", ")
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
          }: GridCellRendererProps<ProductQueryData, BoxProps>) => {
            if (data == null) return undefined;

            return (
              <React.Fragment>
                {app.owns(Permissions.Product.Edit) && (
                  <IconButtonLink
                    title={labels.edit}
                    href={`./edit/${data.id}`}
                  >
                    <EditIcon />
                  </IconButtonLink>
                )}
                {app.owns(Permissions.Product.View) && (
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
      rowHeight={180}
      itemRenderer={(props) =>
        MobileListItemRenderer(props, (data) => {
          return [
            data.name,
            data.assignedId,
            [
              app.owns(Permissions.Product.Edit) && {
                label: labels.edit,
                icon: <EditIcon />,
                action: `./edit/${data.id}`
              },
              app.owns(Permissions.Product.View) && {
                label: labels.view,
                icon: <ArticleIcon />,
                action: `./view/${data.id}`
              }
            ],
            <React.Fragment>
              {data.categories && data.categories.length > 0 && (
                <Typography variant="body2">
                  {labels.categories}:{" "}
                  {data.categories.map((c) => c.names.join(" -> ")).join(", ")}
                </Typography>
              )}
              <Typography variant="body2">
                {data.promotionPrice ? (
                  <React.Fragment>
                    <MoneyText
                      value={data.promotionPrice}
                      currency={app.currency}
                      color="red"
                    />{" "}
                    <MoneyText
                      value={data.retailPrice}
                      currency={data.currency}
                      sx={{ fontSize: 9, textDecoration: "line-through" }}
                    />
                  </React.Fragment>
                ) : (
                  <MoneyText
                    value={data.retailPrice}
                    currency={data.currency}
                  />
                )}{" "}
                /{" "}
                {data.assetQty
                  ? `${data.unitName} (${data.assetQty})`
                  : data.unitName}
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
          ];
        })
      }
    />
  );
}
