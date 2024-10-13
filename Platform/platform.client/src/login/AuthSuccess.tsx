import { ISmartERPUser } from "@etsoo/materialui";
import { useSearchParamsEx } from "@etsoo/react";
import { IActionResult } from "@etsoo/shared";
import { Navigate } from "react-router-dom";
import { app } from "../app/SmartApp";
import { Constants } from "../app/Constants";

function AuthSuccess() {
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
      app.userLogin(r.data, token, true);

      // Remove the auth request cache
      app.storage.setData(Constants.AuthRequestField, null);

      return <Navigate to="./../../../home" replace />;
    } else {
      app.alertResult(r, () => {
        app.tryLogin();
      });

      return <></>;
    }
  }

  return <Navigate to="./../../../" replace />;
}

export default AuthSuccess;
