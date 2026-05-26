import { useParamsEx } from "@etsoo/react";
import { OrgCreateResourceRQ, usePageDataEmpty } from "@etsoo/smarterp-core";
import React from "react";
import { app } from "../../../app/MyApp";
import { EditPage, InputField } from "@etsoo/materialui";
import Grid from "@mui/material/Grid";
import { useNavigate } from "react-router-dom";
import { DomUtils, NumberUtils } from "@etsoo/shared";
import { OrgTiplist } from "../../../components/OrgTiplist";

export default function AddCustomResource() {
  // Route
  const navigate = useNavigate();

  const { id = 0 } = useParamsEx({
    id: "number"
  });

  // Labels
  const labels = app.getLabels(
    "addResource",
    "deleteConfirm",
    "description",
    "editResource",
    "item",
    "jsonData",
    "key",
    "org",
    "title"
  );

  const isEditing = id > 0;

  // State
  const [data, setData] = React.useState<OrgCreateResourceRQ>({});
  const itemsRef = React.useRef<OrgCreateResourceRQ["items"]>(null);

  const isAdmin = app.isAdminUser();

  // Load data
  const reloadData = React.useCallback(async () => {
    if (id < 1) return;
    const result = await app.core.orgApi.updateResourceRead(id);
    if (result == null) return;

    // Deep copy
    itemsRef.current = result.items?.map((item) => ({ ...item }));
    setData(result);
  }, [id]);

  // Page data hook
  usePageDataEmpty(app);

  return (
    <EditPage
      isEditing={isEditing}
      onDelete={
        isAdmin
          ? () => {
              app.notifier.confirm(
                labels.deleteConfirm.format(labels.item),
                undefined,
                async (ok) => {
                  if (!ok) return;

                  const result = await app.core.orgApi.createResource(
                    { id, items: [] },
                    {
                      showLoading: false
                    }
                  );
                  if (result == null) return;

                  if (result.ok) {
                    navigate("./../../");
                    return;
                  }

                  app.alertResult(result);
                }
              );
            }
          : undefined
      }
      onSubmit={(event) => {
        event.preventDefault();
        const form = event.currentTarget;
        const formData = new FormData(form);

        const key = formData.get("key")?.toString().trim();
        if (!key || key.startsWith("etsoo")) {
          DomUtils.setFocus("key");
          return;
        }

        const orgId = NumberUtils.parse(formData.get("orgId")?.toString());

        const items: OrgCreateResourceRQ["items"] = [];

        app.settings.cultures.forEach((c) => {
          let title = formData.get(`${c.name}-title`)?.toString().trim();
          if (!title) return;

          let description = formData
            .get(`${c.name}-description`)
            ?.toString()
            .trim();
          if (description === "") description = undefined;

          const jsonField = `${c.name}-jsonData`;
          let jsonData = formData.get(jsonField)?.toString().trim();
          if (jsonData) {
            try {
              jsonData = JSON.stringify(JSON.parse(jsonData));
            } catch {
              DomUtils.setFocus(jsonField);
              return;
            }
          } else {
            jsonData = undefined;
          }

          // Check if the item already exists
          const item = itemsRef.current?.find(
            (item) => item.culture === c.name
          );

          let updatedFlag = 0;
          if (isEditing && item) {
            if (item.title !== title) {
              updatedFlag |= 1;
            } else {
              title = undefined;
            }

            if (item.description !== description) {
              updatedFlag |= 2;
            } else {
              description = undefined;
            }

            if (item.jsonData !== jsonData) {
              updatedFlag |= 4;
            } else {
              jsonData = undefined;
            }

            if (updatedFlag === 0) return;
          }

          items.push({
            culture: c.name,
            title,
            description,
            jsonData,
            updatedFlag
          });
        });

        if (!isEditing && items.length < 1) {
          const firstTitleField = `${app.settings.cultures[0].name}-title`;
          DomUtils.setFocus(firstTitleField);
          return;
        }

        const rq: OrgCreateResourceRQ = {
          id: id > 0 ? id : undefined,
          orgId,
          key,
          items: isEditing && items.length === 0 ? undefined : items
        };

        app.core.orgApi.createResource(rq).then((result) => {
          if (result == null) return;

          if (result.ok) {
            const root = isEditing ? "./../../" : "./../";
            navigate(root);
            return;
          }

          app.alertResult(result);
        });
      }}
      onUpdate={reloadData}
      paddings={0}
    >
      <Grid size={{ xs: 12, sm: 12 }}>
        <OrgTiplist
          name="orgId"
          label={labels.org}
          search={false}
          idValue={data.orgId}
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 12 }}>
        <InputField
          fullWidth
          required
          name="key"
          slotProps={{ htmlInput: { maxLength: 50 } }}
          label={labels.key}
          value={data.key ?? ""}
          onChange={(event) => {
            const value = event.target.value;
            setData((old) => {
              return { ...old, key: value };
            });
          }}
        />
      </Grid>
      {app.settings.cultures.map((c) => (
        <React.Fragment key={c.name}>
          <Grid size={{ xs: 12, sm: 12 }}>
            <InputField
              fullWidth
              name={`${c.name}-title`}
              slotProps={{ htmlInput: { maxLength: 256 } }}
              label={`${labels.title} (${c.label})`}
              value={
                data.items?.find((item) => item.culture === c.name)?.title ?? ""
              }
              onChange={(event) => {
                const value = event.target.value;
                setData((old) => {
                  const items = [...(old.items ?? [])];
                  const item = items.find((item) => item.culture === c.name);
                  if (item) {
                    item.title = value;
                  } else {
                    items.push({
                      culture: c.name,
                      title: value
                    });
                  }
                  return { ...old, items };
                });
              }}
            />
          </Grid>
          <Grid size={{ xs: 12, sm: 12 }}>
            <InputField
              fullWidth
              multiline
              rows={2}
              name={`${c.name}-description`}
              slotProps={{ htmlInput: { maxLength: 2560 } }}
              label={`${labels.description}`}
              value={
                data.items?.find((item) => item.culture === c.name)
                  ?.description ?? ""
              }
              onChange={(event) => {
                const value = event.target.value;
                setData((old) => {
                  const items = [...(old.items ?? [])];
                  const item = items.find((item) => item.culture === c.name);
                  if (item) {
                    item.description = value;
                    return { ...old, items };
                  } else {
                    return old;
                  }
                });
              }}
            />
          </Grid>
          <Grid size={{ xs: 12, sm: 12 }}>
            <InputField
              fullWidth
              multiline
              rows={3}
              name={`${c.name}-jsonData`}
              label={`${labels.jsonData} (JSON)`}
              value={
                data.items?.find((item) => item.culture === c.name)?.jsonData ??
                ""
              }
              onChange={(event) => {
                const value = event.target.value;
                setData((old) => {
                  const items = [...(old.items ?? [])];
                  const item = items.find((item) => item.culture === c.name);
                  if (item) {
                    item.jsonData = value;
                    return { ...old, items };
                  } else {
                    return old;
                  }
                });
              }}
            />
          </Grid>
        </React.Fragment>
      ))}
    </EditPage>
  );
}
