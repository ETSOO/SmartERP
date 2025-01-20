import { ComboBox, EditPage, InputField } from "@etsoo/materialui";
import { Grid2 } from "@mui/material";
import React from "react";
import { useFormik } from "formik";
import * as Yup from "yup";
import { DateUtils, Utils } from "@etsoo/shared";
import { EntityStatus, UserRole } from "@etsoo/appscript";
import { useNavigate } from "react-router-dom";
import { useParamsEx } from "@etsoo/react";
import { app } from "../../../app/MyApp";
import {
  MemberUpdateReadDto,
  MemberUpdateRQ,
  usePageData
} from "@etsoo/smarterp-core";

export default function EditMember() {
  // Route
  const navigate = useNavigate();
  const { id = 0 } = useParamsEx({ id: "number" });

  // Permissions
  const deletePermission = app.isAdminUser();

  // Labels
  const labels = app.getLabels(
    "assignedId",
    "deleteConfirm",
    "edit",
    "expiry",
    "fullName",
    "noChanges",
    "preferredName",
    "role",
    "status"
  );

  // Form validation schema
  const validationSchema = Yup.object({
    name: Yup.string().required()
  });

  // Edit data
  const [data, setData] = React.useState<MemberUpdateReadDto>({
    id,
    name: "",
    isSelf: false,
    userRole: UserRole.User,
    status: EntityStatus.Normal
  });

  // Is not self
  const isNotSelf = !data.isSelf && data.userRole < app.userData!.role;

  // Formik
  // https://formik.org/docs/examples/with-material-ui
  // https://firxworx.com/blog/coding/react/integrating-formik-with-react-material-ui-and-typescript/
  const formik = useFormik<MemberUpdateRQ>({
    initialValues: data,
    enableReinitialize: true,
    validationSchema: validationSchema,
    onSubmit: async (values) => {
      // Request data
      const rq = { ...values };

      // Changed fields
      const fields = Utils.getDataChanges(
        rq,
        data,
        isNotSelf
          ? ["id", "isSelf", "name"]
          : ["id", "isSelf", "name", "expiry", "status", "userRole"]
      );
      if (fields.length === 0) {
        app.warning(labels.noChanges);
        return;
      }
      rq.changedFields = fields;

      // Submit
      const result = await app.core.memberApi.update(rq);
      if (result == null) return;

      if (result.ok) {
        navigate(`./../../view/${id}`);
        return;
      }

      app.alertResult(result);
    }
  });

  // Load data
  const reloadData = React.useCallback(async () => {
    const data = await app.core.memberApi.updateRead(id);
    if (data == null) return;
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
      onDelete={
        data.status === EntityStatus.Deleted &&
        data.userRole < app.userData!.role &&
        deletePermission
          ? () => {
              app.notifier.confirm(
                labels.deleteConfirm.format(
                  ` [${data.localName ?? data.name}] `
                ),
                undefined,
                async (ok) => {
                  const id = formik.values.id;
                  if (!ok || id == null) return;

                  const result = await app.core.memberApi.delete(id, {
                    showLoading: false
                  });
                  if (result == null) return;

                  if (result.ok) {
                    navigate("./../../all");
                    return;
                  }

                  app.alertResult(result);
                }
              );
            }
          : undefined
      }
    >
      <Grid2 size={{ xs: 12, sm: 6 }}>
        <InputField
          fullWidth
          name="localName"
          slotProps={{ htmlInput: { maxLength: 128 } }}
          label={labels.preferredName}
          value={formik.values.localName ?? ""}
          onChange={formik.handleChange}
          helperText={`${labels.fullName}: ${data.name}`}
        />
      </Grid2>
      <Grid2 size={{ xs: 12, sm: 6 }}>
        <InputField
          fullWidth
          name="assignedId"
          slotProps={{
            htmlInput: { maxLength: 20, style: { textTransform: "uppercase" } }
          }}
          label={labels.assignedId}
          value={formik.values.assignedId ?? ""}
          onChange={formik.handleChange}
        />
      </Grid2>
      {isNotSelf && data.name !== "" && (
        <React.Fragment>
          <Grid2 size={{ xs: 6, sm: 3 }}>
            <ComboBox
              name="userRole"
              label={labels.role}
              inputRequired
              idValue={formik.values.userRole}
              inputOnChange={formik.handleChange}
              options={app.getRoles()}
            />
          </Grid2>
          <Grid2 size={{ xs: 6, sm: 3 }}>
            <InputField
              fullWidth
              name="expiry"
              type="date"
              label={labels.expiry}
              value={DateUtils.formatForInput(formik.values.expiry) ?? ""}
              onChange={formik.handleChange}
            />
          </Grid2>
          <Grid2 size={{ xs: 6, sm: 3 }}>
            <ComboBox
              name="status"
              label={labels.status}
              inputRequired
              idValue={formik.values.status}
              inputOnChange={formik.handleChange}
              options={app.getStatusList()}
            />
          </Grid2>
        </React.Fragment>
      )}
    </EditPage>
  );
}
