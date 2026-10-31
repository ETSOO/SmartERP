import {
  HBox,
  IconButtonLink,
  LinkEx,
  MobileListItemRenderer,
  ResponsivePage,
  SearchField
} from "@etsoo/materialui";
import { GridCellRendererProps, ScrollerListForwardRef } from "@etsoo/react";
import { DataTypes } from "@etsoo/shared";
import { DefaultUI } from "@etsoo/smarterp-core/components";
import { FinanceTransactionQueryData, OrderKind } from "@etsoo/smarterp-crm";
import { app } from "../../../app/MyApp";
import { BoxProps } from "@mui/material/Box";
import React from "react";
import { useNavigate } from "react-router-dom";
import ArticleIcon from "@mui/icons-material/Article";
import Typography from "@mui/material/Typography";

const template = {
  keyword: "string"
} as const satisfies DataTypes.BasicTemplate;

export type AllOrderLinesProps = {
  accountId: number;
  refresh: () => Promise<void>;
};

export function AccountTransactions(props: AllOrderLinesProps) {
  // Route
  const navigate = useNavigate();

  // Destruct
  const { accountId } = props;

  // Labels
  const labels = app.getLabels(
    "actions",
    "amount",
    "keywords",
    "happenDate",
    "order",
    "title",
    "view"
  );

  // Refs
  const ref =
    React.useRef<ScrollerListForwardRef<FinanceTransactionQueryData>>(
      undefined
    );

  // Load data
  const reloadData = React.useCallback(() => ref.current?.reset(), []);

  return (
    <ResponsivePage<FinanceTransactionQueryData, typeof template>
      {...DefaultUI.pageProps({
        onRefresh: reloadData,
        fabButtons: <React.Fragment></React.Fragment>
      })}
      mRef={ref}
      quickAction={(data) => navigate(`./../../viewtransaction/${data.id}`)}
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
        app.financeTransactionApi.query(
          {
            accountId,
            ...data,
            queryPaging: {
              batchSize: 20,
              orderBy: [{ field: "id", desc: false, unique: true }]
            }
          },
          {
            defaultValue: [],
            showLoading: false
          }
        )
      }
      columns={[
        {
          width: 132,
          header: labels.happenDate,
          cellBoxStyle: {
            paddingTop: "10px!important",
            paddingBottom: "10px!important"
          },
          cellRenderer: ({
            data
          }: GridCellRendererProps<FinanceTransactionQueryData, BoxProps>) => {
            if (data == null) return undefined;

            return (
              <React.Fragment>
                <Typography variant="body2">
                  {app.formatDate(data.creation, "dm")}
                </Typography>
                <Typography variant="caption" color="text.secondary">
                  {data.happenDate ? app.formatDate(data.happenDate, "d") : ""}
                </Typography>
              </React.Fragment>
            );
          }
        },
        {
          width: 128,
          header: labels.amount,
          align: "right",
          cellBoxStyle: {
            paddingTop: "10px!important",
            paddingBottom: "10px!important"
          },
          cellRenderer: ({
            data
          }: GridCellRendererProps<FinanceTransactionQueryData, BoxProps>) => {
            if (data == null) return undefined;

            return (
              <React.Fragment>
                <Typography variant="body2">
                  {app.formatMoney(data.amount, undefined, {
                    currency: data.currency
                  })}
                </Typography>
                <Typography variant="caption" color="text.secondary">
                  {app.finance.getTransactionKind(data.kind)}
                </Typography>
              </React.Fragment>
            );
          }
        },
        {
          header: labels.title,
          cellBoxStyle: {
            paddingTop: "10px!important",
            paddingBottom: "10px!important"
          },
          cellRenderer: ({
            data
          }: GridCellRendererProps<FinanceTransactionQueryData, BoxProps>) => {
            if (data == null) return undefined;

            return (
              <React.Fragment>
                <Typography variant="body2">{data.title}</Typography>
                <HBox spacing={1}>
                  <Typography variant="caption" color="text.secondary">
                    {data.authorName}
                  </Typography>
                  {data.orderId ? (
                    <Typography variant="caption" color="text.secondary">
                      <LinkEx
                        to={`./../../../../${data.orderKind === OrderKind.Order ? "order" : "po"}/view/${data.orderId}`}
                      >
                        {labels.order}
                      </LinkEx>
                    </Typography>
                  ) : undefined}
                  {data.referenceId ? (
                    <Typography variant="caption" color="text.secondary">
                      {data.referenceId}
                    </Typography>
                  ) : undefined}
                </HBox>
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
          }: GridCellRendererProps<FinanceTransactionQueryData, BoxProps>) => {
            if (data == null) return undefined;

            return (
              <React.Fragment>
                <IconButtonLink
                  title={labels.view}
                  href={`./../../viewtransaction/${data.id}`}
                >
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
            data.title,
            app.formatDate(data.creation, "ds"),
            [
              {
                label: labels.view,
                icon: <ArticleIcon />,
                action: `./../../viewtransaction/${data.id}`
              }
            ],
            <React.Fragment>
              <Typography variant="body2">
                {labels.amount}:{" "}
                {app.formatMoney(data.amount, undefined, {
                  currency: data.currency
                })}
              </Typography>
              {data.happenDate && (
                <Typography variant="body2">
                  {labels.happenDate}: {app.formatDate(data.happenDate, "ds")}
                </Typography>
              )}
            </React.Fragment>
          ];
        })
      }
    />
  );
}
