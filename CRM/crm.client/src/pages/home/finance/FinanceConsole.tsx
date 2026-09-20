import {
  ButtonLink,
  CommonPage,
  HBox,
  IconButtonLink,
  ResponsibleContainer
} from "@etsoo/materialui";
import { FinanceAccountQueryData, Permissions } from "@etsoo/smarterp-crm";
import { app } from "../../../app/MyApp";
import Button from "@mui/material/Button";
import Divider from "@mui/material/Divider";
import AddIcon from "@mui/icons-material/Add";
import MoneyIcon from "@mui/icons-material/Money";
import EditIcon from "@mui/icons-material/Edit";
import ArticleIcon from "@mui/icons-material/Article";
import { DefaultUI } from "@etsoo/smarterp-core/components";
import { BoxProps } from "@mui/material/Box";
import { GridCellRendererProps } from "@etsoo/react";
import React from "react";

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
            width: DefaultUI.Widths.icon2,
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
                  {app.owns(Permissions.Finance.Edit) && (
                    <IconButtonLink
                      title={labels.edit}
                      href={`./edit/${data.id}`}
                    >
                      <EditIcon />
                    </IconButtonLink>
                  )}
                  {app.owns(Permissions.Finance.View) && (
                    <IconButtonLink
                      title={labels.view}
                      href={`./../../contact/view/${data.id}`}
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
