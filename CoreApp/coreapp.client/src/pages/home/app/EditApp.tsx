import { ComboBox, EditPage, InputField } from "@etsoo/materialui";
import React from "react";
import { useFormik } from "formik";
import * as Yup from "yup";
import { Utils } from "@etsoo/shared";
import { EntityStatus } from "@etsoo/appscript";
import { useNavigate } from "react-router-dom";
import { ReactUtils, useParamsEx, useRefs } from "@etsoo/react";
import { app } from "../../../app/MyApp";
import {
  AppUpdateReadDto,
  AppUpdateRQ,
  usePageData
} from "@etsoo/smarterp-core";
import Grid from "@mui/material/Grid";

export default function EditApp() {
  // Route
  const navigate = useNavigate();
  const { id = 0 } = useParamsEx({ id: "number" });

  // Labels
  const labels = app.getLabels(
    "edit",
    "fullName",
    "localName",
    "localUrls",
    "localUrlsHelp",
    "noChanges",
    "status"
  );

  // Input refs
  const refFields = ["localName", "localUrls"] as const;
  const refs = useRefs(refFields);

  // Form validation schema
  const validationSchema = Yup.object({
    name: Yup.string().required()
  });

  // Edit data
  const [data, setData] = React.useState<AppUpdateReadDto>({
    id,
    name: "",
    status: EntityStatus.Normal
  });

  // Formik
  // https://formik.org/docs/examples/with-material-ui
  // https://firxworx.com/blog/coding/react/integrating-formik-with-react-material-ui-and-typescript/
  const formik = useFormik<AppUpdateRQ>({
    initialValues: data,
    enableReinitialize: true,
    validationSchema: validationSchema,
    onSubmit: async (values) => {
      // Request data
      const rq = { ...values };

      ReactUtils.updateRefValues(refs, rq);

      if (typeof rq.localUrls === "string") {
        try {
          if (rq.localUrls === "") {
            delete rq.localUrls;
          } else {
            const items = JSON.parse(rq.localUrls);
            if (
              Array.isArray(items) &&
              !items.some((item) => !item.web || !item.api)
            ) {
              rq.localUrls = items;
            } else {
              refs.localUrls.current?.focus();
              return;
            }
          }
        } catch (error) {
          refs.localUrls.current?.focus();
          return;
        }
      }

      // Changed fields
      const fields = Utils.getDataChanges(rq, data);
      if (fields.length === 0) {
        app.warning(labels.noChanges);
        return;
      }
      rq.changedFields = fields;

      // Submit
      const result = await app.core.appApi.update(rq);
      if (result == null) return;

      if (result.ok) {
        navigate(`./../../my/${id}`);
        return;
      }

      app.alertResult(result);
    }
  });

  // Load data
  const reloadData = React.useCallback(async () => {
    const data = await app.core.appApi.updateRead(id);
    if (data == null) return;
    ReactUtils.updateRefs(refs, data, (item, value) => {
      if (item.name === "localUrls" && value != null) {
        item.value = JSON.stringify(value);
      } else {
        item.value = value == null ? "" : `${value}`;
      }
    });
    setData(data);
  }, [id]);

  // Page data hook
  usePageData(app, labels.edit, []);

  return (
    <EditPage
      isEditing
      onSubmit={(event) => {
        formik.handleSubmit(event);
      }}
      onUpdate={reloadData}
    >
      <Grid size={{ xs: 12, sm: 6 }}>
        <InputField
          fullWidth
          name="localName"
          slotProps={{ htmlInput: { maxLength: 128 } }}
          label={labels.localName}
          inputRef={refs.localName}
          helperText={`${labels.fullName}: ${data.name}`}
        />
      </Grid>
      <Grid size={{ xs: 6, sm: 6 }}>
        <ComboBox
          name="status"
          label={labels.status}
          inputRequired
          idValue={formik.values.status}
          inputOnChange={formik.handleChange}
          options={app.getStatusList()}
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 12 }}>
        <InputField
          fullWidth
          name="localUrls"
          label={labels.localUrls}
          inputRef={refs.localUrls}
          multiline
          rows={5}
          helperText={labels.localUrlsHelp}
        />
      </Grid>
    </EditPage>
  );
}
