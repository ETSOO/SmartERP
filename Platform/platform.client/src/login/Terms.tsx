import { PList } from "@etsoo/materialui";
import { Link } from "react-router-dom";
import { app } from "../app/SmartApp";
import { SharedLayout } from "./SharedLayout";
import Button from "@mui/material/Button";

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
