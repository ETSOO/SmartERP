import { CommonPage, TextFieldEx, VBox } from "@etsoo/materialui";
import { DomUtils } from "@etsoo/shared";
import { useFormik } from "formik";
import * as Yup from "yup";
import { app } from "../../../app/MyApp";
import { usePageDataEmpty } from "@etsoo/smarterp-core";
import Button from "@mui/material/Button";

// Change password
// https://html.spec.whatwg.org/multipage/form-control-infrastructure.html#autofill
export default function ChangePassword() {
  // Labels
  const labels = app.getLabels(
    "currentPassword",
    "currentPasswordRequired",
    "newPassword",
    "newPasswordRequired",
    "newPasswordTip",
    "passwordChangeSuccess",
    "passwordRepeatError",
    "passwordTip",
    "repeatPassword",
    "repeatPasswordRequired",
    "submit"
  );

  // Form validation schema
  const validationSchema = Yup.object({
    oldPassword: Yup.string().required(labels.currentPasswordRequired),
    password: Yup.string()
      .test((value, context) => {
        if (value == null || !app.isValidPassword(value)) {
          return context.createError({ message: labels.passwordTip });
        }
        return true;
      })
      .required(labels.newPasswordRequired)
      .notOneOf([Yup.ref("oldPassword")], labels.newPasswordTip),
    rePassword: Yup.string()
      .required(labels.repeatPasswordRequired)
      // oneOf([Yup.ref('newPassword'), null], "Passwords mush match") will fail
      // ref is not proper for reach validation, ref field value is not ready
      .oneOf([Yup.ref("password")], labels.passwordRepeatError)
  });

  // Formik
  const formik = useFormik({
    initialValues: {
      oldPassword: "",
      password: "",
      rePassword: ""
    },
    validationSchema: validationSchema,
    onSubmit: async (values) => {
      // Submit data
      var result = await app.core.authApi.changePassword(
        values.oldPassword,
        values.password
      );
      if (result == null) return;

      if (result.ok) {
        // Tip and clear
        app.notifier.succeed(
          labels.passwordChangeSuccess,
          undefined,
          async () => {
            // Sign out
            await app.signout();
          }
        );
      } else {
        formik.setFieldError("oldPassword", result.title);
        DomUtils.setFocus("oldPassword");
      }
    }
  });

  // Page data hook
  usePageDataEmpty(app);

  return (
    <CommonPage maxWidth="xs">
      <form
        onSubmit={(event) => {
          formik.handleSubmit(event);
          DomUtils.setFocus(formik.errors);
        }}
      >
        <VBox spacing={2}>
          <input
            hidden
            name="username"
            defaultValue="SmartERP"
            autoComplete="username"
          />
          <TextFieldEx
            name="oldPassword"
            label={labels.currentPassword}
            showPassword
            autoFocus
            autoComplete="current-password"
            value={formik.values.oldPassword}
            onChange={formik.handleChange}
            error={
              formik.touched.oldPassword && Boolean(formik.errors.oldPassword)
            }
            helperText={formik.touched.oldPassword && formik.errors.oldPassword}
          />
          <TextFieldEx
            name="password"
            label={labels.newPassword}
            showPassword
            autoComplete="new-password"
            value={formik.values.password}
            onChange={formik.handleChange}
            error={formik.touched.password && Boolean(formik.errors.password)}
            helperText={formik.touched.password && formik.errors.password}
          />
          <TextFieldEx
            name="rePassword"
            label={labels.repeatPassword}
            showPassword
            autoComplete="new-password"
            value={formik.values.rePassword}
            onChange={formik.handleChange}
            error={
              formik.touched.rePassword && Boolean(formik.errors.rePassword)
            }
            helperText={formik.touched.rePassword && formik.errors.rePassword}
          />
          <Button variant="contained" type="submit" fullWidth>
            {labels.submit}
          </Button>
        </VBox>
      </form>
    </CommonPage>
  );
}
