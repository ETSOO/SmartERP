import {
  ResponsivePage,
  SearchField,
  MobileListItemRenderer,
  HBox,
  TooltipClick,
  SelectBool
} from "@etsoo/materialui";
import EditIcon from "@mui/icons-material/Edit";
import ArticleIcon from "@mui/icons-material/Article";
import AddIcon from "@mui/icons-material/Add";
import StarIcon from "@mui/icons-material/Star";
import CheckCircleIcon from "@mui/icons-material/CheckCircle";
import React from "react";
import {
  GridCellRendererProps,
  GridDataType,
  NotificationMessageType,
  ScrollerListForwardRef
} from "@etsoo/react";
import { useNavigate } from "react-router-dom";
import { DataTypes } from "@etsoo/shared";
import { DefaultUI } from "@etsoo/smarterp-core/components";
import { BoxProps } from "@mui/material/Box";
import Fab from "@mui/material/Fab";
import { PersonInfoQueryData } from "@etsoo/smarterp-crm";
import { app } from "../../app/MyApp";
import { InfoKindList } from "@etsoo/smarterp-crm/components";
import Typography from "@mui/material/Typography";
import Button from "@mui/material/Button";
import IconButton from "@mui/material/IconButton";
import { useEditContactInfo } from "./useEditContactInfo";

const template = {
  identifier: "string",
  keyword: "string",
  kind: "number",
  subscribed: "boolean"
} as const satisfies DataTypes.BasicTemplate;

export type ContactInfosProps = {
  /**
   * Person editable
   */
  editable: boolean;

  /**
   * Tab index
   */
  index: number;

  /**
   * Person ID
   */
  personId: number;
};

export function ContactInfos(props: ContactInfosProps) {
  // Destruct
  const { editable, index, personId } = props;

  // Route
  const navigate = useNavigate();

  // Labels
  const labels = app.getLabels(
    "actions",
    "add",
    "completeTip",
    "copy",
    "creation",
    "description",
    "edit",
    "identifier",
    "keywords",
    "subscribed",
    "type",
    "view"
  );

  // Refs
  const ref =
    React.useRef<ScrollerListForwardRef<PersonInfoQueryData>>(undefined);

  // View data
  const viewData = React.useCallback(async (id: number) => {
    const data = await app.personApi.readInfo(id);
    if (data == null) return;

    const notifier = app.notifier.alert(
      <React.Fragment>
        <Typography component="span" paddingRight={1}>
          {data}
        </Typography>
        <TooltipClick title={labels.completeTip.format(labels.copy)}>
          {() => (
            <Button
              variant="outlined"
              size="small"
              onClick={() => {
                navigator.clipboard?.writeText(data);
                notifier.dismiss();
              }}
            >
              {labels.copy}
            </Button>
          )}
        </TooltipClick>
      </React.Fragment>,
      undefined,
      NotificationMessageType.Success
    );
    notifier.dismiss(180);
  }, []);

  // Load data
  const reloadData = React.useCallback(() => ref.current?.reset(), []);

  // Edit contact
  const editContact = useEditContactInfo(personId, reloadData);

  return (
    <ResponsivePage<PersonInfoQueryData, typeof template>
      {...DefaultUI.pageProps({
        onRefresh: reloadData,
        fabButtons: (
          <React.Fragment>
            {editable && (
              <Fab
                title={labels.add}
                size="medium"
                color="primary"
                onClick={() =>
                  navigate(`./../../info/${personId}?index=${index}`)
                }
              >
                <AddIcon />
              </Fab>
            )}
          </React.Fragment>
        )
      })}
      mRef={ref}
      defaultOrderBy={[{ field: "creation", desc: true }]}
      fieldTemplate={template}
      fields={(data) => [
        <SearchField
          label={labels.identifier}
          name="identifier"
          minChars={2}
          defaultValue={data.identifier}
        />,
        <SearchField
          label={labels.description}
          name="keyword"
          minChars={2}
          defaultValue={data.keyword}
        />,
        <InfoKindList search value={data.kind} />,
        <SelectBool
          name="subscribed"
          label={labels.subscribed}
          value={data.subscribed}
        />
      ]}
      loadData={async (data) => {
        return await app.personApi.queryInfo(
          { personId, ...data },
          {
            defaultValue: [],
            showLoading: false
          }
        );
      }}
      columns={[
        {
          field: "kind",
          width: 120,
          header: labels.type,
          valueFormatter: ({ data }) => app.person.getInfoKind(data?.kind)
        },
        {
          field: "identifier",
          header: labels.identifier,
          cellRenderer: ({
            data
          }: GridCellRendererProps<PersonInfoQueryData, BoxProps>) => {
            if (data == null) return undefined;

            return (
              <HBox alignItems="center" gap={0.5}>
                <Typography component="span" variant="body2">
                  {data.identifier}
                </Typography>
                {data.isDefault && <StarIcon fontSize="small" />}
                {data.isVerified && <CheckCircleIcon fontSize="small" />}
              </HBox>
            );
          }
        },
        {
          field: "description",
          header: labels.description
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
          cellRenderer: ({
            data,
            cellProps
          }: GridCellRendererProps<PersonInfoQueryData, BoxProps>) => {
            if (data == null) return undefined;

            cellProps.sx = {
              paddingTop: "6px!important",
              paddingBottom: "6px!important"
            };

            return (
              <React.Fragment>
                {editable && (
                  <IconButton
                    title={labels.edit}
                    onClick={() => editContact(data)}
                  >
                    <EditIcon />
                  </IconButton>
                )}
                <IconButton
                  title={labels.view}
                  onClick={() => viewData(data.id)}
                >
                  <ArticleIcon />
                </IconButton>
              </React.Fragment>
            );
          }
        }
      ]}
      itemRenderer={(props) =>
        MobileListItemRenderer(props, (data) => {
          return [
            `[${app.person.getInfoKind(data.kind)}] ${data.identifier}`,
            app.formatDate(data.creation, "d"),
            [
              editable && {
                label: labels.edit,
                icon: <EditIcon />,
                action: () => editContact(data)
              },
              {
                label: labels.view,
                icon: <ArticleIcon />,
                action: () => viewData(data.id)
              }
            ],
            <React.Fragment>{data.description}</React.Fragment>
          ];
        })
      }
    />
  );
}
