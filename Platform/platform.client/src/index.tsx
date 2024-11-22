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
import { app, NotifierProvider } from "./app/SmartApp";
import { Route, Routes } from "react-router-dom";
import { DynamicRouter } from "@etsoo/react";
import { zhCN, zhHK } from "@mui/material/locale";
import AuthSuccess from "./login/AuthSuccess";

// Root
const root = document.getElementById("root")!;

// Lazy load components
const About = React.lazy(() => import("./login/About"));
const AuthFail = React.lazy(() => import("./login/AuthFail"));
// No direct registration
// const Register = React.lazy(() => import("./login/Register"));
const Register10 = React.lazy(() => import("./login/Register10"));
const Register20 = React.lazy(() => import("./login/Register20"));
const Register30 = React.lazy(() => import("./login/Register30"));
const CallbackVerify = React.lazy(() => import("./login/CallbackVerify"));
const CallbackComplete = React.lazy(() => import("./login/CallbackComplete"));
const Terms = React.lazy(() => import("./login/Terms"));
const Invite = React.lazy(() => import("./login/Invite"));

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
  const [init, setInit] = React.useState(app.isReady);

  // Ready
  React.useEffect(() => {
    if (app.isReady) {
      setInit(true);
    } else {
      app.pendings.push(() => {
        app.initCall((result) => {
          setInit(result);
        });
      });
    }
  }, [app.isReady]);

  React.useEffect(() => {
    // Persist app data
    const cleanup = () => {
      app.dispose();
    };

    window.addEventListener("unload", cleanup);
    window.addEventListener("beforeunload", cleanup);

    return () => {
      cleanup();
      window.removeEventListener("unload", cleanup);
      window.removeEventListener("beforeunload", cleanup);
    };
  }, []);

  return init ? (
    // Need new solution for flicker
    <React.Suspense fallback={<LinearProgress />}>
      <DynamicRouter basename={app.settings.homepage}>
        <Routes>
          <Route path="/" element={<App />} />
          <Route path="/login/about" element={<About />} />
          <Route path="/login/authfail" element={<AuthFail />} />
          <Route path="/login/authsuccess" element={<AuthSuccess />} />
          <Route path="/login/terms" element={<Terms />} />
          <Route path="/login/register" element={<Register10 />} />
          <Route path="/login/register10" element={<Register10 />} />
          <Route path="/login/register20" element={<Register20 />} />
          <Route path="/login/register30" element={<Register30 />} />
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
