import React from "react";
import {
  HBox,
  ItemList,
  LoadingButton,
  TextFieldEx,
  TextFieldExMethods
} from "@etsoo/materialui";
import { DataTypes, DomUtils, NumberUtils, Utils } from "@etsoo/shared";
import { Alert, Box, Button, Grid2, SvgIcon, Typography } from "@mui/material";
import { SharedLayout } from "./login/SharedLayout";
import { AccountCircle, Language } from "@mui/icons-material";
import { AuthRequest, BridgeUtils } from "@etsoo/appscript";
import { Constants } from "./app/Constants";
import { app } from "./app/SmartApp";
import DownloadIcon from "@mui/icons-material/Download";
import { Link, useNavigate, useSearchParams } from "react-router-dom";
import { CoreConstants } from "@etsoo/react";
import { AppUtils } from "./app/AppUtils";
import { PublicOrgInfo } from "./api/dto/public/PublicOrgInfo";
import { PublicOrgRequest } from "./api/rq/public/PublicOrgRequest";

function formatLoginTip(
  appId: number,
  appName: string,
  scope: string,
  redirectUri: string
) {
  const tip = app.get("loginTip")!;
  appName = app.get(`app${appId}`) ?? appName;
  const scopes = scope
    .split(" ")
    .map((s) => app.get(`scope${s.formatInitial(true)}`) ?? s);

  // Replace the tip
  const items: [string, boolean][] = [];
  let tips = tip.split("{0}");
  items.push([tips[0], false]);
  items.push([appName, true]);

  tips = tips[1].split("{1}");
  items.push([tips[0], false]);
  items.push([scopes.join(", "), true]);

  tips = tips[1].split("{2}");
  items.push([tips[0], false]);
  items.push([new URL(redirectUri).hostname, true]);

  return items.map((i, index) => (
    <Typography
      component="span"
      key={index}
      fontWeight={i[1] ? "bold" : undefined}
    >
      {i[0]}
    </Typography>
  ));
}

function checkAppUri(redirectUri: string) {
  const uri = new URL(redirectUri);
  return uri.hostname !== window.location.hostname;
}

