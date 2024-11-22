import { Button } from "@mui/material";
import { useNavigate } from "react-router-dom";
import { app } from "./app/SmartApp";
import { SharedLayout } from "./login/SharedLayout";

/**
 * Not found case component
 */
export function NotFound() {
  // Navigator
  const navigate = useNavigate();

  // Labels
  const labels = app.getLabels("pageNotFound", "back");

  // Currently, navigate from props always failed with step number -1
  const goBack = () => {
    navigate(-1);
  };

  return (
    <SharedLayout
      title={labels.pageNotFound}
      buttons={
        <Button variant="contained" onClick={goBack}>
          {labels.back}
        </Button>
      }
    >
      <p style={{ wordBreak: "break-all" }}>
        <b>Origin</b>: {window.location?.origin}
        <br />
        <b>URL</b>: {window.location?.href}
      </p>
    </SharedLayout>
  );
}
