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
import { GridCellRendererProps, ScrollerListForwardRef } from "@etsoo/react";
import { useNavigate } from "react-router-dom";
import { app } from "../../../app/MyApp";
import { usePageDataEmpty } from "@etsoo/smarterp-core";
import { DataTypes } from "@etsoo/shared";
import { DefaultUI } from "@etsoo/smarterp-core/components";
import { BoxProps } from "@mui/material/Box";
import Fab from "@mui/material/Fab";
import { OrderPaymentQueryData } from "@etsoo/smarterp-crm";

const template = {
  keyword: "string",
  isValid: "boolean"
} as const satisfies DataTypes.BasicTemplate;

export default function OrderPayments() {
  const navigate = useNavigate();

  const labels = app.getLabels(
    "actions",
    "add",
    "edit",
    "enabled",
    "isAvailable",
    "keywords",
    "sortOrderPayment",
    "title",
    "type"
  );

  const ref =
    React.useRef<ScrollerListForwardRef<OrderPaymentQueryData>>(undefined);

  const reloadData = React.useCallback(() => ref.current?.reset(), []);

  usePageDataEmpty(app);

  return (
    <ResponsivePage<OrderPaymentQueryData, typeof template>
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
              {labels.sortOrderPayment}
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
      fieldTemplate={template}
      fields={(data) => [
        <SearchField
          label={labels.keywords}
          name="keyword"
          defaultValue={data.keyword}
          minChars={2}
        />,
        <SelectBool
          search
          name="isValid"
          label={labels.isAvailable}
          value={data.isValid}
        />
      ]}
      loadData={async (data) => {
        return await app.orderPaymentApi.query(data, {
          defaultValue: [],
          showLoading: false
        });
      }}
      columns={[
        {
          field: "kind",
          header: labels.type,
          width: 120,
          valueFormatter: ({ data }) => app.order.getPaymentKind(data?.kind)
        },
        {
          field: "title",
          header: labels.title
        },
        {
          field: "isValid",
          width: 100,
          header: labels.enabled,
          valueFormatter: ({ data }) => (data?.isValid ? "True" : "False")
        },
        {
          width: DefaultUI.Widths.icon1,
          header: labels.actions,
          cellRenderer: ({
            data,
            cellProps
          }: GridCellRendererProps<OrderPaymentQueryData, BoxProps>) => {
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
            app.order.getPaymentKind(data.kind),
            [
              {
                label: labels.edit,
                icon: <EditIcon />,
                action: `./edit/${data.id}`
              }
            ],
            <React.Fragment></React.Fragment>
          ];
        })
      }
    />
  );
}
