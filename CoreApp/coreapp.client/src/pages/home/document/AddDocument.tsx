import {
  ReactUtils,
  useParamsEx,
  useRefs,
  useSearchParamsEx
} from "@etsoo/react";
import {
  DocumentCreateRQ,
  DocumentUpdateRQ,
  usePageData
} from "@etsoo/smarterp-core";
import React from "react";
import { app } from "../../../app/MyApp";
import {
  CustomAttributeArea,
  EditPage,
  ErrorAlert,
  InputField
} from "@etsoo/materialui";
import Grid from "@mui/material/Grid";
import { useNavigate } from "react-router-dom";
import { IdActionResult, Utils } from "@etsoo/shared";
import {
  ButtonCultures,
  DocumentKindList,
  DocumentTiplist
} from "@etsoo/smarterp-core/components";
import { useFormik } from "formik";

export default function AddDocument() {
  // Route
  const navigate = useNavigate();

  const { id = 0 } = useParamsEx({
    id: "number"
  });
  const { orgId = 0 } = useSearchParamsEx({
    orgId: "number"
  });

  // Labels
  const labels = app.getLabels(
    "add",
    "deleteConfirm",
    "documentTemplates",
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
  const refFields = ["parameters", "template", "title"] as const;
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

        result = await app.core.documentApi.update(rq);

        url = "./../..";
      } else {
        const rq: DocumentCreateRQ = { ...c };

        result = await app.core.documentApi.create(rq);

        url = "./..";
      }

      if (result == null) return;

      if (result.ok) {
        navigate(`${url}/document/${orgId}`);
        return;
      } else {
        app.alertResult(result);
      }
    }
  });

  // Load data
  const reloadData = React.useCallback(async () => {
    if (id < 1) return;

    const result = await app.core.documentApi.read(id);
    if (result == null) return;

    ReactUtils.updateRefs(refs, result);

    setData(result);
  }, [id]);

  // Page data hook
  const urlPart = id > 0 ? `./..` : `.`;
  usePageData(
    app,
    {
      breadcrumbs: (bc) => {
        bc.splice(
          bc.length - 1,
          0,
          { title: labels.org, path: `${urlPart}/my/${orgId}` },
          {
            title: labels.documentTemplates,
            path: `${urlPart}/document/${orgId}`
          }
        );
        return bc;
      }
    },
    []
  );

  if (orgId < 1) {
    return <ErrorAlert />;
  }

  return (
    <EditPage
      isEditing={id > 0}
      onDelete={() => {
        app.notifier.confirm(
          labels.deleteConfirm.format(labels.item),
          undefined,
          async (ok) => {
            if (!ok) return;

            const result = await app.core.documentApi.delete(id, {
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
      <Grid size={{ xs: 12, sm: 5 }}>
        <DocumentKindList
          fullWidth
          value={data.kind}
          onItemChange={(item, useAction) => {
            if (useAction) formik.setFieldValue("kind", item?.id);
          }}
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 7 }}>
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
        <DocumentTiplist
          kind={formik.values.kind}
          onLoadData={(rq) => {
            const culture = formik.values.cultures?.[0];
            return { ...rq, culture, isSystem: true };
          }}
          onValueChange={(item, _input, reason) => {
            if (item == null || reason !== "selectOption") return;

            app.notifier.confirm(
              labels.systemTemplateGuide,
              item.title,
              async (ok) => {
                if (!ok) return;

                const data = await app.core.documentApi.read(item.id);
                if (data == null) return;

                if ((formik.values.cultures ?? []).length === 0) {
                  formik.setFieldValue("cultures", data.cultures);
                }

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
