import { extendTheme, Theme } from "@mui/material/styles";
import HomeIcon from "@mui/icons-material/Home";
import ContactsIcon from "@mui/icons-material/Contacts";
import ShoppingCartIcon from "@mui/icons-material/ShoppingCart";
import GroupsIcon from "@mui/icons-material/Groups";
import ShopIcon from "@mui/icons-material/Shop";
import HailIcon from "@mui/icons-material/Hail";
import InventoryIcon from "@mui/icons-material/Inventory";
import GroupIcon from "@mui/icons-material/Group";
import SettingsIcon from "@mui/icons-material/Settings";
import DescriptionIcon from "@mui/icons-material/Description";
import React from "react";
import { Outlet } from "react-router-dom";
import { app } from "../../app/MyApp";
import {
  DashboardLayout,
  Navigation,
  PageContainer,
  PageDataContextProvider,
  Session,
  SidebarFooterProps
} from "@etsoo/toolpad";
import { AppProvider } from "@etsoo/toolpad/react-router-dom";
import { CoreCulture } from "@etsoo/smarterp-core";
import {
  AppSwitchPopover,
  OrgSwitchPopover
} from "@etsoo/smarterp-core/components";
import Typography from "@mui/material/Typography";
import useMediaQuery from "@mui/material/useMediaQuery";

function SidebarFooter({ mini }: SidebarFooterProps) {
  return (
    <Typography
      variant="caption"
      sx={{ m: 1, whiteSpace: "nowrap", overflow: "hidden" }}
    >
      {mini
        ? ""
        : `© ${new Date().getFullYear()} ${app.get("etsoor")} (${
            import.meta.env.VITE_APP_VERSION
          })`}
    </Typography>
  );
}

const myTheme = extendTheme({
  colorSchemes: { light: true, dark: false }
});

export default function Home() {
  // Small than sm
  const smDown = useMediaQuery<Theme>((theme) => theme.breakpoints.down("sm"));
  app.smDown = smDown;

  const mdUp = useMediaQuery<Theme>((theme) => theme.breakpoints.up("md"));
  app.mdUp = mdUp;

  // Labels
  const labels = app.getLabels(
    "addProfile",
    "allProfiles",
    "app3",
    "contacts",
    "customers",
    "editAvatar",
    "editProfile",
    "info",
    "menuHome",
    "newTask",
    "offerings",
    "orders",
    "org",
    "purchases",
    "signoutSuccess",
    "system",
    "suppliers",
    "updateSystemSettings",
    "users",
    "view"
  );

  // User context / state
  const { state } = React.useContext(app.userState.context);
  const { authorized, organization, organizationName } = state;

  // Organization data
  const org = React.useCallback(
    () =>
      authorized ? (
        <OrgSwitchPopover organizationName={organizationName} />
      ) : undefined,
    [authorized, organizationName]
  );

  // Navigation
  const NAVIGATION = React.useMemo(() => {
    const items: Navigation = [
      {
        segment: "home",
        title: labels.menuHome,
        icon: <HomeIcon />,
        pageHeader: false
      },
      {
        segment: "home/contact",
        title: labels.contacts,
        icon: <ContactsIcon />,
        children: [
          {
            segment: "view",
            pattern: "view/:id",
            title: labels.view,
            hidden: true
          },
          {
            segment: "avatar",
            pattern: "avatar/:id",
            title: labels.editAvatar,
            hidden: true
          }
        ]
      },
      {
        segment: "home/order",
        title: labels.orders,
        icon: <ShoppingCartIcon />
      },
      {
        segment: "home/customer",
        title: labels.customers,
        icon: <GroupsIcon />
      },
      {
        segment: "home/product",
        title: labels.offerings,
        icon: <ShopIcon />
      },
      {
        segment: "home/po",
        title: labels.purchases,
        icon: <InventoryIcon />
      },
      {
        segment: "home/supplier",
        title: labels.suppliers,
        icon: <HailIcon />
      },
      {
        segment: "home/profile",
        title: labels.allProfiles,
        icon: <GroupsIcon />,
        hidden: true,
        children: [
          {
            segment: "add",
            title: labels.addProfile,
            hidden: true
          },
          {
            segment: "addTask",
            title: labels.newTask,
            hidden: true
          },
          {
            segment: "view",
            pattern: "view/:id",
            title: labels.view,
            hidden: true
          },
          {
            segment: "edit",
            pattern: "edit/:id",
            title: labels.editProfile,
            hidden: true
          }
        ]
      },
      {
        kind: "divider"
      },
      {
        kind: "header",
        title: labels.org
      },
      {
        segment: "home/org/data",
        title: labels.info,
        icon: <DescriptionIcon />
      },
      {
        segment: "home/user",
        title: labels.users,
        icon: <GroupIcon />
      },
      {
        segment: "home/system",
        title: labels.system,
        icon: <SettingsIcon />,
        children: [
          {
            segment: "updateSettings",
            title: labels.updateSystemSettings,
            hidden: true
          }
        ]
      }
    ];

    return items;
  }, [organization]);

  // When unauthorized (by refresh)
  // Return blank and try login
  React.useEffect(() => {
    if (authorized) {
      app.checkSession(async (isSame) => {
        if (!isSame) {
          // First time login
          const result = await app.core.userApi.checkSession({
            showLoading: false
          });
          if (result == null || !result.ok) return false;

          // Load organization custom resources
          //app.core.authApi.loadCustomResources();
        }
      });
    } else {
      app.tryLogin();
    }
  }, [authorized]);

  if (!authorized) {
    return <React.Fragment />;
  }

  // User data
  const user: Session["user"] = {
    image: state.avatar,
    name: state.name,
    latinName: `${state.latinFamilyName ?? ""} ${state.latinGivenName ?? ""}`
  };

  return (
    <AppProvider
      authentication={{
        signIn: () => app.toLoginPage(),
        signOut: () => {
          app
            .signout(() => false)
            .then(() => {
              app.notifier.alert(labels.signoutSuccess, () =>
                app.loadCore(false)
              );
            });
        }
      }}
      localeText={CoreCulture.getToolpadLocale(app)}
      session={{ user }}
      navigation={NAVIGATION}
      theme={myTheme}
      branding={{
        logo: "",
        title: <AppSwitchPopover appName={labels.app3} />
      }}
    >
      <DashboardLayout
        sidebarExpandedWidth={220}
        slots={{ sidebarFooter: SidebarFooter, toolbarActions: org }}
      >
        <PageDataContextProvider>
          <PageContainer titleBar={false}>
            <Outlet />
          </PageContainer>
        </PageDataContextProvider>
      </DashboardLayout>
    </AppProvider>
  );
}
