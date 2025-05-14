import { useParamsEx } from "@etsoo/react";
import { ViewPerson } from "../../../components/person/ViewPerson";

export default function ViewUser() {
  // Route
  const { id = 0 } = useParamsEx({ id: "number" });

  return <ViewPerson personId={id} />;
}
