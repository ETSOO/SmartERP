import { ComboBox, EditPage, InputField } from '@etsoo/materialui';
import { FormControlLabel, Grid, Switch } from '@mui/material';
import React from 'react';
import { useFormik } from 'formik';
import * as Yup from 'yup';
import { DataTypes, Utils } from '@etsoo/shared';
import { UserRole } from '@etsoo/appscript';
import { app } from '../../app/SmartApp';
import { useNavigate, useParams } from 'react-router-dom';
import { MemberDto } from '../../api/dto/member/MemberDto';
import { AppCache } from '../../app/AppCache';

function EditMember() {
  // Route
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();

  // Data type
  type DataType = DataTypes.AddAndEditType<MemberDto>;

  // Roles
  const role = app.userData?.role ?? UserRole.User;
  const maxRole = role > UserRole.Admin ? UserRole.Admin : role;
  const roles = app.getRoles(2 * maxRole - 1);

  // Labels
  const labels = app.getLabels(
    'noChanges',
    'preferredName',
    'externalId',
    'enabled',
    'role',
    'editMember'
  );

  // Form validation schema
  const validationSchema = Yup.object({
    localName: Yup.string().required()
  });

  // Edit data
  const [data, setData] = React.useState<DataType>({});

  // Formik
  // https://formik.org/docs/examples/with-material-ui
  // https://firxworx.com/blog/coding/react/integrating-formik-with-react-material-ui-and-typescript/
  const formik = useFormik<DataType>({
    initialValues: data,
    enableReinitialize: true,
    validationSchema: validationSchema,
    onSubmit: async (values) => {
      // Edit only
      if (values.id == null) return;

      // Request data
      const rq = { ...values };

      // Changed fields
      const fields: string[] = Utils.getDataChanges(rq, data);
      if (fields.length === 0) {
        app.warning(labels.noChanges);
        return;
      }
      rq.changedFields = fields;

      // Submit
      const result = await app.memberApi.update(rq);
      if (result == null) return;

      if (result.ok) {
        AppCache.removeMemberCache();

        app.orgApi.sendActionMessage({
          kind: 'edit-member',
          title: app.formatAction(labels.editMember, formik.values.localName!)
        });

        navigate('./../../all', {
          state: { id }
        });
        return;
      }

      app.alertResult(result);
    }
  });

  // Load data
  const reloadData = async () => {
    if (id == null) return;
    const data = await app.memberApi.updateRead(id);
    if (data == null) return;
    setData(data);
  };

  React.useEffect(() => {
    // Page title
    app.setPageTitle(labels.editMember);

    return () => {
      app.pageExit();
    };
  }, [labels.editMember]);

  return (
    <EditPage
      onSubmit={(event) => {
        formik.handleSubmit(event);
      }}
      onUpdate={reloadData}
    >
      <Grid item xs={12} sm={6}>
        <InputField
          fullWidth
          name="localName"
          required
          inputProps={{ maxLength: 128 }}
          label={labels.preferredName}
          value={formik.values.localName ?? ''}
          onChange={formik.handleChange}
        />
      </Grid>
      <Grid item xs={12} sm={6}>
        <FormControlLabel
          control={
            <Switch
              name="enabled"
              checked={formik.values.enabled ?? true}
              onChange={formik.handleChange}
            />
          }
          label={labels.enabled}
        />
      </Grid>
      <Grid item xs={12} sm={6}>
        <ComboBox
          options={roles}
          name="entityRole"
          label={labels.role}
          idValue={formik.values.entityRole}
          inputRequired
          inputOnChange={formik.handleChange}
        />
      </Grid>
      <Grid item xs={12} sm={6}>
        <InputField
          fullWidth
          name="externalId"
          inputProps={{ maxLength: 128 }}
          label={labels.externalId}
          value={formik.values.externalId ?? ''}
          onChange={formik.handleChange}
          error={formik.touched.externalId && Boolean(formik.errors.externalId)}
          helperText={formik.touched.externalId && formik.errors.externalId}
        />
      </Grid>
    </EditPage>
  );
}

export default EditMember;
