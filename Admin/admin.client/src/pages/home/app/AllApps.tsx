import {
  MUGlobal,
  ResponsivePage,
  SearchField,
  SelectEx,
  MobileListItemRenderer,
  MUUtils,
  IconButtonLink
} from "@etsoo/materialui";
import { BoxProps, Typography } from "@mui/material";
import React from "react";
import { DataTypes, DateUtils } from "@etsoo/shared";
import {
  GridCellRendererProps,
  GridDataType,
  GridDeletedCellBoxStyle,
  ScrollerListForwardRef
} from "@etsoo/react";
import { app } from "../../../app/MyApp";
import { usePageDataEmpty } from "@etsoo/smarterp-core";
import { useNavigate } from "react-router-dom";
import { DefaultUI, IdentityType } from "@etsoo/smarterp-core/components";
import { AllAppDto } from "../../../api/dto/query/AllAppDto";
import ArticleIcon from "@mui/icons-material/Article";
import { OrgTiplist } from "../../../components/OrgTiplist";
import { AppTiplist } from "../../../components/AppTiplist";

const template = {
  orgId: "number",
  keyword: "string",
  identityType: "number",
  expiry: "date",
  expiryDays: "number",
  appId: "number",
  creationStart: "date",
  creationEnd: "date"
} as const satisfies DataTypes.BasicTemplate;

export default function AllApps() {
  // Route
  const navigate = useNavigate();

  // Labels
  const labels = app.getLabels(
    "actions",
    "app",
    "appName",
    "creation",
    "days",
    "endDate",
    "expiry",
    "identityType",
    "org",
    "startDate",
    "view"
  );

  // Refs
  const ref = React.useRef<ScrollerListForwardRef<AllAppDto>>();

  // Load data
  const reloadData = () => ref.current?.reset();

  const margin = MUGlobal.pagePaddings;
  const creationEndRef = React.useRef<HTMLInputElement>();

  // Page data hook
  usePageDataEmpty(app);

  return (
    <ResponsivePage<AllAppDto, typeof template>
      {...DefaultUI.createProps({
        onRefresh: reloadData
      })}
      mRef={ref}
      defaultOrderBy={[{ field: "creation", desc: true }]}
      quickAction={(data) => navigate(`./../view/${data.id}`)}
      fieldTemplate={template}
      fields={(data) => [
        <OrgTiplist idValue={data.orgId} />,
        <SearchField
          label={labels.appName}
          name="keyword"
          defaultValue={data.keyword}
          slotProps={{ htmlInput: { maxLength: 128 } }}
        />,
        <IdentityType value={data.identityType} search />,
        <SearchField
          label={labels.expiry}
          name="expiry"
          type="date"
          defaultValue={DateUtils.formatForInput(data.expiry)}
        />,
        <SelectEx
          label={labels.days}
          name="expiryDays"
          search
          options={[
            { id: "", label: "---" },
            { id: 3, label: "3" },
            { id: 7, label: "7" },
            { id: 15, label: "15" },
            { id: 30, label: "30" },
            { id: 60, label: "60" },
            { id: 90, label: "90" },
            { id: 180, label: "180" }
          ]}
          value={data.expiryDays}
        />,
        <AppTiplist idValue={data.appId} />,
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
      loadData={(data, lastItem) =>
        app.queryApi.allApps(MUUtils.setupPagingKeysets(data, lastItem, "id"), {
          defaultValue: [],
          showLoading: false
        })
      }
      columns={[
        {
          field: "identityType",
          header: labels.identityType,
          width: 120,
          valueFormatter: ({ data }) =>
            app.core.getIdentityLabel(data?.identityType),
          sortable: true
        },
        {
          field: "orgName",
          header: labels.org,
          sortable: false
        },
        {
          field: "name",
          header: labels.appName,
          sortable: false,
          cellBoxStyle: GridDeletedCellBoxStyle
        },
        {
          field: "expiry",
          type: GridDataType.Date,
          width: 116,
          header: labels.expiry,
          sortable: true,
          sortAsc: false,
          renderProps: { nearDays: 30 }
        },
        {
          field: "expiryDays",
          type: GridDataType.Int,
          header: labels.days,
          width: 72,
          sortable: false
        },
        {
          field: "creation",
          type: GridDataType.Date,
          width: 116,
          header: labels.creation,
          sortable: true,
          sortAsc: false
        },
        {
          width: DefaultUI.Widths.icon1,
          header: labels.actions,
          align: "center",
          cellRenderer: ({
            data,
            cellProps
          }: GridCellRendererProps<AllAppDto, BoxProps>) => {
            if (data == null) return undefined;

            cellProps.sx = {
              paddingTop: "10px!important",
              paddingBottom: "9px!important"
            };

            return (
              <React.Fragment>
                <IconButtonLink
                  title={labels.view}
                  href={`./../view/${data.id}`}
                >
                  <ArticleIcon />
                </IconButtonLink>
              </React.Fragment>
            );
          }
        }
      ]}
      itemSize={[134, margin]}
      innerItemRenderer={(props) =>
        MobileListItemRenderer(props, (data) => [
          data.name,
          data.orgName,
          [
            {
              label: labels.view,
              icon: <ArticleIcon />,
              action: `./../view/${data.id}`
            }
          ],
          <React.Fragment>
            <Typography variant="body2">
              {app.core.getIdentityLabel(data.identityType)}
            </Typography>
            <Typography variant="body2">
              {app.formatDate(data.expiry)}
              {data.expiryDays == null
                ? ""
                : ` (${app.formatNumber(data.expiryDays)} ${labels.days})`}
            </Typography>
          </React.Fragment>
        ])
      }
    />
  );
}
