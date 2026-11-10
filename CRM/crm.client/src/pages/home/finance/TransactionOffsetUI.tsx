import { InputField, OptionBool, VBox } from "@etsoo/materialui";
import { app } from "../../../app/MyApp";
import { DomUtils } from "@etsoo/shared";

export function TransactionOffset(
  id: number,
  title: string,
  refresh: () => void
) {
  const labels = app.getLabels("all", "description");
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
      const { all, description } = DomUtils.dataAs(new FormData(form), {
        all: "boolean",
        description: "string"
      });

      if (!description) {
        return false;
      }

      const result = await app.financeTransactionApi.offset({
        id,
        all,
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
        <OptionBool name="all" label={labels.all} />
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
