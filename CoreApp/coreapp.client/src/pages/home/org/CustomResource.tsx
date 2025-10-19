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
import {
  GridCellRendererProps,
  ScrollerListForwardRef,
  useParamsEx
} from "@etsoo/react";
import { app } from "../../../app/MyApp";
import { OrgQueryResourceData, usePageData } from "@etsoo/smarterp-core";
import { DataTypes } from "@etsoo/shared";
import { CultureList, DefaultUI } from "@etsoo/smarterp-core/components";
import Fab from "@mui/material/Fab";
import { BoxProps } from "@mui/material/Box";
import { BusinessUtils } from "@etsoo/appscript";

const template = {
  keyword: "string",
  culture: "string"
} as const satisfies DataTypes.BasicTemplate;

export default function CustomResource() {
  // Route
  const navigate = useNavigate();
  const { id = 0 } = useParamsEx({ id: "number" });

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

  usePageData(
    app,
    {
      breadcrumbs: (bc) => {
        bc.splice(bc.length - 1, 0, {
          title: labels.org,
          path: `./../my/${id}`
        });
        return bc;
      }
    },
    []
  );

  return (
    <ResponsivePage<OrgQueryResourceData, typeof template>
      {...DefaultUI.pageProps({
        onRefresh: reloadData,
        fabButtons: (
          <Fab
            title={labels.add}
            size="medium"
            color="primary"
            onClick={() => navigate(`./../../addcustomresource?orgId=${id}`)}
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
        <CultureList search autoAddBlankItem />
      ]}
      loadData={(data, lastItem) =>
        app.core.orgApi.queryResource(
          {
            ...BusinessUtils.setupPagingKeysets(data, lastItem, "id"),
            orgId: id
          },
          {
            defaultValue: [],
            showLoading: false
          }
        )
      }
      columns={[
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
          cellRenderer: ({
            data,
            cellProps
          }: GridCellRendererProps<OrgQueryResourceData, BoxProps>) => {
            if (data == null) return undefined;

            cellProps.sx = {
              paddingTop: "6px!important",
              paddingBottom: "6px!important"
            };

            return (
              <React.Fragment>
                <IconButtonLink
                  title={labels.edit}
                  href={`./../../editcustomresource/${data.id}?orgId=${id}`}
                >
                  <EditIcon />
                </IconButtonLink>
              </React.Fragment>
            );
          }
        }
      ]}
      itemRenderer={(props) =>
        MobileListItemRenderer(props, (data) => {
          return [
            data.title,
            data.key,
            [
              {
                label: labels.edit,
                icon: <EditIcon />,
                action: `./../../editcustomresource/${data.id}?orgId=${id}`
              }
            ],
            <React.Fragment>{data.description}</React.Fragment>
          ];
        })
      }
    />
  );
}
