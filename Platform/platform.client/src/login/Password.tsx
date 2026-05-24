import React from "react";
import { SharedLayout } from "./SharedLayout";
import { AuthRequest } from "@etsoo/appscript";
import {
  HBox,
  ISmartERPUser,
  TextFieldEx,
  TextFieldExMethods
} from "@etsoo/materialui";
import { Lock } from "@mui/icons-material";
import { Constants } from "../app/Constants";
import { app } from "../app/SmartApp";
import { Link, Navigate, useNavigate, useParams } from "react-router-dom";
import { CoreConstants } from "@etsoo/react";
import { DynamicActionResult } from "@etsoo/shared";
import { PublicOrgRequest } from "../api/rq/public/PublicOrgRequest";
import Button from "@mui/material/Button";
import Box from "@mui/material/Box";
import FormControlLabel from "@mui/material/FormControlLabel";
import Switch from "@mui/material/Switch";

const homeUrl = "./../../../";
function NavigateHome() {
  return <Navigate to={homeUrl} replace />;
}

export default function Password() {
  // Router
  const navigate = useNavigate();
  const { username } = useParams<{ username: string }>();

  // Labels
  const labels = app.getLabels(
    "unknownError",
    "submit",
    "yourPassword",
    "keepLogged",
    "forgotPasswordTip",
    "serviceUnpurchased",
    "environmentChanged"
  );

  // Password ref
  const passwordRef = React.useRef<HTMLInputElement>(null);
  const mRef = React.createRef<TextFieldExMethods>();

  // Button
  const [buttonDisabled, updateButtonDisabled] = React.useState<boolean>(false);

  if (username == null) {
    return <NavigateHome />;
  }

  // Decode
  const usernameDecoded = decodeURIComponent(username);
  const id = app.decrypt(usernameDecoded);

  if (id == null || id === "") {
    return <NavigateHome />;
  }

  // Hold on cache
  app.storage.setData(CoreConstants.FieldUserIdSaved, usernameDecoded);

  // Format title
  const formatTitle = (result: DynamicActionResult) => {
    let disabled: boolean = false;
    let title: string = result.title ?? "Unknown";

    switch (result.type) {
      case "UserFrozen":
      case "DeviceFrozen":
        const frozenTime = new Date(result.data?.frozenTime);
        title = title.format(frozenTime.toLocaleString(app.culture));
        disabled = true;
        break;
      case "AccountExpired":
        const expiry = new Date(result.data?.expiry);
        title = title.format(expiry.toLocaleString(app.culture));
        disabled = true;
        break;
      case "OrgExpired":
        const orgExpiry = new Date(result.data?.orgExpiry);
        title = title.format(orgExpiry.toLocaleString(app.culture));
        disabled = true;
        break;
      case "DeviceDisabled":
      case "AccountDisabled":
      case "OrgDisabled":
        disabled = true;
        break;
    }

    return [disabled, title];
  };

  // Submit
  const submit = async () => {
    // password
    const password = passwordRef.current?.value.trim();
    if (password == null || password.length < 6) {
      passwordRef.current?.focus();
      return;
    }

    // Auth request
    const org = app.storage.getData<PublicOrgRequest>(
      Constants.OrgRequestField
    );
    const auth = app.storage.getData<AuthRequest>(Constants.AuthRequestField);

    const [result, refreshToken] = await app.authApi.login<ISmartERPUser>({
      id: usernameDecoded,
      pwd: app.encrypt(app.hash(password)),
      org: org?.orgId,
      auth
    });

    if (result == null) return;

    if (result.ok) {
      if (refreshToken == null || result.data == null) {
        app.notifier.alert(labels.unknownError);
        return;
      }

      // Login success
      app.loginComplete(auth, result.data, refreshToken);
    } else if (app.checkDeviceResult(result)) {
      app.notifier.alert(labels.environmentChanged, () => {
        navigate(homeUrl);
      });
    } else {
      const [disabled, title] = formatTitle(result);
      mRef.current?.setError(title);

      if (disabled) {
        updateButtonDisabled(true);
      } else {
        passwordRef.current?.focus();
      }
    }
  };

  return (
    <SharedLayout
      title={id.hideEmail()}
      buttons={
        <Button variant="contained" onClick={submit} disabled={buttonDisabled}>
          {labels.submit}
        </Button>
      }
    >
      <form>
        <input
          hidden
          name="username"
          defaultValue="SmartERP"
          autoComplete="username"
        />
        <HBox spacing={1} sx={{ alignItems: "flex-start" }}>
          <Box sx={{ paddingTop: 3 }}>
            <Lock color="primary" />
          </Box>
          <TextFieldEx
            name="password"
            label={labels.yourPassword}
            showPassword
            autoComplete="current-password"
            inputRef={passwordRef}
            mRef={mRef}
            autoFocus
            onEnter={(e) => {
              submit();
              e.preventDefault();
            }}
          />
        </HBox>
        <FormControlLabel
          control={
            <Switch
              defaultChecked={app.keepLogin}
              onChange={(e) => (app.keepLogin = e.target.checked)}
            />
          }
          label={labels.keepLogged}
        />
      </form>
      <div>
        <Link to={`./../../callbackverify/${encodeURIComponent(username)}`}>
          {labels.forgotPasswordTip}
        </Link>
      </div>
    </SharedLayout>
  );
}
