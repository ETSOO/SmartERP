import { PList } from "@etsoo/materialui";
import { Button } from "@mui/material";
import { Link } from "react-router-dom";
import { app } from "../app/SmartApp";
import { SharedLayout } from "./SharedLayout";

export default function About() {
  // Labels
  const labels = app.getLabels("about", "back");

  return (
    <SharedLayout
      title={labels.about}
      buttons={
        <Button variant="contained" component={Link} to="./../../">
          {labels.back}
        </Button>
      }
    >
      <PList items={app.get<string[]>("aboutPage")} />
    </SharedLayout>
  );
}
