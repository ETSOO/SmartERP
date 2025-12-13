import { useParamsEx, useSearchParamsEx } from "@etsoo/react";
import { ViewPerson } from "../../../components/person/ViewPerson";
import { ErrorAlert } from "@etsoo/materialui";

export default function ViewContact() {
  // Route
  const { id = 0 } = useParamsEx({ id: "number" });
  const { index } = useSearchParamsEx({
    index: "number"
  });

  if (id < 1) {
    return <ErrorAlert />;
  }

  return (
    <ViewPerson key={index === 0 ? `id-${Date.now()}` : 1} personId={id} />
  );
}
