import React from "react";
import { useSearchParamsEx } from "@etsoo/react";
import { useNavigate } from "react-router-dom";
import { app } from "../app/MyApp";

export default function Index() {
  // Queries
  const { embedded, tryLogin } = useSearchParamsEx({
    embedded: "boolean",
    tryLogin: "boolean"
  });

  // Navigate
  const navigate = useNavigate();

  React.useEffect(() => {
    app.updateEmbedded(embedded, true);
  }, [embedded]);

  React.useEffect(() => {
    app.tryLogin({
      params: {
        tryLogin
      },
      onSuccess: () => {
        navigate("./home/");
      }
    });
  }, [tryLogin]);

  return <React.Fragment></React.Fragment>;
}
