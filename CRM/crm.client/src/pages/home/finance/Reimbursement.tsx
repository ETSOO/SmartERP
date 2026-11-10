import React from "react";
import { useParamsEx } from "@etsoo/react";
import { useNavigate } from "react-router-dom";
import { app } from "../../../app/MyApp";
import { CommonPage, InputField, MoneyInputField } from "@etsoo/materialui";
import { CurrencyList } from "../../../components/CurrencyList";
import Grid from "@mui/material/Grid";
import {
  AccountTiplist,
  OrderAllList,
  UserTiplist
} from "@etsoo/smarterp-crm/components";
import {
  FinanceAccountItem,
  FinanceTransactionReimburseRQ
} from "@etsoo/smarterp-crm";
import { AccountPaymentInput } from "../../../components/AccountPaymentInput";
import Divider from "@mui/material/Divider";
import Button from "@mui/material/Button";
import PaymentIcon from "@mui/icons-material/Payment";

export default function Reimbursement() {
  // Route
  const navigate = useNavigate();
  const { id } = useParamsEx({ id: "number" });

  // Labels
  const labels = app.getLabels(
    "amount",
    "currency",
    "description",
    "happenDate",
    "orderPaymentKindBankTransfer",
    "orderPaymentKindCash",
    "referenceId",
    "submit"
  );

  // States
  const [currency, setCurrency] = React.useState<string>(app.currency);
  const [amount, setAmount] = React.useState<number>(0);
  const [targetPersonId, setTargetPersonId] = React.useState<
    number | undefined
  >(undefined);
  const [accounts, setAccounts] = React.useState<FinanceAccountItem[]>([]);

  React.useEffect(() => {
    app.financeAccountApi.viewOrgAccounts(currency).then((accounts) => {
      if (accounts == null) return;
      setAccounts(accounts);
    });
  }, [currency]);

  return (
    <CommonPage paddings={0}>
      <form
        onSubmit={async (e) => {
          e.preventDefault();

          const form = e.currentTarget;

          let total: number = 0;

          const rq: FinanceTransactionReimburseRQ = {
            currency,
            targetPersonId: 0,
            amount: total,
            description: "",
            items: []
          };

          const elements = form.elements;

          let firstAccountInput: HTMLInputElement | null = null;

          for (let i = 0; i < elements.length; i++) {
            const element = elements[i];
            if (element instanceof HTMLInputElement) {
              const name = element.name;

              if (name.startsWith("account") && firstAccountInput == null) {
                firstAccountInput = element;
              }

              if (element.value !== "") {
                const value = element.value;
                if (name.startsWith("account")) {
                  const amount = Number(value) || 0;
                  if (amount > 0) {
                    total += amount;
                    rq.items.push({
                      accountId: parseInt(name.replace("account", "")),
                      amount
                    });
                  }
                } else if (name === "orderId") {
                  rq.orderId = parseInt(value);
                } else if (name === "referenceId") {
                  rq.referenceId = value;
                } else if (name === "targetPersonId") {
                  rq.targetPersonId = parseInt(value);
                } else if (name === "targetAccountId") {
                  rq.targetAccountId = parseInt(value);
                }
              }
            } else if (element instanceof HTMLTextAreaElement) {
              const name = element.name;
              if (name === "description") {
                rq.description = element.value;
              }
            }
          }

          if (total === 0) {
            firstAccountInput?.focus();
            return;
          }

          rq.amount = total;

          const result = await app.financeTransactionApi.reimburse(rq);
          if (result == null) return;

          if (result.ok) {
            navigate(`./../viewtransaction/${result.data?.id}`);
            return;
          }

          app.alertResult(result);
        }}
      >
        <Grid container spacing={2}>
          <Grid size={{ xs: 6, sm: 3, xl: 1 }}>
            <CurrencyList
              name="currency"
              label={labels.currency}
              required
              fullWidth
              value={currency}
              onItemChange={(item) => {
                if (item == null) return;
                setCurrency(item.id);
              }}
            />
          </Grid>
          <Grid size={{ xs: 6, sm: 3, xl: 1 }}>
            <UserTiplist
              name="targetPersonId"
              inputRequired
              fullWidth
              onValueChange={(value) => setTargetPersonId(value?.id)}
            />
          </Grid>
          <Grid size={{ xs: 12, sm: 6, md: 4, xl: 2 }}>
            <AccountTiplist
              name="targetAccountId"
              disabled={!currency || !targetPersonId}
              fullWidth
              onLoadData={(rq) => {
                rq.enabled = true;
                rq.personId = targetPersonId;
                rq.currency = currency;

                return rq;
              }}
            />
          </Grid>
          <Grid size={{ xs: 6, sm: 3, xl: 1 }}>
            <MoneyInputField
              fullWidth
              name="amount"
              label={labels.amount}
              value={amount}
              onChange={(e) => setAmount(Number(e.target.value))}
              required
            />
          </Grid>
          <Grid size={{ xs: 12, sm: 6, md: 4, xl: 2 }}>
            <OrderAllList
              name="orderId"
              idValue={id}
              fullWidth
              rq={{ currency }}
            />
          </Grid>
          <Grid size={{ xs: 12, sm: 6, md: 4, xl: 2 }}>
            <InputField
              fullWidth
              name="referenceId"
              slotProps={{
                htmlInput: { maxLength: 30 }
              }}
              label={labels.referenceId}
            />
          </Grid>
          <Grid size={{ xs: 12 }}>
            <Divider />
          </Grid>
          {accounts.map((a) => (
            <Grid size={{ xs: 12, sm: 6, xl: 3 }} key={a.id}>
              <AccountPaymentInput
                name={`account${a.id}`}
                label={
                  app.finance.getAccountKind(a.kind) +
                  `${a.productId ? ` (${a.productId})` : ""}`
                }
                helperText={`${a.bank} - ${a.accountNumber}`}
                onFullAmount={amount}
              />
            </Grid>
          ))}
          <Grid size={{ xs: 12 }}>
            <InputField
              fullWidth
              name="description"
              required
              slotProps={{
                htmlInput: { maxLength: 128 }
              }}
              label={labels.description}
              multiline
              rows={2}
            />
          </Grid>
          <Grid size={{ xs: 12 }}>
            <Button
              variant="contained"
              startIcon={<PaymentIcon />}
              fullWidth
              type="submit"
            >
              {labels.submit}
            </Button>
          </Grid>
        </Grid>
      </form>
    </CommonPage>
  );
}
