import React from "react";
import { createRoot } from "react-dom/client";
import "./index.css";
import App from "./App";
import { app, NotifierProvider } from "./app/MyApp";
import {
  createTheme,
  CssBaseline,
  LinearProgress,
  ThemeProvider
} from "@mui/material";
import { Route, Routes } from "react-router-dom";
import { DynamicRouter } from "@etsoo/react";
import { zhCN, zhHK } from "@mui/material/locale";
import AuthSuccess from "./login/AuthSuccess";
import Home from "./main/Home";

// Lazy load components
const AuthFail = React.lazy(() => import("./login/AuthFail"));

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

function MyRouter() {
  // Init state
  const [init, setInit] = React.useState(app.isReady);

  const messageHandler = React.useCallback((event: MessageEvent<any>) => {
    if (app.coreOrigin !== event.origin || !Array.isArray(event.data)) return;

    const [type, data] = event.data;

    switch (type) {
      case "login":
        globalThis.location.replace(data);
        break;
    }
  }, []);

  // Ready
  React.useEffect(() => {
    if (app.isReady) {
      setInit(true);
    } else {
      app.pendings.push(() => setInit(true));
    }
  }, [app.isReady]);

  React.useEffect(() => {
    // Persist app data
    const cleanup = () => {
      app.persist();
    };

    window.addEventListener("unload", cleanup);
    window.addEventListener("beforeunload", cleanup);
    window.addEventListener("message", messageHandler);

    return () => {
      cleanup();
      window.removeEventListener("unload", cleanup);
      window.removeEventListener("beforeunload", cleanup);
      window.removeEventListener("message", messageHandler);
    };
  }, []);

  return init ? (
    // Need new solution for flicker
    <React.Suspense fallback={<LinearProgress />}>
      <DynamicRouter basename={app.settings.homepage}>
        <Routes>
          <Route path="/" element={<App />} />
          <Route path="/login/authfail" element={<AuthFail />} />
          <Route path="/login/authsuccess" element={<AuthSuccess />} />
          <Route path="/home" element={<Home />}></Route>
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

const reactRoot = createRoot(document.getElementById("root")!);
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
