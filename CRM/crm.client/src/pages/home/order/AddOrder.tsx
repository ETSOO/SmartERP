import { CommonPage } from "@etsoo/materialui";
import React from "react";
import { app } from "../../../app/MyApp";

export default function AddOrder() {
  React.useEffect(() => {
    app.productApi
      .queryForSale({ currency: "CNY", culture: "en", customerId: 1001 })
      .then((products) => {
        console.log("Products for sale:", products);
      });
  }, []);

  return <CommonPage paddings={0}></CommonPage>;
}
