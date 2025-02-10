import {
  DialogButton,
  MUGlobal,
  SearchField,
  MobileListItemRenderer,
  Tiplist,
  MUUtils,
  ResponsivePage
} from "@etsoo/materialui";
import { DataTypes, DateUtils } from "@etsoo/shared";
import { BoxProps, Typography } from "@mui/material";
import InfoIcon from "@mui/icons-material/Info";
import React from "react";
import {
  GridCellRendererProps,
  GridDataType,
  ScrollerListForwardRef
} from "@etsoo/react";
import { app } from "../../../app/MyApp";
import {
  AuditHistoryDto,
  DeviceListDto,
  usePageDataEmpty
} from "@etsoo/smarterp-core";
import { DefaultUI } from "@etsoo/smarterp-core/components";

const template = {
  keyword: "string",
  deviceId: "number",
  creationStart: "date",
  creationEnd: "date"
} as const satisfies DataTypes.BasicTemplate;

export default function LoginHistory() {
  // Labels
  const labels = app.getLabels(
    "actions",
    "creation",
    "device",
    "endDate",
    "startDate",
    "title",
    "type"
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
      cacheKey="search-history-cache"
      fieldTemplate={template}
      fields={(data) => [
        <SearchField
          label={labels.title}
          name="keyword"
          minChars={2}
          defaultValue={data.keyword}
        />,
        <Tiplist<DeviceListDto>
          label={labels.device}
          name="deviceId"
          minChars={2}
          search
          loadData={(keyword, id) =>
            app.core.userApi.deviceList(
              { id, keyword },
              { defaultValue: [], showLoading: false }
            )
          }
          idValue={data.deviceId}
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
        app.core.userApi.auditHistory(
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
        { field: "title", header: labels.title },
        {
          width: DefaultUI.Widths.icon1,
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
            );
          }
        }
      ]}
      itemSize={[112, margin]}
      innerItemRenderer={(props) =>
        MobileListItemRenderer(props, (data) => {
          return [
            data.title,
            app.formatDate(data.creation, "ds"),
            <DialogButton
              content={JSON.stringify(data, undefined, 2)}
              contentPre
              disableScrollLock
              maxWidth="xs"
              size="small"
              icon={<InfoIcon />}
            >
              JSON data
            </DialogButton>,
            <React.Fragment>
              <Typography variant="caption">{data.kind}</Typography>
            </React.Fragment>
          ];
        })
      }
    />
  );
}
