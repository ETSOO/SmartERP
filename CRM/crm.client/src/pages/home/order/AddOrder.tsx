import {
  CommonPage,
  CustomFieldUI,
  HBox,
  InputField,
  MenuButton,
  MoneyText,
  NotificationMUDataMethods,
  NotificationMUDataProps,
  NumberSpinner,
  SearchBar,
  SearchField,
  VBox
} from "@etsoo/materialui";
import React from "react";
import { app } from "../../../app/MyApp";
import { useDimensions, useSearchParamsEx } from "@etsoo/react";
import { useNavigate } from "react-router-dom";
import {
  CustomerList,
  ProductCategoryTiplist
} from "@etsoo/smarterp-crm/components";
import { CurrencyList } from "../../../components/CurrencyList";
import { CultureList } from "../../../components/CultureList";
import { DomUtils, NumberUtils, Utils } from "@etsoo/shared";
import {
  CustomerReadForSaleData,
  OrderUtils,
  PromotionCodeCalculation,
  PromotionItem,
  PromotionOrderLine,
  PromotionSaleItemBase,
  QueryForSaleData,
  QueryForSaleRQ
} from "@etsoo/smarterp-crm";
import LinearProgress from "@mui/material/LinearProgress";
import ImageList from "@mui/material/ImageList";
import ImageListItem from "@mui/material/ImageListItem";
import ImageListItemBar from "@mui/material/ImageListItemBar";
import useMediaQuery from "@mui/material/useMediaQuery";
import { useTheme } from "@mui/material/styles";
import Typography from "@mui/material/Typography";
import IconButton from "@mui/material/IconButton";
import AddShoppingCartIcon from "@mui/icons-material/AddShoppingCart";
import ShoppingCartIcon from "@mui/icons-material/ShoppingCart";
import CelebrationIcon from "@mui/icons-material/Celebration";
import ClearIcon from "@mui/icons-material/Clear";
import AppBar from "@mui/material/AppBar";
import Toolbar from "@mui/material/Toolbar";
import { CustomFieldRef, QueryPagingData } from "@etsoo/appscript";
import Avatar from "@mui/material/Avatar";
import Badge from "@mui/material/Badge";
import Grid from "@mui/material/Grid";
import Divider from "@mui/material/Divider";
import Chip from "@mui/material/Chip";
import List from "@mui/material/List";
import ListItem from "@mui/material/ListItem";
import ListItemAvatar from "@mui/material/ListItemAvatar";
import ListItemText from "@mui/material/ListItemText";
import { LocalUtils } from "../../../app/LocalUtils";
import { Button } from "@mui/material";

function formatName(data: QueryForSaleData) {
  return data.assignedId ? `${data.assignedId} - ${data.name}` : data.name;
}

function formatPriceLine(
  data: QueryForSaleData,
  currencySymbol?: string,
  price?: number
) {
  price ??= app.order.getPrice(data);
  return (
    <Typography variant="body2">
      {currencySymbol}
      {app.formatNumber(price)}
      {price < data.retailPrice ? (
        <Typography
          variant="caption"
          sx={{ textDecoration: "line-through", marginLeft: 0.5 }}
        >
          {app.formatNumber(data.retailPrice)}
        </Typography>
      ) : (
        ""
      )}{" "}
      / {data.assetQty && data.assetQty > 1 ? `${data.assetQty}` : ""}
      {data.unitName}
    </Typography>
  );
}

