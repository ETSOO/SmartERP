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
            path: "/home/contact",
            lazy: async () => {
              const AllContacts = await import(
                "./pages/home/contact/AllContacts"
              );
              return { Component: AllContacts.default };
            }
          },
          {
            path: "/home/contact/view/:id",
            lazy: async () => {
              const ViewContact = await import(
                "./pages/home/contact/ViewContact"
              );
              return { Component: ViewContact.default };
            }
          },
          {
            path: "/home/contact/avatar/:id",
            lazy: async () => {
              const ContactAvatar = await import(
                "./pages/home/contact/ContactAvatar"
              );
              return { Component: ContactAvatar.default };
            }
          },

          {
            path: "/home/customer",
            lazy: async () => {
              const AllCustomers = await import(
                "./pages/home/customer/AllCustomers"
              );
              return { Component: AllCustomers.default };
            }
          },

          {
            path: "/home/order",
            lazy: async () => {
              const AllOrders = await import("./pages/home/order/AllOrders");
              return { Component: AllOrders.default };
            }
          },

          {
            path: "/home/report",
            lazy: async () => {
              const AllReports = await import("./pages/home/report/AllReports");
              return { Component: AllReports.default };
            }
          },

          {
            path: "/home/po",
            lazy: async () => {
              const AllPOs = await import("./pages/home/po/AllPOs");
              return { Component: AllPOs.default };
            }
          },

          {
            path: "/home/product",
            lazy: async () => {
              const AllProducts = await import(
                "./pages/home/product/AllProducts"
              );
              return { Component: AllProducts.default };
            }
          },

          {
            path: "/home/profile",
            lazy: async () => {
              const AllProfiles = await import(
                "./pages/home/profile/AllProfiles"
              );
              return { Component: AllProfiles.default };
            }
          },
          {
            path: "/home/profile/add",
            lazy: async () => {
              const AddProfile = await import(
                "./pages/home/profile/AddProfile"
              );
              return { Component: AddProfile.default };
            }
          },
          {
            path: "/home/profile/addTask",
            lazy: async () => {
              const AddTask = await import("./pages/home/profile/AddTask");
              return { Component: AddTask.default };
            }
          },
          {
            path: "/home/profile/edit/:id",
            lazy: async () => {
              const AddProfile = await import(
                "./pages/home/profile/AddProfile"
              );
              return { Component: AddProfile.default };
            }
          },
          {
            path: "/home/profile/view/:id",
            lazy: async () => {
              const ViewProfile = await import(
                "./pages/home/profile/ViewProfile"
              );
              return { Component: ViewProfile.default };
            }
          },

          {
            path: "/home/supplier",
            lazy: async () => {
              const AllSuppliers = await import(
                "./pages/home/supplier/AllSuppliers"
              );
              return { Component: AllSuppliers.default };
            }
          },

          {
            path: "/home/inventory",
            lazy: async () => {
              const AllInventory = await import(
                "./pages/home/inventory/AllInventory"
              );
              return { Component: AllInventory.default };
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
            path: "/home/user/edit/:id",
            lazy: async () => {
              const EditUser = await import("./pages/home/user/EditUser");
              return { Component: EditUser.default };
            }
          },
          {
            path: "/home/system",
            lazy: async () => {
              const AllSystem = await import("./pages/home/system/AllSystem");
              return { Component: AllSystem.default };
            }
          },
          {
            path: "/home/system/updateSettings",
            lazy: async () => {
              const UpdateSettings = await import(
                "./pages/home/system/UpdateSettings"
              );
              return { Component: UpdateSettings.default };
            }
          },
          {
            path: "/home/system/dept",
            lazy: async () => {
              const AllDepts = await import("./pages/home/dept/AllDepts");
              return { Component: AllDepts.default };
            }
          },
          {
            path: "/home/system/dept/add",
            lazy: async () => {
              const AddDept = await import("./pages/home/dept/AddDept");
              return { Component: AddDept.default };
            }
          },
          {
            path: "/home/system/dept/edit/:id",
            lazy: async () => {
              const AddDept = await import("./pages/home/dept/AddDept");
              return { Component: AddDept.default };
            }
          },
          {
            path: "/home/system/group",
            lazy: async () => {
              const AllGroups = await import("./pages/home/group/AllGroups");
              return { Component: AllGroups.default };
            }
          },
          {
            path: "/home/system/group/view/:id",
            lazy: async () => {
              const ViewGroup = await import("./pages/home/group/ViewGroup");
              return { Component: ViewGroup.default };
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
