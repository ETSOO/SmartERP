import { useParamsEx } from "@etsoo/react";
import { ViewPerson } from "../../../components/person/ViewPerson";
import { ErrorAlert } from "@etsoo/materialui";

export default function ViewContact() {
  // Route
  const { id = 0 } = useParamsEx({ id: "number" });

  if (id < 1) {
    return <ErrorAlert />;
  }

  return <ViewPerson personId={id} />;
}
