import {
  ButtonLink,
  HBox,
  IconButtonLink,
  MoneyInputField,
  VBox,
  ViewPage
} from "@etsoo/materialui";
import { GridDataType, ReactUtils, useParamsEx, useRefs } from "@etsoo/react";
import {
  CoreUtils,
  CurrencyItem,
  usePageDataEmpty
} from "@etsoo/smarterp-core";
import { app } from "../../../app/MyApp";
import { CustomCultureKind, ProductViewData } from "@etsoo/smarterp-crm";
import { Permissions } from "@etsoo/smarterp-crm";
import EditIcon from "@mui/icons-material/Edit";
import LinkIcon from "@mui/icons-material/Link";
import PriceChangeIcon from "@mui/icons-material/PriceChange";
import React from "react";
import Typography from "@mui/material/Typography";
import { NameCulture } from "../../../components/NameCulture";
import TableContainer from "@mui/material/TableContainer";
import Table from "@mui/material/Table";
import TableHead from "@mui/material/TableHead";
import TableRow from "@mui/material/TableRow";
import TableCell from "@mui/material/TableCell";
import TableBody from "@mui/material/TableBody";
import Button from "@mui/material/Button";
import { CurrencyList } from "../../../components/CurrencyList";
import { DomUtils, NumberUtils } from "@etsoo/shared";

function ProductPriceUI({ id }: { id: number }) {
  // Labels
  const labels = app.getLabels(
    "channelPrice",
    "costPrice",
    "promotionPrice",
    "retailPrice"
  );

  const [currency, setCurrency] = React.useState<CurrencyItem>();

  // Input refs
  const refFields = [
    "channelPrice",
    "costPrice",
    "promotionPrice",
    "retailPrice"
  ] as const;
  const refs = useRefs(refFields);

  const symbol = currency?.symbol;

  return (
    <VBox gap={1} spacing={1} paddingTop={1}>
      <CurrencyList
        fullWidth
        onItemChange={(item) => {
          if (item) {
            app.productApi.readPrice(id, item.id).then((data) => {
              setCurrency(item);
              ReactUtils.updateRefs(refs, data ?? {});
            });
          } else {
            setCurrency(undefined);
            ReactUtils.updateRefs(refs, {});
          }
        }}
        required
      />
      <MoneyInputField
        fullWidth
        name="retailPrice"
        label={
          labels.retailPrice + (currency?.name ? ` (${currency.name})` : "")
        }
        symbol={symbol}
        required
        inputRef={refs.retailPrice}
      />
      <MoneyInputField
        fullWidth
        name="promotionPrice"
        label={labels.promotionPrice}
        symbol={symbol}
        inputRef={refs.promotionPrice}
      />
      <MoneyInputField
        fullWidth
        name="channelPrice"
        label={labels.channelPrice}
        symbol={symbol}
        inputRef={refs.channelPrice}
      />
      <MoneyInputField
        fullWidth
        name="costPrice"
        label={labels.costPrice}
        symbol={symbol}
        inputRef={refs.costPrice}
      />
    </VBox>
  );
}

