import { PList } from "@etsoo/materialui";
import { app } from "../app/SmartApp";
import { SharedLayout } from "./SharedLayout";

export default function About() {
  // Labels
  const labels = app.getLabels("about", "back");

  return (
    <SharedLayout title={labels.about} buttons={[]} homeUrl="./../../">
      <PList items={app.get<string[]>("aboutPage")} />
    </SharedLayout>
  );
}
