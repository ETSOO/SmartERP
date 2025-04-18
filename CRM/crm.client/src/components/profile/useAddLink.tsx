import { DomUtils, IActionResult, Utils } from "@etsoo/shared";
import { app } from "../../app/MyApp";
import {
  PersonProfileLinkCreateRQ,
  PersonProfileLinkItem,
  PersonProfileLinkUpdateRQ
} from "@etsoo/smarterp-crm";
import { ProfileLink } from "./ProfileLink";
import React from "react";
import { EOEditorElement } from "@etsoo/reacteditor";

export function useAddLink(id: number, loadData: () => Promise<void>) {
  // Labels
  const labels = app.getLabels("addComment", "noChanges");

  // Editor ref
  const editorRef = React.useRef<EOEditorElement>(null);

  return React.useCallback(
    (data?: PersonProfileLinkItem) => {
      // Show
      app.showInputDialog({
        title: labels.addComment,
        message: "",
        callback: async (form) => {
          // Cancelled
          if (form == null) return;

          // Form data
          const { kind, targetProfileId } = DomUtils.dataAs(
            new FormData(form),
            {
              kind: "number",
              targetProfileId: "number"
            }
          );

          if (kind == null) {
            DomUtils.setFocus("kindInput", form);
            return false;
          }

          // Content
          const content = editorRef.current?.value;
          if (!content) {
            editorRef.current?.restoreFocus();
            return false;
          }

          let result: IActionResult | undefined;
          if (data == null) {
            const rq: PersonProfileLinkCreateRQ = {
              profileId: id,
              kind,
              targetProfileId,
              content
            };

            Utils.removeEmptyValues(rq);

            result = await app.profileApi.createLink(rq, {
              showLoading: false
            });
          } else {
            const rq: PersonProfileLinkUpdateRQ = {
              id: data.id,
              kind,
              targetProfileId,
              content
            };

            // Changed fields
            const fields = Utils.getDataChanges(rq, data);
            if (fields.length === 0) {
              app.warning(labels.noChanges);
              return;
            }
            rq.changedFields = fields;

            result = await app.profileApi.updateLink(rq, {
              showLoading: false
            });
          }

          if (result == null) return;

          if (result.ok) {
            editorRef.current?.clearBackup();
            loadData();
          } else {
            return app.formatResult(result);
          }
        },
        inputs: (
          <ProfileLink profileId={id} data={data} editorRef={editorRef} />
        ),
        fullScreen: true
      });
    },
    [id]
  );
}
