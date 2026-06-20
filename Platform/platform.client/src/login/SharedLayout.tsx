import React from "react";
import {
  ButtonLink,
  HBox,
  IconButtonLink,
  MUGlobal,
  VBox
} from "@etsoo/materialui";
import logo from "./../images/etsoo.png";
import { app } from "../app/SmartApp";
import Box from "@mui/material/Box";
import Typography from "@mui/material/Typography";
import HomeIcon from "@mui/icons-material/Home";

/**
 * Shared layout props
 */
export type SharedLayoutProps = {
  /**
   * App name
   */
  appName?: string;

  /**
   * Header right part component
   */
  headerRight?: React.ReactNode;

  /**
   * Page right part component
   */
  pageRight?: React.ReactNode;

  /**
   * Naviagating buttons
   */
  buttons?: React.ReactElement | React.ReactElement[];

  /**
   * Main part children
   */
  children?: React.ReactNode;

  /**
   * Bottom components
   */
  bottom?: React.ReactNode;

  /**
   * Bottom added area components
   */
  bottomAdd?: React.ReactNode;

  /**
   * Home URL
   */
  homeUrl?: string;

  /**
   * Title
   */
  title: React.ReactNode;

  /**
   * Subtitle
   */
  subTitle?: React.ReactNode;

  /**
   * Visibility
   */
  visible?: boolean;

  /**
   * Live minutes
   */
  liveMinutes?: number;
};

/**
 * Shared layout
 * @param props Props
 * @returns Component
 */
export function SharedLayout(props: SharedLayoutProps) {
  // Destructure
  const {
    appName,
    headerRight,
    pageRight,
    buttons,
    children,
    bottom,
    bottomAdd,
    homeUrl,
    title,
    subTitle,
    visible = true,
    liveMinutes = 0
  } = props;

  // Culture context
  const Context = app.cultureState.context;

  if (homeUrl && Array.isArray(buttons) && buttons.length === 0) {
    buttons.push(
      <ButtonLink key="home" href={homeUrl}>
        {app.get("home")}
      </ButtonLink>
    );
  }

  React.useEffect(() => {
    if (liveMinutes > 0) {
      const timer = setTimeout(() => {
        app.navigate(homeUrl ?? "./../../");
      }, liveMinutes * 60000);

      return () => {
        clearTimeout(timer);
      };
    }
  }, [liveMinutes, homeUrl]);

  return (
    <React.Fragment>
      <Box
        sx={{
          position: "relative",
          padding: MUGlobal.pagePaddings,
          width: { xs: "100%", sm: 450 },
          marginLeft: "auto",
          marginRight: "auto",
          visibility: visible ? "visible" : "hidden"
        }}
      >
        <HBox
          sx={{
            padding: "16px 24px 12px 24px",
            justifyContent: "space-between",
            alignItems: "flex-end"
          }}
        >
          <Box
            component="img"
            src={logo}
            sx={{
              height: { xs: "36px", sm: "48px" },
              userSelect: "none"
            }}
          />
          {headerRight}
          <Context.Consumer>
            {(value) => (
              <VBox sx={{ alignItems: "flex-end" }}>
                <Typography variant="subtitle1">
                  {value.get<string>("appName")} (AI+)
                </Typography>
                <Typography variant="subtitle2" sx={{ fontWeight: "bold" }}>
                  {appName
                    ? (value.get<string>(appName) ?? appName)
                    : app.get("login")}
                </Typography>
              </VBox>
            )}
          </Context.Consumer>
        </HBox>
        <Typography
          variant="caption"
          component="div"
          align="center"
          sx={{ paddingBottom: "4px" }}
        >
          {app.get("slogan")}
        </Typography>
        <VBox
          spacing={2}
          sx={{
            alignItems: "flex-start",
            borderRadius: 0.5,
            boxShadow: 1,
            backgroundColor: "#fff",
            padding: 3
          }}
        >
          <VBox sx={{ width: "100%" }}>
            <HBox
              sx={{
                justifyContent: "space-between",
                alignItems: "center"
              }}
            >
              <Typography variant="h5">{title}</Typography>
              {pageRight}
              {homeUrl ? (
                <IconButtonLink href={homeUrl} title={app.get("home")}>
                  <HomeIcon />
                </IconButtonLink>
              ) : undefined}
            </HBox>
            {subTitle &&
              (typeof subTitle === "string" ? (
                <Typography
                  variant="body2"
                  sx={{ color: (theme) => theme.palette.grey[600] }}
                >
                  {subTitle}
                </Typography>
              ) : (
                subTitle
              ))}
          </VBox>
          {children}
          <HBox
            sx={{
              width: "100%",
              justifyContent:
                Array.isArray(buttons) && buttons.length > 1
                  ? "space-between"
                  : "flex-end"
            }}
            spacing={2}
          >
            {buttons}
          </HBox>
        </VBox>
        <HBox
          spacing={2}
          sx={{
            padding: "8px 24px",
            fontSize: "smaller",
            justifyContent: "center"
          }}
        >
          {bottom}
        </HBox>
        {bottomAdd}
      </Box>
    </React.Fragment>
  );
}
