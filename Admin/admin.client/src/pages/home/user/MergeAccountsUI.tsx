import { InputField, VBox } from "@etsoo/materialui";
import { ReadUserDto } from "../../../api/dto/query/ReadUserDto";
import { app } from "../../../app/MyApp";
import { UserTiplist } from "../../../components/UserTiplist";
import { DomUtils } from "@etsoo/shared";

function MergeAccountsUI({ data }: { data: ReadUserDto }) {
  const labels = app.getLabels("description", "removedUser", "retainedUser");

  return (
    <VBox sx={{ justifyContent: "center", flexWrap: "wrap", gap: 2 }}>
      <InputField
        fullWidth
        required
        label={labels.retainedUser}
        disabled
        defaultValue={`${data.name}(${data.id})`}
      />
      <UserTiplist
        name="mergedUserId"
        inputRequired
        fullWidth
        search={false}
        rq={{ excludedIds: [data.id], excludeSelf: true }}
        label={labels.removedUser}
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
  );
}

export function MergeAccounts(data: ReadUserDto, title: string) {
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
      const { mergedUserId, description } = DomUtils.dataAs(
        new FormData(form),
        {
          mergedUserId: "number",
          description: "string"
        }
      );

      if (!mergedUserId || !description) {
        return false;
      }

      const result = await app.adminApi.mergeAccounts({
        id: data.id,
        mergedUserId,
        description
      });

      if (result == null) return;

      if (!result.ok) {
        return app.formatResult(result);
      }

      app.notifier.succeed(app.get("operationSucceeded"));

      return true;
    },
    inputs: <MergeAccountsUI data={data} />
  });
}
