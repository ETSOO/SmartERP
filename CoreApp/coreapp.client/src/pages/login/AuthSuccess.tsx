import { useSearchParamsEx } from "@etsoo/react";
import { IServiceUser, ServiceUserToken } from "@etsoo/materialui";
import { ApiRefreshTokenDto } from "@etsoo/appscript";
import { IActionResult } from "@etsoo/shared";
import { Navigate } from "react-router-dom";
import React from "react";
import { app } from "../../app/MyApp";

export default function AuthSuccess() {
  // Query params
  const { core, result } = useSearchParamsEx({
    core: "string",
    result: "string"
  });

  let pageResult: IActionResult;
  if (result) {
    try {
      const resultObj: IActionResult<IServiceUser & ServiceUserToken> =
        JSON.parse(result);

      if (resultObj.ok && resultObj.data) {
        const userData = resultObj.data;
        const coreObj: ApiRefreshTokenDto | undefined = core
          ? JSON.parse(core)
          : undefined;

        app.userLoginEx(userData, coreObj, false);

        return <Navigate to="./../../home" replace />;
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

  return (
    <Navigate
      to={`./../authfail?error=${encodeURIComponent(
        JSON.stringify(pageResult)
      )}`}
      replace
    />
  );
}
