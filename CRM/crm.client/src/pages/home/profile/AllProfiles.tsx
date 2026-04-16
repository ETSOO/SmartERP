import {
  ResponsivePage,
  IconButtonLink,
  MobileListItemRenderer,
  ComboBox,
  SelectBool,
  SearchField,
  DateText,
  HBox
} from "@etsoo/materialui";
import EditIcon from "@mui/icons-material/Edit";
import ArticleIcon from "@mui/icons-material/Article";
import AddIcon from "@mui/icons-material/Add";
import React from "react";
import {
  GridCellRendererProps,
  GridDataType,
  ScrollerListForwardRef
} from "@etsoo/react";
import { useNavigate } from "react-router-dom";
import { app } from "../../../app/MyApp";
import { PersonProfileQueryData } from "@etsoo/smarterp-crm";
import { DataTypes, DateUtils } from "@etsoo/shared";
import { DefaultUI, UserTiplist } from "@etsoo/smarterp-core/components";
import { BoxProps } from "@mui/material/Box";
import { ImportanceText } from "@etsoo/smarterp-crm/components";
import { BusinessUtils, EntityStatus } from "@etsoo/appscript";
import Fab from "@mui/material/Fab";
import Typography from "@mui/material/Typography";
import { usePageDataEmpty } from "@etsoo/smarterp-core";

const template = {
  importance: "number",
  keyword: "string",
  kind: "number",
  happenDateStart: "date",
  happenDateEnd: "date",
  isTask: "boolean",
  participantId: "number",
  userId: "number"
} as const satisfies DataTypes.BasicTemplate;

export default function AllProfiles() {
  // Route
  const navigate = useNavigate();

  // Labels
  const labels = app.getLabels(
    "actions",
    "add",
    "creation",
    "dateTo",
    "edit",
    "happenDate",
    "importance",
    "isTask",
    "owner",
    "participant",
    "profileTitle",
    "profiles",
    "type",
    "view"
  );

  // Refs
  const ref =
    React.useRef<ScrollerListForwardRef<PersonProfileQueryData>>(undefined);

  // Load data
  const reloadData = React.useCallback(() => ref.current?.reset(), []);

  // Page data hook
  usePageDataEmpty(app);

  return (
    <ResponsivePage<PersonProfileQueryData, typeof template>
      {...DefaultUI.pageProps({
        onRefresh: reloadData,
        fabButtons: (
          <React.Fragment>
            <Fab
              title={labels.add}
              size="medium"
              color="primary"
              onClick={() => navigate("./add")}
            >
              <AddIcon />
            </Fab>
          </React.Fragment>
        )
      })}
      mRef={ref}
      defaultOrderBy={[{ field: "creation", desc: true }]}
      quickAction={(data) => navigate(`./view/${data.id}`)}
      fieldTemplate={template}
      fields={(data) => [
        <SearchField
          label={labels.profileTitle}
          name="keyword"
          minChars={2}
          defaultValue={data.keyword}
        />,
        <ComboBox
          options={app.profile.getKinds()}
          name="kind"
          label={labels.type}
          search
          idValue={data.kind}
        />,
        <UserTiplist search idValue={data.userId} />,
        <ComboBox
          options={app.profile.getImportances()}
          name="importance"
          label={labels.importance}
          search
          idValue={data.importance}
        />,
        <SelectBool
          search
          name="isTask"
          label={labels.isTask}
          value={data.isTask}
        />,
        <UserTiplist
          name="participantId"
          label={labels.participant}
          search
          idValue={data.participantId}
        />,
        <SearchField
          label={labels.happenDate}
          name="happenDateStart"
          type="date"
          defaultValue={DateUtils.formatForInput(data.happenDateStart)}
        />,
        <SearchField
          label={labels.dateTo}
          name="happenDateEnd"
          type="date"
          defaultValue={DateUtils.formatForInput(data.happenDateEnd)}
        />
      ]}
      loadData={async (data, lastItem) => {
        return await app.profileApi.query(
          BusinessUtils.setupPagingKeysets(data, lastItem, "id"),
          {
            defaultValue: [],
            showLoading: false
          }
        );
      }}
      columns={[
        {
          field: "kind",
          header: labels.type,
          valueFormatter: ({ data }) => app.profile.getKind(data?.kind),
          width: 90
        },
        {
          field: "title",
          header: labels.profileTitle,
          cellRenderer: ({
            data
          }: GridCellRendererProps<PersonProfileQueryData, BoxProps>) => {
            if (data == null) return undefined;

            const statusFlag =
              data.status === EntityStatus.Inactivated ||
              data.status === EntityStatus.Deleted;

            return (
              <React.Fragment>
                <Typography
                  component="span"
                  variant="body2"
                  sx={{
                    textDecoration: statusFlag ? "line-through" : undefined
                  }}
                >
                  {data.title}
                </Typography>
                <ImportanceText
                  importance={data.importance}
                  variant="caption"
                  sx={{ marginLeft: 0.5 }}
                />
              </React.Fragment>
            );
          }
        },
        {
          field: "userName",
          header: labels.owner,
          width: 120
        },
        {
          field: "happenDate",
          header: labels.happenDate,
          width: 116,
          cellRenderer: ({
            data
          }: GridCellRendererProps<PersonProfileQueryData, BoxProps>) => {
            if (data == null) return undefined;

            const happenDate = app.formatDate(data.happenDate, "d");
            const creation = app.formatDate(data.creation, "d");
            if (happenDate === creation && data.happenDateEnd == null)
              return undefined;

            const title = `${labels.dateTo} ${app.formatDate(
              data.happenDateEnd,
              "d"
            )}`;

            return (
              <Typography component="span" variant="body2" title={title}>
                {happenDate}
              </Typography>
            );
          }
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
          width: DefaultUI.Widths.icon2,
          header: labels.actions,
          cellBoxStyle: {
            paddingTop: "6px!important",
            paddingBottom: "6px!important"
          },
          cellRenderer: ({
            data
          }: GridCellRendererProps<PersonProfileQueryData, BoxProps>) => {
            if (data == null) return undefined;

            return (
              <React.Fragment>
                <IconButtonLink title={labels.edit} href={`./edit/${data.id}`}>
                  <EditIcon />
                </IconButtonLink>
                <IconButtonLink title={labels.view} href={`./view/${data.id}`}>
                  <ArticleIcon />
                </IconButtonLink>
              </React.Fragment>
            );
          }
        }
      ]}
      rowHeight={164}
      itemRenderer={(props) =>
        MobileListItemRenderer(props, (data) => {
          return [
            data.title,
            app.formatDate(data.creation, "d"),
            [
              {
                label: labels.edit,
                icon: <EditIcon />,
                action: `./edit/${data.id}`
              },
              {
                label: labels.view,
                icon: <ArticleIcon />,
                action: `./view/${data.id}`
              }
            ],
            <React.Fragment>
              <HBox spacing={1}>
                <Typography component="span" variant="body2">
                  {app.profile.getKind(data.kind)}
                </Typography>
                <ImportanceText importance={data.importance} />
                <Typography component="span" variant="body2">
                  {data.userName}
                </Typography>
              </HBox>
              <Typography variant="body2">
                <DateText value={data.happenDate} />
                <span> - </span>
                {data.happenDateEnd ? (
                  <DateText value={data.happenDateEnd} />
                ) : (
                  "n/a"
                )}
              </Typography>
            </React.Fragment>
          ];
        })
      }
    />
  );
}
