import React from 'react';
import {
  HBox,
  ItemList,
  TextFieldEx,
  TextFieldExMethods
} from '@etsoo/materialui';
import { DataTypes, DomUtils, Utils } from '@etsoo/shared';
import { Box, Button } from '@mui/material';
import { SharedLayout } from './login/SharedLayout';
import { AccountCircle, Language } from '@mui/icons-material';
import {
  BridgeUtils,
  PublicProductDto,
  RefreshTokenRQ
} from '@etsoo/appscript';
import { Constants } from './app/Constants';
import { app } from './app/SmartApp';
import DownloadIcon from '@mui/icons-material/Download';
import { Link, useNavigate, useSearchParams } from 'react-router-dom';
import { CoreConstants } from '@etsoo/react';

function App() {
  // Navigate
  const navigate = useNavigate();
  const [search] = useSearchParams();
  const params = DomUtils.dataAs(search, {
    serviceId: 'string',
    loginid: 'string',
    url: 'string',
    tryLogin: 'string'
  });

  // Cache URL
  if (params.url) {
    const url = decodeURIComponent(params.url);
    app.storage.setData(Constants.RedirectUrlCache, url);
  }

  // Cached service data
  const service = app.storage.getObject<PublicProductDto>(
    Constants.CurentService
  );

  // Service id or service uid (service id + organization)
  const serviceId = params.serviceId ?? service?.queryId;

  // User login id, email or mobile, saved
  const userIdEncrypted = app.storage.getData<string>(
    CoreConstants.FieldUserIdSaved
  );

  const userIdSaved =
    userIdEncrypted === '' || userIdEncrypted == null
      ? ''
      : app.decrypt(userIdEncrypted) ?? '';

  let passedLoginId = params.loginid ?? null;
  if (
    passedLoginId != null &&
    !Utils.isEmail(passedLoginId) &&
    !Utils.isDigits(passedLoginId)
  )
    passedLoginId =
      app.decrypt(decodeURIComponent(passedLoginId), app.name) ?? null;

  // Query id or saved
  const id = passedLoginId ?? userIdSaved;

  // Register id
  const [registerId, updateRegisterId] = React.useState('');

  // Device validataion
  const deviceValidated = React.useRef(false);

  // Country or region
  const regionId = app.region;

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
      setVisible(false);
      app.changeCultureEx(dispatch, item);
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
    if (inputId == null || inputId === '') {
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

        // Put last avoid skipping next updates
        updateRegisterId(encodeURIComponent(idEncrypted));
      }
    } else {
      // Without password verification, no user id returned
      navigate('./login/password/' + encodeURIComponent(idEncrypted));
    }
  };

  // Refresh token
  const refreshToken = app.getCacheToken();

  // Save login
  const trySaveLogin =
    params.tryLogin !== 'false' &&
    (id === '' || id === userIdSaved) &&
    refreshToken != null;

  // Visible
  const [visible, setVisible] = React.useState(false);

  // QRCode
  const [mobileQRCode, setMobileQRCode] = React.useState<string>();

  const culture = app.culture;

  React.useEffect(() => {
    // Load service data
    const loadServiceData = (serviceToken?: string) => {
      if (serviceId == null) {
        if (serviceToken === '') app.toHome(navigate, './home');
        else setVisible(true);
        return;
      }

      // Load service data
      app.publicApi.product(serviceId, culture).then((result) => {
        if (result == null || !isMounted.current) return;
        // Hold the query id
        result.queryId = serviceId;

        if (serviceToken === '') app.toHome(navigate, './home');
        else if (serviceToken) {
          app.toServiceUrl(result.id, result.webUrl, serviceToken);
        } else {
          // Cache data
          app.storage.setData(Constants.CurentService, result);
          setVisible(true);
        }
      });
    };

    if (!trySaveLogin) {
      loadServiceData();
      return;
    }

    const sdata: Partial<RefreshTokenRQ> = {};
    if (serviceId) {
      if (typeof serviceId === 'number') sdata.serviceId = serviceId;
      else if (Utils.isDigits(serviceId)) sdata.serviceId = parseInt(serviceId);
      else sdata.serviceUid = serviceId;
    }

    // Refresh token
    app.refreshToken({
      callback: (result) => {
        if (!isMounted.current) return;
        if (result === true) {
          // Navigate to service
          loadServiceData(app.userData?.serviceToken ?? '');
        } else {
          loadServiceData();
        }
      },
      data: sdata,
      showLoading: true,
      relogin: false
    });
  }, [regionId, trySaveLogin, culture, serviceId, refreshToken, navigate]);

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
                position: 'absolute',
                top: 0,
                right: 0,
                display: { xs: 'none', sm: 'none', md: 'flex' }
              }}
            >
              <img
                alt="Mobile QRCode"
                src={mobileQRCode}
                title={value.get('scanQRCodeTip')}
              />
            </Box>
          )}
          <SharedLayout
            visible={visible}
            pageRight={
              <HBox width={200} spacing={0.5} justifyContent="flex-end">
                <ItemList
                  items={app.settings.cultures}
                  idField="name"
                  size="small"
                  title={value.get('languages')}
                  onClose={closeCultureChoose}
                  selectedValue={app.culture}
                  className="noneTransformButton"
                  minWidth="200px"
                  icon={<Language />}
                />
              </HBox>
            }
            title={value.get('login')!}
            subTitle={app.settings.currentRegion.label}
            buttons={[
              <Button variant="contained" key="next" onClick={nextClick}>
                {value.get('nextStep')}
              </Button>
            ]}
            bottom={
              service == null && [
                <Link to="./login/about" key="about">
                  {value.get('about')}
                </Link>,
                <Link to="./login/terms" key="terms">
                  {value.get('terms')}
                </Link>,
                <div key="version">{process.env.REACT_APP_VERSION}</div>
              ]
            }
            bottomAdd={
              BridgeUtils.host == null && (
                <Box
                  sx={{
                    textAlign: 'center',
                    display: { xs: 'none', sm: 'none', md: 'inherit' }
                  }}
                >
                  <Button
                    size="small"
                    variant="outlined"
                    startIcon={<DownloadIcon />}
                    href={`${window.location.origin}/apps/SmartERP.zip`}
                    target="_blank"
                  >
                    {value.get('downloadWinApp')}
                  </Button>
                </Box>
              )
            }
          >
            <HBox spacing={1} alignItems="flex-start">
              <Box sx={{ paddingTop: 3 }}>
                <AccountCircle color="primary" />
              </Box>
              <TextFieldEx
                label={value.get('loginId')}
                ref={mRef}
                inputRef={loginRef}
                defaultValue={id?.hideEmail()}
                autoFocus
                autoCorrect="off"
                autoCapitalize="none"
                inputProps={{ inputMode: 'email', spellCheck: false }}
                showClear
                autoComplete="username"
                onEnter={(e) => {
                  nextClick();
                  e.preventDefault();
                }}
              />
            </HBox>
            <div>
              {value.get('noAccountTip')}&nbsp;
              <Link to={'./login/register/' + registerId}>
                {value.get('noAccountCreate')}
              </Link>
            </div>
          </SharedLayout>
        </React.Fragment>
      )}
    </Context.Consumer>
  );
}

export default App;
