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
import { OrgQueryResourceData, usePageDataEmpty } from "@etsoo/smarterp-core";
import { DataTypes } from "@etsoo/shared";
import { CultureList, DefaultUI } from "@etsoo/smarterp-core/components";
import Fab from "@mui/material/Fab";
import { BoxProps } from "@mui/material/Box";
import { OrgTiplist } from "../../../components/OrgTiplist";
import { BusinessUtils } from "@etsoo/appscript";
import Typography from "@mui/material/Typography";

const template = {
  keyword: "string",
  culture: "string",
  orgId: "number"
} as const satisfies DataTypes.BasicTemplate;

export default function CustomResource() {
  // Route
  const navigate = useNavigate();

  // Labels
  const labels = app.getLabels(
    "actions",
    "add",
    "culture",
    "description",
    "edit",
    "key",
    "org",
    "title"
  );

  // Refs
  const ref =
    React.useRef<ScrollerListForwardRef<OrgQueryResourceData>>(undefined);

  // Load data
  const reloadData = React.useCallback(() => ref.current?.reset(), []);

  const isAdmin = app.isAdminUser();

  // Page data hook
  usePageDataEmpty(app);

  return (
    <ResponsivePage<OrgQueryResourceData, typeof template>
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
      defaultOrderBy={[{ field: "key" }]}
      fieldTemplate={template}
      fields={(data) => [
        <SearchField
          label={`${labels.key} / ${labels.title}`}
          name="keyword"
          minChars={2}
          defaultValue={data.keyword}
        />,
        <CultureList search autoAddBlankItem />,
        <OrgTiplist name="orgId" label={labels.org} idValue={data.orgId} />
      ]}
      loadData={(data, lastItem) =>
        app.core.orgApi.queryResource(
          BusinessUtils.setupPagingKeysets(data, lastItem, "id"),
          {
            defaultValue: [],
            showLoading: false
          }
        )
      }
      columns={[
        {
          field: "orgName",
          header: labels.org,
          width: 232
        },
        {
          field: "key",
          header: labels.key,
          width: 150
        },
        {
          field: "culture",
          header: labels.culture,
          width: 90
        },
        {
          field: "title",
          header: labels.title,
          width: 200
        },
        {
          field: "description",
          header: labels.description
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
          }: GridCellRendererProps<OrgQueryResourceData, BoxProps>) => {
            if (data == null) return undefined;

            return (
              <React.Fragment>
                {isAdmin && (
                  <IconButtonLink
                    title={labels.edit}
                    href={`./edit/${data.id}`}
                  >
                    <EditIcon />
                  </IconButtonLink>
                )}
              </React.Fragment>
            );
          }
        }
      ]}
      rowHeight={160}
      itemRenderer={(props) =>
        MobileListItemRenderer(props, (data) => {
          return [
            `${data.title} (${data.culture})`,
            data.key,
            [
              isAdmin && {
                label: labels.edit,
                icon: <EditIcon />,
                action: `./edit/${data.id}`
              }
            ],
            <React.Fragment>
              {data.orgName && (
                <Typography variant="body2" noWrap>
                  {data.orgName}
                </Typography>
              )}
              {data.description && (
                <Typography variant="body2" noWrap>
                  {data.description}
                </Typography>
              )}
            </React.Fragment>
          ];
        })
      }
    />
  );
}
