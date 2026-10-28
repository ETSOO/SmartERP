import React from "react";
import Button from "@mui/material/Button";
import { GridDataType, useParamsEx } from "@etsoo/react";
import { useNavigate } from "react-router-dom";
import {
  FinanceTransactionPayOrderRQ,
  FinanceTransactionViewOrderData,
  Permissions
} from "@etsoo/smarterp-crm";
import { app } from "../../../app/MyApp";
import {
  CommonPage,
  InputField,
  LinkEx,
  ViewContainer
} from "@etsoo/materialui";
import LinearProgress from "@mui/material/LinearProgress";
import Paper from "@mui/material/Paper";
import Grid from "@mui/material/Grid";
import PaymentIcon from "@mui/icons-material/Payment";
import { DateUtils, NumberUtils } from "@etsoo/shared";
import { AccountPaymentInput } from "../../../components/AccountPaymentInput";

export default function OrderCollect() {
  // Route
  const navigate = useNavigate();
  const { id = 0 } = useParamsEx({ id: "number" });

  // Labels
  const labels = app.getLabels(
    "amount",
    "description",
    "happenDate",
    "orderPaymentKindBankTransfer",
    "orderPaymentKindCash",
    "submit"
  );

  // State
  const [data, setData] = React.useState<FinanceTransactionViewOrderData>();

  // Ref
  const amountRef = React.useRef<HTMLElement>(null);

  // Symbol
  const symbol = data?.order.currency
    ? NumberUtils.getCurrencySymbol(data?.order.currency)
    : "";

  // Load data
  const loadData = React.useCallback(async () => {
    const data = await app.financeTransactionApi.viewOrder(id);
    setData(data);
  }, [id]);

  return (
    <CommonPage paddings={0} onRefresh={loadData}>
      {data == null ? (
        <LinearProgress />
      ) : (
        <ViewContainer
          data={data.order}
          refresh={loadData}
          fields={[
            {
              data: (item) => (
                <LinkEx
                  to={`./../../../contact/view/${item.customerId}`}
                  variant="body2"
                  disabled={!app.owns(Permissions.Customer.View)}
                >
                  {item.customerName}
                </LinkEx>
              ),
              singleRow: "large",
              label: "customer"
            },
            {
              data: (item) => (
                <LinkEx
                  to={`./../../../order/view/${item.id}`}
                  variant="body2"
                  disabled={!app.owns(Permissions.Order.View)}
                >
                  {item.title}
                </LinkEx>
              ),
              singleRow: "large",
              label: "order"
            },
            [
              "totalPayable",
              GridDataType.Money,
              { currency: data.order.currency }
            ],
            {
              data: "paidAmount",
              label: "amountPaid",
              renderProps: { currency: data.order.currency },
              dataType: GridDataType.Money
            },
            ["creation", GridDataType.DateTime],
            {
              data: "payment",
              label: "orderPayment"
            },
            {
              data: "paymentInstruction",
              singleRow: "large"
            },
            {
              data: (item) => {
                const orgAccounts = data.orgAccounts;
                const accounts = data.accounts;
                const balance = item.totalPayable - item.paidAmount;

                const handleAccountChange = (
                  e: React.ChangeEvent<HTMLInputElement, Element>
                ) => handleAccountInputChange(e.target);

                const handleAccountInputChange = (
                  sourceInput: HTMLInputElement
                ) => {
                  const form = sourceInput.form;
                  if (form == null) return;

                  let total = 0;

                  const elements = form.elements;
                  for (let i = 0; i < elements.length; i++) {
                    const element = elements[i];
                    if (
                      element instanceof HTMLInputElement &&
                      element.value !== "" &&
                      element.name.startsWith("account")
                    ) {
                      total += Number(element.value) || 0;
                    }
                  }

                  if (amountRef.current) {
                    amountRef.current.textContent = app.formatNumber(total);
                  }
                };

                return (
                  <Paper sx={{ padding: 2 }}>
                    <form
                      onSubmit={async (e) => {
                        e.preventDefault();

                        const form = e.currentTarget;

                        let total: number = 0;

                        const rq: FinanceTransactionPayOrderRQ = {
                          orderId: item.id,
                          amount: total,
                          items: []
                        };

                        const elements = form.elements;
                        for (let i = 0; i < elements.length; i++) {
                          const element = elements[i];
                          if (
                            element instanceof HTMLInputElement &&
                            element.value !== ""
                          ) {
                            const name = element.name;
                            const value = element.value;
                            if (name.startsWith("account")) {
                              const amount = Number(value) || 0;
                              if (amount > 0) {
                                total += amount;
                                rq.items.push({
                                  accountId: parseInt(
                                    name.replace("account", "")
                                  ),
                                  amount
                                });
                              }
                            } else if (name === "description") {
                              rq.description = value;
                            } else if (name === "happenDate") {
                              rq.happenDate = DateUtils.parse(value);
                            }
                          }
                        }

                        if (total === 0) {
                          (elements[0] as HTMLInputElement | null)?.focus();
                          return;
                        }

                        rq.amount = total;

                        const result =
                          await app.financeTransactionApi.payOrder(rq);
                        if (result == null) return;

                        if (result.ok) {
                          navigate(`./../../view/${id}`);
                          return;
                        }

                        app.alertResult(result);
                      }}
                    >
                      <Grid container spacing={2}>
                        {orgAccounts.map((a) => (
                          <Grid size={{ xs: 12, sm: 6, xl: 3 }} key={a.id}>
                            <AccountPaymentInput
                              name={`account${a.id}`}
                              label={
                                app.finance.getAccountKind(a.kind) +
                                `${a.productId ? ` (${a.productId})` : ""}`
                              }
                              symbol={symbol}
                              max={balance}
                              helperText={`${a.bank} - ${a.accountNumber}`}
                              onChangeDelay={handleAccountChange}
                              onFullAmount={balance}
                            />
                          </Grid>
                        ))}
                        {accounts.map((a) => (
                          <Grid size={{ xs: 12, sm: 6, xl: 3 }} key={a.id}>
                            <AccountPaymentInput
                              name={`account${a.id}`}
                              label={
                                app.finance.getAccountKind(a.kind) +
                                ` (${app.formatNumber(a.balance)}${a.productId ? `, ${a.productId}` : ""})`
                              }
                              symbol={symbol}
                              max={Math.min(balance, a.balance)}
                              helperText={`${a.bank} - ${a.accountNumber}`}
                              onChangeDelay={handleAccountChange}
                              onFullAmount={balance}
                            />
                          </Grid>
                        ))}
                        <Grid size={{ xs: 6, sm: 3, xl: 1 }}>
                          <InputField
                            fullWidth
                            name="happenDate"
                            label={labels.happenDate}
                            type="datetime-local"
                            slotProps={{ htmlInput: { step: 60 } }}
                          />
                        </Grid>
                        <Grid size={{ xs: 12, sm: 6, xl: 3 }}>
                          <InputField
                            fullWidth
                            name="description"
                            slotProps={{
                              htmlInput: { maxLength: 128 }
                            }}
                            label={labels.description}
                            multiline
                            rows={2}
                          />
                        </Grid>
                        <Grid size={{ xs: 12, sm: 6, xl: 3 }}>
                          {labels.amount}: {symbol}
                          <b ref={amountRef}>0</b>
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
                  </Paper>
                );
              },
              singleRow: true
            }
          ]}
        />
      )}
    </CommonPage>
  );
}
