import {
  ButtonLink,
  CommonPage,
  HBox,
  IconButtonLink,
  ResponsibleContainer
} from "@etsoo/materialui";
import {
  FinanceAccountKind,
  FinanceAccountQueryData,
  Permissions
} from "@etsoo/smarterp-crm";
import { app } from "../../../app/MyApp";
import Button from "@mui/material/Button";
import Divider from "@mui/material/Divider";
import AddIcon from "@mui/icons-material/Add";
import MoneyIcon from "@mui/icons-material/Money";
import EditIcon from "@mui/icons-material/Edit";
import ArticleIcon from "@mui/icons-material/Article";
import ContentCopyIcon from "@mui/icons-material/ContentCopy";
import { DefaultUI } from "@etsoo/smarterp-core/components";
import { BoxProps } from "@mui/material/Box";
import { GridCellRendererProps, NotificationMessageType } from "@etsoo/react";
import React from "react";
import IconButton from "@mui/material/IconButton";

export default function FinanceConsole() {
  // Labels
  const labels = app.getLabels(
    "accountNumber",
    "actions",
    "addAccount",
    "addCashAccount",
    "allAccounts",
    "bank",
    "bulkCreateAccounts",
    "completeTip",
    "copyPaymentInfo",
    "currency",
    "edit",
    "type",
    "view"
  );

  // Org person id
  const orgPersonId = app.userData?.system?.personId;

  // Layout
  return (
    <CommonPage paddings={0}>
      <ResponsibleContainer<FinanceAccountQueryData, object>
        adjustHeight={(h) => {
          if (h < 360) return 0;
          return h - 360;
        }}
        fields={[]}
        columns={[
          {
            field: "kind",
            header: labels.type,
            width: 110,
            valueFormatter: ({ data }) => app.finance.getAccountKind(data?.kind)
          },
          {
            field: "bank",
            header: labels.bank
          },
          {
            field: "currency",
            header: labels.currency,
            width: 88
          },
          {
            field: "accountNumber",
            header: labels.accountNumber,
            width: 200
          },
          {
            width: DefaultUI.Widths.icon3,
            header: labels.actions,
            cellBoxStyle: {
              paddingTop: "6px!important",
              paddingBottom: "6px!important"
            },
            cellRenderer: ({
              data
            }: GridCellRendererProps<FinanceAccountQueryData, BoxProps>) => {
              if (data == null) return undefined;

              return (
                <React.Fragment>
                  {(data.kind === FinanceAccountKind.Super ||
                    data.kind === FinanceAccountKind.Transfer) && (
                    <IconButton
                      title={labels.copyPaymentInfo}
                      onClick={() => {
                        navigator.clipboard?.writeText(
                          `${app.userData?.organizationName}\n${data.bank}\n${data.accountNumber}`
                        );
                        app.notifier.message(
                          NotificationMessageType.Success,
                          labels.completeTip.format(labels.copyPaymentInfo)
                        );
                      }}
                    >
                      <ContentCopyIcon />
                    </IconButton>
                  )}
                  {app.owns(Permissions.Finance.Edit) && (
                    <IconButtonLink
                      title={labels.edit}
                      href={`./editaccount/${data.id}`}
                    >
                      <EditIcon />
                    </IconButtonLink>
                  )}
                  {app.owns(Permissions.Finance.View) && (
                    <IconButtonLink
                      title={labels.view}
                      href={`./viewaccount/${data.id}`}
                    >
                      <ArticleIcon />
                    </IconButtonLink>
                  )}
                </React.Fragment>
              );
            }
          }
        ]}
        hideFooter
        loadData={async (data) => {
          return await app.financeAccountApi.query(
            { ...data, personId: orgPersonId, enabled: true },
            {
              defaultValue: [],
              showLoading: false
            }
          );
        }}
      />
      <HBox sx={{ paddingY: 2, flexWrap: "wrap", gap: 2 }}>
        <ButtonLink
          variant="contained"
          startIcon={<AddIcon />}
          href={`./addaccount?personId=${orgPersonId}`}
        >
          {labels.addAccount}
        </ButtonLink>
        <Button variant="outlined" startIcon={<MoneyIcon />}>
          {labels.addCashAccount}
        </Button>
        <Button variant="outlined">{labels.bulkCreateAccounts}</Button>
        <Divider orientation="vertical" flexItem />
        <ButtonLink variant="outlined" href="./accounts">
          {labels.allAccounts}
        </ButtonLink>
      </HBox>
    </CommonPage>
  );
}
