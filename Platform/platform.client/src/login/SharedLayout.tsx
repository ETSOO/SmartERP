import React from "react";
import { Box, Typography } from "@mui/material";
import { HBox, MUGlobal, VBox } from "@etsoo/materialui";
import logo from "./../images/etsoo.png";
import { app } from "../app/SmartApp";
import { Constants } from "../app/Constants";
import { PublicProductDto } from "@etsoo/appscript";

/**
 * Shared layout props
 */
export type SharedLayoutProps = {
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
   * Title
   */
  title: string;

  /**
   * Subtitle
   */
  subTitle?: React.ReactNode;

  /**
   * Visibility
   */
  visible?: boolean;
};

/**
 * Shared layout
 * @param props Props
 * @returns Component
 */
export function SharedLayout(props: SharedLayoutProps) {
  // Destructure
  const {
    headerRight,
    pageRight,
    buttons,
    children,
    bottom,
    bottomAdd,
    title,
    subTitle,
    visible = true
  } = props;

  // Culture context
  const Context = app.cultureState.context;

  // Current service
  const service = app.storage.getObject<PublicProductDto>(
    Constants.CurentService
  );

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
          padding="16px 24px 12px 24px"
          justifyContent="space-between"
          alignItems="flex-end"
        >
          <Box
            component="img"
            src={service?.logo ?? logo}
            sx={{
              height: { xs: "36px", sm: "48px" },
              userSelect: "none"
            }}
          />
          {headerRight}
          <Typography variant="subtitle1">
            <Context.Consumer>
              {(value) => service?.name ?? value.get<string>("appName")}
            </Context.Consumer>
          </Typography>
        </HBox>
        <Typography
          variant="caption"
          component="div"
          textAlign="center"
          paddingBottom="4px"
        >
          {app.get("slogan")}
        </Typography>
        <VBox
          borderRadius={0.5}
          padding={3}
          spacing={2}
          boxShadow={1}
          alignItems="flex-start"
          sx={{ backgroundColor: "#fff" }}
        >
          <VBox width="100%">
            <HBox justifyContent="space-between" alignItems="center">
              <Typography variant="h5">{title}</Typography>
              {pageRight}
            </HBox>
            {subTitle &&
              (typeof subTitle === "string" ? (
                <Typography
                  variant="body2"
                  color={(theme) => theme.palette.grey[600]}
                >
                  {subTitle}
                </Typography>
              ) : (
                subTitle
              ))}
          </VBox>
          {children}
          <HBox
            justifyContent={
              Array.isArray(buttons) && buttons.length > 1
                ? "space-between"
                : "flex-end"
            }
            spacing={2}
          >
            {buttons}
          </HBox>
        </VBox>
        <HBox
          padding="8px 24px"
          spacing={2}
          fontSize="smaller"
          justifyContent="center"
        >
          {bottom}
        </HBox>
        {bottomAdd}
      </Box>
    </React.Fragment>
  );
}
