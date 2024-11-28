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
import { Button, ButtonGroup, Typography, useMediaQuery } from "@mui/material";
import { Outlet } from "react-router-dom";
import { app } from "../../app/MyApp";
import {
  DashboardLayout,
  Navigation,
  PageContainer,
  Session,
  SidebarFooterProps
} from "@etsoo/toolpad";
import { AppProvider } from "@etsoo/toolpad/react-router-dom";
import { CoreCulture } from "@etsoo/smarterp-core";

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
    "joinedOrgs",
    "me",
    "menuHome",
    "personalData",
    "purchasedApps",
    "signoutSuccess",
    "updateAvator"
  );

  // User context / state
  const { state } = React.useContext(app.userState.context);
  const { authorized, organizationName } = state;

  // Organization data
  const org = React.useCallback(
    () => (
      <React.Fragment>
        {organizationName && (
          <ButtonGroup variant="text">
            <Button
              sx={{ display: { xs: "none", md: "block" } }}
              title="当前机构"
            >
              {organizationName}
            </Button>
            <Button title="点击切换到其他机构">
              <AccountTreeIcon />
            </Button>
          </ButtonGroup>
        )}
      </React.Fragment>
    ),
    [organizationName]
  );

  // Navigation
  const NAVIGATION = React.useMemo<Navigation>(
    () => [
      {
        segment: "home",
        title: labels.menuHome,
        icon: <HomeIcon />
      },
      {
        segment: "home/member/all",
        title: labels.allMembers,
        icon: <PeopleIcon />
      },
      {
        segment: "home/organization/my",
        title: labels.joinedOrgs,
        icon: <AccountTreeIcon />
      },
      {
        segment: "home/app/my",
        title: labels.purchasedApps,
        icon: <PaidIcon />
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
        segment: "home/user/loginhistory",
        title: labels.auditHistory,
        icon: <HistoryIcon />
      }
    ],
    []
  );

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
        title: labels.app1
      }}
    >
      <DashboardLayout
        sidebarExpandedWidth={220}
        slots={{ sidebarFooter: SidebarFooter, toolbarActions: org }}
      >
        <PageContainer title="">
          <Outlet />
        </PageContainer>
      </DashboardLayout>
    </AppProvider>
  );
}
