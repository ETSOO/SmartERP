import { app } from "../../../app/MyApp";
import { ViewPerson } from "../../../components/person/ViewPerson";
import { Navigate } from "react-router-dom";

export default function OrgData() {
  // Organization person id
  const orgPersonId = app.userData?.system?.personId;

  return orgPersonId == null || orgPersonId < 1 ? (
    <Navigate to="./../../system/updateSettings" />
  ) : (
    <ViewPerson personId={orgPersonId} />
  );
}
