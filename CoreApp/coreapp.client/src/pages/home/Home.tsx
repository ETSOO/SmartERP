import { ButtonLink, CommonPage, HBox } from "@etsoo/materialui";
import Paper from "@mui/material/Paper";
import React from "react";
import { app } from "../../app/MyApp";
import { Typography } from "@mui/material";

export default function Home() {
  // User context / state
  const { state } = React.useContext(app.userState.context);
  const { organization, organizationName } = state;

  // Labels
  const labels = app.getLabels(
    "currentOrg",
    "homeGuide",
    "homeOrgGuide",
    "homeUserGuide"
  );

  return (
    <CommonPage paddings={0}>
      <Paper sx={{ padding: 1 }}>
        <HBox sx={{ alignItems: "center", flexWrap: "wrap" }}>
          <Typography variant="body2">{labels.currentOrg}:</Typography>
          <ButtonLink href={`./org/my/${organization}`}>
            {organizationName}
          </ButtonLink>
        </HBox>
        <Typography variant="caption">{labels.homeGuide}</Typography>
      </Paper>
      <Paper sx={{ padding: 1, marginTop: 2 }}>
        <Typography component="div" variant="caption">
          1. {labels.homeUserGuide}
        </Typography>
        <Typography component="div" variant="caption">
          2. {labels.homeOrgGuide}
        </Typography>
      </Paper>
    </CommonPage>
  );
}
