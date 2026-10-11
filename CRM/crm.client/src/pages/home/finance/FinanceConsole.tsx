import {
  ButtonLink,
  CommonPage,
  HBox,
  IconButtonLink,
  ResponsibleContainer,
  VBox
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
import { GridCellRendererProps, GridMethodRef } from "@etsoo/react";
import React from "react";
import IconButton from "@mui/material/IconButton";
import { useNavigate } from "react-router-dom";
import { CurrencyList } from "../../../components/CurrencyList";
import { DomUtils } from "@etsoo/shared";

export default function FinanceConsole() {
  // Route
  const navigate = useNavigate();

  // Labels
  const labels = app.getLabels(
    "accountNumber",
    "actions",
    "addAccount",
    "addCashAccount",
    "allAccounts",
    "bank",
    "bulkCreateAccounts",
    "copyPaymentInfo",
    "currency",
    "edit",
    "type",
    "view"
  );

  // Refs
  const ref = React.useRef<GridMethodRef<FinanceAccountQueryData>>(undefined);

  // Org person id
  const orgPersonId = app.userData?.system?.personId;

  const hasAdd = app.owns(Permissions.Finance.Add);

  const addCashAccount = () => {
    app.showInputDialog({
      title: labels.addCashAccount,
      message: "",
      callback: async (form) => {
        // Cancelled
        if (form == null) {
          return;
        }

        if (!form.reportValidity()) {
          return false;
        }

        // Form data
        const { currency } = DomUtils.dataAs(new FormData(form), {
          currency: "string"
        });

        if (!currency) {
          DomUtils.setFocus("currency", form);
          return false;
        }

        const result = await app.financeAccountApi.createCash({ currency });
        if (result == null) return;

        if (result.ok) {
          ref.current?.reset();
          return true;
        }

        app.alertResult(result);
        return false;
      },
      inputs: (
        <VBox spacing={2} sx={{ marginTop: 1 }}>
          <CurrencyList required fullWidth />
        </VBox>
      )
    });
  };

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
                      onClick={() => app.formatPaymentInfo(data)}
                    >
                      <ContentCopyIcon />
                    </IconButton>
                  )}
                  {app.owns(Permissions.Finance.Edit) && (
                    <IconButtonLink
                      title={labels.edit}
                      href={`./editaccount/${data.id}?from=console`}
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
            {
              ...data,
              personId: orgPersonId,
              enabled: true,
              kinds: [
                FinanceAccountKind.Cash,
                FinanceAccountKind.Transfer,
                FinanceAccountKind.Super
              ]
            },
            {
              defaultValue: [],
              showLoading: false
            }
          );
        }}
        mRef={ref}
        quickAction={(data) => navigate(`./viewaccount/${data.id}`)}
      />
      <HBox sx={{ paddingY: 2, flexWrap: "wrap", gap: 2 }}>
        {hasAdd && (
          <React.Fragment>
            <ButtonLink
              variant="contained"
              startIcon={<AddIcon />}
              href={`./addaccount?personId=${orgPersonId}&from=console`}
            >
              {labels.addAccount}
            </ButtonLink>
            <Button
              variant="outlined"
              startIcon={<MoneyIcon />}
              onClick={() => addCashAccount()}
            >
              {labels.addCashAccount}
            </Button>
            <ButtonLink variant="outlined" href="./bulkcreateaccounts">
              {labels.bulkCreateAccounts}
            </ButtonLink>
          </React.Fragment>
        )}
        <Divider orientation="vertical" flexItem />
        <ButtonLink variant="outlined" href="./accounts">
          {labels.allAccounts}
        </ButtonLink>
      </HBox>
    </CommonPage>
  );
}
