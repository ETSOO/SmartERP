import {
  MobileListItemRenderer,
  ResponsivePage,
  SearchField
} from "@etsoo/materialui";
import {
  GridCellRendererProps,
  GridDataType,
  GridDeletedCellBoxStyle,
  ScrollerListForwardRef
} from "@etsoo/react";
import { DataTypes } from "@etsoo/shared";
import { DefaultUI } from "@etsoo/smarterp-core/components";
import { OrderLineQueryData } from "@etsoo/smarterp-crm";
import { app } from "../../../app/MyApp";
import { BoxProps } from "@mui/material/Box";
import React from "react";

const template = {
  keyword: "string"
} as const satisfies DataTypes.BasicTemplate;

export type AllOrderLinesProps = {
  orderId: number;
  refresh: () => Promise<void>;
};

export function AllOrderLines(props: AllOrderLinesProps) {
  // Destruct
  const { orderId, refresh } = props;

  // Labels
  const labels = app.getLabels("actions", "keywords", "startTime", "title");

  // Refs
  const ref =
    React.useRef<ScrollerListForwardRef<OrderLineQueryData>>(undefined);

  // Load data
  const reloadData = React.useCallback(() => ref.current?.reset(), []);

  return (
    <ResponsivePage<OrderLineQueryData, typeof template>
      {...DefaultUI.pageProps({
        onRefresh: reloadData,
        fabButtons: <React.Fragment></React.Fragment>
      })}
      mRef={ref}
      fieldTemplate={template}
      fields={(data) => [
        <SearchField
          label={labels.keywords}
          name="keyword"
          defaultValue={data.keyword}
          minChars={2}
        />
      ]}
      loadData={(data) =>
        app.orderLineApi.query(
          { orderId, ...data },
          {
            defaultValue: [],
            showLoading: false
          }
        )
      }
      columns={[
        {
          field: "title",
          header: labels.title,
          sortable: true,
          cellBoxStyle: GridDeletedCellBoxStyle
        },
        {
          field: "startTime",
          type: GridDataType.DateTime,
          width: 116,
          header: labels.startTime,
          sortable: true,
          sortAsc: false
        },
        {
          width: DefaultUI.Widths.icon2,
          header: labels.actions,
          cellRenderer: ({
            data,
            cellProps
          }: GridCellRendererProps<OrderLineQueryData, BoxProps>) => {
            if (data == null) return undefined;

            cellProps.sx = {
              paddingTop: "6px!important",
              paddingBottom: "6px!important"
            };

            return <React.Fragment></React.Fragment>;
          }
        }
      ]}
      itemRenderer={(props) =>
        MobileListItemRenderer(props, (data) => {
          return [
            data.title,
            app.formatDate(data.startTime, "ds"),
            [],
            <React.Fragment></React.Fragment>
          ];
        })
      }
    />
  );
}
