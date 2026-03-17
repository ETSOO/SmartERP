import { DownUpButton, EditPage, InputField } from "@etsoo/materialui";
import { app } from "../../../app/MyApp";
import React from "react";
import { useNavigate } from "react-router-dom";
import { usePageDataEmpty } from "@etsoo/smarterp-core";
import { CustomCultureKind, ProductUnitItem } from "@etsoo/smarterp-crm";
import Grid from "@mui/material/Grid";
import { ProductBaseUnits } from "@etsoo/smarterp-core/components";
import Alert from "@mui/material/Alert";
import { DataTypes } from "@etsoo/shared";
import { ProductUnit } from "@etsoo/appscript";
import { NameCulture } from "../../../components/NameCulture";

export default function ProductUnits() {
  // Route
  const navigate = useNavigate();

  // State
  const [units, setUnits] = React.useState<ProductUnitItem[]>([]);

  React.useEffect(() => {
    app.productApi.queryUnit().then((units) => {
      if (units == null) return;
      setUnits(units);
    });
  }, []);

  usePageDataEmpty(app);

  // Culture permission
  const canManageCultures = app.system.canManageCultures();

  // Labels
  const labels = app.getLabels("nameB", "noChanges", "productUnitTip");

  const systemLen = units.filter((unit) => unit.isSystem).length;
  const newLen = 3;

  function onUpClick(event: React.MouseEvent<HTMLButtonElement>) {
    const button = event.currentTarget;
    const container = button.closest(".MuiGrid-root");
    if (!container) return;

    const previousElements = [
      container.previousElementSibling,
      container.previousElementSibling?.previousElementSibling,
      container.previousElementSibling?.previousElementSibling
        ?.previousElementSibling
    ].filter(Boolean) as Element[];

    const nextElements = [
      container.nextElementSibling,
      container.nextElementSibling?.nextElementSibling,
      container.nextElementSibling?.nextElementSibling?.nextElementSibling
    ].filter(Boolean) as Element[];

    if (previousElements.length !== 3 || nextElements.length !== 3) return;

    nextElements.forEach((element) => {
      container.before(element);
    });

    previousElements.forEach((element) => {
      container.after(element);
    });
  }

  function onDownClick(event: React.MouseEvent<HTMLButtonElement>) {
    const button = event.currentTarget;
    const container = button.closest(".MuiGrid-root");
    if (!container) return;

    // Next button container
    const nc =
      container.nextElementSibling?.nextElementSibling?.nextElementSibling
        ?.nextElementSibling;
    if (!nc) return;

    const nextElements = [
      nc.nextElementSibling,
      nc.nextElementSibling?.nextElementSibling,
      nc.nextElementSibling?.nextElementSibling?.nextElementSibling
    ].filter(Boolean) as Element[];

    if (nextElements.length !== 3) return;

    const elements = [
      container.nextElementSibling,
      container.nextElementSibling.nextElementSibling,
      container.nextElementSibling.nextElementSibling.nextElementSibling
    ];

    nextElements.reverse().forEach((element) => {
      container.after(element);
    });

    elements.reverse().forEach((element) => {
      nc.after(element);
    });
  }

  return (
    <EditPage
      isEditing={false}
      paddings={0}
      topPart={
        <Alert severity="warning" sx={{ mb: 1 }}>
          {labels.productUnitTip}
        </Alert>
      }
      onSubmit={async (event) => {
        event.preventDefault();

        const form = event.currentTarget;

        const removedIds: number[] = [];
        const items: ProductUnitItem[] = [];

        const baseUnits = form.querySelectorAll<HTMLInputElement>(
          `input[name="baseUnit"]`
        );

        baseUnits.forEach((baseUnitInput) => {
          const container = baseUnitInput.closest(".MuiGrid-root");
          if (container == null) return;

          const idInput =
            container.querySelector<HTMLInputElement>(`input[name="id"]`);

          const nameInput =
            container.nextElementSibling?.querySelector<HTMLInputElement>(
              `input[name="name"]`
            );

          if (nameInput == null) return;

          const id = idInput ? parseInt(idInput.value) : undefined;
          const baseUnit = parseInt(baseUnitInput.value);

          const baseUnitValue = DataTypes.getEnumByValue(
            ProductUnit,
            baseUnit
          ) as ProductUnit | null | undefined;

          if (baseUnitValue == null) {
            if (id) removedIds.push(id);
            return;
          }

          const name = nameInput.value.trim();

          if (baseUnitValue && name === "") {
            nameInput.focus();
            return;
          }

          items.push({
            id,
            baseUnit: baseUnitValue,
            name,
            isSystem: false
          });
        });

        if (removedIds.length === 0 && items.length === 0) {
          app.warning(labels.noChanges);
          return;
        }

        const result = await app.productApi.updateUnit({
          items,
          removedIds
        });

        if (result == null) return;

        navigate("./../");
      }}
    >
      {units.map((unit, index) => (
        <React.Fragment key={`Unit${index}`}>
          <Grid size={{ xs: 6, sm: 1 }}>
            {!unit.isSystem && (
              <DownUpButton
                upDisabled={index === systemLen}
                onDownClick={onDownClick}
                onUpClick={onUpClick}
                fullWidth
              />
            )}
          </Grid>
          <Grid size={{ xs: 6, sm: 3 }}>
            <input type="hidden" name={`id`} value={unit.id} />
            <ProductBaseUnits
              idValue={unit.baseUnit}
              disabled={unit.isSystem}
              name={`baseUnit`}
            />
          </Grid>
          <Grid size={{ xs: 6, sm: 4 }}>
            <InputField
              fullWidth
              slotProps={{
                htmlInput: { maxLength: 30 }
              }}
              disabled={unit.isSystem}
              label={labels.nameB}
              name={`name`}
              defaultValue={unit.name}
            />
          </Grid>
          <Grid size={{ xs: 6, sm: 4 }}>
            {!unit.isSystem && canManageCultures && unit.id && (
              <NameCulture id={unit.id} kind={CustomCultureKind.ProductUnit} />
            )}
          </Grid>
        </React.Fragment>
      ))}
      {[...Array(newLen)].map((_, index) => (
        <React.Fragment key={`New${index}`}>
          <Grid size={{ xs: 6, sm: 1 }}>
            <DownUpButton
              downDisabled={index === newLen - 1}
              onDownClick={onDownClick}
              onUpClick={onUpClick}
              fullWidth
            />
          </Grid>
          <Grid size={{ xs: 6, sm: 3 }}>
            <ProductBaseUnits
              name={`baseUnit`}
              onValueChange={(value, input) => {
                if (input == null) return;
                const container = input.closest(".MuiGrid-root");
                const nameInput =
                  container?.nextElementSibling?.querySelector<HTMLInputElement>(
                    `input[name="name"]`
                  );
                if (nameInput) {
                  nameInput.value = value?.label ?? "";
                }
              }}
            />
          </Grid>
          <Grid size={{ xs: 6, sm: 4 }}>
            <InputField
              fullWidth
              slotProps={{
                htmlInput: { maxLength: 30 }
              }}
              label={labels.nameB}
              name={`name`}
            />
          </Grid>
          <Grid size={{ xs: 6, sm: 4 }}></Grid>
        </React.Fragment>
      ))}
    </EditPage>
  );
}
