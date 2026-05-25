import { ReactUtils, useParamsEx, useRefs } from "@etsoo/react";
import { usePageDataEmpty } from "@etsoo/smarterp-core";
import React from "react";
import { app } from "../../../app/MyApp";
import { CustomAttributeArea, EditPage, InputField } from "@etsoo/materialui";
import Grid from "@mui/material/Grid";
import { useNavigate } from "react-router-dom";
import { OrgTiplist } from "../../../components/OrgTiplist";
import { DocumentCreateRQ } from "../../../api/rq/document/DocumentCreateRQ";
import { IdActionResult, Utils } from "@etsoo/shared";
import { DocumentUpdateRQ } from "../../../api/rq/document/DocumentUpdateRQ";
import {
  ButtonCultures,
  DocumentKindList,
  SystemDocumentTiplist
} from "@etsoo/smarterp-core/components";
import { useFormik } from "formik";

export default function AddDocument() {
  // Route
  const navigate = useNavigate();

  const { id = 0 } = useParamsEx({
    id: "number"
  });

  // Labels
  const labels = app.getLabels(
    "add",
    "deleteConfirm",
    "edit",
    "item",
    "noChanges",
    "org",
    "parameters",
    "systemTemplateGuide",
    "template",
    "title",
    "type"
  );

  // State
  const [data, setData] = React.useState<DocumentCreateRQ>({
    kind: "",
    title: "",
    template: ""
  });

  // Input refs
  const refFields = ["kind", "parameters", "template", "title"] as const;
  const refs = useRefs(refFields);

  const formik = useFormik<DocumentCreateRQ>({
    initialValues: data,
    enableReinitialize: true,
    onSubmit: async (values) => {
      // Request data
      const c = structuredClone(values);
      ReactUtils.updateRefValues(refs, c);

      if (!!c.parameters && typeof c.parameters === "string") {
        c.parameters = JSON.parse(c.parameters);
      }

      let result: IdActionResult | undefined;
      let url: string;

      if (id > 0) {
        const rq: DocumentUpdateRQ = { ...c, id };

        // Changed fields
        const fields = Utils.getDataChanges(rq, data);
        if (fields.length === 0) {
          app.warning(labels.noChanges);
          return;
        }
        rq.changedFields = fields;

        if (rq.parameters == null) {
          delete rq.parameters;
        }

        result = await app.documentApi.update(rq);

        url = "./../../";
      } else {
        const rq: DocumentCreateRQ = { ...c };

        result = await app.documentApi.create(rq);

        url = "./../";
      }

      if (result == null) return;

      if (result.ok) {
        navigate(url);
        return;
      } else {
        app.alertResult(result);
      }
    }
  });

  // Load data
  const reloadData = React.useCallback(async () => {
    if (id < 1) return;

    const result = await app.documentApi.read(id);
    if (result == null) return;

    ReactUtils.updateRefs(refs, result);

    setData(result);
  }, [id]);

  // Page data hook
  usePageDataEmpty(app);

  return (
    <EditPage
      isEditing={id > 0}
      onDelete={() => {
        app.notifier.confirm(
          labels.deleteConfirm.format(labels.item),
          undefined,
          async (ok) => {
            if (!ok) return;

            const result = await app.documentApi.delete(id, {
              showLoading: false
            });

            if (result == null) return;

            if (result.ok) {
              navigate("./../../");
              return;
            }

            app.alertResult(result);
          }
        );
      }}
      onSubmit={formik.handleSubmit}
      onUpdate={reloadData}
      paddings={0}
    >
      <Grid size={{ xs: 12, sm: 12 }}>
        <OrgTiplist
          name="orgId"
          label={labels.org}
          search={false}
          idValue={data.orgId}
          inputOnChange={formik.handleChange}
        />
      </Grid>
      <Grid size={{ xs: 6, sm: 3 }}>
        <InputField
          fullWidth
          name="kind"
          required
          slotProps={{ htmlInput: { maxLength: 20 } }}
          label={labels.type}
          inputRef={refs.kind}
        />
      </Grid>
      <Grid size={{ xs: 6, sm: 3 }}>
        <DocumentKindList
          fullWidth
          value={data.kind}
          onItemChange={(item) => {
            if (item == null || refs.kind.current == null) return;
            refs.kind.current.value = item.id;
          }}
        />
      </Grid>
      <Grid size={{ xs: 6, sm: 6 }}>
        <InputField
          fullWidth
          name="title"
          required
          slotProps={{ htmlInput: { maxLength: 128 } }}
          label={labels.title}
          inputRef={refs.title}
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 12 }}>
        <ButtonCultures
          fullWidth
          value={formik.values.cultures ?? []}
          onValueChange={(ids) => formik.setFieldValue("cultures", ids)}
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 12 }}>
        <SystemDocumentTiplist
          onLoadData={(rq) => {
            const kind = refs.kind.current?.value || "n/a";
            const culture = formik.values.cultures?.[0] || app.culture;
            return { ...rq, kind, culture };
          }}
          onValueChange={(item, _input, reason) => {
            if (item == null || reason !== "selectOption") return;

            app.notifier.confirm(
              labels.systemTemplateGuide,
              item.title,
              async (ok) => {
                if (!ok) return;

                const data = await app.documentApi.read(item.id);
                if (data == null) return;

                refs.template.current!.value = data.template;
                refs.parameters.current!.value = JSON.stringify(
                  data.parameters
                );
              }
            );
          }}
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 12 }}>
        <InputField
          fullWidth
          name="template"
          multiline
          rows={8}
          required
          label={labels.template}
          inputRef={refs.template}
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 12 }}>
        <CustomAttributeArea
          label={labels.parameters}
          name="parameters"
          inputRef={refs.parameters}
        />
      </Grid>
    </EditPage>
  );
}
