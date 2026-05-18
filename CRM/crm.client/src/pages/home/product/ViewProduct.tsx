import {
  ButtonLink,
  CustomFieldViewUI,
  DataGrid,
  GridColDef,
  GridRowId,
  HBox,
  IconButtonLink,
  LinkEx,
  MoneyInputField,
  NotificationMUDataMethods,
  NotificationMUDataProps,
  NumberInputField,
  Toolbar,
  ToolbarButton,
  useGridApiRef,
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
import {
  CustomCultureKind,
  ProductScope,
  ProductViewData,
  ProductBomNameItem,
  ProductBomItem
} from "@etsoo/smarterp-crm";
import { Permissions } from "@etsoo/smarterp-crm";
import AddIcon from "@mui/icons-material/Add";
import DeleteIcon from "@mui/icons-material/Delete";
import EditIcon from "@mui/icons-material/Edit";
import HistoryIcon from "@mui/icons-material/History";
import WidgetsIcon from "@mui/icons-material/Widgets";
import LinkIcon from "@mui/icons-material/Link";
import ListIcon from "@mui/icons-material/List";
import ImageIcon from "@mui/icons-material/Image";
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
import { DataTypes, DomUtils, NumberUtils } from "@etsoo/shared";
import { CustomFieldData } from "@etsoo/appscript";
import { ProductList } from "@etsoo/smarterp-crm/components";
import { StockByWarehouse } from "../inventory/StockByWarehouse";
import IconButton from "@mui/material/IconButton";

function isBom(scope: ProductScope) {
  return (
    (scope & ProductScope.Bundle) > 0 ||
    (scope & ProductScope.Disassemble) > 0 ||
    (scope & ProductScope.Production) > 0
  );
}

function BomAddUI({
  excludeId,
  item
}: {
  excludeId: number;
  item?: ProductBomNameItem;
}) {
  // Labels
  const labels = app.getLabels("qty");

  const inputRef = React.useRef<HTMLInputElement>(null);
  const qtyRef = React.useRef<HTMLInputElement>(null);

  return (
    <VBox spacing={2} sx={{ paddingTop: 1 }}>
      <input
        type="hidden"
        name="name"
        ref={inputRef}
        value={item?.name ?? ""}
      />
      <ProductList
        fullWidth
        inputRequired
        idValue={item?.productId}
        rq={{ excludedIds: [excludeId] }}
        onValueChange={(p) => {
          if (inputRef.current) inputRef.current.value = p?.name ?? "";
          if (p != null) qtyRef.current?.focus();
        }}
      />
      <NumberInputField
        fullWidth
        name="qty"
        defaultValue={item?.qty ?? ""}
        label={labels.qty}
        inputRef={qtyRef}
        required
      />
    </VBox>
  );
}

function BomUI({
  excludeId,
  items,
  mRef
}: NotificationMUDataProps & {
  excludeId: number;
  items: ProductBomNameItem[];
}) {
  // Labels
  const labels = app.getLabels(
    "add",
    "delete",
    "edit",
    "noRows",
    "product",
    "qty"
  );

  const [rows, setRows] = React.useState(items);
  const [selectedId, setSelectedId] = React.useState<GridRowId>();

  const gridRef = useGridApiRef();

  React.useImperativeHandle(mRef, () => ({
    getValue: (): ProductBomItem[] | undefined => {
      return rows;
    }
  }));

  const columns: GridColDef<ProductBomNameItem>[] = [
    {
      field: "name",
      headerName: labels.product,
      editable: false,
      flex: 2
    },
    {
      field: "qty",
      headerName: labels.qty,
      type: "number",
      width: 110,
      editable: true
    }
  ];

  const addItem = (item?: ProductBomNameItem) => {
    app.showInputDialog({
      title: labels.add,
      message: "",
      callback: async (form) => {
        // Cancelled
        if (form == null) return;

        // Validate form
        if (!form.reportValidity()) {
          return false;
        }

        const { productId, name, qty } = DomUtils.dataAs(new FormData(form), {
          productId: "number",
          name: "string",
          qty: "number"
        });

        if (productId == null || name == null || qty == null) {
          return false;
        } else if (qty <= 0) {
          DomUtils.setFocus("qty", form);
          return false;
        } else if (
          rows.some(
            (r) => r.productId === productId && (item == null || item != r)
          )
        ) {
          DomUtils.setFocus("productIdInput", form);
          return false;
        }

        /*
        const id = gridRef.current?.state.rowSelection.ids
          ?.values()
          .next().value;

        const rowIndex = id
          ? gridRef.current?.getRowIndexRelativeToVisibleRows(id)
          : undefined;
        */

        if (item) {
          setRows((prev) =>
            prev.map((r) =>
              r.productId === item.productId ? { productId, name, qty } : r
            )
          );
        } else {
          setRows((prev) => [...prev, { productId, name, qty }]);
        }

        return;
      },
      inputs: <BomAddUI excludeId={excludeId} item={item} />
    });
  };

  function CustomToolbar() {
    return (
      <Toolbar>
        <ToolbarButton
          disabled={selectedId == null}
          title={labels.delete}
          onClick={() => {
            if (selectedId == null) return;
            setRows((prev) => prev.filter((r) => r.productId !== selectedId));
          }}
        >
          <DeleteIcon fontSize="small" />
        </ToolbarButton>
        <ToolbarButton
          disabled={selectedId == null}
          title={labels.edit}
          onClick={() => addItem(rows.find((r) => r.productId === selectedId))}
        >
          <EditIcon fontSize="small" />
        </ToolbarButton>
        <ToolbarButton title={labels.add} onClick={() => addItem()}>
          <AddIcon fontSize="small" />
        </ToolbarButton>
      </Toolbar>
    );
  }

  return (
    <VBox sx={{ height: 400, width: "100%" }}>
      <DataGrid
        apiRef={gridRef}
        rows={rows}
        columns={columns}
        editMode="row"
        hideFooter
        disableColumnMenu
        disableColumnSorting
        disableMultipleRowSelection
        localeText={{ noRowsLabel: labels.noRows }}
        showToolbar
        slots={{
          toolbar: CustomToolbar
        }}
        getRowId={(row) => row.productId}
        onRowSelectionModelChange={(row) => {
          setSelectedId(row.ids.values().next().value);
        }}
      />
    </VBox>
  );
}

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
    <VBox spacing={2} sx={{ paddingTop: 1 }}>
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

  const [customFields, setCustomFields] = React.useState<CustomFieldData[]>([]);

  // Load data
  const loadData = React.useCallback(async () => {
    const data = await app.productApi.read(id);
    if (data?.categories.length && data?.data?.attributes) {
      const fields = await app.productCategoryApi.getAttributes(
        data.categories.map((c) => c.id)
      );
      if (fields == null) return;
      setCustomFields(fields);
    }
    return data;
  }, [id]);

  // Labels
  const labels = app.getLabels(
    "bom",
    "channelPrice",
    "costPrice",
    "culture",
    "currency",
    "days",
    "definePrices",
    "description",
    "edit",
    "editLogo",
    "history",
    "introductionUrl",
    "logo",
    "nameB",
    "product",
    "promotionPrice",
    "qty",
    "retailPrice",
    "stockByWarehouse",
    "validity"
  );

  // Culture permission
  const canManageCultures = app.system.canManageCultures();

  const editable = app.owns(Permissions.Product.Edit);
  const isQueryInventory = app.owns(Permissions.Inventory.Query);

  const editBoms = (items: ProductBomNameItem[], onSuccess: () => void) => {
    app.notifier.data<ProductBomItem[]>(
      <BomUI
        excludeId={id}
        items={items}
        mRef={React.createRef<NotificationMUDataMethods>()}
      />,
      async (data) => {
        if (data == null) return;

        const result = await app.productApi.editBoms({
          parentId: id,
          items: data
        });

        if (result == null) return;

        if (result.ok) {
          onSuccess();
          return true;
        } else {
          return app.formatResult(result);
        }
      },
      labels.bom
    );
  };

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
      leftContainer={(item) =>
        item.logo ? (
          <HBox sx={{ justifyContent: { xs: "center", sm: "flex-start" } }}>
            <a href={item.logo} target="_blank" rel="noopener noreferrer">
              <img
                src={item.logo}
                alt={labels.logo}
                style={CoreUtils.avatarStyles()}
              />
            </a>
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
        ) : undefined
      }
      titleBar={(item) => (
        <HBox
          sx={{
            justifyContent: "center",
            alignItems: "center",
            marginBottom: 2
          }}
        >
          <Typography
            variant="subtitle2"
            sx={{ textAlign: "center", paddingRight: 2 }}
          >
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
        { data: "unit", label: "productUnit" },
        "assignedId",
        {
          data: (item) =>
            app.product
              .getScope(item.scope)
              ?.map((s) => s.label)
              .join(", "),
          label: "productScope",
          singleRow: "large"
        },
        {
          data: (item) => app.product.getUsage(item.usage),
          label: "productUsage"
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
          data: "validity",
          dataType: GridDataType.Number,
          label: `${labels.validity} (${labels.days})`
        },
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
            (item.scope & ProductScope.Inventory) > 0 && isQueryInventory ? (
              <HBox
                spacing={1}
                sx={{ justifyContent: "center", flexWrap: "wrap" }}
              >
                <Button
                  startIcon={<WidgetsIcon />}
                  variant="outlined"
                  onClick={() => StockByWarehouse.show(item.id)}
                >
                  {labels.stockByWarehouse}
                </Button>
                <ButtonLink
                  startIcon={<HistoryIcon />}
                  variant="outlined"
                  href={`./../../../inventory/history/${item.id}`}
                >
                  {labels.history}
                </ButtonLink>
              </HBox>
            ) : undefined,
          singleRow: true
        },
        {
          data: (item) =>
            item.prices.length ? (
              <TableContainer>
                <Table size={app.smDown ? "small" : undefined}>
                  <TableHead>
                    <TableRow>
                      <TableCell>{labels.currency}</TableCell>
                      <TableCell align="right">
                        {labels.retailPrice} / {labels.promotionPrice}
                      </TableCell>
                      <TableCell align="right">
                        {labels.channelPrice} / {labels.costPrice}
                      </TableCell>
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
                          {app.formatNumber(p.retailPrice ?? 0)} /{" "}
                          {p.promotionPrice == null
                            ? "--"
                            : app.formatNumber(p.promotionPrice)}
                        </TableCell>
                        <TableCell align="right">
                          {p.channelPrice == null
                            ? "--"
                            : app.formatNumber(p.channelPrice)}{" "}
                          /{" "}
                          {p.costPrice == null
                            ? "--"
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
              startIcon={<ImageIcon />}
              variant="outlined"
              href={`./../../logo/${data.id}`}
              state={data.logo}
            >
              {labels.editLogo}
            </ButtonLink>
          )}
          {editable && isBom(data.scope) && (
            <Button
              startIcon={<ListIcon />}
              variant="outlined"
              onClick={() => editBoms(data.boms, refresh)}
            >
              {labels.bom}
            </Button>
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
          {editable && (
            <ButtonLink
              startIcon={<EditIcon />}
              variant="outlined"
              href={`./../../edit/${data.id}`}
            >
              {labels.edit}
            </ButtonLink>
          )}
        </React.Fragment>
      )}
    >
      {(item) => (
        <React.Fragment>
          {customFields.length > 0 && (
            <CustomFieldViewUI
              fields={customFields}
              data={item.data?.attributes as DataTypes.StringRecord}
              refresh={async () => {
                loadData();
              }}
            />
          )}
          {item.boms.length > 0 && (
            <TableContainer>
              <Table>
                <TableHead>
                  <TableRow>
                    <TableCell>{labels.product} (BOM)</TableCell>
                    <TableCell width={100} align="right">
                      {labels.qty}
                    </TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {item.boms.map((b) => (
                    <TableRow key={b.productId}>
                      <TableCell component="th" scope="row">
                        <LinkEx to={`./../${b.productId}`}>{b.name}</LinkEx>
                        {(item.scope & ProductScope.Inventory) > 0 &&
                          isQueryInventory && (
                            <IconButton
                              size="small"
                              onClick={() => StockByWarehouse.show(b.productId)}
                              title={labels.stockByWarehouse}
                            >
                              <WidgetsIcon />
                            </IconButton>
                          )}
                      </TableCell>
                      <TableCell align="right">{b.qty}</TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </TableContainer>
          )}
          {item.cultures.length > 0 && (
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
          )}
        </React.Fragment>
      )}
    </ViewPage>
  );
}
