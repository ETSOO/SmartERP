import {
  ResponsivePage,
  SearchField,
  IconButtonLink,
  MobileListItemRenderer,
  SelectBool,
  ButtonLink
} from "@etsoo/materialui";
import EditIcon from "@mui/icons-material/Edit";
import AddIcon from "@mui/icons-material/Add";
import SortIcon from "@mui/icons-material/Sort";
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
import { PromotionQueryData } from "@etsoo/smarterp-crm";
import { DataTypes } from "@etsoo/shared";
import { DefaultUI, StatusList } from "@etsoo/smarterp-core/components";
import { BoxProps } from "@mui/material/Box";
import Fab from "@mui/material/Fab";
import { Typography } from "@mui/material";
import { CustomerList, ProductList } from "@etsoo/smarterp-crm/components";

const template = {
  keyword: "string",
  isValid: "boolean",
  productId: "number",
  personId: "number",
  status: "number"
} as const satisfies DataTypes.BasicTemplate;

export default function Promotions() {
  // Route
  const navigate = useNavigate();

  // Labels
  const labels = app.getLabels(
    "actions",
    "add",
    "couponsApplied",
    "creation",
    "discount",
    "edit",
    "keywords",
    "enabled",
    "isAvailable",
    "minAmount",
    "sortPromotion",
    "title"
  );

  // Refs
  const ref =
    React.useRef<ScrollerListForwardRef<PromotionQueryData>>(undefined);

  // Load data
  const reloadData = React.useCallback(() => ref.current?.reset(), []);

  // Page data hook
  usePageDataEmpty(app);

  return (
    <ResponsivePage<PromotionQueryData, typeof template>
      {...DefaultUI.pageProps({
        onRefresh: reloadData,
        fabButtons: (
          <React.Fragment>
            <ButtonLink
              href="./sort"
              size="small"
              variant="outlined"
              startIcon={<SortIcon />}
            >
              {labels.sortPromotion}
            </ButtonLink>
            <Fab
              title={labels.add}
              size="medium"
              color="primary"
              onClick={() => navigate("./add")}
            >
              <AddIcon />
            </Fab>
          </React.Fragment>
        )
      })}
      mRef={ref}
      defaultOrderBy={[{ field: "creation", desc: true }]}
      fieldTemplate={template}
      fields={(data) => [
        <SearchField
          label={labels.keywords}
          name="keyword"
          defaultValue={data.keyword}
        />,
        <SelectBool
          search
          name="isValid"
          label={labels.isAvailable}
          value={data.isValid}
        />,
        <ProductList search idValue={data.productId} />,
        <CustomerList name="personId" search idValue={data.personId} />,
        <StatusList search idValue={data.status} />
      ]}
      loadData={async (data) => {
        return await app.promotionApi.query(data, {
          defaultValue: [],
          showLoading: false
        });
      }}
      columns={[
        {
          field: "title",
          header: labels.title,
          cellBoxStyle: GridDeletedCellBoxStyle
        },
        {
          field: "minAmount",
          type: GridDataType.IntMoney,
          width: 120,
          header: labels.minAmount,
          renderProps: (data) => app.getMoneyFormatProps(data?.currency)
        },
        {
          field: "discount",
          type: GridDataType.Int,
          width: 100,
          header: labels.discount
        },
        {
          field: "couponsApplied",
          type: GridDataType.Int,
          width: 132,
          header: labels.couponsApplied,
          valueFormatter: ({ data }) => {
            if (data == null) return undefined;

            if (data.coupons)
              return `${app.formatNumber(
                data.couponsApplied
              )} / ${app.formatNumber(data.coupons)}`;
            else return app.formatNumber(data.couponsApplied);
          }
        },
        {
          field: "creation",
          type: GridDataType.Date,
          width: 116,
          header: labels.creation
        },
        {
          width: DefaultUI.Widths.icon1,
          header: labels.actions,
          cellRenderer: ({
            data,
            cellProps
          }: GridCellRendererProps<PromotionQueryData, BoxProps>) => {
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
              </React.Fragment>
            );
          }
        }
      ]}
      itemRenderer={(props) =>
        MobileListItemRenderer(props, (data) => {
          return [
            data.title,
            app.formatDate(data.creation, "d"),
            [
              {
                label: labels.edit,
                icon: <EditIcon />,
                action: `./edit/${data.id}`
              }
            ],
            <React.Fragment>
              <Typography variant="body2"></Typography>
            </React.Fragment>
          ];
        })
      }
    />
  );
}
