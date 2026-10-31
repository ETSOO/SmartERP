import { ButtonLink, HBox, LinkEx, ViewContainer } from "@etsoo/materialui";
import {
  FinanceAccountKind,
  FinanceAccountViewData,
  Permissions
} from "@etsoo/smarterp-crm";
import { app } from "../../../app/MyApp";
import { GridDataType } from "@etsoo/react";
import EditIcon from "@mui/icons-material/Edit";
import BuildIcon from "@mui/icons-material/Build";
import ContentCopyIcon from "@mui/icons-material/ContentCopy";
import Button from "@mui/material/Button";
import React from "react";

export function ViewAccountUI({
  data,
  refresh
}: {
  data: FinanceAccountViewData;
  refresh: () => Promise<void>;
}) {
  // Labels
  const labels = app.getLabels(
    "copyPaymentInfo",
    "edit",
    "financeTransactionKindAdjustment",
    "financeTransactionKindInit"
  );

  // Layout
  return (
    <ViewContainer
      data={data}
      refresh={refresh}
      fields={[
        {
          data: (item) => (
            <LinkEx
              variant="body2"
              to={`./../../../../contact/view/${item.personId}`}
            >
              {item.personName}
            </LinkEx>
          ),
          label: "owner"
        },
        {
          data: (item) => app.finance.getAccountKind(item.kind),
          label: "type"
        },
        "bank",
        "currency",
        {
          data: "accountNumber",
          singleRow: "large"
        },
        ["balance", GridDataType.Money],
        {
          data: "swift",
          singleRow: true
        },
        {
          data: (item) => (
            <HBox
              spacing={1}
              sx={{ justifyContent: "center", flexWrap: "wrap" }}
            >
              {(data.kind === FinanceAccountKind.Super ||
                data.kind === FinanceAccountKind.Transfer) && (
                <Button
                  startIcon={<ContentCopyIcon />}
                  variant="outlined"
                  onClick={() => app.formatPaymentInfo(data)}
                >
                  {labels.copyPaymentInfo}
                </Button>
              )}
              {app.owns(Permissions.Finance.Edit) && (
                <React.Fragment>
                  <ButtonLink
                    startIcon={<BuildIcon />}
                    variant="outlined"
                    href={`./../../editaccount/${data.id}`}
                  >
                    {item.refreshTime == null
                      ? labels.financeTransactionKindInit
                      : labels.financeTransactionKindAdjustment}
                  </ButtonLink>
                  <ButtonLink
                    startIcon={<EditIcon />}
                    variant="outlined"
                    href={`./../../editaccount/${data.id}`}
                  >
                    {labels.edit}
                  </ButtonLink>
                </React.Fragment>
              )}
            </HBox>
          ),
          singleRow: true
        },
        {
          data: "description",
          singleRow: true
        },
        {
          data: (item) =>
            item.productName ? (
              app.owns(Permissions.Product.View) ? (
                <LinkEx
                  variant="body2"
                  to={`./../../../../product/view/${item.productId}`}
                >
                  {item.productName}
                </LinkEx>
              ) : (
                item.productName
              )
            ) : undefined,
          label: "product"
        },
        ["creation", GridDataType.DateTime],
        ["refreshTime", GridDataType.DateTime],
        ["expiry", GridDataType.DateTime]
      ]}
    />
  );
}
