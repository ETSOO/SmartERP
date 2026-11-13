import {
  HBox,
  InputField,
  MoneyInputField,
  NumberInputField,
  VBox
} from "@etsoo/materialui";
import { app } from "../../../app/MyApp";
import { DomUtils } from "@etsoo/shared";
import {
  FinanceAccountKind,
  FinanceAccountViewData
} from "@etsoo/smarterp-crm";
import { AccountTiplist } from "@etsoo/smarterp-crm/components";
import { CurrencyList } from "../../../components/CurrencyList";
import React from "react";
import { useRefs } from "@etsoo/react";

function AccountExchangeUI({ data }: { data: FinanceAccountViewData }) {
  // Labels
  const labels = app.getLabels(
    "amount",
    "description",
    "exchangeAmount",
    "exchangeCurrency",
    "exchangeFee",
    "exchangeRate",
    "referenceId",
    "sourceAccount"
  );

  // Destination account kinds
  const kind = data.kind;
  const kinds = [kind];
  if (kind === FinanceAccountKind.Super) {
    kinds.push(FinanceAccountKind.Cash);
  }

  // Input refs
  const refFields = [
    "amount",
    "exchangeAmount",
    "exchangeFee",
    "exchangeRate"
  ] as const;
  const refs = useRefs(refFields);

  // State
  const [currency, setCurrency] = React.useState<string>();

  const calculateAmount = () => {
    const exchangeAmountText = refs.exchangeAmount.current?.value;
    if (!exchangeAmountText) return;
    const exchangeAmount = parseFloat(exchangeAmountText);
    if (exchangeAmount <= 0) return;

    const exchangeRateText = refs.exchangeRate.current?.value;
    if (!exchangeRateText) return;
    const exchangeRate = parseFloat(exchangeRateText);
    if (exchangeRate <= 0) return;

    const exchangeFeeText = refs.exchangeFee.current?.value;
    const exchangeFee = exchangeFeeText ? parseFloat(exchangeFeeText) : 0;

    const amount = exchangeAmount * exchangeRate + exchangeFee;
    if (refs.amount.current) {
      refs.amount.current.value = amount.toFixed(2);
    }
  };

  // Layout
  return (
    <VBox spacing={2} sx={{ paddingTop: 1 }}>
      <HBox spacing={1}>
        <CurrencyList
          name="currency"
          label={labels.exchangeCurrency}
          required
          filterCurrencies={(currencies) => {
            return currencies.filter((c) => c !== data.currency);
          }}
          onItemChange={(item) => setCurrency(item?.id)}
        />
        <MoneyInputField
          fullWidth
          name="exchangeAmount"
          label={labels.exchangeAmount}
          inputRef={refs.exchangeAmount}
          onChange={calculateAmount}
          required
        />
      </HBox>
      <AccountTiplist
        name="destinationAccountId"
        disabled={!currency}
        inputRequired
        fullWidth
        onLoadData={(rq) => {
          rq.enabled = true;
          rq.personId = data.personId;
          rq.currency = currency;
          rq.kinds = kinds;

          return rq;
        }}
      />
      <InputField
        fullWidth
        name="sourceAccount"
        label={labels.sourceAccount}
        value={data.accountNumber}
        disabled
      />
      <HBox spacing={1}>
        <NumberInputField
          fullWidth
          name="exchangeRate"
          label={labels.exchangeRate}
          min={0.0001}
          step={0.0001}
          inputRef={refs.exchangeRate}
          onChange={calculateAmount}
          required
        />
        <MoneyInputField
          fullWidth
          name="exchangeFee"
          label={labels.exchangeFee}
          inputRef={refs.exchangeFee}
          onChange={calculateAmount}
        />
      </HBox>
      <HBox spacing={1}>
        <MoneyInputField
          fullWidth
          name="amount"
          label={labels.amount}
          inputRef={refs.amount}
          required
          max={data.balance}
        />
        <InputField
          fullWidth
          name="referenceId"
          slotProps={{
            htmlInput: { maxLength: 30 }
          }}
          label={labels.referenceId}
        />
      </HBox>
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
    </VBox>
  );
}

export function AccountExchange(
  data: FinanceAccountViewData,
  title: string,
  refresh: () => void
) {
  app.showInputDialog({
    title,
    message: "",
    fullScreen: app.smDown,
    callback: async (form) => {
      // Cancelled
      if (form == null) {
        return;
      }

      // Validate form
      if (!form.reportValidity()) {
        return false;
      }

      // Form data
      const {
        amount,
        currency,
        description,
        destinationAccountId,
        exchangeRate,
        exchangeFee = 0,
        exchangeAmount,
        referenceId
      } = DomUtils.dataAs(new FormData(form), {
        amount: "number",
        currency: "string",
        description: "string",
        destinationAccountId: "number",
        exchangeRate: "number",
        exchangeFee: "number",
        exchangeAmount: "number",
        referenceId: "string"
      });

      if (!currency || currency === data.currency) {
        DomUtils.setFocus("currency", form);
        return false;
      }

      if (
        exchangeAmount == null ||
        isNaN(exchangeAmount) ||
        exchangeAmount <= 0
      ) {
        DomUtils.setFocus("exchangeAmount", form);
        return false;
      }

      if (exchangeRate == null || isNaN(exchangeRate) || exchangeRate <= 0) {
        DomUtils.setFocus("exchangeRate", form);
        return false;
      }

      if (
        amount == null ||
        isNaN(amount) ||
        amount <= 0 ||
        Math.abs(exchangeAmount * exchangeRate + exchangeFee - amount) > 10
      ) {
        DomUtils.setFocus("amount", form);
        return false;
      }

      if (!destinationAccountId) {
        return false;
      }

      const result = await app.financeTransactionApi.currencyExchange({
        amount,
        currency,
        description,
        destinationAccountId,
        exchangeRate,
        exchangeFee,
        exchangeAmount,
        referenceId,
        sourceAccountId: data.id
      });

      if (result == null) return;

      if (!result.ok) {
        return app.formatResult(result);
      }

      refresh();
      return true;
    },
    inputs: <AccountExchangeUI data={data} />
  });
}