function AddItem({
  data,
  currencySymbol,
  line,
  mRef,
  onClear
}: NotificationMUDataProps & {
  data: QueryForSaleData;
  currencySymbol?: string;
  line?: LocalUtils.OrderLine;
  onClear?: () => void;
}) {
  // Labels
  const labels = app.getLabels(
    "clear",
    "confirmAction",
    "description",
    "promotions",
    "qty",
    "title"
  );

  const price = app.order.getPrice(data);

  // States
  const [qty, setQty] = React.useState<number | null>(
    line?.qty ?? data.minQty ?? 1
  );
  const [promotions, setPromotions] = React.useState<
    PromotionCodeCalculation[]
  >([]);

  const formRef = React.useRef<HTMLFormElement>(null);

  const modifiersRef =
    React.useRef<CustomFieldRef<Record<string, unknown>>>(null);

  const amount = price * (qty ?? 0);
  const pamount = promotions.reduce((sum, p) => sum + p.amount, 0);

  function changeQty(value: number | null) {
    setQty(value);

    if (value != null && value > 0 && data.promotions.length > 0) {
      const line: PromotionOrderLine = {
        price,
        qty: value
      };

      const results = OrderUtils.calculatePromotions(
        data.promotions,
        undefined,
        line
      );

      setPromotions(results);
    } else {
      setPromotions([]);
    }
  }

  React.useEffect(() => {
    if (line) {
      changeQty(line.qty);
      modifiersRef.current?.setValue(
        line.data?.modifiers as Record<string, unknown>
      );
    }
  }, [line]);

  React.useImperativeHandle(mRef, () => ({
    getValue: (): LocalUtils.OrderLine | undefined => {
      if (formRef.current == null) return undefined;

      if (!formRef.current.reportValidity()) {
        return undefined;
      }

      const { title, description } = DomUtils.dataAs(
        new FormData(formRef.current),
        {
          title: "string",
          description: "string"
        }
      );

      if (!title || qty == null) {
        return;
      }

      const modifiers = modifiersRef.current?.getValue();

      if (line == null) {
        return {
          id: Utils.newGUID(),
          productId: data.id,
          title,
          description,
          originalPrice: data.retailPrice,
          price,
          qty,
          amount: amount - pamount,
          discount: pamount,
          promotions: promotions.length > 0 ? promotions : undefined,
          data: modifiers ? { modifiers } : undefined
        };
      } else {
        return {
          ...line,
          title,
          description,
          price,
          qty,
          amount: amount - pamount,
          discount: pamount,
          promotions: promotions.length > 0 ? promotions : undefined,
          data: modifiers ? { modifiers } : undefined
        };
      }
    }
  }));

  return (
    <form ref={formRef}>
      <VBox spacing={1} sx={{ paddingTop: 1 }}>
        <InputField
          fullWidth
          required
          name="title"
          slotProps={{ htmlInput: { maxLength: 256 } }}
          label={labels.title}
          defaultValue={line?.title ?? data.name}
        />
        <Grid container spacing={1}>
          <Grid
            size={{ xs: 12, sm: 5 }}
            sx={{
              display: "flex",
              alignItems: "center",
              justifyContent: "flex-end",
              gap: 1
            }}
          >
            {formatPriceLine(data, currencySymbol, price)}
            <Typography variant="body2">x</Typography>
          </Grid>
          <Grid
            size={{ xs: 12, sm: 7 }}
            sx={{ display: "flex", alignItems: "center", gap: 1 }}
          >
            <NumberSpinner
              size="small"
              min={data.minQty ?? 1}
              max={data.capQty ?? 9999999}
              step={data.stepQty ?? 1}
              value={qty}
              required
              onValueChange={(value) => changeQty(value)}
            />
            {line != null && (
              <IconButton
                color="warning"
                title={labels.clear}
                onClick={() => {
                  app.notifier.confirm(
                    labels.confirmAction.format(labels.clear),
                    undefined,
                    (result) => {
                      if (result) {
                        onClear?.();
                      }
                    }
                  );
                }}
              >
                <ClearIcon />
              </IconButton>
            )}
          </Grid>
          <Grid
            size={{ xs: 12 }}
            sx={{
              display: "flex",
              alignItems: "center",
              justifyContent: "flex-end",
              gap: 1
            }}
          >
            <Typography> = </Typography>
            <MoneyText value={amount} currency={data.currency} />
            {pamount > 0 && (
              <React.Fragment>
                <Typography>-</Typography>
                <MoneyText value={pamount} currency={data.currency} />
                <Typography>=</Typography>
                <MoneyText value={amount - pamount} currency={data.currency} />
              </React.Fragment>
            )}
          </Grid>
          {promotions.length > 0 && (
            <Grid size={{ xs: 12 }}>
              <Typography
                sx={{
                  whiteSpace: "break-spaces",
                  lineHeight: "10px",
                  textAlign: "right"
                }}
              >
                <Typography variant="caption">{labels.promotions}: </Typography>
                {promotions.map((p, index) => (
                  <Typography variant="caption" key={p.id}>
                    {index === 0 ? "" : "; "}
                    <MoneyText value={p.amount} currency={data.currency} /> (
                    {p.title})
                  </Typography>
                ))}
              </Typography>
            </Grid>
          )}
        </Grid>
        {data.modifiers != null && data.modifiers.length > 0 && (
          <Grid container spacing={2}>
            <Grid size={{ xs: 12, sm: 12 }}>
              <Divider />
            </Grid>
            <CustomFieldUI fields={data.modifiers} mref={modifiersRef} />
          </Grid>
        )}
        <InputField
          fullWidth
          name="description"
          slotProps={{ htmlInput: { maxLength: 1280 } }}
          label={labels.description}
          defaultValue={line?.description ?? ""}
          multiline
          rows={2}
        />
      </VBox>
    </form>
  );
}

