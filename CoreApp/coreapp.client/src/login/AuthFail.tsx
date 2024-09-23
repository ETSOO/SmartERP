import { Alert, Button, Typography } from "@mui/material";
import { useSearchParamsEx } from "@etsoo/react";
import { app } from "../app/MyApp";
import { IActionResult } from "@etsoo/shared";
import { VBox } from "@etsoo/materialui";
import { Link } from "react-router-dom";

function AuthFail() {
  // Labels
  const labels = app.getLabels("authFailed", "back");

  const { error } = useSearchParamsEx({
    error: "string"
  });

  if (error) {
    const result: IActionResult = JSON.parse(error);
    return (
      <VBox gap={2} padding={4}>
        <Alert severity="error">
          <Typography>
            {labels.authFailed} - {result.title}
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
  } else {
    return <Typography></Typography>;
  }
}

export default AuthFail;
