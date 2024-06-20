import "./index.css";
import React from "react";
import { createRoot } from "react-dom/client";
import App from "./App";
import { NotFound } from "./NotFound";
import Password from "./login/Password";
import {
  createTheme,
  CssBaseline,
  LinearProgress,
  ThemeProvider
} from "@mui/material";
import Home from "./main/Home";
import { app, NotifierProvider } from "./app/SmartApp";
import { Route, Routes } from "react-router-dom";
import Dashboard from "./main/Dashboard";
import { DynamicRouter } from "@etsoo/react";
import { zhCN, zhHK } from "@mui/material/locale";

// Root
const root = document.getElementById("root")!;

// Lazy load components
const About = React.lazy(() => import("./login/About"));
const Register = React.lazy(() => import("./login/Register"));
const RegisterPassword = React.lazy(() => import("./login/RegisterPassword"));
const RegisterVerify = React.lazy(() => import("./login/RegisterVerify"));
const RegisterComplete = React.lazy(() => import("./login/RegisterComplete"));
const CallbackVerify = React.lazy(() => import("./login/CallbackVerify"));
const CallbackComplete = React.lazy(() => import("./login/CallbackComplete"));
const Terms = React.lazy(() => import("./login/Terms"));
const Invite = React.lazy(() => import("./login/Invite"));

const LoginHistory = React.lazy(() => import("./main/user/LoginHistory"));
const UpdateAvatar = React.lazy(() => import("./main/user/UpdateAvatar"));
const PrivateData = React.lazy(() => import("./main/user/PrivateData"));
const ChangePassword = React.lazy(() => import("./main/user/ChangePassword"));

const AllOrganizations = React.lazy(
  () => import("./main/organization/AllOrganizations")
);
const EditOrganization = React.lazy(
  () => import("./main/organization/EditOrganization")
);
const ViewOrganization = React.lazy(
  () => import("./main/organization/ViewOrganizaion")
);
const UpdateOrgAvatar = React.lazy(
  () => import("./main/organization/UpdateOrgAvatar")
);
const AddApi = React.lazy(() => import("./main/organization/AddApi"));

const AllMembers = React.lazy(() => import("./main/member/AllMembers"));
const EditMember = React.lazy(() => import("./main/member/EditMember"));

const AllServices = React.lazy(() => import("./main/service/AllServices"));
const MyServices = React.lazy(() => import("./main/service/MyServices"));

const ExchangeRate = React.lazy(() => import("./main/tools/ExchangeRate"));

// Culture provider
const CultureStateProvider = app.cultureState.provider;
const CultureContext = app.cultureState.context;

// User state
const UserStateProvider = app.userState.provider;

// Page state
const PageStateProvider = app.pageState.provider;

// Theme
// https://mui.com/customization/theming/
// https://material.io/resources/color
const theme = createTheme({
  palette: {
    primary: {
      main: "#3f51b5"
    }
  },
  components: {
    MuiCardContent: {
      styleOverrides: {
        root: {
          // other styles
          "&:last-child": {
            paddingBottom: "16px"
          }
        }
      }
    },
    MuiFormLabel: {
      styleOverrides: {
        asterisk: {
          color: "#db3131",
          "&$error": {
            color: "#ff0000"
          }
        }
      }
    }
  }
});

// Router
function MyRouter() {
  // Init state
  const [init, setInit] = React.useState(false);

  // Ready
  React.useEffect(() => {
    // Persist app data
    const cleanup = () => {
      app.persist();
    };

    window.addEventListener("unload", cleanup);
    window.addEventListener("beforeunload", cleanup);

    // Init call
    const init = () => {
      app.initCall((result) => {
        setInit(result);
      });
    };
    if (app.isReady) {
      init();
    } else {
      app.pendings.push(init);
    }
  }, []);

  return init ? (
    // Need new solution for flicker
    <React.Suspense fallback={<LinearProgress />}>
      <DynamicRouter basename={app.settings.homepage}>
        <Routes>
          <Route path="/" element={<App />} />
          <Route path="/login/about" element={<About />} />
          <Route path="/login/terms" element={<Terms />} />
          <Route path="/login/register" element={<Register />} />
          <Route
            path="/login/registerpassword/:username"
            element={<RegisterPassword />}
          />
          <Route
            path="/login/registerverify/:username"
            element={<RegisterVerify />}
          />
          <Route
            path="/login/registercomplete/:username"
            element={<RegisterComplete />}
          />
          <Route
            path="/login/callbackverify/:username"
            element={<CallbackVerify />}
          />
          <Route
            path="/login/callbackcomplete/:username"
            element={<CallbackComplete />}
          />
          <Route path="/login/password/:username" element={<Password />} />
          <Route path="/invite/:id" element={<Invite />} />
          <Route path="/home" element={<Home />}>
            <Route index element={<Dashboard />} />

            <Route path="user/loginhistory" element={<LoginHistory />} />
            <Route path="user/updateavatar" element={<UpdateAvatar />} />
            <Route path="user/changepassword" element={<ChangePassword />} />
            <Route path="user/privatedata" element={<PrivateData />} />

            <Route path="organization/all" element={<AllOrganizations />} />
            <Route
              path="organization/edit/:id"
              element={<EditOrganization />}
            />
            <Route
              path="organization/view/:id"
              element={<ViewOrganization />}
            />
            <Route
              path="organization/avatar/:id"
              element={<UpdateOrgAvatar />}
            />
            <Route path="organization/addapi" element={<AddApi />} />
            <Route path="organization/editapi/:id" element={<AddApi />} />

            <Route path="member/all" element={<AllMembers />} />
            <Route path="member/edit/:id" element={<EditMember />} />

            <Route path="service/all" element={<AllServices />} />
            <Route path="service/my" element={<MyServices />} />

            <Route path="tools/exchangerate" element={<ExchangeRate />} />
          </Route>
          <Route path="*" element={<NotFound />} />
        </Routes>
      </DynamicRouter>
    </React.Suspense>
  ) : (
    <React.Fragment />
  );
}

const getThemeCulture = (name: string) => {
  switch (name) {
    case "zh-Hans":
      return zhCN;
    case "zh-Hant":
      return zhHK;
  }
  return {};
};

const reactRoot = createRoot(root);
reactRoot.render(
  <CultureStateProvider>
    <CultureContext.Consumer>
      {(culture) => (
        <ThemeProvider
          theme={createTheme(theme, getThemeCulture(culture.state.name))}
        >
          <NotifierProvider />
          <UserStateProvider
            update={(dispatch) => {
              app.userStateDispatch = dispatch;
            }}
          >
            <PageStateProvider
              update={(dispatch) => {
                app.pageStateDispatch = dispatch;
              }}
            >
              <CssBaseline />
              <MyRouter />
            </PageStateProvider>
          </UserStateProvider>
        </ThemeProvider>
      )}
    </CultureContext.Consumer>
  </CultureStateProvider>
);

// If you want to start measuring performance in your app, pass a function
// to log results (for example: reportWebVitals(console.log))
// or send to an analytics endpoint. Learn more: https://bit.ly/CRA-vitals
/*
if (process.env.NODE_ENV !== 'production') {
  reportWebVitals(console.log);
}
*/
