import {
  DialogButton,
  MUGlobal,
  SearchField,
  MobileListItemRenderer,
  MUUtils,
  ResponsivePage,
  IconButtonLink,
  HBox
} from "@etsoo/materialui";
import { DataTypes, DateUtils } from "@etsoo/shared";
import { BoxProps, Typography } from "@mui/material";
import InfoIcon from "@mui/icons-material/Info";
import PersonIcon from "@mui/icons-material/Person";
import React from "react";
import {
  GridCellRendererProps,
  GridDataType,
  ScrollerListForwardRef,
  useSearchParamsEx
} from "@etsoo/react";
import { app } from "../../../app/MyApp";
import { usePageDataEmpty } from "@etsoo/smarterp-core";
import { DefaultUI } from "@etsoo/smarterp-core/components";
import { AuditHistoryDto } from "../../../api/dto/query/AuditHistoryDto";
import { AppTiplist } from "../../../components/AppTiplist";
import { OrgTiplist } from "../../../components/OrgTiplist";
import { UserTiplist } from "../../../components/UserTiplist";

const template = {
  id: "number",
  userId: "number",
  orgId: "number",
  appId: "number",
  kind: "string",
  ip: "string",
  targetId: "number",
  keyword: "string",
  creationStart: "date",
  creationEnd: "date"
} as const satisfies DataTypes.BasicTemplate;

export default function LoginHistory() {
  // Route
  const { userId } = useSearchParamsEx({ userId: "number" });

  // Labels
  const labels = app.getLabels(
    "actions",
    "creation",
    "endDate",
    "id",
    "org",
    "app",
    "startDate",
    "targetId",
    "title",
    "type",
    "user"
  );

  // Refs
  const ref = React.useRef<ScrollerListForwardRef<AuditHistoryDto>>();

  // Load data
  const reloadData = () => ref.current?.reset();

  const margin = MUGlobal.pagePaddings;
  const creationEndRef = React.useRef<HTMLInputElement>();

  // Page data hook
  usePageDataEmpty(app);

  return (
    <ResponsivePage<AuditHistoryDto, typeof template>
      {...DefaultUI.createProps({
        onRefresh: reloadData
      })}
      mRef={ref}
      defaultOrderBy={[{ field: "creation", desc: true }]}
      fieldTemplate={template}
      fields={(data) => [
        <SearchField
          label={labels.id}
          name="id"
          minChars={2}
          type="number"
          inputMode="numeric"
          defaultValue={data.id}
        />,
        <SearchField
          label={labels.title}
          name="keyword"
          minChars={2}
          defaultValue={data.keyword}
          slotProps={{ htmlInput: { maxLength: 128 } }}
        />,
        <UserTiplist idValue={data.userId ?? userId} />,
        <OrgTiplist idValue={data.orgId} />,
        <AppTiplist idValue={data.appId} />,
        <SearchField
          label={labels.type}
          name="kind"
          minChars={2}
          defaultValue={data.kind}
          slotProps={{ htmlInput: { maxLength: 30 } }}
        />,
        <SearchField
          label="IP"
          name="ip"
          minChars={2}
          defaultValue={data.ip}
          slotProps={{ htmlInput: { maxLength: 45 } }}
        />,
        <SearchField
          label={labels.targetId}
          name="targetId"
          type="number"
          inputMode="numeric"
          defaultValue={data.targetId}
        />,
        <SearchField
          label={labels.startDate}
          name="creationStart"
          type="date"
          onChange={(event: React.ChangeEvent<HTMLInputElement>) => {
            if (creationEndRef.current == null) return;
            const date = DateUtils.formatForInput(
              event.currentTarget.valueAsDate
            );
            if (date) creationEndRef.current.min = date;
          }}
          slotProps={{
            htmlInput: { max: DateUtils.formatForInput(new Date()) }
          }}
          defaultValue={DateUtils.formatForInput(data.creationStart)}
        />,
        <SearchField
          label={labels.endDate}
          name="creationEnd"
          type="date"
          inputRef={creationEndRef}
          slotProps={{
            htmlInput: { max: DateUtils.formatForInput(new Date()) }
          }}
          defaultValue={DateUtils.formatForInput(data.creationEnd)}
        />
      ]}
      loadData={async (data, lastItem) =>
        app.queryApi.auditHistory(
          MUUtils.setupPagingKeysets(data, lastItem, "id"),
          {
            defaultValue: [],
            showLoading: false
          }
        )
      }
      columns={[
        {
          field: "creation",
          type: GridDataType.DateTime,
          width: 164,
          header: labels.creation,
          sortable: true,
          sortAsc: false,
          renderProps: app.getDateFormatProps()
        },
        {
          field: "userId",
          header: labels.user,
          width: 80,
          type: GridDataType.Unkwown
        },
        {
          field: "organizationId",
          header: labels.org,
          width: 80,
          type: GridDataType.Unkwown
        },
        { field: "title", header: labels.title },
        {
          field: "appId",
          header: labels.app,
          width: 72,
          type: GridDataType.Unkwown
        },
        {
          width: DefaultUI.Widths.icon2,
          header: labels.actions,
          align: "center",
          cellRenderer: ({
            data,
            cellProps
          }: GridCellRendererProps<AuditHistoryDto, BoxProps>) => {
            if (data == null) return undefined;

            cellProps.sx = {
              paddingTop: "6px!important",
              paddingBottom: "6px!important"
            };

            return (
              <React.Fragment>
                <DialogButton
                  content={JSON.stringify(data, undefined, 2)}
                  contentPre
                  disableScrollLock
                  maxWidth="xs"
                  size="small"
                  icon={<InfoIcon />}
                >
                  JSON data
                </DialogButton>
                <IconButtonLink
                  size="small"
                  href={`./../user/view/${data.userId}`}
                >
                  <PersonIcon />
                </IconButtonLink>
              </React.Fragment>
            );
          }
        }
      ]}
      itemSize={[112, margin]}
      innerItemRenderer={(props) =>
        MobileListItemRenderer(props, (data) => [
          data.title,
          app.formatDate(data.creation, "ds"),
          [
            {
              label: labels.user,
              icon: <PersonIcon />,
              action: `./../user/view/${data.userId}`
            }
          ],
          <HBox alignItems="center">
            <DialogButton
              content={JSON.stringify(data, undefined, 2)}
              contentPre
              disableScrollLock
              maxWidth="xs"
              size="small"
              icon={<InfoIcon />}
            >
              JSON data
            </DialogButton>
            <Typography variant="caption">{data.kind}</Typography>
          </HBox>
        ])
      }
    />
  );
}
