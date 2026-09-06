import { ISmartERPUser } from "@etsoo/materialui";
import { useSearchParamsEx } from "@etsoo/react";
import { IActionResult } from "@etsoo/shared";
import { app } from "../app/SmartApp";
import { Constants } from "../app/Constants";
import Box from "@mui/material/Box";
import LinearProgress from "@mui/material/LinearProgress";
import React from "react";
import { useNavigate } from "react-router-dom";

export default function AuthSuccess() {
  // Query params
  const { result, token } = useSearchParamsEx({
    result: "string",
    token: "string"
  });

  // Route
  const navigate = useNavigate();

  React.useEffect(() => {
    if (result) {
      // Parse result
      const r: IActionResult<ISmartERPUser> = JSON.parse(
        result
      ) as IActionResult<ISmartERPUser>;
      if (r.ok && r.data && token) {
        // User login
        app.userLogin(r.data, token, false).then(() => {
          // Remove the auth request cache
          app.storage.setData(Constants.AuthRequestField, null);

          // Navigate to main URL
          app.toMain();
        });

        return;
      } else {
        app.alertResult(r, () => {
          app.tryLogin();
        });
      }
    } else {
      const pageResult = {
        ok: false,
        title: "No Auth Result"
      };

      navigate(
        `./../authfail?error=${encodeURIComponent(JSON.stringify(pageResult))}`
      );
    }
  }, []);

  return (
    <Box sx={{ width: "100%" }}>
      <LinearProgress />
    </Box>
  );
}
