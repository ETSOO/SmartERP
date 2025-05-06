import React from "react";
import { createRoot } from "react-dom/client";
import "./index.css";
import { app, NotifierProvider } from "./app/MyApp";
import { RouterProvider, RouterProviderProps } from "react-router-dom";
import { createDynamicRouter } from "@etsoo/react";
import { zhCN, zhHK } from "@mui/material/locale";
import AuthSuccess from "./pages/login/AuthSuccess";
import Index from "./pages/Index";
import Home from "./pages/home/Home";
import Layout from "./pages/home/Layout";
import { ReactAppContext } from "@etsoo/materialui";
import { createTheme, ThemeProvider } from "@mui/material/styles";
import CssBaseline from "@mui/material/CssBaseline";
import LinearProgress from "@mui/material/LinearProgress";

// Culture provider
const CultureStateProvider = app.cultureState.provider;
const CultureContext = app.cultureState.context;

// User state
const UserStateProvider = app.userState.provider;

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
const router = createDynamicRouter([
  {
    hydrateFallbackElement: <LinearProgress />,
    children: [
      {
        path: "/",
        Component: Index
      },
      {
        path: "/login/authsuccess",
        Component: AuthSuccess
      },
      {
        path: "/login/authfail",
        lazy: async () => {
          const AuthFail = await import("./pages/login/AuthFail");
          return { Component: AuthFail.default };
        }
      },
      {
        path: "/home",
        Component: Layout,
        children: [
          {
            path: "/home",
            Component: Home
          },
          {
            path: "/home/app",
            lazy: async () => {
              const AllApps = await import("./pages/home/app/AllApps");
              return { Component: AllApps.default };
            }
          },
          {
            path: "/home/app/view/:id",
            lazy: async () => {
              const ViewApp = await import("./pages/home/app/ViewApp");
              return { Component: ViewApp.default };
            }
          },
          {
            path: "/home/org",
            lazy: async () => {
              const AllOrgs = await import("./pages/home/org/AllOrgs");
              return { Component: AllOrgs.default };
            }
          },
          {
            path: "/home/org/view/:id",
            lazy: async () => {
              const ViewOrg = await import("./pages/home/org/ViewOrg");
              return { Component: ViewOrg.default };
            }
          },
          {
            path: "/home/user",
            lazy: async () => {
              const AllUsers = await import("./pages/home/user/AllUsers");
              return { Component: AllUsers.default };
            }
          },
          {
            path: "/home/user/view/:id",
            lazy: async () => {
              const ViewUser = await import("./pages/home/user/ViewUser");
              return { Component: ViewUser.default };
            }
          },
          {
            path: "/home/audithistory",
            lazy: async () => {
              const AuditHistory = await import(
                "./pages/home/system/AuditHistory"
              );
              return { Component: AuditHistory.default };
            }
          },
          {
            path: "/home/custom",
            lazy: async () => {
              const AllCustom = await import("./pages/home/custom/AllCustom");
              return { Component: AllCustom.default };
            }
          },
          {
            path: "/home/custom/resources",
            lazy: async () => {
              const CustomResources = await import(
                "./pages/home/custom/CustomResources"
              );
              return { Component: CustomResources.default };
            }
          },
          {
            path: "/home/custom/resources/add",
            lazy: async () => {
              const AddCustomResource = await import(
                "./pages/home/custom/AddCustomResource"
              );
              return { Component: AddCustomResource.default };
            }
          },
          {
            path: "/home/custom/resources/edit/:id",
            lazy: async () => {
              const AddCustomResource = await import(
                "./pages/home/custom/AddCustomResource"
              );
              return { Component: AddCustomResource.default };
            }
          }
        ]
      }
    ]
  }
]);

const getThemeCulture = (name: string) => {
  switch (name) {
    case "zh-Hans":
      return zhCN;
    case "zh-Hant":
      return zhHK;
  }
  return {};
};

function AppRouterProvider(props: RouterProviderProps) {
  // Init state
  const [init, setInit] = React.useState(app.isReady);

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
      app.dispose();
    };

    window.addEventListener("beforeunload", cleanup);

    return () => {
      cleanup();
      window.removeEventListener("beforeunload", cleanup);
    };
  }, []);

  return React.useMemo(() => {
    return init ? <RouterProvider {...props} /> : <React.Fragment />;
  }, [init]);
}

const reactRoot = createRoot(document.getElementById("root")!);
reactRoot.render(
  <ReactAppContext.Provider value={app}>
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
              <CssBaseline />
              <AppRouterProvider router={router} />
            </UserStateProvider>
          </ThemeProvider>
        )}
      </CultureContext.Consumer>
    </CultureStateProvider>
  </ReactAppContext.Provider>
);
