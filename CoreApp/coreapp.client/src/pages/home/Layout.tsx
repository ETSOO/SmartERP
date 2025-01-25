import { extendTheme, Theme } from "@mui/material/styles";
import HomeIcon from "@mui/icons-material/Home";
import PaidIcon from "@mui/icons-material/Paid";
import PeopleIcon from "@mui/icons-material/People";
import PortraitIcon from "@mui/icons-material/Portrait";
import LockIcon from "@mui/icons-material/Lock";
import HistoryIcon from "@mui/icons-material/History";
import AccountCircleIcon from "@mui/icons-material/AccountCircle";
import AccountTreeIcon from "@mui/icons-material/AccountTree";
import AppsIcon from "@mui/icons-material/Apps";
import React from "react";
import { Typography, useMediaQuery } from "@mui/material";
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
    "allApps",
    "allMembers",
    "app1",
    "auditHistory",
    "changePassword",
    "currentOrg",
    "joinedOrgs",
    "me",
    "menuHome",
    "personalData",
    "purchasedApps",
    "signoutSuccess",
    "switchOrg",
    "updateAvator"
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
        segment: "home/org/my",
        title: labels.joinedOrgs,
        icon: <AccountTreeIcon />,
        subs: ["/home/org/.*"]
      },
      {
        segment: "home/app/all",
        title: labels.allApps,
        icon: <AppsIcon />
      },
      {
        kind: "divider"
      },
      {
        kind: "header",
        title: labels.me
      },
      {
        segment: "home/user/data",
        title: labels.personalData,
        icon: <PortraitIcon />
      },
      {
        segment: "home/user/updateavatar",
        title: labels.updateAvator,
        icon: <AccountCircleIcon />
      },
      {
        segment: "home/user/changepassword",
        title: labels.changePassword,
        icon: <LockIcon />
      },
      {
        segment: "home/user/audithistory",
        title: labels.auditHistory,
        icon: <HistoryIcon />
      }
    ];

    if (organization) {
      let spliceIndex = 3;
      if (app.isManagerUser()) {
        items.splice(1, 0, {
          segment: "home/member/all",
          title: labels.allMembers,
          icon: <PeopleIcon />,
          subs: ["/home/member/.*"]
        });

        spliceIndex++;
      }

      if (app.isFinanceUser()) {
        items.splice(spliceIndex, 0, {
          segment: "home/app/my",
          title: labels.purchasedApps,
          icon: <PaidIcon />,
          subs: ["/home/app/.*"]
        });
      }
    }

    return items;
  }, [organization]);

  // When unauthorized (by refresh)
  // Return blank and try login
  React.useEffect(() => {
    if (!authorized) app.tryLogin();
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
        title: <AppSwitchPopover appName={labels.app1} />
      }}
    >
      <DashboardLayout
        sidebarExpandedWidth={220}
        slots={{ sidebarFooter: SidebarFooter, toolbarActions: org }}
      >
        <PageDataContextProvider>
          <PageContainer defaultTitle="" maxWidth="xl">
            <Outlet />
          </PageContainer>
        </PageDataContextProvider>
      </DashboardLayout>
    </AppProvider>
  );
}
