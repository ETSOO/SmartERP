import { ComboBox, EditPage, InputField } from "@etsoo/materialui";
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
  usePageDataEmpty
} from "@etsoo/smarterp-core";
import { StatusList, UserTiplist } from "@etsoo/smarterp-core/components";
import Grid from "@mui/material/Grid";

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
    "expiry",
    "fullName",
    "noChanges",
    "preferredName",
    "reportTo",
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
  const isNotSelf = !data.isSelf && data.userRole <= app.userData!.role;

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
          ? ["id", "localName", "changedFields"]
          : [
              "id",
              "localName",
              "changedFields",
              "expiry",
              "status",
              "reportTo",
              "userRole"
            ]
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
  usePageDataEmpty(app);

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
      <Grid size={{ xs: 12, sm: 6 }}>
        <InputField
          fullWidth
          name="localName"
          slotProps={{ htmlInput: { maxLength: 128 } }}
          label={labels.preferredName}
          value={formik.values.localName ?? ""}
          onChange={formik.handleChange}
          helperText={`${labels.fullName}: ${data.name}`}
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 6 }}>
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
      </Grid>
      {isNotSelf && data.name !== "" && (
        <React.Fragment>
          <Grid size={{ xs: 6, sm: 3 }}>
            <UserTiplist
              label={labels.reportTo}
              idValue={formik.values.reportTo}
              rq={{ enabled: true, excludedIds: [id] }}
              onChange={(_event, value) =>
                // Set null instead of undefined to avoid remove the property causing
                // Utils.getDataChanges ignore the field
                formik.setFieldValue("reportTo", value?.id ?? null)
              }
            />
          </Grid>
          <Grid size={{ xs: 6, sm: 3 }}>
            <ComboBox
              name="userRole"
              label={labels.role}
              inputRequired
              idValue={formik.values.userRole}
              inputOnChange={formik.handleChange}
              options={app.getRoles()}
            />
          </Grid>
          <Grid size={{ xs: 6, sm: 3 }}>
            <InputField
              fullWidth
              name="expiry"
              type="date"
              label={labels.expiry}
              value={DateUtils.formatForInput(formik.values.expiry) ?? ""}
              onChange={formik.handleChange}
            />
          </Grid>
          <Grid size={{ xs: 6, sm: 3 }}>
            <StatusList
              fullWidth
              inputRequired
              idValue={formik.values.status}
              inputOnChange={formik.handleChange}
            />
          </Grid>
        </React.Fragment>
      )}
    </EditPage>
  );
}
