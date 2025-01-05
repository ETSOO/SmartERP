import React from "react";
import { createRoot } from "react-dom/client";
import "./index.css";
import { app, NotifierProvider } from "./app/MyApp";
import {
  createTheme,
  CssBaseline,
  LinearProgress,
  ThemeProvider
} from "@mui/material";
import { RouterProvider, RouterProviderProps } from "react-router-dom";
import { createDynamicRouter } from "@etsoo/react";
import { zhCN, zhHK } from "@mui/material/locale";
import AuthSuccess from "./pages/login/AuthSuccess";
import Index from "./pages/Index";
import Home from "./pages/home/Home";
import Layout from "./pages/home/Layout";

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
            path: "/home/app/all",
            lazy: async () => {
              const AllApps = await import("./pages/home/app/AllApps");
              return { Component: AllApps.default };
            }
          },
          {
            path: "/home/app/my",
            lazy: async () => {
              const MyApps = await import("./pages/home/app/MyApps");
              return { Component: MyApps.default };
            }
          },
          {
            path: "/home/member/all",
            lazy: async () => {
              const AllMembers = await import("./pages/home/member/AllMembers");
              return { Component: AllMembers.default };
            }
          },
          {
            path: "/home/org/my",
            lazy: async () => {
              const AllOrgs = await import("./pages/home/org/AllOrgs");
              return { Component: AllOrgs.default };
            }
          },
          {
            path: "/home/user/audithistory",
            lazy: async () => {
              const LoginHistory = await import(
                "./pages/home/user/AuditHistory"
              );
              return { Component: LoginHistory.default };
            }
          },
          {
            path: "/home/user/changepassword",
            lazy: async () => {
              const ChangePassword = await import(
                "./pages/home/user/ChangePassword"
              );
              return { Component: ChangePassword.default };
            }
          },
          {
            path: "/home/user/data",
            lazy: async () => {
              const UserData = await import("./pages/home/user/UserData");
              return { Component: UserData.default };
            }
          },
          {
            path: "/home/user/updateavatar",
            lazy: async () => {
              const UpdateAvatar = await import(
                "./pages/home/user/UpdateAvatar"
              );
              return { Component: UpdateAvatar.default };
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
              <AppRouterProvider router={router} />
            </PageStateProvider>
          </UserStateProvider>
        </ThemeProvider>
      )}
    </CultureContext.Consumer>
  </CultureStateProvider>
);