export default function ViewProduct() {
  // Route
  const { id = 0 } = useParamsEx({ id: "number" });

  // Load data
  const loadData = React.useCallback(() => {
    return app.productApi.read(id);
  }, [id]);

  // Labels
  const labels = app.getLabels(
    "channelPrice",
    "costPrice",
    "culture",
    "currency",
    "definePrices",
    "description",
    "edit",
    "editLogo",
    "introductionUrl",
    "logo",
    "nameB",
    "promotionPrice",
    "retailPrice"
  );

  // Culture permission
  const canManageCultures =
    app.owns(Permissions.Org.Manage) &&
    (app.userData?.system?.cultures.length ?? 0) > 1;

  const editable = app.owns(Permissions.Product.Edit);

  const definePrices = (onSuccess: () => void) => {
    app.showInputDialog({
      title: labels.definePrices,
      message: "",
      callback: async (form) => {
        // Cancelled
        if (form == null) return;

        // Validate form
        if (!form.reportValidity()) {
          return false;
        }

        // Form data
        const {
          currency,
          retailPrice,
          promotionPrice,
          channelPrice,
          costPrice
        } = DomUtils.dataAs(new FormData(form), {
          currency: "string",
          retailPrice: "number",
          promotionPrice: "number",
          channelPrice: "number",
          costPrice: "number"
        });

        if (!currency) {
          DomUtils.setFocus("currency", form);
          return false;
        }

        if (!retailPrice) {
          DomUtils.setFocus("retailPrice", form);
          return false;
        }

        const result = await app.productApi.updatePrice(id, {
          currency,
          retailPrice,
          promotionPrice,
          channelPrice,
          costPrice
        });

        if (result == null) return;

        if (result.ok) {
          onSuccess();
          return true;
        } else {
          return app.formatResult(result);
        }
      },
      inputs: <ProductPriceUI id={id} />
    });
  };

  // Page data hook
  usePageDataEmpty(app);

  return (
    <ViewPage<ProductViewData>
      paddings={0}
      leftContainerLines={3}
      leftContainer={(item) => (
        <HBox justifyContent={{ xs: "center", sm: "flex-start" }}>
          {item.logo && (
            <a href={item.logo} target="_blank" rel="noopener noreferrer">
              <img
                src={item.logo}
                alt={labels.logo}
                style={CoreUtils.avatarStyles()}
              />
            </a>
          )}
          {editable && (
            <IconButtonLink
              href={`./../../logo/${item.id}`}
              state={item.logo}
              title={labels.editLogo}
              size="small"
            >
              <EditIcon />
            </IconButtonLink>
          )}
        </HBox>
      )}
      titleBar={(item) => (
        <HBox justifyContent="center" alignItems="center" marginBottom={2}>
          <Typography variant="subtitle2" textAlign="center" paddingRight={2}>
            {item.name}
          </Typography>
          {editable && (
            <IconButtonLink
              href={`./../../edit/${item.id}`}
              title={labels.edit}
              size="small"
            >
              <EditIcon />
            </IconButtonLink>
          )}
          {item.introductionUrl && (
            <ButtonLink startIcon={<LinkIcon />} href={item.introductionUrl}>
              {labels.introductionUrl}
            </ButtonLink>
          )}
        </HBox>
      )}
      fields={[
        "assignedId",
        {
          data: (item) => app.product.getScope(item.scope),
          label: "productScope"
        },
        {
          data: (item) => app.product.getUsage(item.usage),
          label: "productUsage"
        },
        {
          data: (item) => app.product.getInventoryWay(item.inventoryWay),
          label: "productInventoryWay"
        },
        "minQty",
        "stepQty",
        "capQty",
        "assetQty",
        "taxRate",
        {
          data: (item) => item.tags?.join(", "),
          label: "tags"
        },
        {
          data: (item) => app.getStatusLabel(item.status),
          label: "status"
        },
        ["creation", GridDataType.DateTime],
        "queryKeyword",
        {
          data: (item) =>
            item.categories.map((c) => c.names.join(" -> ")).join(", "),
          label: "categories",
          singleRow: "large"
        },
        {
          data: "description",
          singleRow: true
        },
        {
          data: (item) =>
            item.prices.length ? (
              <TableContainer>
                <Table>
                  <TableHead>
                    <TableRow>
                      <TableCell width={100}>{labels.currency}</TableCell>
                      <TableCell align="right">{labels.retailPrice}</TableCell>
                      <TableCell align="right">
                        {labels.promotionPrice}
                      </TableCell>
                      <TableCell align="right">{labels.channelPrice}</TableCell>
                      <TableCell align="right">{labels.costPrice}</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {item.prices.map((p) => (
                      <TableRow key={p.currency}>
                        <TableCell component="th" scope="row">
                          {p.currency}
                        </TableCell>
                        <TableCell align="right">
                          {NumberUtils.getCurrencySymbol(p.currency)}
                          {app.formatNumber(p.retailPrice)}
                        </TableCell>
                        <TableCell align="right">
                          {p.promotionPrice == null
                            ? ""
                            : app.formatNumber(p.promotionPrice)}
                        </TableCell>
                        <TableCell align="right">
                          {p.channelPrice == null
                            ? ""
                            : app.formatNumber(p.channelPrice)}
                        </TableCell>
                        <TableCell align="right">
                          {p.costPrice == null
                            ? ""
                            : app.formatNumber(p.costPrice)}
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </TableContainer>
            ) : undefined,
          label: "",
          singleRow: true
        }
      ]}
      loadData={loadData}
      actions={(data, refresh) => (
        <React.Fragment>
          {editable && (
            <ButtonLink
              startIcon={<EditIcon />}
              variant="outlined"
              href={`./../../edit/${data.id}`}
            >
              {labels.edit}
            </ButtonLink>
          )}
          {editable && (
            <Button
              startIcon={<PriceChangeIcon />}
              variant="outlined"
              onClick={() => definePrices(refresh)}
            >
              {labels.definePrices}
            </Button>
          )}
          {canManageCultures && (
            <NameCulture
              id={id}
              kind={CustomCultureKind.Product}
              onSuccess={refresh}
            />
          )}
        </React.Fragment>
      )}
    >
      {(item) =>
        item.cultures.length && (
          <TableContainer>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell width={100}>{labels.culture}</TableCell>
                  <TableCell width={200}>{labels.nameB}</TableCell>
                  <TableCell>{labels.description}</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {item.cultures.map((c) => (
                  <TableRow key={c.culture}>
                    <TableCell component="th" scope="row">
                      {c.culture}
                    </TableCell>
                    <TableCell>{c.title}</TableCell>
                    <TableCell>{c.description}</TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </TableContainer>
        )
      }
    </ViewPage>
  );
}