export default function App() {
  // Navigate
  const navigate = useNavigate();
  const [search] = useSearchParams();
  const params = DomUtils.dataAs(search, {
    auth: "string",
    loginid: "string",
    org: "string",
    tryLogin: "string"
  });

  // Cached auth request
  const auth = params.auth
    ? React.useMemo(() => {
        try {
          const auth: AuthRequest = JSON.parse(
            decodeURIComponent(params.auth!)
          );
          app.storage.setData(Constants.AuthRequestField, auth);
          return auth;
        } catch (error) {
          console.error("Authorization request parse failed", error);
        }
      }, [params.auth])
    : app.storage.getData<AuthRequest>(Constants.AuthRequestField);

  // User login id, email or mobile, saved
  const userIdEncrypted = app.storage.getData<string>(
    CoreConstants.FieldUserIdSaved
  );

  // Cached organization data
  const org = params.org
    ? { org: params.org }
    : app.storage.getData<PublicOrgRequest>(Constants.OrgRequestField);

  const userIdSaved =
    userIdEncrypted === "" || userIdEncrypted == null
      ? ""
      : app.decrypt(userIdEncrypted) ?? "";

  let passedLoginId = params.loginid ?? auth?.loginHint ?? null;
  if (
    passedLoginId != null &&
    !Utils.isEmail(passedLoginId) &&
    !Utils.isDigits(passedLoginId)
  )
    passedLoginId =
      app.decrypt(decodeURIComponent(passedLoginId), app.name) ?? null;

  // Query id or saved
  const id = passedLoginId ?? userIdSaved;

  // Device validataion
  const deviceValidated = React.useRef(false);

  // Culture context
  const Context = app.cultureState.context;

  // Culture dispatch
  const { dispatch } = React.useContext(Context);

  // Change culture
  const closeCultureChoose = (
    item: DataTypes.CultureDefinition,
    changed: boolean
  ) => {
    if (changed) {
      app.initCall((result) => {
        if (result) {
          if (loginRef.current) loginRef.current.value = "";
          app.changeCultureEx(dispatch, item);
        }
      }, true);
    }
  };

  // Is mounted or not
  const isMounted = React.useRef(true);

  // Login id field
  const loginRef = React.useRef<HTMLInputElement>();
  const mRef = React.createRef<TextFieldExMethods>();

  // Next button click
  const nextClick = async () => {
    // Input check
    const inputId = loginRef.current?.value.trim();
    if (inputId == null || inputId === "") {
      loginRef.current?.focus();
      return;
    }

    // Encryption
    const checkId = id != null && inputId === id.hideEmail() ? id : inputId;
    const idEncrypted = app.encrypt(checkId);

    // Get the result
    const result = await app.authApi.loginId(checkId);
    if (result == null || !isMounted.current) return;

    if (!result.ok) {
      if (app.checkDeviceResult(result) && !deviceValidated.current) {
        await app.initCall((result) => {
          if (result) {
            nextClick();
          }
        }, true);
        deviceValidated.current = true;
      } else {
        mRef.current?.setError(result.title);
        loginRef.current?.focus();
      }
    } else {
      // Make sure the registration is done
      if (
        result.data != null &&
        "step" in result.data &&
        NumberUtils.parse(result.data.step, 0) > 0
      ) {
        app.notifier.alert(app.get("continueRegistrationDetail"), () =>
          navigate(`./login/register/`)
        );
        return;
      }

      // Without password verification, no user id returned
      navigate("./login/password/" + encodeURIComponent(idEncrypted));
    }
  };

  // Refresh token
  const refreshToken = app.getCacheToken();

  // Save login
  const trySaveLogin =
    params.tryLogin !== "false" &&
    app.keepLogin &&
    (id === "" || id === userIdSaved) &&
    refreshToken;

  // Visible
  const [visible, setVisible] = React.useState(false);

  // App data
  const [appData, setAppData] = React.useState<PublicOrgInfo>();

  // QRCode
  const [mobileQRCode, setMobileQRCode] = React.useState<string>();

  // Get app name
  const getAppName = React.useCallback(() => {
    return appData?.appId
      ? app.get(`app${appData.appId}`) ?? appData?.appName
      : app.get("app1");
  }, [appData]);

  // Load application data
  const loadAppData = React.useCallback(() => {
    // No data to load
    if (!org?.org && !auth?.appId) {
      setAppData(undefined);
      setVisible(true);
      return;
    }

    app.publicApi
      .orgInfo({
        appId: auth?.appId,
        appKey: auth?.appKey,
        orgUid: org?.org,
        deviceId: app.deviceId
      })
      .then((data) => {
        if (data) {
          // Device data maybe expired and causing the org data return null
          if (org != null) {
            org.orgId = data.orgId;
          }

          app.storage.setData(Constants.OrgRequestField, org);

          setAppData(data);
        }

        setVisible(true);
      });
  }, [org?.org, auth?.appId, auth?.appKey]);

  React.useEffect(() => {
    if (!trySaveLogin) {
      loadAppData();
      return;
    }

    // Refresh token
    app.refreshToken({ showLoading: true }, (result) => {
      if (!isMounted.current) return;
      if (result === true) {
        // Login success
        app.loginComplete(auth);
      } else {
        // Load app data and the login UI
        loadAppData();
      }
    });
  }, [trySaveLogin, loadAppData]);

  // Do auth
  const doAuth = React.useCallback(async (ac: string) => {
    const url = await app.authApi.getAuthLogInUrl(ac);
    if (url) {
      globalThis.location.href = url;
    }
  }, []);

  React.useEffect(() => {
    if (!visible) return;

    const idResult = app.encrypt(id, app.name);
    app.publicApi
      .mobileQRCode(idResult, undefined, {
        showLoading: false,
        onError: () => false
      })
      .then((result) => {
        if (result == null) return;
        setMobileQRCode(result);
      });
  }, [id, visible]);

  React.useEffect(() => {
    return () => {
      isMounted.current = false;
      app.notifier.hideLoading();
    };
  }, []);

  return (
    <Context.Consumer>
      {(value) => (
        <React.Fragment>
          {mobileQRCode && (
            <Box
              gap={0.5}
              sx={{
                position: "absolute",
                top: 0,
                right: 0,
                display: { xs: "none", sm: "none", md: "flex" }
              }}
            >
              <img
                alt="Mobile QRCode"
                src={mobileQRCode}
                title={value.get("scanQRCodeTip")}
              />
            </Box>
          )}
          <SharedLayout
            appName={getAppName()}
            visible={visible}
            pageRight={
              <HBox width={200} spacing={0.5} justifyContent="flex-end">
                <ItemList
                  items={app.settings.cultures}
                  idField="name"
                  size="small"
                  title={value.get("languages")}
                  onClose={closeCultureChoose}
                  selectedValue={app.culture}
                  className="noneTransformButton"
                  minWidth="200px"
                  icon={<Language />}
                />
              </HBox>
            }
            title={value.get("login")!}
            subTitle={appData?.orgName ?? app.settings.currentRegion.label}
            buttons={[
              <Button variant="contained" key="next" onClick={nextClick}>
                {value.get("nextStep")}
              </Button>
            ]}
            bottom={
              appData?.orgId == null && [
                <Link to="./login/about" key="about">
                  {value.get("about")}
                </Link>,
                <Link to="./login/terms" key="terms">
                  {value.get("terms")}
                </Link>,
                <div key="version">{import.meta.env.VITE_APP_VERSION}</div>
              ]
            }
            bottomAdd={
              BridgeUtils.host == null && (
                <Box
                  sx={{
                    textAlign: "center",
                    display: { xs: "none", sm: "none", md: "inherit" }
                  }}
                >
                  <Button
                    size="small"
                    variant="outlined"
                    startIcon={<DownloadIcon />}
                    href={`${window.location.origin}/apps/SmartERP.zip`}
                    target="_blank"
                  >
                    {value.get("downloadWinApp")}
                  </Button>
                </Box>
              )
            }
          >
            {auth && appData?.appName && checkAppUri(auth.redirectUri) && (
              <Alert severity="warning" sx={{ width: "100%" }}>
                {formatLoginTip(
                  auth.appId,
                  appData.appName,
                  auth.scope,
                  auth.redirectUri
                )}
              </Alert>
            )}
            <HBox spacing={1} alignItems="flex-start">
              <Box sx={{ paddingTop: 3 }}>
                <AccountCircle color="primary" />
              </Box>
              <TextFieldEx
                label={value.get("loginId")}
                ref={mRef}
                inputRef={loginRef}
                defaultValue={id?.hideEmail()}
                autoFocus
                autoCorrect="off"
                autoCapitalize="none"
                slotProps={{
                  htmlInput: { inputMode: "email", spellCheck: false }
                }}
                showClear
                onEnter={(e) => {
                  nextClick();
                  e.preventDefault();
                }}
              />
            </HBox>
            <Typography variant="caption">{value.get("signInWith")}</Typography>
            {app.settings.authClients.length > 0 && (
              <Grid2 container spacing={0.5}>
                {app.settings.authClients.map((ac) => (
                  <Grid2 size={{ xs: 6 }} key={ac}>
                    <LoadingButton
                      variant="outlined"
                      fullWidth
                      startIcon={
                        <SvgIcon
                          component={AppUtils.getBrandIcon(ac)}
                          inheritViewBox
                        />
                      }
                      onClick={() => doAuth(ac)}
                    >
                      {value.get(`brand${ac}`)}
                    </LoadingButton>
                  </Grid2>
                ))}
              </Grid2>
            )}
            <div>
              {value.get("noAccountTip")}&nbsp;
              <Link to="./login/register/">{value.get("noAccountCreate")}</Link>
            </div>
          </SharedLayout>
        </React.Fragment>
      )}
    </Context.Consumer>
  );
}
