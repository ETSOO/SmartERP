import { Alert, Button, Typography } from "@mui/material";
import { useSearchParamsEx } from "@etsoo/react";
import { app } from "../app/MyApp";
import { IActionResult } from "@etsoo/shared";
import { VBox } from "@etsoo/materialui";
import { Link, Navigate } from "react-router-dom";

function AuthFail() {
  // Labels
  const labels = app.getLabels(
    "authFailed",
    "back",
    "smarterpCore",
    "unknownError"
  );

  const { error } = useSearchParamsEx({
    error: "string"
  });

  if (error) {
    const result: IActionResult = JSON.parse(error);
    return (
      <VBox gap={2} padding={4}>
        <Typography textAlign="center">{labels.smarterpCore}</Typography>
        <Alert severity="error">
          <Typography>
            {labels.authFailed} - {result.title ?? labels.unknownError}
          </Typography>
          <Typography variant="caption">
            {[result.type, result.field].join(", ")}
          </Typography>
        </Alert>
        <Button
          variant="contained"
          color="primary"
          component={Link}
          to="./../../"
        >
          {labels.back}
        </Button>
      </VBox>
    );
  }

  return <Navigate to="./../../" replace />;
}

export default AuthFail;
