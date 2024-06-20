import { EditPage, InputField, MaskInput, Tiplist } from '@etsoo/materialui';
import { Grid } from '@mui/material';
import React from 'react';
import { useFormik } from 'formik';
import * as Yup from 'yup';
import { DataTypes, Utils } from '@etsoo/shared';
import { BusinessTax, OrgDto, TiplistRQ } from '@etsoo/appscript';
import { app } from '../../app/SmartApp';
import { useNavigate } from 'react-router-dom';
import { useParamsEx } from '@etsoo/react';
import { AppCache } from '../../app/AppCache';

function EditOrganization() {
  // Route
  const navigate = useNavigate();
  const { id } = useParamsEx({ id: 'number' });

  // Data type
  type DataType = DataTypes.AddAndEditType<OrgDto>;

  // Labels
  const labels = app.getLabels(
    'noChanges',
    'organizationName',
    'tradeAs',
    'brand',
    'parentOrg',
    'editOrganization'
  );

  // Form validation schema
  const validationSchema = Yup.object({
    name: Yup.string().required()
  });

  // Edit data
  const [data, setData] = React.useState<DataType>({
    name: '',
    identifier: '',
    regionId: app.region
  });

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

      // Format identifier
      rq.identifier = Utils.removeNonLetters(rq.identifier);

      // Changed fields
      const fields: string[] = Utils.getDataChanges(rq, data);
      if (fields.length === 0) {
        app.warning(labels.noChanges);
        return;
      }
      rq.changedFields = fields;

      // Submit
      const result = await app.orgApi.update(rq);
      if (result == null) return;

      if (result.ok) {
        AppCache.removeOrgCache();

        app.orgApi.sendActionMessage({
          kind: 'edit-organization',
          title: app.formatAction(labels.editOrganization, formik.values.name!),
          relatedId: result.data?.id
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
    const data = await app.orgApi.updateRead(id);
    if (data == null) return;
    setData(data);
  };

  // Tax
  const tax = BusinessTax.getById(app.region);

  React.useEffect(() => {
    // Page title
    app.setPageTitle(labels.editOrganization);

    return () => {
      app.pageExit();
    };
  }, [labels.editOrganization]);

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
          required
          name="name"
          inputProps={{ maxLength: 128 }}
          label={labels.organizationName}
          value={formik.values.name ?? ''}
          onChange={formik.handleChange}
          error={formik.touched.name && Boolean(formik.errors.name)}
          helperText={formik.touched.name && formik.errors.name}
        />
      </Grid>
      <Grid item xs={12} sm={6}>
        <MaskInput
          mask={{ mask: tax?.mask ?? '' }}
          name="identifier"
          label={app.get(tax?.labelKey ?? 'taxId')}
          fullWidth
          inputProps={{
            maxLength: 20,
            style: { textTransform: 'uppercase' }
          }}
          value={formik.values.identifier ?? ''}
          onChange={formik.handleChange}
          error={formik.touched.identifier && Boolean(formik.errors.identifier)}
          helperText={formik.touched.identifier && formik.errors.identifier}
        />
      </Grid>
      <Grid item xs={12} sm={6}>
        <InputField
          fullWidth
          name="tradeAs"
          inputProps={{ maxLength: 128 }}
          label={labels.tradeAs}
          value={formik.values.tradeAs ?? ''}
          onChange={formik.handleChange}
        />
      </Grid>
      <Grid item xs={12} sm={6}>
        <InputField
          fullWidth
          name="brand"
          inputProps={{ maxLength: 30 }}
          label={labels.brand}
          value={formik.values.brand ?? ''}
          onChange={formik.handleChange}
        />
      </Grid>
      <Grid item xs={12} sm={6}>
        <Tiplist
          label={labels.parentOrg}
          name="parentId"
          idValue={formik.values.parentId}
          loadData={async (keyword, id) => {
            const rq: TiplistRQ = {
              id,
              keyword
            };
            if (data.id) rq.excludedIds = [data.id];
            return await app.orgApi.list(rq, {
              defaultValue: [],
              showLoading: false
            });
          }}
          inputOnChange={formik.handleChange}
          inputError={
            formik.touched.parentId && Boolean(formik.errors.parentId)
          }
          inputHelperText={formik.touched.parentId && formik.errors.parentId}
        />
      </Grid>
    </EditPage>
  );
}

export default EditOrganization;
