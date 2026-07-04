import { GridDataType, useParamsEx } from "@etsoo/react";
import {
  ButtonLink,
  HBox,
  HBoxList,
  ImagePreviewButton,
  ViewPage
} from "@etsoo/materialui";
import HistoryIcon from "@mui/icons-material/History";
import RemoveCircleIcon from "@mui/icons-material/RemoveCircle";
import { app } from "../../../app/MyApp";
import { usePageDataEmpty } from "@etsoo/smarterp-core";
import React from "react";
import { ReadUserDto } from "../../../api/dto/query/ReadUserDto";
import { DateUtils } from "@etsoo/shared";
import Typography from "@mui/material/Typography";
import Chip from "@mui/material/Chip";
import Button from "@mui/material/Button";

export default function ViewUser() {
  // Route
  const { id = 0 } = useParamsEx({ id: "number" });

  // Load data
  const loadData = React.useCallback(() => {
    return app.queryApi.readUser(id);
  }, [id]);

  // Labels
  const labels = app.getLabels(
    "avatar",
    "auditHistory",
    "clearUserFrozen",
    "confirmAction"
  );

  // Page data hook
  usePageDataEmpty(app);

  return (
    <ViewPage<ReadUserDto>
      paddings={0}
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
            align="center"
            sx={{ paddingRight: 2 }}
          >
            {item.name}
            {item.preferredName ? ` (${item.preferredName})` : ""}
          </Typography>
        </HBox>
      )}
      leftContainerLines={3}
      leftContainer={(item) =>
        item.avatar ? (
          <ImagePreviewButton
            size={[130, 160]}
            image={item.avatar}
            buttonProps={{ title: labels.avatar }}
          />
        ) : undefined
      }
      fields={[
        "familyName",
        "givenName",
        "latinFamilyName",
        "latinGivenName",
        "pin",
        ["creation", GridDataType.DateTime],
        ["frozenTime", GridDataType.DateTime],
        {
          data: (item) =>
            item.deviceList.length === 0 ? undefined : (
              <HBoxList>
                {item.deviceList.map((d) => (
                  <Chip key={d.id} label={d.name} variant="outlined" />
                ))}
              </HBoxList>
            ),
          singleRow: "large",
          label: (item) => `${app.get("deviceList")} (${item.devices})`
        },
        {
          data: (item) =>
            item.orgList.length === 0 ? undefined : (
              <HBoxList>
                {item.orgList.map((o) => (
                  <ButtonLink
                    href={`./../../../org/view/${o.id}`}
                    key={o.id}
                    size="small"
                    variant="outlined"
                  >
                    {o.name}
                  </ButtonLink>
                ))}
              </HBoxList>
            ),
          singleRow: true,
          label: (item) => `${app.get("orgs")} (${item.orgs})`
        },
        {
          data: (item) =>
            item.identifierList.length === 0 ? undefined : (
              <HBoxList>
                {item.identifierList.map((d) => (
                  <Chip
                    key={d.id}
                    label={`${app.core.getIdentifierTypeLabel(d.type)}: ${
                      d.value
                    }`}
                    variant="outlined"
                  />
                ))}
              </HBoxList>
            ),
          singleRow: true,
          label: "identifier"
        }
      ]}
      loadData={loadData}
      actions={(data, refresh) => (
        <React.Fragment>
          <ButtonLink
            variant="contained"
            href={`./../../../audithistory?userId=${id}`}
            startIcon={<HistoryIcon />}
          >
            {labels.auditHistory}
          </ButtonLink>
          {data.frozenTime &&
            DateUtils.parse(data.frozenTime)! >= new Date() && (
              <Button
                variant="contained"
                startIcon={<RemoveCircleIcon />}
                onClick={() => {
                  const title = `${labels.clearUserFrozen} (${data.name})`;
                  app.notifier.confirm(
                    labels.confirmAction.format(labels.clearUserFrozen),
                    title,
                    async (ok) => {
                      if (!ok) return;
                      const result = await app.adminApi.clearUserFrozen(id);
                      if (result == null) return;
                      if (result.ok) {
                        app.notifier.succeed(title, undefined, () => refresh());
                        return;
                      }
                      app.alertResult(result);
                    }
                  );
                }}
              >
                {labels.clearUserFrozen}
              </Button>
            )}
        </React.Fragment>
      )}
    ></ViewPage>
  );
}
