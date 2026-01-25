import {
  ResponsivePage,
  SearchField,
  IconButtonLink,
  MobileListItemRenderer,
  ButtonLink
} from "@etsoo/materialui";
import AddIcon from "@mui/icons-material/Add";
import CategoryIcon from "@mui/icons-material/Category";
import EditIcon from "@mui/icons-material/Edit";
import ArticleIcon from "@mui/icons-material/Article";
import CardGiftcardIcon from "@mui/icons-material/CardGiftcard";
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
import { CustomerQueryData } from "@etsoo/smarterp-crm";
import { DataTypes } from "@etsoo/shared";
import { DefaultUI, StatusList } from "@etsoo/smarterp-core/components";
import { BoxProps } from "@mui/material/Box";
import Fab from "@mui/material/Fab";
import { Permissions } from "@etsoo/smarterp-crm";
import { IdentityTypeFlags } from "@etsoo/appscript";
import { PersonCategoryTiplist } from "@etsoo/smarterp-crm/components";

const template = {
  keyword: "string",
  info: "string",
  city: "string",
  categoryId: "number",
  status: "number"
} as const satisfies DataTypes.BasicTemplate;

export default function AllCustomers() {
  // Route
  const navigate = useNavigate();

  // Labels
  const labels = app.getLabels(
    "actions",
    "add",
    "assets",
    "assignedId",
    "category",
    "categories",
    "city",
    "confirmAction",
    "contactInfo",
    "creation",
    "description",
    "edit",
    "keywords",
    "personName",
    "view"
  );

  // Refs
  const ref =
    React.useRef<ScrollerListForwardRef<CustomerQueryData>>(undefined);

  // Load data
  const reloadData = React.useCallback(() => ref.current?.reset(), []);

  // Page data hook
  usePageDataEmpty(app);

  return (
    <ResponsivePage<CustomerQueryData, typeof template>
      {...DefaultUI.pageProps({
        onRefresh: reloadData,
        fabButtons: (
          <React.Fragment>
            {app.owns(Permissions.Org.Manage) && (
              <React.Fragment>
                <ButtonLink
                  href="./asset"
                  size="small"
                  variant="outlined"
                  startIcon={<CardGiftcardIcon />}
                >
                  {labels.assets}
                </ButtonLink>
                <ButtonLink
                  href={`./../contact/category?identityType=${IdentityTypeFlags.Customer}`}
                  size="small"
                  variant="outlined"
                  startIcon={<CategoryIcon />}
                >
                  {labels.categories}
                </ButtonLink>
              </React.Fragment>
            )}
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
      quickAction={(data) => navigate(`./../contact/view/${data.id}`)}
      fieldTemplate={template}
      fields={(data) => [
        <SearchField
          label={labels.keywords}
          name="keyword"
          defaultValue={data.keyword}
        />,
        <SearchField
          label={labels.contactInfo}
          name="info"
          minChars={2}
          defaultValue={data.info}
        />,
        <SearchField
          label={labels.city}
          name="city"
          minChars={2}
          defaultValue={data.city}
        />,
        <PersonCategoryTiplist
          label={labels.category}
          name="categoryId"
          onLoadData={(rq) =>
            Object.assign(rq, { identityType: IdentityTypeFlags.Customer })
          }
          search
        />,
        <StatusList search idValue={data.status} />
      ]}
      loadData={(data) =>
        app.customerApi.query(data, {
          defaultValue: [],
          showLoading: false
        })
      }
      columns={[
        {
          field: "name",
          header: labels.personName,
          sortable: true,
          width: 240,
          cellBoxStyle: GridDeletedCellBoxStyle
        },
        {
          header: labels.categories,
          width: 160,
          valueFormatter: ({ data }) =>
            data?.categories?.map((c) => c.names.join(" -> ")).join(", ")
        },
        {
          field: "description",
          header: labels.description
        },
        {
          field: "assignedId",
          width: 142,
          header: labels.assignedId
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
          }: GridCellRendererProps<CustomerQueryData, BoxProps>) => {
            if (data == null) return undefined;

            cellProps.sx = {
              paddingTop: "6px!important",
              paddingBottom: "6px!important"
            };

            return (
              <React.Fragment>
                <IconButtonLink title={labels.edit} href={`./edit/${data.id}`}>
                  <EditIcon />
                </IconButtonLink>
                <IconButtonLink
                  title={labels.view}
                  href={`./../contact/view/${data.id}`}
                >
                  <ArticleIcon />
                </IconButtonLink>
              </React.Fragment>
            );
          }
        }
      ]}
      itemRenderer={(props) =>
        MobileListItemRenderer(props, (data) => {
          return [
            data.name,
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
            <React.Fragment></React.Fragment>
          ];
        })
      }
    />
  );
}