function CustomerChooser({ data }: { data: LocalUtils.CustomerQueryData }) {
  // Default culture
  data.culture ??= app.system.getDefaultCulture();

  // Layout
  return (
    <VBox spacing={2} sx={{ paddingTop: 1 }}>
      <CustomerList idValue={data.customerId} inputRequired />
      <CurrencyList value={data.currency} fullWidth required />
      <CultureList value={data.culture} fullWidth required />
    </VBox>
  );
}

function CartList({
  currencySymbol,
  lines,
  promotions
}: {
  currencySymbol?: string;
  lines: [LocalUtils.OrderLine, QueryForSaleData][];
  promotions: PromotionCodeCalculation[];
}) {
  // Labels
  const labels = app.getLabels("promotions", "total");

  // Total amount
  const total = lines.reduce((sum, [line]) => sum + line.amount, 0);
  const pamount = promotions.reduce((sum, p) => sum + (p.amount ?? 0), 0);

  return (
    <List
      sx={{
        "& .MuiListItem-root": {
          paddingRight: "160px"
        }
      }}
    >
      {lines.map(([line, product], index) => {
        const lps = line.promotions ?? [];
        return (
          <React.Fragment key={line.id}>
            {index > 0 && <Divider variant="inset" component="li" />}
            <ListItem
              alignItems="flex-start"
              disableGutters
              key={line.id}
              secondaryAction={
                <VBox sx={{ maxWidth: 140 }}>
                  <HBox sx={{ justifyContent: "flex-end" }}>
                    {formatPriceLine(product, currencySymbol)}
                  </HBox>
                  <Typography variant="body2" align="right">
                    x {line.qty} ={" "}
                    <MoneyText
                      value={line.price * line.qty}
                      sx={{ fontWeight: lps.length > 0 ? undefined : "bold" }}
                    />
                  </Typography>
                  {lps.length > 0 && (
                    <Typography variant="body2" align="right">
                      <MoneyText
                        value={-lps.reduce((acc, p) => acc + p.amount, 0)}
                        color="warning"
                      />
                      {" = "}
                      <MoneyText
                        value={line.amount}
                        sx={{ fontWeight: "bold" }}
                      />
                    </Typography>
                  )}
                </VBox>
              }
            >
              <ListItemAvatar title={formatName(product)}>
                <Avatar src={product.logo} />
              </ListItemAvatar>
              <ListItemText
                primary={`${line.title}${product.assignedId ? ` (${product.assignedId})` : ""}`}
                secondary={
                  <React.Fragment>
                    {lps.length > 0 && (
                      <Typography
                        variant="caption"
                        align="right"
                        color="warning"
                      >
                        ({labels.promotions}){" "}
                      </Typography>
                    )}
                    {lps.map((p, index) => (
                      <Typography variant="caption" align="right" key={p.id}>
                        {index === 0 ? "" : "; "}
                        {p.title},{" "}
                        <MoneyText
                          variant="caption"
                          sx={{ fontWeight: "bold" }}
                          value={-p.amount}
                        />
                      </Typography>
                    ))}
                    <Typography
                      component="div"
                      variant="body2"
                      sx={{
                        display: "-webkit-box",
                        WebkitBoxOrient: "vertical",
                        WebkitLineClamp: 2,
                        overflow: "hidden",
                        textOverflow: "ellipsis"
                      }}
                    >
                      {line.description}
                    </Typography>
                  </React.Fragment>
                }
                slotProps={{
                  secondary: {
                    component: "div"
                  }
                }}
              />
            </ListItem>
          </React.Fragment>
        );
      })}
      <Divider variant="inset" component="li" />
      <ListItem
        alignItems="flex-start"
        disableGutters
        secondaryAction={
          <React.Fragment>
            <Typography
              variant="body2"
              align="right"
              sx={{ paddingTop: { xs: 0, sm: 2 } }}
            >
              {currencySymbol}
              <MoneyText
                value={total}
                sx={{ fontWeight: promotions.length > 0 ? undefined : "bold" }}
              />
            </Typography>
            {promotions
              .filter((p) => p.amount != null)
              .map((p) => (
                <Typography
                  key={p.id}
                  component="div"
                  variant="caption"
                  align="right"
                >
                  <MoneyText value={-p.amount!} />
                </Typography>
              ))}
            {pamount > 0 && (
              <Typography variant="body2" align="right">
                {currencySymbol}
                <MoneyText
                  value={total - pamount}
                  sx={{ fontWeight: "bold" }}
                />
              </Typography>
            )}
          </React.Fragment>
        }
      >
        <ListItemText
          primary={
            <Typography variant="body2">{labels.total + ":"}</Typography>
          }
          secondary={promotions
            .filter((p) => p.amount != null)
            .map((p) => (
              <Typography
                key={p.id}
                component="div"
                align="right"
                variant="caption"
              >
                {p.title}
              </Typography>
            ))}
          slotProps={{
            secondary: {
              component: "div"
            }
          }}
        />
      </ListItem>
    </List>
  );
}

