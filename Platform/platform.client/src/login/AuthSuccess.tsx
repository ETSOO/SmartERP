import { ISmartERPUser } from "@etsoo/materialui";
import { useSearchParamsEx } from "@etsoo/react";
import { IActionResult } from "@etsoo/shared";
import { Navigate } from "react-router-dom";
import { app } from "../app/SmartApp";
import { Constants } from "../app/Constants";

export default function AuthSuccess() {
  // Query params
  const { result, token } = useSearchParamsEx({
    result: "string",
    token: "string"
  });

  if (result) {
    // Parse result
    const r: IActionResult<ISmartERPUser> = JSON.parse(
      result
    ) as IActionResult<ISmartERPUser>;
    if (r.ok && r.data && token) {
      // User login
      app.userLogin(r.data, token, false);

      // Remove the auth request cache
      app.storage.setData(Constants.AuthRequestField, null);

      // Navigate to main URL
      app.toMain();

      return;
    } else {
      app.alertResult(r, () => {
        app.tryLogin();
      });

      return <></>;
    }
  }

  return <Navigate to="./../../../" replace />;
}
