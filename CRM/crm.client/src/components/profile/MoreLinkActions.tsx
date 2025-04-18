import { PersonProfileLinkItem } from "@etsoo/smarterp-crm";
import { app } from "../../app/MyApp";
import { MoreFab } from "@etsoo/materialui";

type MoreLinkActionsProps = {
  link: PersonProfileLinkItem;
  callback: () => void;
  onEdit: () => void;
};

export function MoreLinkActions({
  link,
  callback,
  onEdit
}: MoreLinkActionsProps) {
  const labels = app.getLabels("actions", "delete", "edit");
  return (
    <MoreFab
      color="inherit"
      size="small"
      iconButton
      title={labels.actions}
      anchorOrigin={{
        vertical: "bottom",
        horizontal: "right"
      }}
      transformOrigin={{
        vertical: "top",
        horizontal: "right"
      }}
      actions={[
        {
          label: labels.edit,
          action: onEdit
        },
        { label: "-" },
        {
          label: labels.delete,
          action: async () => {
            const result = await app.profileApi.deleteLink(link.id);
            if (result == null) return;

            if (result.ok) {
              callback();
            } else {
              app.alertResult(result);
            }
          }
        }
      ]}
    />
  );
}
