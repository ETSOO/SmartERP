import { usePageDataEmpty } from "@etsoo/smarterp-core";
import { app } from "../../../app/MyApp";
import { EditPage, InputField } from "@etsoo/materialui";
import { useParamsEx } from "@etsoo/react";
import { useFormik } from "formik";
import React from "react";
import Grid from "@mui/material/Grid";
import { StatusList } from "@etsoo/smarterp-core/components";
import { IdActionResult, Utils } from "@etsoo/shared";
import { DeptCreateRQ, DeptUpdateRQ } from "@etsoo/smarterp-crm";
import { useNavigate } from "react-router-dom";
import { UserTiplist } from "@etsoo/smarterp-crm/components";
import { EntityStatus } from "@etsoo/appscript";

export default function AddDept() {
  // Route
  const navigate = useNavigate();
  const { id = 0 } = useParamsEx({
    id: "number"
  });

  const isEditing = id > 0;

  // Labels
  const labels = app.getLabels("leader", "nameB", "noChanges", "status");

  // Type
  type DataType = DeptCreateRQ;

  // State
  const [data, setData] = React.useState<DataType>({
    name: ""
  });

  // Formik
  const formik = useFormik<DataType>({
    initialValues: data,
    enableReinitialize: true,
    validateOnChange: false,
    onSubmit: async (v) => {
      // Submit
      let result: IdActionResult | undefined;
      let redirectUrl: string;
      if (id > 0) {
        const rq: DeptUpdateRQ = {
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

        result = await app.deptApi.update(rq);
      } else {
        const rq: DeptCreateRQ = {
          ...v
        };

        Utils.removeEmptyValues(rq);

        redirectUrl = "./..";

        result = await app.deptApi.create(rq);
      }

      if (result == null) return;

      if (result.ok) {
        navigate(redirectUrl);
        return;
      }

      app.alertResult(result);
    }
  });

  // Load data
  const reloadData = React.useCallback(async () => {
    if (id < 1) return;
    const result = await app.deptApi.updateRead(id);
    if (result == null) return;
    setData(result);
  }, [id]);

  // Page data hook
  usePageDataEmpty(app);

  return (
    <EditPage
      isEditing={isEditing}
      onSubmit={formik.handleSubmit}
      onUpdate={reloadData}
      paddings={0}
    >
      <Grid size={{ xs: 12, sm: 12 }}>
        <InputField
          fullWidth
          required
          name="name"
          slotProps={{ htmlInput: { maxLength: 128 } }}
          label={labels.nameB}
          value={formik.values.name}
          onChange={formik.handleChange}
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 6 }}>
        <UserTiplist
          label={labels.leader}
          name="leaderId"
          idValue={data.leaderId}
          inputOnChange={formik.handleChange}
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 6 }}>
        <StatusList
          inputRequired
          idValue={formik.values.status ?? EntityStatus.Normal}
          inputOnChange={formik.handleChange}
        />
      </Grid>
    </EditPage>
  );
}
