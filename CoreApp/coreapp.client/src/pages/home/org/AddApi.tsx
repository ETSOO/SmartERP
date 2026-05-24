import {
  ReactUtils,
  useParamsEx,
  useRefs,
  useSearchParamsEx
} from "@etsoo/react";
import {
  CoreApiService,
  CoreUtils,
  OrgCreateApiRQ,
  OrgUpdateApiRQ,
  usePageData
} from "@etsoo/smarterp-core";
import React from "react";
import { app } from "../../../app/MyApp";
import {
  EditPage,
  ErrorAlert,
  InputField,
  JsonDataInput,
  OptionBool
} from "@etsoo/materialui";
import Grid from "@mui/material/Grid";
import { useNavigate } from "react-router-dom";
import { useFormik } from "formik";
import { ButtonApiServices } from "@etsoo/smarterp-core/components";
import { DomUtils, IdActionResult, Utils } from "@etsoo/shared";

export default function AddApi() {
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
    "addApi",
    "allowInheritance",
    "appApiUrl",
    "appId",
    "appSecret",
    "edit",
    "enabled",
    "endpoint",
    "externalApis",
    "noChanges",
    "options",
    "org",
    "ratePolicy",
    "title"
  );

  const isEditing = id > 0;

  // State
  const [data, setData] = React.useState<OrgCreateApiRQ>({
    service: CoreApiService.SMTP,
    title: "",
    endpoint: "",
    appId: "",
    appSecret: "",
    enabled: true,
    inheritance: true
  });

  const [schemaError, setSchemaError] = React.useState<string | null>(null);

  // Formik
  const formik = useFormik<OrgCreateApiRQ>({
    initialValues: data,
    enableReinitialize: true,
    onSubmit: async (values) => {
      // Get updated values
      const v = { ...values };
      ReactUtils.updateRefValues(refs, v);

      // JSON data
      if (v.options) {
        try {
          v.options = JSON.stringify(JSON.parse(v.options));

          if (schemaRef.current) {
            const [valid, errors] = await CoreUtils.validateJson(
              schemaRef.current,
              v.options ?? "{}"
            );

            if (!valid && errors?.length) {
              setSchemaError(
                errors
                  .map(
                    (e) =>
                      `${e.instancePath ? `${e.instancePath}: ` : ""}${
                        e.message
                      }`
                  )
                  .join("; ")
              );
              DomUtils.setFocus("options");
              return;
            } else {
              setSchemaError(null);
            }
          }
        } catch (e) {
          console.log("JSON parse error", e);
          DomUtils.setFocus("options");
          return;
        }
      }

      // Submit
      let result: IdActionResult | undefined;
      let redirectUrl: string;
      if (id > 0) {
        const rq: OrgUpdateApiRQ = {
          ...v,
          id
        };

        // Changed fields
        const fields = Utils.getDataChanges(rq, data);
        if (fields.length === 0) {
          app.warning(labels.noChanges);
          return;
        }
        rq.changedFields = fields;

        redirectUrl = "./../..";

        result = await app.core.orgApi.updateApi(rq);
      } else {
        const rq: OrgCreateApiRQ = {
          ...v
        };

        Utils.removeEmptyValues(rq);

        redirectUrl = "./..";

        result = await app.core.orgApi.createApi(rq);
      }

      if (result == null) return;

      if (result.ok) {
        navigate(`${redirectUrl}/apis/${orgId}`);
        return;
      }

      app.alertResult(result);
    }
  });

  // Schema ref
  const schemaRef = React.useRef<object>(undefined);

  // Input refs
  const refFields = [
    "appId",
    "appSecret",
    "endpoint",
    "options",
    "ratePolicy",
    "title"
  ] as const;
  const refs = useRefs(refFields);

  // Load data
  const reloadData = React.useCallback(async () => {
    if (id < 1) return;
    const result = await app.core.orgApi.updateApiRead(id);
    if (result == null) return;

    // Format JSON
    result.options =
      result.options == null ? "" : JSON.stringify(result.options);

    ReactUtils.updateRefs(refs, result);
    setData(result);

    changeService(result.service);
  }, [id]);

  // Change service
  const changeService = React.useCallback((service: CoreApiService) => {
    app.core.orgApi
      .readApiSchema(service)
      .then((schema) => (schemaRef.current = schema));
  }, []);

  // Page data hook
  const urlPart = isEditing ? `./..` : `.`;
  usePageData(
    app,
    {
      breadcrumbs: (bc) => {
        bc.splice(
          bc.length - 1,
          0,
          { title: labels.org, path: `${urlPart}/my/${orgId}` },
          { title: labels.externalApis, path: `${urlPart}/apis/${orgId}` }
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
      isEditing={isEditing}
      onSubmit={formik.handleSubmit}
      onUpdate={reloadData}
      paddings={0}
    >
      <Grid size={{ xs: 12, sm: 8 }}>
        <ButtonApiServices
          fullWidth
          required
          value={formik.values.service}
          onValueChange={(value) => {
            formik.setFieldValue("service", value);
            changeService(value);
          }}
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 4 }}>
        <OptionBool
          name="enabled"
          fullWidth
          label={labels.enabled}
          defaultValue={formik.values.enabled}
          onChange={formik.handleChange}
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 12 }}>
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
        <InputField
          fullWidth
          name="endpoint"
          required
          slotProps={{ htmlInput: { type: "url", maxLength: 256 } }}
          label={labels.appApiUrl}
          inputRef={refs.endpoint}
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 6 }}>
        <InputField
          fullWidth
          name="appId"
          required
          slotProps={{ htmlInput: { maxLength: 64 } }}
          label={labels.appId}
          inputRef={refs.appId}
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 6 }}>
        <InputField
          fullWidth
          name="appSecret"
          required
          slotProps={{ htmlInput: { maxLength: 128 } }}
          label={labels.appSecret}
          inputRef={refs.appSecret}
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 12 }}>
        <JsonDataInput
          rows={5}
          fullWidth
          name="options"
          label={labels.options}
          inputRef={refs.options}
          error={!!schemaError}
          helperText={schemaError}
          onChange={() => setSchemaError(null)}
        />
      </Grid>
      <Grid size={{ xs: 6, sm: 3 }}>
        <InputField
          fullWidth
          name="ratePolicy"
          type="number"
          inputMode="numeric"
          label={labels.ratePolicy}
          inputRef={refs.ratePolicy}
        />
      </Grid>
      <Grid size={{ xs: 6, sm: 3 }}>
        <OptionBool
          name="inheritance"
          fullWidth
          label={labels.allowInheritance}
          defaultValue={formik.values.inheritance}
          onChange={formik.handleChange}
        />
      </Grid>
    </EditPage>
  );
}
