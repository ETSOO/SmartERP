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
    "app3",
    "contacts",
    "customers",
    "info",
    "menuHome",
    "offerings",
    "orders",
    "org",
    "purchases",
    "signoutSuccess",
    "system",
    "suppliers",
    "users"
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
        icon: <HomeIcon />
      },
      {
        segment: "home/contact/all",
        title: labels.contacts,
        icon: <ContactsIcon />,
        subs: ["/home/contact/.*"]
      },
      {
        segment: "home/order/all",
        title: labels.orders,
        icon: <ShoppingCartIcon />,
        subs: ["/home/order/.*"]
      },
      {
        segment: "home/customer/all",
        title: labels.customers,
        icon: <GroupsIcon />,
        subs: ["/home/customer/.*"]
      },
      {
        segment: "home/product/all",
        title: labels.offerings,
        icon: <ShopIcon />,
        subs: ["/home/product/.*"]
      },
      {
        segment: "home/po/all",
        title: labels.purchases,
        icon: <InventoryIcon />,
        subs: ["/home/po/.*"]
      },
      {
        segment: "home/supplier/all",
        title: labels.suppliers,
        icon: <HailIcon />,
        subs: ["/home/supplier/.*"]
      },
      {
        kind: "divider"
      },
      {
        kind: "header",
        title: labels.org
      },
      {
        segment: "home/org/profile",
        title: labels.info,
        icon: <DescriptionIcon />,
        subs: ["/home/org/.*"]
      },
      {
        segment: "home/user/all",
        title: labels.users,
        icon: <GroupIcon />,
        subs: ["/home/user/.*"]
      },
      {
        segment: "home/system/all",
        title: labels.system,
        icon: <SettingsIcon />,
        subs: ["/home/system/.*"]
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
          <PageContainer defaultTitle="">
            <Outlet />
          </PageContainer>
        </PageDataContextProvider>
      </DashboardLayout>
    </AppProvider>
  );
}
