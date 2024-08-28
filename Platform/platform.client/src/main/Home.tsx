import {
  AppBar,
  Box,
  IconButton,
  Theme,
  Toolbar,
  Typography,
  useMediaQuery
} from "@mui/material";
import React from "react";
import MenuIcon from "@mui/icons-material/Menu";
import { app } from "../app/SmartApp";
import { Outlet } from "react-router-dom";
import { DrawerHeader } from "@etsoo/materialui";
import { LeftDrawerLocal } from "../app/LeftDrawerLocal";
import { UserMenuLocal } from "../app/UserMenuLocal";

// Size
const width = 220;

function Home() {
  // Page context
  const PageContext = app.pageState.context;

  // User context / state
  const { state } = React.useContext(app.userState.context);

  // Small than sm
  const smDown = useMediaQuery<Theme>((theme) => theme.breakpoints.down("sm"));
  app.smDown = smDown;

  const mdUp = useMediaQuery<Theme>((theme) => theme.breakpoints.up("md"));
  app.mdUp = mdUp;

  const { authorized } = state;

  // Persist
  const [open, setOpen] = React.useState(mdUp);

  React.useEffect(() => {
    setOpen(mdUp);
  }, [mdUp]);

  // When unauthorized (by refresh)
  // Return blank and try login
  React.useEffect(() => {
    if (!authorized) app.tryLogin();
  }, [authorized]);

  if (!authorized) {
    return <React.Fragment />;
  }

  return (
    <React.Fragment>
      <AppBar
        position="sticky"
        sx={{ ...(mdUp && open && { paddingLeft: `${width}px` }) }}
      >
        <Toolbar>
          <IconButton
            edge="start"
            color="inherit"
            onClick={() => setOpen(true)}
            sx={{ ...(open && { display: "none" }) }}
          >
            <MenuIcon />
          </IconButton>
          <Typography variant="h6" noWrap component="div">
            <PageContext.Consumer>
              {(page) => page.state.title}
            </PageContext.Consumer>
          </Typography>
          <Box sx={{ flexGrow: 1 }} />
          <UserMenuLocal
            organization={state.organization}
            name={state.name}
            avatar={state.avatar}
            smDown={smDown}
          />
        </Toolbar>
      </AppBar>
      <iframe
        style={{
          position: "fixed",
          border: 0,
          width: "100vw",
          height: "100vh"
        }}
        src="https://etsoo.nz"
      ></iframe>
    </React.Fragment>
  );
}

export default Home;
