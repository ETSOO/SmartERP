import { usePageData } from "@etsoo/smarterp-core";
import { app } from "../../../app/MyApp";
import { ComboBox, EditPage, InputField } from "@etsoo/materialui";
import { useParamsEx } from "@etsoo/react";
import { useFormik } from "formik";
import React from "react";
import Grid from "@mui/material/Grid";
import {
  StatusList,
  UserRoleList,
  UserTiplist
} from "@etsoo/smarterp-core/components";
import { PersonsList } from "@etsoo/smarterp-crm/components";
import { DateUtils, DomUtils, IdActionResult, Utils } from "@etsoo/shared";
import { EOEditorElement, EOEditorEx } from "@etsoo/reacteditor";
import {
  PersonProfileUpdateReadData,
  PersonProfileUpdateRQ,
  PersonTaskCreateRQ
} from "@etsoo/smarterp-crm";
import { EntityStatus } from "@etsoo/appscript";
import { useNavigate } from "react-router-dom";

export default function AddTask() {
  // Route
  const navigate = useNavigate();
  const { id = 0 } = useParamsEx({ id: "number" });
  const isEditing = id > 0;

  // Labels
  const labels = app.getLabels(
    "assignee",
    "comment",
    "dateTo",
    "happenDate",
    "importance",
    "newTask",
    "noChanges",
    "otherParticipants",
    "profileRole",
    "profileTitle",
    "status"
  );

  // Type
  type DataType = Omit<PersonProfileUpdateReadData, "id" | "personId" | "kind">;

  // State
  const [data, setData] = React.useState<DataType>({
    status: EntityStatus.Normal,
    title: "",
    comment: "",
    happenDate: app.profile.getFutureDate()
  });

  // Refs
  const editorRef = React.useRef<EOEditorElement>(null);

  // Formik
  const formik = useFormik<DataType>({
    initialValues: data,
    enableReinitialize: true,
    validateOnChange: false,
    onSubmit: async (v) => {
      // Content
      const content = editorRef.current?.value;
      if (!content) {
        editorRef.current?.restoreFocus();
        return;
      }

      // Request data
      const { persons, ...rest } = v;

      // Validate happenDate
      if (v.happenDate && v.happenDateEnd && v.happenDateEnd <= v.happenDate) {
        DomUtils.setFocus("happenDateEnd");
        return;
      }

      // Auth
      var auth = app.getTokenAuthRQ();

      // Submit
      let result: IdActionResult | undefined;
      if (id > 0) {
        const rq: PersonProfileUpdateRQ = {
          ...rest,
          id,
          comment: content,
          persons: persons?.map((p) => p.id)
        };

        // Changed fields
        const fields = Utils.getDataChanges(rq, data);
        if (fields.length === 0) {
          app.warning(labels.noChanges);
          return;
        }
        rq.changedFields = fields;
        rq.auth = auth;

        result = await app.profileApi.update(rq);
      } else {
        const rq: PersonTaskCreateRQ = {
          ...rest,
          comment: content,
          persons: persons?.map((p) => p.id),
          auth
        };

        Utils.removeEmptyValues(rq);

        result = await app.profileApi.createTask(rq);
      }

      if (result == null) return;

      if (result.ok) {
        editorRef.current?.clearBackup();
        navigate(`./../../../`);
        return;
      }

      app.alertResult(result);
    }
  });

  // Load data
  const reloadData = React.useCallback(async () => {
    if (id < 1) return;
    const result = await app.profileApi.updateRead(id);
    if (result == null) return;
    setData(result);
  }, [id]);

  // Page data hook
  usePageData(app, labels.newTask, []);

  return (
    <EditPage
      isEditing={isEditing}
      onSubmit={(event) => {
        formik.handleSubmit(event);
      }}
      onUpdate={reloadData}
      paddings={0}
    >
      <Grid size={{ xs: 12, sm: 12 }}>
        <InputField
          fullWidth
          required
          name="title"
          slotProps={{ htmlInput: { maxLength: 256 } }}
          label={labels.profileTitle}
          value={formik.values.title ?? ""}
          onChange={formik.handleChange}
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 4 }}>
        <InputField
          fullWidth
          required
          name="happenDate"
          label={labels.happenDate}
          type="datetime-local"
          slotProps={{ htmlInput: { step: 60 } }}
          value={DateUtils.formatForInput(formik.values.happenDate, true) ?? ""}
          onChange={formik.handleChange}
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 4 }}>
        <InputField
          fullWidth
          name="happenDateEnd"
          label={labels.dateTo}
          type="datetime-local"
          slotProps={{ htmlInput: { step: 60 } }}
          value={
            DateUtils.formatForInput(formik.values.happenDateEnd, true) ?? ""
          }
          onChange={formik.handleChange}
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 4 }}>
        <StatusList
          inputRequired
          idValue={formik.values.status}
          inputOnChange={formik.handleChange}
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 4 }}>
        <UserRoleList
          label={labels.profileRole}
          idValue={formik.values.userRole}
          inputOnChange={formik.handleChange}
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 4 }}>
        <UserTiplist
          name="assigneeId"
          label={labels.assignee}
          idValue={data.assigneeId}
          inputOnChange={formik.handleChange}
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 4 }}>
        <ComboBox
          name="importance"
          label={labels.importance}
          idValue={formik.values.importance}
          inputOnChange={formik.handleChange}
          options={app.profile.getImportances()}
          fullWidth
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 12 }}>
        <PersonsList
          label={labels.otherParticipants}
          onChange={(_event, value) => formik.setFieldValue("persons", value)}
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 12 }}>
        <EOEditorEx
          ref={editorRef}
          content={formik.values.comment ?? ""}
          backupKey={`profile-task-${id}`}
          language={app.culture}
        />
      </Grid>
    </EditPage>
  );
}
