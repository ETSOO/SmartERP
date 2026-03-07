import {
  CommonPage,
  HBox,
  SearchBar,
  SearchField,
  VBox
} from "@etsoo/materialui";
import React from "react";
import { app } from "../../../app/MyApp";
import { useParamsEx, useSearchParamsEx } from "@etsoo/react";
import { useNavigate } from "react-router-dom";
import {
  CustomerList,
  ProductCategoryTiplist
} from "@etsoo/smarterp-crm/components";
import { CurrencyList } from "../../../components/CurrencyList";
import { CultureList } from "../../../components/CultureList";
import { DomUtils, NumberUtils } from "@etsoo/shared";
import { QueryForSaleData, QueryForSaleRQ } from "@etsoo/smarterp-crm";
import LinearProgress from "@mui/material/LinearProgress";
import ImageList from "@mui/material/ImageList";
import ImageListItem from "@mui/material/ImageListItem";
import ImageListItemBar from "@mui/material/ImageListItemBar";
import useMediaQuery from "@mui/material/useMediaQuery";
import { useTheme } from "@mui/material/styles";
import Typography from "@mui/material/Typography";
import IconButton from "@mui/material/IconButton";
import AddShoppingCartIcon from "@mui/icons-material/AddShoppingCart";
import AppBar from "@mui/material/AppBar";
import Toolbar from "@mui/material/Toolbar";

function getDefaultCulture() {
  return app.userData?.system?.cultures[0] ?? app.culture;
}

function CustomerChooser({ customerId }: { customerId?: number }) {
  // Default currency
  const defaultCurrency = app.userData?.system?.currencies[0] ?? app.currency;

  // Default culture
  const defaultCulture = getDefaultCulture();

  // Layout
  return (
    <VBox gap={1} spacing={1} paddingTop={1}>
      <CustomerList name="customerId" idValue={customerId} inputRequired />
      <CurrencyList value={defaultCurrency} fullWidth required />
      <CultureList value={defaultCulture} fullWidth required />
    </VBox>
  );
}

export default function AddOrder() {
  // Route
  const navigate = useNavigate();
  const { id = 0 } = useParamsEx({
    id: "number"
  });

  const { customerId } = useSearchParamsEx({
    customerId: "number"
  });

  // Label
  const labels = app.getLabels(
    "assignedId",
    "category",
    "chooseCustomer",
    "productName"
  );

  const [cart, setCart] = React.useState<QueryForSaleRQ>();

  const [products, setProducts] = React.useState<QueryForSaleData[]>();

  React.useEffect(() => {
    if (cart) {
      app.productApi
        .queryForSale({ ...cart, queryPaging: 15 })
        .then((items) => {
          setProducts(items);
        });
    } else {
      app.showInputDialog({
        title: labels.chooseCustomer,
        message: "",
        callback: async (form) => {
          // Cancelled
          if (form == null) {
            navigate(id > 0 ? "./../.." : "./..");
            return;
          }

          // Validate form
          if (!form.reportValidity()) {
            return false;
          }

          // Form data
          const { customerId, currency, culture } = DomUtils.dataAs(
            new FormData(form),
            {
              customerId: "number",
              currency: "string",
              culture: "string"
            }
          );

          if (!customerId || !currency || !culture) {
            return false;
          }

          setCart({
            customerId,
            currency,
            culture: culture === getDefaultCulture() ? undefined : culture
          });

          return true;
        },
        inputs: <CustomerChooser customerId={customerId} />
      });
    }
  }, [cart]);

  const theme = useTheme();
  const cols = useMediaQuery(theme.breakpoints.down("sm"))
    ? 1
    : useMediaQuery(theme.breakpoints.down("md"))
      ? 2
      : useMediaQuery(theme.breakpoints.down("lg"))
        ? 3
        : 5;

  if (products == null) {
    return <LinearProgress />;
  }

  const symbol = NumberUtils.getCurrencySymbol(cart?.currency!);

  return (
    <React.Fragment>
      <AppBar position="sticky">
        <Toolbar>
          <SearchBar
            fields={[
              <SearchField
                label={labels.productName}
                name="keyword"
                minChars={2}
              />,
              <SearchField
                label={labels.assignedId}
                name="AssignedIdStart"
                minChars={3}
              />,
              <ProductCategoryTiplist
                label={labels.category}
                name="categoryIdAll"
                search
              />
            ]}
            className="searchBarGrid"
            width={300}
            top={true}
            onSubmit={(data, reset) => {
              const { keyword, assignedIdStart, categoryIdAll } = reset
                ? {}
                : DomUtils.dataAs(data, {
                    keyword: "string",
                    assignedIdStart: "string",
                    categoryIdAll: "number"
                  });

              setCart({ ...cart!, keyword, assignedIdStart, categoryIdAll });
            }}
          />
        </Toolbar>
      </AppBar>
      <CommonPage paddings={0}>
        <ImageList gap={8} cols={cols} rowHeight={180}>
          {products.map((p) => (
            <ImageListItem key={p.id}>
              {p.logo && (
                <img
                  src={p.logo}
                  alt={p.name}
                  style={{ maxHeight: 180 }}
                  loading="lazy"
                />
              )}
              <ImageListItemBar
                title={
                  <VBox whiteSpace="wrap">
                    <Typography>{p.name}</Typography>
                    {p.description && (
                      <Typography variant="caption">{p.description}</Typography>
                    )}
                    <HBox>
                      <Typography variant="body1">
                        {symbol}
                        {app.formatNumber(p.retailPrice)} /{" "}
                        {p.assetQty && p.assetQty > 1 ? `${p.assetQty}` : ""}
                        {p.unitName}
                      </Typography>
                    </HBox>
                    <HBox justifyContent="flex-end">
                      <IconButton color="warning">
                        <AddShoppingCartIcon />
                      </IconButton>
                    </HBox>
                  </VBox>
                }
              />
            </ImageListItem>
          ))}
        </ImageList>
      </CommonPage>
    </React.Fragment>
  );
}
