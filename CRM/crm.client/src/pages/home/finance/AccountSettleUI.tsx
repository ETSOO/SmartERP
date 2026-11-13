import { Typography } from "@mui/material";
import { app } from "../../../app/MyApp";
import { VBox } from "@etsoo/materialui";

export function AccountSettle(title: string, refresh: () => void) {
  const labels = app.getLabels("settleTip");
  app.showInputDialog({
    title,
    message: "",
    callback: async (form) => {
      // Cancelled
      if (form == null) {
        return;
      }

      const result = await app.financeTransactionApi.settle();

      if (result == null) return;

      if (!result.ok) {
        return app.formatResult(result);
      }

      refresh();
      return true;
    },
    inputs: (
      <VBox spacing={2} sx={{ paddingTop: 1 }}>
        <Typography>{labels.settleTip}</Typography>
      </VBox>
    )
  });
}
