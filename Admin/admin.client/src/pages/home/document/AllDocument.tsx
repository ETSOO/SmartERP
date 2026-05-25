import {
  ResponsivePage,
  SearchField,
  MobileListItemRenderer,
  IconButtonLink,
  SelectBool
} from "@etsoo/materialui";
import AddIcon from "@mui/icons-material/Add";
import EditIcon from "@mui/icons-material/Edit";
import React from "react";
import { useNavigate } from "react-router-dom";
import {
  GridCellRendererProps,
  GridDataType,
  ScrollerListForwardRef
} from "@etsoo/react";
import { app } from "../../../app/MyApp";
import { usePageDataEmpty } from "@etsoo/smarterp-core";
import { DataTypes } from "@etsoo/shared";
import { DefaultUI } from "@etsoo/smarterp-core/components";
import Fab from "@mui/material/Fab";
import { BoxProps } from "@mui/material/Box";
import { OrgTiplist } from "../../../components/OrgTiplist";
import { BusinessUtils } from "@etsoo/appscript";
import { DocumentQueryData } from "../../../api/dto/document/DocumentQueryData";
import { Typography } from "@mui/material";

const template = {
  keyword: "string",
  kind: "string",
  systemTemplate: "boolean",
  orgId: "number"
} as const satisfies DataTypes.BasicTemplate;

export default function AllDocument() {
  // Route
  const navigate = useNavigate();

  // Labels
  const labels = app.getLabels(
    "actions",
    "add",
    "edit",
    "keywords",
    "org",
    "parameters",
    "refreshTime",
    "systemTemplate",
    "title",
    "type"
  );

  // Refs
  const ref =
    React.useRef<ScrollerListForwardRef<DocumentQueryData>>(undefined);

  // Load data
  const reloadData = React.useCallback(() => ref.current?.reset(), []);

  // Page data hook
  usePageDataEmpty(app);

  return (
    <ResponsivePage<DocumentQueryData, typeof template>
      {...DefaultUI.pageProps({
        onRefresh: reloadData,
        fabButtons: (
          <Fab
            title={labels.add}
            size="medium"
            color="primary"
            onClick={() => navigate("./add")}
          >
            <AddIcon />
          </Fab>
        )
      })}
      mRef={ref}
      fieldTemplate={template}
      fields={(data) => [
        <SearchField
          label={`${labels.keywords}`}
          name="keyword"
          minChars={2}
          defaultValue={data.keyword}
        />,
        <SearchField
          label={labels.type}
          name="kind"
          defaultValue={data.kind}
        />,
        <SelectBool label={labels.systemTemplate} name="systemTemplate" />,
        <OrgTiplist name="orgId" label={labels.org} idValue={data.orgId} />
      ]}
      loadData={(data, lastItem) =>
        app.documentApi.query(
          BusinessUtils.setupPagingKeysets(data, lastItem, "id"),
          {
            defaultValue: [],
            showLoading: false
          }
        )
      }
      columns={[
        {
          field: "refreshTime",
          type: GridDataType.Date,
          width: 104,
          header: labels.refreshTime,
          renderProps: app.getDateFormatProps()
        },
        {
          field: "kind",
          header: labels.type,
          width: 150
        },
        {
          field: "title",
          header: labels.title
        },
        {
          field: "hasParameters",
          header: labels.parameters,
          width: 80
        },
        {
          field: "orgName",
          header: labels.org,
          width: 250
        },
        {
          width: DefaultUI.Widths.icon1,
          header: labels.actions,
          cellBoxStyle: {
            paddingTop: "6px!important",
            paddingBottom: "6px!important"
          },
          cellRenderer: ({
            data
          }: GridCellRendererProps<DocumentQueryData, BoxProps>) => {
            if (data == null) return undefined;

            return (
              <React.Fragment>
                <IconButtonLink title={labels.edit} href={`./edit/${data.id}`}>
                  <EditIcon />
                </IconButtonLink>
              </React.Fragment>
            );
          }
        }
      ]}
      rowHeight={160}
      itemRenderer={(props) =>
        MobileListItemRenderer(props, (data) => {
          return [
            `${data.title}`,
            app.formatDate(data.refreshTime),
            [
              {
                label: labels.edit,
                icon: <EditIcon />,
                action: `./edit/${data.id}`
              }
            ],
            <React.Fragment>
              {data.orgName && (
                <Typography variant="body2">{data.orgName}</Typography>
              )}
              <Typography variant="body2">{data.kind}</Typography>
            </React.Fragment>
          ];
        })
      }
    />
  );
}