type CustomerQuery = Omit<QueryForSaleRQ, "queryPaging"> & {
  queryPaging: QueryPagingData;
};

type CustomerData = {
  data?: CustomerReadForSaleData;
  promotions: LocalUtils.PromotionItemWithAmount[];
  query: CustomerQuery;
};

export default function AddOrder() {
  // Route
  const navigate = useNavigate();

  const { customerId } = useSearchParamsEx({
    customerId: "number"
  });

  // Label
  const labels = app.getLabels(
    "assignedId",
    "category",
    "chooseCustomer",
    "clear",
    "confirmAction",
    "productDisappeared",
    "productName",
    "promotions",
    "shoppingCart",
    "tooManyItemsToDisplay"
  );

  const queryRef = React.useRef<CustomerData>(undefined);

  const [products, setProducts] = React.useState<QueryForSaleData[]>();
  const [orderLines, setOrderLines] = React.useState<LocalUtils.OrderLine[]>(
    []
  );
  const [moreProducts, setMoreProducts] = React.useState<boolean>(false);

  // Watch container
  const searchBarWidthRef = React.useRef<number>(0);
  const { dimensions } = useDimensions(1, (_, rect) => {
    if (rect.width - searchBarWidthRef.current <= 32) {
      return false;
    }

    if (rect.width > searchBarWidthRef.current) {
      searchBarWidthRef.current = rect.width;
    }

    return true;
  });

  // Load products
  const initPagingData: QueryPagingData = {
    currentPage: 0,
    batchSize: 15,
    orderBy: [{ field: "id", desc: false, unique: true }]
  };

  const loadProducts = async (reset: boolean = false) => {
    if (queryRef.current == null) return;

    const items = await app.productApi.queryForSale(queryRef.current.query);
    if (items == null) return;

    if (products == null || reset) {
      setProducts(items);
    } else {
      setProducts([...products, ...items]);
    }

    setMoreProducts(items.length >= initPagingData.batchSize);
  };

  const observerRef = React.useRef<IntersectionObserver>(undefined);
  const divRef = React.useRef<HTMLDivElement>(null);

  React.useEffect(() => {
    if (observerRef.current) {
      observerRef.current.disconnect();
      observerRef.current = undefined;
    }

    if (!moreProducts) {
      return;
    }

    observerRef.current = new IntersectionObserver(
      (entries) => {
        if (entries[0].isIntersecting && queryRef.current) {
          const query = queryRef.current.query.queryPaging;
          if (query.currentPage != null) {
            query.currentPage++;
          }

          if (products != null && products.length > 0) {
            const lastItem = products[products.length - 1];
            query.keysets = [lastItem.id];
          }

          loadProducts();
        }
      },
      { threshold: 1 }
    );

    if (divRef.current) {
      observerRef.current.observe(divRef.current);
    }

    return () => observerRef.current?.disconnect();
  }, [products, moreProducts]);

  // Add order item
  const addOrderItem = (
    data: QueryForSaleData,
    orderLine?: LocalUtils.OrderLine,
    onClear?: () => void
  ) => {
    const title =
      (orderLine ? app.get("edit") : app.get("add")) +
      (data.assignedId ? ` (${data.assignedId})` : "");
    const notifier = app.notifier.data<LocalUtils.OrderLine>(
      <AddItem
        data={data}
        currencySymbol={currencySymbol}
        line={orderLine}
        mRef={React.createRef<NotificationMUDataMethods>()}
        onClear={() => {
          onClear?.();
          notifier.dismiss();
        }}
      />,
      (line) => {
        if (line == null) return;

        setOrderLines((prev) => {
          // Check id
          const idIndex = prev.findIndex((item) => item.id === line.id);
          if (idIndex >= 0) {
            prev[idIndex] = line;
            return [...prev];
          } else {
            // Check if product already exists
            // with same name, description and modifiers
            const existingItem = prev.find(
              (item) =>
                item.productId === line.productId &&
                item.title === line.title &&
                item.description === line.description &&
                data.promotions.length === 0 &&
                JSON.stringify(item.data) === JSON.stringify(line.data)
            );

            if (existingItem) {
              // Update quantity and amount
              existingItem.qty += line.qty;
              existingItem.amount += line.amount;
              return [...prev];
            } else {
              return [...prev, line];
            }
          }
        });
      },
      title
    );
  };

  // Load customer
  const loadCustomer = async (
    customerId: number,
    currency: string,
    culture?: string
  ) => {
    // Load customer data
    const data = await app.customerApi.readForSale({ customerId, currency });
    if (data == null || data.customer == null) {
      return false;
    }

    // Local storage
    app.storage.setPersistedData(LocalUtils.ORDER_CUSTOMER_DATA_KEY, {
      customerId,
      currency,
      culture
    });

    const promotions = [
      ...data.promotions,
      ...(data.customer?.promotions ?? [])
    ];

    queryRef.current = {
      data,
      promotions,
      query: {
        customerId,
        currency,
        culture:
          culture === app.system.getDefaultCulture() ? undefined : culture,
        queryPaging: { ...initPagingData }
      }
    };

    loadProducts(true);

    return true;
  };

  // Choose customer
  const chooseCustomer = (data?: LocalUtils.CustomerQueryData | number) => {
    if (data == null) {
      data = { currency: app.system.getDefaultCurrency() };
    } else if (typeof data === "number") {
      data = { customerId: data, currency: app.system.getDefaultCurrency() };
    }

    app.showInputDialog({
      title: labels.chooseCustomer,
      message: "",
      callback: async (form) => {
        // Cancelled
        if (form == null) {
          if (queryRef.current == null) {
            navigate(-1);
          }
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

        // Load customer
        return await loadCustomer(customerId, currency, culture);
      },
      inputs: <CustomerChooser data={data} />,
      fullScreen: app.smDown
    });
  };

  // Total amount
  const { cartLabel, currencySymbol, orderPromotions } = React.useMemo(() => {
    // Calculate
    const total = orderLines.reduce((sum, line) => sum + line.amount, 0);
    const discount = orderLines.reduce((sum, line) => sum + line.discount, 0);
    const qty = orderLines.reduce((sum, line) => sum + line.qty, 0);

    // Cache
    let currencySymbol: string | undefined = undefined;
    let cartLabel: string | undefined = undefined;
    let orderPromotions: PromotionCodeCalculation[] = [];
    if (queryRef.current != null) {
      app.storage.setPersistedData(LocalUtils.ORDER_LINES_DATA_KEY, orderLines);

      currencySymbol = NumberUtils.getCurrencySymbol(
        queryRef.current.query.currency
      );

      const promotions = queryRef.current.promotions;
      orderPromotions = OrderUtils.calculatePromotions(promotions, total);
      let amount = 0;
      for (const p of promotions) {
        const op = orderPromotions.find((o) => o.id === p.id);
        if (op) {
          amount += op.amount;
          p.amount = op.amount;
          p.formattedTitle = `${p.title} (-${currencySymbol}${app.formatNumber(op.amount)})`;
        } else {
          p.amount = undefined;
          p.formattedTitle = undefined;
        }
      }

      cartLabel = `${labels.shoppingCart}\n${qty}${discount > 0 ? `\n(-${currencySymbol}${app.formatNumber(discount)})\n` : ""}${amount > 0 ? `(-${currencySymbol}${app.formatNumber(amount)})*\n` : ""}${currencySymbol}${app.formatNumber(total - amount)}`;
    }

    return {
      cartLabel,
      currencySymbol,
      orderPromotions
    };
  }, [orderLines, queryRef.current?.query.currency]);

  async function showCart() {
    if (queryRef.current == null) return;

    // Maximum to load 100 items, which should be enough for most cases
    const maxItems = 100;
    if (orderLines.length > maxItems) {
      app.notifier.alert(labels.tooManyItemsToDisplay);
      return;
    }

    const products = await app.productApi.queryForSale({
      ...queryRef.current.query,
      ids: orderLines.map((line) => line.productId),
      queryPaging: maxItems
    });

    if (products == null) return;

    const lines: [LocalUtils.OrderLine, QueryForSaleData][] = [];
    const emptyLines: LocalUtils.OrderLine[] = [];
    for (const line of orderLines) {
      const product = products.find((p) => p.id === line.productId);
      if (product == null) {
        emptyLines.push(line);
        continue;
      }
      lines.push([line, product]);
    }

    if (emptyLines.length > 0) {
      // Remove
      setOrderLines((prev) =>
        prev.filter((line) => !emptyLines.includes(line))
      );

      app.notifier.alert(
        labels.productDisappeared.format(
          emptyLines.map((line) => line.title).join(", ")
        )
      );
      return;
    }

    app
      .showInputDialog({
        title: labels.shoppingCart,
        message: "",
        fullScreen: app.smDown,
        inputs: (
          <CartList
            currencySymbol={currencySymbol}
            lines={lines}
            promotions={orderPromotions}
          />
        ),
        buttons: (n, _callback, base) => (
          <React.Fragment>
            <Button
              startIcon={<ClearIcon />}
              variant="outlined"
              onClick={() => {
                app.notifier.confirm(
                  labels.confirmAction.format(labels.clear),
                  undefined,
                  (result) => {
                    if (result) {
                      LocalUtils.clearOrderData(false);
                      setOrderLines([]);
                      n.dismiss();
                    }
                  }
                );
              }}
            >
              {labels.clear}
            </Button>
            {base()}
          </React.Fragment>
        ),
        callback: (form) => {
          if (form == null) {
            return;
          }

          // Cache order promotions
          const pItems: PromotionSaleItemBase[] = promotions
            .filter((p) => p.amount != null)
            .map(({ id, amount }) => ({ id, amount: amount! }));

          app.storage.setPersistedData(
            LocalUtils.ORDER_PROMOTIONS_DATA_KEY,
            pItems
          );

          // Navigate to order confirmation page
          navigate("./../confirm");

          return true;
        }
      })
      .dismiss(1800, true);
  }

  React.useEffect(() => {
    // Already chose a customer
    if (queryRef.current != null) return;

    // Local storage
    const data = app.storage.getPersistedObject<LocalUtils.CustomerQueryData>(
      LocalUtils.ORDER_CUSTOMER_DATA_KEY
    );
    if (data == null || !data.customerId) {
      chooseCustomer(customerId);
    } else {
      const { customerId, currency, culture } = data;
      loadCustomer(customerId, currency, culture).then((success) => {
        if (success) {
          // Order lines
          const pLines = app.storage.getPersistedObject<LocalUtils.OrderLine[]>(
            LocalUtils.ORDER_LINES_DATA_KEY
          );
          if (pLines != null && pLines.length > 0) {
            // Promotions may change
            app.productApi
              .queryForSale({
                customerId,
                currency,
                culture,
                ids: pLines.map((line) => line.productId)
              })
              .then((products) => {
                if (products == null) return;

                const lines: LocalUtils.OrderLine[] = [];

                for (const line of pLines) {
                  const product = products.find((p) => p.id === line.productId);
                  if (product == null) continue;

                  const price = app.order.getPrice(product);
                  const amount = price * line.qty;

                  const promotions = OrderUtils.calculatePromotions(
                    product.promotions,
                    undefined,
                    {
                      price,
                      qty: line.qty
                    }
                  );

                  const pamount = promotions.reduce(
                    (sum, p) => sum + p.amount,
                    0
                  );

                  lines.push({
                    ...line,
                    price,
                    amount: amount - pamount,
                    discount: pamount,
                    promotions
                  });
                }

                setOrderLines(lines);
              });
          }
        }
      });
    }
  }, []);

  const theme = useTheme();
  const cols = useMediaQuery(theme.breakpoints.down("sm"))
    ? 1
    : useMediaQuery(theme.breakpoints.down("md"))
      ? 2
      : useMediaQuery(theme.breakpoints.down("lg"))
        ? 3
        : 5;

  const searchBarWidth = (dimensions[0][2]?.width ?? 0) - 172;

  if (products == null || queryRef.current == null) {
    return <LinearProgress />;
  }

  const promotions = queryRef.current.promotions;

  return (
    <React.Fragment>
      <AppBar position="sticky">
        <Toolbar
          disableGutters
          sx={{
            paddingX: 1,
            gap: 1,
            backgroundColor: theme.palette.background.default
          }}
          ref={dimensions[0][0]}
        >
          {searchBarWidth > 0 ? (
            <React.Fragment>
              <IconButton
                onClick={() => chooseCustomer(queryRef.current?.query)}
                title={queryRef.current?.data?.customer?.name}
                size="small"
              >
                <Avatar>{queryRef.current?.data?.customer?.name}</Avatar>
              </IconButton>
              {promotions.length > 0 && (
                <MenuButton<LocalUtils.PromotionItemWithAmount>
                  items={promotions}
                  labelField={(data) => data.formattedTitle ?? data.title}
                  button={(clickHandler) => {
                    return (
                      <IconButton
                        onClick={clickHandler}
                        size="small"
                        title={[
                          labels.promotions,
                          ...promotions.map((p) => p.formattedTitle ?? p.title)
                        ].join("\n")}
                      >
                        <Badge
                          badgeContent={promotions.length}
                          color="secondary"
                        >
                          <CelebrationIcon color="action" />
                        </Badge>
                      </IconButton>
                    );
                  }}
                />
              )}
              <SearchBar
                fields={[
                  <SearchField
                    label={labels.productName}
                    name="keyword"
                    minChars={2}
                  />,
                  <SearchField
                    label={labels.assignedId}
                    name="assignedIdStart"
                  />,
                  <ProductCategoryTiplist
                    label={labels.category}
                    name="categoryIdAll"
                    search
                  />
                ]}
                width={searchBarWidth}
                top={true}
                autoSubmitDelay={0}
                onSubmit={(data, reset) => {
                  const { keyword, assignedIdStart, categoryIdAll } = reset
                    ? {}
                    : DomUtils.dataAs(data, {
                        keyword: "string",
                        assignedIdStart: "string",
                        categoryIdAll: "number"
                      });

                  if (queryRef.current?.query != null) {
                    queryRef.current.query = {
                      ...queryRef.current.query,
                      keyword,
                      assignedIdStart,
                      categoryIdAll,
                      queryPaging: { ...initPagingData }
                    };

                    loadProducts(true);
                  }
                }}
              />
              <IconButton
                title={cartLabel}
                onClick={orderLines.length ? () => showCart() : undefined}
              >
                <Badge badgeContent={orderLines.length} color="warning">
                  <ShoppingCartIcon color="primary" />
                </Badge>
              </IconButton>
            </React.Fragment>
          ) : undefined}
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
                  <VBox sx={{ whiteSpace: "wrap" }}>
                    <Typography>{formatName(p)}</Typography>
                    {p.description && (
                      <Typography variant="caption">{p.description}</Typography>
                    )}
                    <HBox>
                      {formatPriceLine(p, currencySymbol)}{" "}
                      {p.promotions.length > 0 && (
                        <MenuButton<PromotionItem>
                          items={p.promotions}
                          labelField={(data) => data.title}
                          button={(clickHandler) => {
                            return (
                              <IconButton
                                onClick={clickHandler}
                                size="small"
                                sx={{ marginLeft: 1.5 }}
                                title={[
                                  labels.promotions,
                                  ...p.promotions.map((p) => p.title)
                                ].join("\n")}
                              >
                                <Badge
                                  badgeContent={p.promotions.length}
                                  color="secondary"
                                />
                              </IconButton>
                            );
                          }}
                        />
                      )}
                    </HBox>
                    <HBox
                      spacing={0.5}
                      sx={{
                        alignItems: "center",
                        justifyContent: "flex-end",
                        height: 40
                      }}
                    >
                      {orderLines
                        .filter((line) => line.productId === p.id)
                        .map((line) => (
                          <Chip
                            key={line.id}
                            label={line.qty}
                            title={`${currencySymbol}${app.formatNumber(line.amount)}`}
                            clickable
                            color="success"
                            onClick={() =>
                              addOrderItem(p, line, () => {
                                // Clear the line
                                setOrderLines((prev) =>
                                  prev.filter((item) => item.id !== line.id)
                                );
                              })
                            }
                          />
                        ))}
                      {(p.promotions.length === 0 ||
                        !orderLines.some(
                          (line) => line.productId === p.id
                        )) && (
                        <IconButton
                          color="warning"
                          onClick={() => addOrderItem(p)}
                        >
                          <AddShoppingCartIcon />
                        </IconButton>
                      )}
                    </HBox>
                  </VBox>
                }
              />
            </ImageListItem>
          ))}
        </ImageList>
        {moreProducts && <div ref={divRef} style={{ height: 48 }} />}
      </CommonPage>
    </React.Fragment>
  );
}
