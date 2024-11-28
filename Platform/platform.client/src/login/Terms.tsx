import { PList } from "@etsoo/materialui";
import { Button } from "@mui/material";
import { Link } from "react-router-dom";
import { app } from "../app/SmartApp";
import { SharedLayout } from "./SharedLayout";

export default function Terms() {
  // Labels
  const labels = app.getLabels("terms", "back");

  return (
    <SharedLayout
      title={labels.terms}
      buttons={
        <Button variant="contained" component={Link} to="./../../">
          {labels.back}
        </Button>
      }
    >
      <PList items={app.get<string[]>("termsPage")} />
    </SharedLayout>
  );
}
