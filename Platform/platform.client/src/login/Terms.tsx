import { PList } from "@etsoo/materialui";
import { app } from "../app/SmartApp";
import { SharedLayout } from "./SharedLayout";

export default function Terms() {
  // Labels
  const labels = app.getLabels("terms", "back");

  return (
    <SharedLayout title={labels.terms} buttons={[]} homeUrl="./../../">
      <PList items={app.get<string[]>("termsPage")} />
    </SharedLayout>
  );
}
