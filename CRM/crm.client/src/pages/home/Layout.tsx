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
import LocalShippingIcon from "@mui/icons-material/LocalShipping";
import React from "react";
import { Outlet } from "react-router-dom";
import { app } from "../../app/MyApp";
import {
  DashboardLayout,
  NavigationItem,
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
import { Permissions } from "@etsoo/smarterp-crm";

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
    "addContact",
    "addContactInfo",
    "addProfile",
    "address",
    "addresses",
    "allProfiles",
    "app3",
    "assets",
    "contacts",
    "categories",
    "confirmOrder",
    "customers",
    "depts",
    "edit",
    "editAvatar",
    "editLogo",
    "editProfile",
    "info",
    "menuHome",
    "newTask",
    "offerings",
    "orderDeliveries",
    "orderPayments",
    "orders",
    "org",
    "permissionGroups",
    "personProducts",
    "productUnits",
    "promotions",
    "purchases",
    "signoutSuccess",
    "simpleInventory",
    "sortCategory",
    "sortOrderDelivery",
    "sortOrderPayment",
    "sortPromotion",
    "stakeholders",
    "system",
    "suppliers",
    "updateSystemSettings",
    "users",
    "view",
    "viewProfile"
  );

  // User context / state
  const { state } = React.useContext(app.userState.context);
  const { authorized, organization, organizationName } = state;

  // Organization person id
  const orgPersonId = app.userData?.system?.personId;
  const appId = app.settings.appId;

  // Organization data
  const org = React.useCallback(
    () =>
      authorized ? (
        <OrgSwitchPopover organizationName={organizationName} appId={appId} />
      ) : undefined,
    [authorized, organizationName, appId]
  );

  // Navigation
  const NAVIGATION = React.useMemo(() => {
    // Permissions
    const queryUser = app.owns(Permissions.User.Query);
    const queryOrg = app.owns(Permissions.Org.Manage);

    const items: (NavigationItem | false)[] = [
      {
        segment: "home",
        title: labels.menuHome,
        icon: <HomeIcon />,
        pageHeader: false
      },
      {
        segment: "home/contact",
        title: labels.stakeholders,
        icon: <ContactsIcon />,
        hidden: true,
        children: [
          {
            segment: "view",
            pattern: "view/:id",
            title: labels.view,
            hidden: true
          },
          {
            segment: "edit",
            pattern: "edit/:id",
            title: labels.edit,
            hidden: true
          },
          {
            segment: "avatar",
            pattern: "avatar/:id",
            title: labels.editAvatar,
            hidden: true
          },
          {
            segment: "address",
            pattern: "address/:id",
            title: labels.address,
            hidden: true
          },
          {
            segment: "addresses",
            pattern: "addresses/:id",
            title: labels.addresses,
            hidden: true
          },
          {
            segment: "info",
            pattern: "info/:id",
            title: labels.addContactInfo,
            hidden: true
          },
          {
            segment: "category",
            title: labels.categories,
            hidden: true,
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
              },
              {
                segment: "sort",
                title: labels.sortCategory,
                hidden: true
              }
            ]
          },
          {
            segment: "relation/add",
            pattern: "relation/add/:id",
            title: labels.addContact,
            hidden: true
          }
        ]
      },

      app.owns(Permissions.Customer.Query) && {
        segment: "home/customer",
        title: labels.customers,
        icon: <GroupsIcon />,
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
          },
          {
            segment: "asset",
            title: labels.assets,
            hidden: true,
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
      },
      app.owns(Permissions.Product.Query) && {
        segment: "home/product",
        title: labels.offerings,
        icon: <ShopIcon />,
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
          },
          {
            segment: "logo",
            pattern: "logo/:id",
            title: labels.editLogo,
            hidden: true
          },
          {
            segment: "view",
            pattern: "view/:id",
            title: labels.view,
            hidden: true
          },
          {
            segment: "unit",
            title: labels.productUnits,
            hidden: true
          },
          {
            segment: "category",
            title: labels.categories,
            hidden: true,
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
              },
              {
                segment: "sort",
                title: labels.sortCategory,
                hidden: true
              }
            ]
          },
          {
            segment: "personProduct",
            title: labels.personProducts,
            hidden: true,
            children: [
              {
                segment: "add",
                title: labels.add,
                hidden: true
              },
              {
                segment: "edit",
                pattern: "edit/:productId/:personId",
                title: labels.edit,
                hidden: true
              }
            ]
          },
          {
            segment: "promotion",
            title: labels.promotions,
            hidden: true,
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
              },
              {
                segment: "sort",
                title: labels.sortPromotion,
                hidden: true
              }
            ]
          }
        ]
      },

      app.owns(Permissions.Order.Query) && {
        segment: "home/order",
        title: labels.orders,
        icon: <ShoppingCartIcon />,
        children: [
          {
            segment: "add",
            title: labels.add,
            hidden: true
          },
          {
            segment: "confirm",
            title: labels.confirmOrder,
            hidden: true
          },
          {
            segment: "edit",
            pattern: "edit/:id",
            title: labels.edit,
            hidden: true
          },
          {
            segment: "view",
            pattern: "view/:id",
            title: labels.view,
            hidden: true
          },
          {
            segment: "payment",
            title: labels.orderPayments,
            hidden: true,
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
              },
              {
                segment: "sort",
                title: labels.sortOrderPayment,
                hidden: true
              }
            ]
          },
          {
            segment: "delivery",
            title: labels.orderDeliveries,
            hidden: true,
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
              },
              {
                segment: "sort",
                title: labels.sortOrderDelivery,
                hidden: true
              }
            ]
          }
        ]
      },

      app.owns(Permissions.PO.Query) && {
        segment: "home/po",
        title: labels.purchases,
        icon: <InventoryIcon />
      },

      app.owns(Permissions.Supplier.Query) && {
        segment: "home/supplier",
        title: labels.suppliers,
        icon: <HailIcon />,
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
      },
      app.userData?.system?.hasInventory === true && {
        segment: "home/inventory",
        title: labels.simpleInventory,
        icon: <LocalShippingIcon />
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
            title: labels.viewProfile,
            hidden: true
          },
          {
            segment: "edit",
            pattern: "edit/:id",
            title: labels.editProfile,
            hidden: true
          }
        ]
      }
    ];

    const allItems = items.filter((item) => item !== false);

    const orgItems: NavigationItem[] = [];

    if (queryUser) {
      orgItems.push({
        segment: "home/user",
        title: labels.users,
        icon: <GroupIcon />,
        children: [
          {
            segment: "edit",
            pattern: "edit/:id",
            title: labels.edit,
            hidden: true
          }
        ]
      });
    }

    if (queryOrg) {
      orgItems.push({
        segment: "home/system",
        title: labels.system,
        icon: <SettingsIcon />,
        children: [
          {
            segment: "updateSettings",
            title: labels.updateSystemSettings,
            hidden: true
          },
          {
            segment: "dept",
            title: labels.depts,
            hidden: true,
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
          },
          {
            segment: "group",
            title: labels.permissionGroups,
            hidden: true,
            children: [
              {
                segment: "view",
                pattern: "view/:id",
                title: labels.view,
                hidden: true
              }
            ]
          }
        ]
      });
    }

    if (orgItems.length > 0) {
      orgItems.unshift(
        {
          kind: "divider"
        },
        {
          kind: "header",
          title: labels.org
        }
      );
    }

    return [...allItems, ...orgItems];
  }, [organization, orgPersonId, state.permissionItems]);

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
