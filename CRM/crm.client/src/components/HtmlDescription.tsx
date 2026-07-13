import {
  HtmlDescriptionBase,
  HtmlDescriptionBaseProps
} from "@etsoo/materialui";
import { app } from "../app/MyApp";
import { EOEditorEx, EOEditorElement } from "@etsoo/reacteditor";
import React from "react";
import { DomUtils } from "@etsoo/shared";

export function HtmlDescription(
  props: Omit<HtmlDescriptionBaseProps, "onEdit">
) {
  const editorRef = React.useRef<EOEditorElement>(null);

  return (
    <HtmlDescriptionBase
      onEdit={(input) => {
        app.showInputDialog({
          title: app.get("edit"),
          message: "",
          callback: (form) => {
            if (form == null) {
              return;
            }

            const editor = editorRef.current;
            if (editor == null) return;

            const content = editor.content;
            if (!content) {
              editor.focus();
              return false;
            }

            input.value = DomUtils.trimTagPairs(content, ["p", "div"]);

            return true;
          },
          fullScreen: app.smDown,
          inputs: (
            <EOEditorEx
              name="description"
              ref={(editor) => {
                if (editor == null) return;
                editorRef.current = editor;
                editor.value = input.value;
              }}
              height={360}
              language={app.culture}
            />
          )
        });
      }}
      {...props}
    />
  );
}
