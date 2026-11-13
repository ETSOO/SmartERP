import { DomUtils } from "@etsoo/shared";
import { app } from "../../../app/MyApp";
import { InputField, MoneyInputField, VBox } from "@etsoo/materialui";

export function AccountAdjust(id: number, title: string, refresh: () => void) {
  const labels = app.getLabels("amount", "description");
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
      const { amount, description } = DomUtils.dataAs(new FormData(form), {
        amount: "number",
        description: "string"
      });

      if (amount == null || isNaN(amount) || amount == 0) {
        DomUtils.setFocus("amount", form);
        return false;
      }

      if (!description) {
        return false;
      }

      const result = await app.financeTransactionApi.adjust({
        accountId: id,
        amount,
        description
      });

      if (result == null) return;

      if (!result.ok) {
        return app.formatResult(result);
      }

      refresh();
      return true;
    },
    inputs: (
      <VBox spacing={2} sx={{ paddingTop: 1 }}>
        <MoneyInputField
          fullWidth
          name="amount"
          label={labels.amount}
          required
          min={-999999999}
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
      </VBox>
    )
  });
}
