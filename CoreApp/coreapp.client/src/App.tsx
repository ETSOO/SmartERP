import React from "react";
import { app } from "./app/MyApp";
import { useSearchParamsEx } from "@etsoo/react";

function App() {
  // Queries
  const { embedded, tryLogin } = useSearchParamsEx({
    embedded: "boolean",
    tryLogin: "boolean"
  });

  React.useEffect(() => {
    app.updateEmbedded(embedded, true);
  }, [embedded]);

  React.useEffect(() => {
    app.toLoginPage(tryLogin);
  }, [tryLogin]);

  return <React.Fragment></React.Fragment>;
}

export default App;
