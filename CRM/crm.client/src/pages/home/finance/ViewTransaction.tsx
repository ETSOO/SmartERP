import { GridDataType, useParamsEx } from "@etsoo/react";
import { HBox, LinkEx, ViewPage } from "@etsoo/materialui";
import { app } from "../../../app/MyApp";
import { usePageDataEmpty } from "@etsoo/smarterp-core";
import React from "react";
import Typography from "@mui/material/Typography";
import {
  FinanceTransactionViewData,
  OrderKind,
  Permissions
} from "@etsoo/smarterp-crm";

export default function ViewTransaction() {
  // Route
  const { id = 0 } = useParamsEx({ id: "number" });

  // Labels
  const labels = app.getLabels("view");

  // Load data
  const loadData = React.useCallback(
    async () => app.financeTransactionApi.read(id),
    [id]
  );

  const editable = app.owns(Permissions.PO.Edit);

  // Page data hook
  usePageDataEmpty(app);

  return (
    <ViewPage<FinanceTransactionViewData>
      paddings={0}
      titleBar={(item) => (
        <HBox
          sx={{
            justifyContent: "center",
            alignItems: "center",
            marginBottom: 2
          }}
        >
          <Typography
            variant="subtitle2"
            sx={{ textAlign: "center", paddingRight: 2 }}
          >
            {item.title}
          </Typography>
        </HBox>
      )}
      fields={[
        {
          data: (item) => app.finance.getTransactionKind(item.kind),
          label: "type"
        },
        {
          data: (item) =>
            app.formatMoney(item.amount, false, { currency: item.currency }),
          label: "amount"
        },
        {
          data: (item) =>
            item.orderId ? (
              <LinkEx
                variant="body2"
                to={`./../../../../${item.orderKind == OrderKind.Order ? "order" : "po"}/view/${item.orderId}`}
              >
                {item.orderTitle}
              </LinkEx>
            ) : undefined,
          label: "order",
          singleRow: "large"
        },
        "referenceId",
        {
          data: (item) => (
            <LinkEx
              variant="body2"
              to={`./../../viewaccount/${item.accountId}`}
            >
              {item.account}
            </LinkEx>
          ),
          label: "account"
        },
        {
          data: (item) => (
            <LinkEx
              variant="body2"
              to={`./../../../../contact/view/${item.authorId}`}
            >
              {item.authorName}
            </LinkEx>
          ),
          label: "author"
        },
        ["happenDate", GridDataType.DateTime],
        ["creation", GridDataType.DateTime],
        {
          data: (item) =>
            item.offsetId || item.offsetSourceId ? (
              <LinkEx
                variant="body2"
                to={`./../${item.offsetId || item.offsetSourceId}`}
              >
                {labels.view}
              </LinkEx>
            ) : undefined,
          label: "financeTransactionKindOffset"
        }
      ]}
      loadData={loadData}
      actions={(data, refresh) => <React.Fragment></React.Fragment>}
    ></ViewPage>
  );
}
