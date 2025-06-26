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
import LinearProgress from "@mui/material/LinearProgress";
import CssBaseline from "@mui/material/CssBaseline";

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
            path: "/home/myapp/view/:id",
            lazy: async () => {
              const ViewApp = await import("./pages/home/app/ViewApp");
              return { Component: ViewApp.default };
            }
          },
          {
            path: "/home/myapp",
            lazy: async () => {
              const MyApps = await import("./pages/home/app/MyApps");
              return { Component: MyApps.default };
            }
          },
          {
            path: "/home/myapp/edit/:id",
            lazy: async () => {
              const EditApp = await import("./pages/home/app/EditApp");
              return { Component: EditApp.default };
            }
          },
          {
            path: "/home/member",
            lazy: async () => {
              const AllMembers = await import("./pages/home/member/AllMembers");
              return { Component: AllMembers.default };
            }
          },
          {
            path: "/home/member/view/:id",
            lazy: async () => {
              const ViewMember = await import("./pages/home/member/ViewMember");
              return { Component: ViewMember.default };
            }
          },
          {
            path: "/home/member/edit/:id",
            lazy: async () => {
              const EditMember = await import("./pages/home/member/EditMember");
              return { Component: EditMember.default };
            }
          },
          {
            path: "/home/member/avatar/:id",
            lazy: async () => {
              const MemberUpdateAvatar = await import(
                "./pages/home/member/MemberAvatar"
              );
              return { Component: MemberUpdateAvatar.default };
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
            path: "/home/org/my/:id",
            lazy: async () => {
              const ViewOrg = await import("./pages/home/org/ViewOrg");
              return { Component: ViewOrg.default };
            }
          },
          {
            path: "/home/org/edit/:id",
            lazy: async () => {
              const EditOrg = await import("./pages/home/org/EditOrg");
              return { Component: EditOrg.default };
            }
          },
          {
            path: "/home/org/avatar/:id",
            lazy: async () => {
              const OrgUpdateAvatar = await import(
                "./pages/home/org/UpdateAvatar"
              );
              return { Component: OrgUpdateAvatar.default };
            }
          },
          {
            path: "/home/org/customresource/:id",
            lazy: async () => {
              const CustomResource = await import(
                "./pages/home/org/CustomResource"
              );
              return { Component: CustomResource.default };
            }
          },
          {
            path: "/home/org/addcustomresource",
            lazy: async () => {
              const AddCustomResource = await import(
                "./pages/home/org/AddCustomResource"
              );
              return { Component: AddCustomResource.default };
            }
          },
          {
            path: "/home/org/editcustomresource/:id",
            lazy: async () => {
              const AddCustomResource = await import(
                "./pages/home/org/AddCustomResource"
              );
              return { Component: AddCustomResource.default };
            }
          },
          {
            path: "/home/org/apis/:id",
            lazy: async () => {
              const AllApis = await import("./pages/home/org/AllApis");
              return { Component: AllApis.default };
            }
          },
          {
            path: "/home/org/addapi",
            lazy: async () => {
              const AddApi = await import("./pages/home/org/AddApi");
              return { Component: AddApi.default };
            }
          },
          {
            path: "/home/org/editapi/:id",
            lazy: async () => {
              const AddApi = await import("./pages/home/org/AddApi");
              return { Component: AddApi.default };
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
            path: "/home/user/data/edit",
            lazy: async () => {
              const EditUser = await import("./pages/home/user/EditUser");
              return { Component: EditUser.default };
            }
          },
          {
            path: "/home/user/data/addemail",
            lazy: async () => {
              const AddEmail = await import("./pages/home/user/AddEmail");
              return { Component: AddEmail.default };
            }
          },
          {
            path: "/home/user/data/addmobile",
            lazy: async () => {
              const AddMobile = await import("./pages/home/user/AddMobile");
              return { Component: AddMobile.default };
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
