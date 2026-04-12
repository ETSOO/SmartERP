import { useSearchParamsEx } from "@etsoo/react";
import { IActionResult } from "@etsoo/shared";
import { VBox } from "@etsoo/materialui";
import { Link, Navigate } from "react-router-dom";
import { app } from "../../app/MyApp";
import Typography from "@mui/material/Typography";
import Alert from "@mui/material/Alert";
import Button from "@mui/material/Button";

export default function AuthFail() {
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
      <VBox spacing={2} sx={{ padding: 4 }}>
        <Typography align="center">{labels.smarterpCore}</Typography>
        <Alert severity="error">
          <Typography>
            {labels.authFailed} - {result.title ?? labels.unknownError}
          </Typography>
          {(result.type || result.field) && (
            <Typography variant="caption">
              {[result.type, result.field].join(", ")}
            </Typography>
          )}
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
