import React from "react";
import {
  Button,
  Card,
  CardContent,
  CardHeader,
  Grid,
  IconButton,
  List,
  ListItem,
  ListItemText,
  Paper,
  Theme,
  Typography,
  useMediaQuery
} from "@mui/material";
import { ButtonLink, CommonPage, HBox, MUGlobal } from "@etsoo/materialui";
import MoreHorizIcon from "@mui/icons-material/MoreHoriz";
import AddIcon from "@mui/icons-material/Add";
import { MemberDialogs } from "./member/MemberDialogs";
import { app } from "../app/SmartApp";
import CurrencyExchangeIcon from "@mui/icons-material/CurrencyExchange";
import { useNavigate } from "react-router-dom";
import { RefreshTokenRQ } from "@etsoo/appscript";
import { DashboardView } from "../api/dto/system/DashboardView";
import { SystemServiceDto } from "../api/dto/system/SystemServiceDto";

function Dashboard() {
  // Route
  const navigate = useNavigate();

  // Labels
  const labels = app.getLabels(
    "menuLoginHistory",
    "welcome",
    "organization",
    "newOrganization",
    "device",
    "underDevelopment",
    "tools",
    "exchangeRate"
  );

  // User context
  const Context = app.userState.context;

  // Paddings
  const paddings = MUGlobal.pagePaddings;

  // Page data
  const {
    state: { inviteId }
  } = React.useContext(app.pageState.context);

  // Screen size detection
  const smDown = useMediaQuery<Theme>((theme) => theme.breakpoints.down("sm"));

  // View
  const [view, setView] = React.useState<DashboardView>();

  const isMounted = React.useRef(true);

  // Load data
  const reloadData = async (
    authorized?: boolean,
    _changedFields?: string[]
  ) => {
    if (authorized === false) return;

    /*
    const view = await app.systemApi.dashboard();
    if (view == null || !isMounted.current) return;

    setView(view);

    // Invitation
    if (inviteId) {
      MemberDialogs.accept(inviteId, navigate);
    }
      */
  };

  // Visit service
  const visitService = async (service: SystemServiceDto) => {
    if (service.entityStatus > 0) return;

    // Reqest data
    const data: RefreshTokenRQ = {
      deviceId: app.deviceId,
      region: app.region
    };

    // Reqest result
    await app.refreshToken({
      data,
      showLoading: true,
      callback: (result, serviceToken) => {
        if (result === true && serviceToken) {
          // Show loading bar
          app.notifier.showLoading();

          // Redirect
          app.toServiceUrl(service.appId, service.webUrl, serviceToken, true);

          // Hide loading bar
          app.notifier.hideLoading(true);
          return;
        }
        app.notifier.alert(app.formatRefreshTokenResult(result));
      }
    });
  };

  // Load more history
  const loadMoreHistory = () => {
    navigate("./user/loginhistory");
  };

  // New organization
  const newOrganization = () => {
    navigate("./service/all", {
      state: { kind: 2 }
    });
  };

  React.useEffect(() => {
    // Page title
    //app.setPageKey("menuHome");

    return () => {
      isMounted.current = false;
    };
  }, []);

  return (
    <CommonPage
      targetFields={["organization"]}
      onUpdateAll={reloadData}
      paddings={paddings}
    >
      <Context.Consumer>
        {(user) => (
          <React.Fragment>
            <HBox paddingBottom={1} paddingLeft={paddings}>
              <Typography variant="subtitle1">
                {labels.welcome.format(user.state.name)}
                {user.state.organization != null && smDown && (
                  <Typography variant="caption">
                    ({view?.organization?.name})
                  </Typography>
                )}
              </Typography>
              {user.state.organization == null && (
                <Button
                  variant="outlined"
                  size="small"
                  onClick={() => newOrganization()}
                  endIcon={<AddIcon />}
                >
                  {labels.newOrganization}
                </Button>
              )}
            </HBox>
          </React.Fragment>
        )}
      </Context.Consumer>
      <Grid container spacing={paddings} paddingBottom={paddings}>
        {view?.services.map((service) => (
          <Grid item xs={6} md={4} xl={2} key={service.id}>
            <Paper
              sx={{ padding: paddings, cursor: "pointer" }}
              onClick={async () => await visitService(service)}
            >
              <Typography variant="subtitle1" height="56px" overflow="hidden">
                {service.name}
                {service.entityStatus === 199 && (
                  <Typography variant="caption" color="red">
                    {" - " + labels.underDevelopment + "..."}
                  </Typography>
                )}
              </Typography>
            </Paper>
          </Grid>
        ))}
      </Grid>
      <Paper sx={{ padding: paddings }}>
        <ButtonLink
          variant="outlined"
          startIcon={<CurrencyExchangeIcon />}
          href="./tools/exchangerate"
        >
          {labels.exchangeRate}
        </ButtonLink>
      </Paper>
      <Card sx={{ marginTop: paddings }}>
        <CardHeader
          title={labels.device}
          titleTypographyProps={{ variant: "h6" }}
          action={
            <IconButton
              title={labels.menuLoginHistory}
              onClick={loadMoreHistory}
            >
              <MoreHorizIcon />
            </IconButton>
          }
          sx={{
            paddingLeft: paddings,
            paddingRight: paddings,
            paddingTop: paddings,
            paddingBottom: MUGlobal.half(paddings)
          }}
        />
        <CardContent
          sx={{
            paddingLeft: paddings,
            paddingRight: paddings,
            paddingTop: 0,
            paddingBottom: paddings
          }}
        >
          <List disablePadding dense>
            {view?.devices.map((device, _index, _devices) => (
              <ListItem key={device.id} disableGutters disablePadding>
                <ListItemText
                  primary={device.name}
                  secondary={app.formatDate(device.lastLogin, "ds")}
                />
              </ListItem>
            ))}
          </List>
        </CardContent>
      </Card>
    </CommonPage>
  );
}

export default Dashboard;
