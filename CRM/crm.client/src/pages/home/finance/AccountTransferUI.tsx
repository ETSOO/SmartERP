import { InputField, MoneyInputField, VBox } from "@etsoo/materialui";
import { app } from "../../../app/MyApp";
import { DomUtils } from "@etsoo/shared";
import {
  FinanceAccountKind,
  FinanceAccountListRQ,
  FinanceAccountViewData
} from "@etsoo/smarterp-crm";
import { AccountTiplist } from "@etsoo/smarterp-crm/components";

function AccountTransferUI({ data }: { data: FinanceAccountViewData }) {
  // Labels
  const labels = app.getLabels(
    "amount",
    "description",
    "referenceId",
    "sourceAccount"
  );

  // Destination account kinds
  const kind = data.kind;
  const kinds = [kind];
  if (kind === FinanceAccountKind.Super) {
    kinds.push(FinanceAccountKind.Cash);
  }

  const rq: Partial<FinanceAccountListRQ> = {
    enabled: true,
    excludedIds: [data.id],
    personId: data.personId,
    currency: data.currency,
    kinds
  };

  // Layout
  return (
    <VBox spacing={2} sx={{ paddingTop: 1 }}>
      <InputField
        fullWidth
        name="sourceAccount"
        label={labels.sourceAccount}
        value={data.accountNumber}
        disabled
      />
      <AccountTiplist fullWidth name="destinationAccountId" rq={rq} />
      <MoneyInputField
        fullWidth
        name="amount"
        label={labels.amount}
        required
        max={data.balance}
      />
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
      <InputField
        fullWidth
        name="referenceId"
        slotProps={{
          htmlInput: { maxLength: 30 }
        }}
        label={labels.referenceId}
      />
    </VBox>
  );
}

export function AccountTransfer(
  data: FinanceAccountViewData,
  title: string,
  refresh: () => void
) {
  app.showInputDialog({
    title,
    message: "",
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
      const { amount, description, destinationAccountId, referenceId } =
        DomUtils.dataAs(new FormData(form), {
          amount: "number",
          description: "string",
          destinationAccountId: "number",
          referenceId: "string"
        });

      if (amount == null || isNaN(amount) || amount <= 0) {
        DomUtils.setFocus("amount", form);
        return false;
      }

      if (!description || !destinationAccountId) {
        return false;
      }

      const result = await app.financeTransactionApi.transfer({
        amount,
        description,
        destinationAccountId,
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
    inputs: <AccountTransferUI data={data} />
  });
}
