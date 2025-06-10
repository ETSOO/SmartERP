import { useSearchParamsEx } from "@etsoo/react";
import { ServiceUserToken } from "@etsoo/materialui";
import { ApiRefreshTokenDto } from "@etsoo/appscript";
import { IActionResult } from "@etsoo/shared";
import { useNavigate } from "react-router-dom";
import { app } from "../../app/MyApp";
import React from "react";
import Box from "@mui/material/Box";
import LinearProgress from "@mui/material/LinearProgress";
import { CrmUser } from "@etsoo/smarterp-crm";

export default function AuthSuccess() {
  // Query params
  const { core, result } = useSearchParamsEx({
    core: "string",
    result: "string"
  });

  // Route
  const navigate = useNavigate();

  React.useEffect(() => {
    let pageResult: IActionResult;
    if (result) {
      try {
        const resultObj: IActionResult<CrmUser & ServiceUserToken> = JSON.parse(
          decodeURIComponent(result)
        );

        if (resultObj.ok && resultObj.data) {
          const userData = resultObj.data;

          const coreObj: ApiRefreshTokenDto | undefined = core
            ? JSON.parse(decodeURIComponent(core))
            : undefined;

          app.userLoginEx(userData, coreObj);

          navigate("./../../home");
          return;
        } else {
          pageResult = {
            ok: false,
            title: "No Valid Auth Result"
          };
        }
      } catch (error) {
        pageResult = {
          ok: false,
          title: `Auth Exception ${error}`
        };
        console.error("AuthSuccess", error);
      }
    } else {
      pageResult = {
        ok: false,
        title: "No Auth Result"
      };
    }

    navigate(
      `./../authfail?error=${encodeURIComponent(JSON.stringify(pageResult))}`
    );
  }, []);

  return (
    <Box sx={{ width: "100%" }}>
      <LinearProgress />
    </Box>
  );
}
