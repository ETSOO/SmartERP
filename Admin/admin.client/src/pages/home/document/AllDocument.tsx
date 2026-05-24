import {
  ResponsivePage,
  SearchField,
  MobileListItemRenderer,
  IconButtonLink
} from "@etsoo/materialui";
import AddIcon from "@mui/icons-material/Add";
import EditIcon from "@mui/icons-material/Edit";
import React from "react";
import { useNavigate } from "react-router-dom";
import { GridCellRendererProps, ScrollerListForwardRef } from "@etsoo/react";
import { app } from "../../../app/MyApp";
import { usePageDataEmpty } from "@etsoo/smarterp-core";
import { DataTypes } from "@etsoo/shared";
import { DefaultUI } from "@etsoo/smarterp-core/components";
import Fab from "@mui/material/Fab";
import { BoxProps } from "@mui/material/Box";
import { OrgTiplist } from "../../../components/OrgTiplist";
import { BusinessUtils } from "@etsoo/appscript";
import { DocumentQueryData } from "../../../api/dto/document/DocumentQueryData";

const template = {
  keyword: "string",
  kind: "string",
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
    "refeshTime",
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
          field: "title",
          header: labels.title
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
            <React.Fragment></React.Fragment>
          ];
        })
      }
    />
  );
}
