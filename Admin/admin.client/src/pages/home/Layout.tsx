import { extendTheme, Theme } from "@mui/material/styles";
import HomeIcon from "@mui/icons-material/Home";
import PeopleIcon from "@mui/icons-material/People";
import HistoryIcon from "@mui/icons-material/History";
import AccountTreeIcon from "@mui/icons-material/AccountTree";
import AppsIcon from "@mui/icons-material/Apps";
import TuneIcon from "@mui/icons-material/Tune";
import LanguageIcon from "@mui/icons-material/Language";
import ArticleIcon from "@mui/icons-material/Article";
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
import { AppSwitchPopover } from "@etsoo/smarterp-core/components";
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
    "add",
    "addApi",
    "addResource",
    "allApps",
    "allOrgs",
    "allUsers",
    "app2",
    "auditHistory",
    "customize",
    "customResources",
    "documentTemplates",
    "edit",
    "editResource",
    "externalApis",
    "menuHome",
    "signoutSuccess",
    "usageReport",
    "view"
  );

  // User context / state
  const { state } = React.useContext(app.userState.context);
  const { authorized } = state;

  // Navigation
  const NAVIGATION = React.useMemo(
    () =>
      [
        {
          segment: "home",
          title: labels.menuHome,
          icon: <HomeIcon />
        },
        {
          segment: "home/user",
          title: labels.allUsers,
          icon: <PeopleIcon />,
          children: [
            {
              segment: "view",
              pattern: "view/:id",
              title: labels.view,
              hidden: true
            }
          ]
        },
        {
          segment: "home/org",
          title: labels.allOrgs,
          icon: <AccountTreeIcon />,
          children: [
            {
              segment: "view",
              pattern: "view/:id",
              title: labels.view,
              hidden: true
            },
            {
              segment: "apis",
              pattern: "apis/:id",
              title: labels.externalApis,
              hidden: true
            },
            {
              segment: "addapi",
              title: labels.addApi,
              hidden: true
            },
            {
              segment: "editapi",
              pattern: "editapi/:id",
              title: labels.edit,
              hidden: true
            },
            {
              segment: "usage",
              pattern: "usage/:id",
              title: labels.usageReport,
              hidden: true
            }
          ]
        },
        {
          segment: "home/app",
          title: labels.allApps,
          icon: <AppsIcon />,
          children: [
            {
              segment: "view",
              pattern: "view/:id",
              title: labels.view,
              hidden: true
            }
          ]
        },
        {
          segment: "home/audithistory",
          title: labels.auditHistory,
          icon: <HistoryIcon />
        },
        {
          segment: "home/custom",
          title: labels.customize,
          icon: <TuneIcon />,
          children: [
            {
              segment: "resources",
              title: labels.customResources,
              icon: <LanguageIcon />,
              children: [
                {
                  segment: "add",
                  title: labels.addResource,
                  hidden: true
                },
                {
                  segment: "edit",
                  pattern: "edit/:id",
                  title: labels.editResource,
                  hidden: true
                }
              ]
            },
            {
              segment: "document",
              title: labels.documentTemplates,
              icon: <ArticleIcon />,
              children: [
                {
                  segment: "add",
                  title: labels.add,
                  hidden: true
                },
                {
                  segment: "edit",
                  pattern: "edit/:id",
                  title: labels.edit,
                  hidden: true
                }
              ]
            }
          ]
        }
      ] as Navigation,
    []
  );

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
        title: <AppSwitchPopover appName={labels.app2} />
      }}
    >
      <DashboardLayout
        sidebarExpandedWidth={220}
        slots={{ sidebarFooter: SidebarFooter }}
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
