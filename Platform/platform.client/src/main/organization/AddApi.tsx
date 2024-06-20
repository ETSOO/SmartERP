import { DataTypes, Utils } from '@etsoo/shared';
import { useFormik } from 'formik';
import React from 'react';
import { useNavigate } from 'react-router-dom';
import * as Yup from 'yup';
import { ApiService, IdActionResult } from '@etsoo/appscript';
import { EditPage, InputField, OptionBool, SelectEx } from '@etsoo/materialui';
import { Grid } from '@mui/material';
import { useParamsEx, useSearchParamsEx } from '@etsoo/react';
import { app } from '../../app/SmartApp';
import { ApiServiceDto } from '../../api/dto/apiservice/ApiServiceEditDto';
import { SMTPApiService } from './ApiService/SMTPApiService';

function AddApi() {
  // Route
  const navigate = useNavigate();
  const { id } = useParamsEx({ id: 'number' });
  const { org = 0 } = useSearchParamsEx({ org: 'number' });

  // Labels
  const labels = app.getLabels('noChanges', 'title', 'type', 'enabled');

  // Is editing
  const isEditing = id != null;
  type DataType = DataTypes.AddAndEditType<ApiServiceDto>;

  // Edit data
  // For create, set the default value here, not in the input file value
  const [data, setData] = React.useState<DataType>({
    organizationId: org,
    title: '',
    appId: '',
    appSecret: '',
    enabled: true
  });

  // Form validation schema
  const validationSchema = Yup.object({});

  // Formik
  const formik = useFormik<DataType>({
    initialValues: data,
    enableReinitialize: true,
    validationSchema: validationSchema,
    onSubmit: async (values) => {
      // Request data
      const rq = { ...values };

      if (rq.serviceId !== ApiService.SMTP) return;

      if (rq.settings) {
        if (rq.serviceId === ApiService.SMTP) {
          Utils.correctTypes(rq.settings, { useSsl: 'boolean' });
        }
      }

      let result: IdActionResult | undefined;
      if (rq.id != null) {
        // Changed fields
        const fields: string[] = Utils.getDataChanges(rq, data);
        if (fields.length === 0) {
          app.warning(labels.noChanges);
          return;
        }
        rq.changedFields = fields;

        result = await app.apiServiceApi.update(rq);
      } else {
        result = await app.apiServiceApi.create(rq);
      }

      if (result == null) return;

      if (result.ok) {
        const url = isEditing ? `./../../view/${org}` : `./../view/${org}`;
        navigate(url);
        return;
      }

      app.alertResult(result);
    }
  });

  // Load data
  const reloadData = async () => {
    if (id == null) return;
    const data = await app.apiServiceApi.updateRead(id);
    if (data == null) return;
    setData(data);
  };

  React.useEffect(() => {
    // Page title
    app.setPageKey(isEditing ? 'editApi' : 'addApi');

    return () => {
      app.pageExit();
    };
  }, [isEditing]);

  let settings: React.ReactNode;
  switch (formik.values.serviceId) {
    case ApiService.SMTP:
      settings = (
        <SMTPApiService
          data={formik.values}
          handleChange={formik.handleChange}
        />
      );
      break;
    default:
      settings = <React.Fragment />;
      break;
  }

  return (
    <EditPage
      isEditing={isEditing}
      onSubmit={(event) => {
        formik.handleSubmit(event);
      }}
      onUpdate={reloadData}
    >
      <Grid item xs={12} sm={6}>
        <SelectEx
          name="serviceId"
          label={labels.type}
          onChange={formik.handleChange}
          onItemChange={(item, userAction) => {
            if (item == null || !userAction) return;
            formik.setFieldValue('title', item.label);
          }}
          options={app.getApiServices()}
          value={formik.values.serviceId}
          fullWidth
          required
        />
      </Grid>
      <Grid item xs={12} sm={6}>
        <InputField
          fullWidth
          required
          name="title"
          inputProps={{ maxLength: 128 }}
          label={labels.title}
          value={formik.values.title ?? ''}
          onChange={formik.handleChange}
        />
      </Grid>
      {settings}
      <Grid item xs={6} sm={3}>
        <OptionBool
          name="enabled"
          label={labels.enabled}
          variant="outlined"
          defaultValue={`${formik.values.enabled}`}
          fullWidth
          onChange={formik.handleChange}
        />
      </Grid>
    </EditPage>
  );
}

export default AddApi;
