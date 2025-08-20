import {
  MUGlobal,
  ResponsivePage,
  SearchField,
  IconButtonLink,
  MobileListItemRenderer,
  SelectBool
} from "@etsoo/materialui";
import ArticleIcon from "@mui/icons-material/Article";
import React from "react";
import {
  GridCellRendererProps,
  GridDataType,
  GridDeletedCellBoxStyle,
  ScrollerListForwardRef
} from "@etsoo/react";
import { useNavigate } from "react-router-dom";
import { app } from "../../../app/MyApp";
import { usePageDataEmpty } from "@etsoo/smarterp-core";
import { DataTypes, DateUtils } from "@etsoo/shared";
import { DefaultUI, StatusList } from "@etsoo/smarterp-core/components";
import { AllUserDto } from "../../../api/dto/query/AllUserDto";
import { BoxProps } from "@mui/material/Box";

const template = {
  keyword: "string",
  pin: "string",
  identifier: "string",
  isFrozen: "boolean",
  status: "number",
  creationStart: "date",
  creationEnd: "date"
} as const satisfies DataTypes.BasicTemplate;

export default function AllUsers() {
  // Route
  const navigate = useNavigate();

  // Labels
  const labels = app.getLabels(
    "actions",
    "creation",
    "endDate",
    "identifier",
    "isFrozen",
    "joinedOrgs",
    "name",
    "pin",
    "preferredName",
    "startDate",
    "status",
    "view"
  );

  // Refs
  const ref = React.useRef<ScrollerListForwardRef<AllUserDto>>(undefined);

  // Load data
  const reloadData = React.useCallback(() => ref.current?.reset(), []);

  const margin = MUGlobal.pagePaddings;
  const creationEndRef = React.useRef<HTMLInputElement>(null);

  // Page data hook
  usePageDataEmpty(app);

  return (
    <ResponsivePage<AllUserDto, typeof template>
      {...DefaultUI.pageProps({
        onRefresh: reloadData
      })}
      mRef={ref}
      defaultOrderBy={[{ field: "creation", desc: true }]}
      quickAction={(data) => navigate(`./view/${data.id}`)}
      fieldTemplate={template}
      fields={(data) => [
        <SearchField
          label={labels.name}
          name="keyword"
          defaultValue={data.keyword}
          slotProps={{
            input: { sx: { width: "120px", htmlInput: { maxLength: 128 } } }
          }}
        />,
        <SearchField
          label={labels.pin}
          name="pin"
          minChars={2}
          slotProps={{ htmlInput: { maxLength: 20 } }}
          defaultValue={data.pin}
        />,
        <SearchField
          label={labels.identifier}
          name="identifier"
          minChars={5}
          slotProps={{ htmlInput: { maxLength: 256 } }}
          defaultValue={data.identifier}
        />,
        <SelectBool
          search
          name="isFrozen"
          label={labels.isFrozen}
          value={data.isFrozen}
        />,
        <StatusList search idValue={data.status} />,
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
      loadData={async (data) => {
        return await app.queryApi.allUsers(data, {
          defaultValue: [],
          showLoading: false
        });
      }}
      columns={[
        {
          field: "name",
          header: labels.name,
          sortable: true,
          cellBoxStyle: GridDeletedCellBoxStyle
        },
        {
          field: "preferredName",
          header: labels.preferredName
        },
        {
          field: "pin",
          width: 120,
          header: labels.pin,
          sortable: false
        },
        {
          field: "orgs",
          width: 80,
          header: labels.joinedOrgs,
          sortable: false,
          align: "right"
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
          }: GridCellRendererProps<AllUserDto, BoxProps>) => {
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
            <React.Fragment></React.Fragment>
          ];
        })
      }
    />
  );
}
