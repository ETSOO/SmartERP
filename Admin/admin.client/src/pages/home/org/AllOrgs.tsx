import {
  MUGlobal,
  ResponsivePage,
  SearchField,
  IconButtonLink,
  MobileListItemRenderer,
  MUUtils,
  SelectBool
} from "@etsoo/materialui";
import ArticleIcon from "@mui/icons-material/Article";
import React from "react";
import { useNavigate } from "react-router-dom";
import {
  GridCellRendererProps,
  GridDataType,
  GridDeletedCellBoxStyle,
  ScrollerListForwardRef
} from "@etsoo/react";
import { app } from "../../../app/MyApp";
import { usePageDataEmpty } from "@etsoo/smarterp-core";
import { DataTypes, DateUtils } from "@etsoo/shared";
import { DefaultUI } from "@etsoo/smarterp-core/components";
import { AllOrgDto } from "../../../api/dto/query/AllOrgDto";
import { OrgTiplist } from "../../../components/OrgTiplist";
import { UserTiplist } from "../../../components/UserTiplist";
import { BoxProps } from "@mui/material/Box";
import Typography from "@mui/material/Typography";

const template = {
  keyword: "string",
  ownerId: "number",
  pin: "string",
  parentId: "number",
  enabled: "boolean",
  creationStart: "date",
  creationEnd: "date"
} as const satisfies DataTypes.BasicTemplate;

export default function AllOrgs() {
  // Route
  const navigate = useNavigate();

  // Labels
  const labels = app.getLabels(
    "actions",
    "apps",
    "brand",
    "companyNo",
    "createNewOrganization",
    "creation",
    "edit",
    "endDate",
    "id",
    "members",
    "orgName",
    "orgPin",
    "orgs",
    "owner",
    "parentOrg",
    "startDate",
    "statusNormal",
    "switchOrg",
    "view"
  );

  // Refs
  const ref = React.useRef<ScrollerListForwardRef<AllOrgDto>>(undefined);

  // Load data
  const reloadData = React.useCallback(() => ref.current?.reset(), []);

  const margin = MUGlobal.pagePaddings;
  const creationEndRef = React.useRef<HTMLInputElement>(null);

  // Page data hook
  usePageDataEmpty(app);

  return (
    <ResponsivePage<AllOrgDto, typeof template>
      {...DefaultUI.pageProps({
        onRefresh: reloadData
      })}
      mRef={ref}
      defaultOrderBy={[{ field: "creation", desc: true }]}
      quickAction={(data) => navigate(`./view/${data.id}`)}
      fieldTemplate={template}
      fields={(data) => [
        <SearchField
          label={labels.orgName}
          name="keyword"
          defaultValue={data.keyword}
          slotProps={{ htmlInput: { maxLength: 128 } }}
        />,
        <UserTiplist
          name="ownerId"
          label={labels.owner}
          idValue={data.ownerId}
        />,
        <OrgTiplist
          name="parentId"
          label={labels.parentOrg}
          idValue={data.parentId}
        />,
        <SearchField
          label={labels.companyNo}
          name="pin"
          minChars={2}
          defaultValue={data.pin}
        />,
        <SelectBool
          search
          name="enabled"
          label={labels.statusNormal}
          value={data.enabled}
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
      loadData={(data, lastItem) =>
        app.queryApi.allOrgs(MUUtils.setupPagingKeysets(data, lastItem, "id"), {
          defaultValue: [],
          showLoading: false
        })
      }
      columns={[
        {
          field: "id",
          width: 90,
          header: labels.id,
          sortable: false,
          type: GridDataType.Unkwown
        },
        {
          field: "name",
          header: labels.orgName,
          sortable: true,
          cellBoxStyle: GridDeletedCellBoxStyle
        },
        {
          field: "apps",
          width: 80,
          header: labels.apps,
          sortable: false,
          align: "right"
        },
        {
          field: "users",
          width: 80,
          header: labels.members,
          valueFormatter: ({ data }) =>
            data == null ? undefined : `${data.users} / ${data.persons}`,
          sortable: false,
          align: "right"
        },
        {
          field: "pin",
          width: 120,
          header: labels.orgPin,
          sortable: false
        },
        {
          field: "brand",
          width: 100,
          header: labels.brand,
          sortable: false
        },
        {
          field: "owner",
          width: 90,
          header: labels.owner,
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
          cellRenderer: ({
            data,
            cellProps
          }: GridCellRendererProps<AllOrgDto, BoxProps>) => {
            if (data == null) return undefined;

            cellProps.sx = {
              paddingTop: "6px!important",
              paddingBottom: "6px!important"
            };

            return (
              <React.Fragment>
                <IconButtonLink title={labels.view} href={`./view/${data.id}`}>
                  <ArticleIcon />
                </IconButtonLink>
              </React.Fragment>
            );
          }
        }
      ]}
      itemSize={[116, margin]}
      innerItemRenderer={(props) =>
        MobileListItemRenderer(props, (data) => {
          return [
            data.name,
            app.formatDate(data.creation, "d"),
            [
              {
                label: labels.view,
                icon: <ArticleIcon />,
                action: `./view/${data.id}`
              }
            ],
            <React.Fragment>
              {data.pin && (
                <Typography variant="body2" noWrap>
                  {data.pin}
                </Typography>
              )}
              {data.brand && (
                <Typography variant="body2" noWrap>
                  {labels.brand + ": " + data.brand}
                </Typography>
              )}
            </React.Fragment>
          ];
        })
      }
    />
  );
}
