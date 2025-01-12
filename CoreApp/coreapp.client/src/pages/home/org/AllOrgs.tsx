import {
  MUGlobal,
  ResponsivePage,
  SearchField,
  IconButtonLink,
  MobileListItemRenderer,
  MUUtils
} from "@etsoo/materialui";
import { BoxProps, Fab, IconButton, Typography } from "@mui/material";
import AddIcon from "@mui/icons-material/Add";
import AccountTreeIcon from "@mui/icons-material/AccountTree";
import EditIcon from "@mui/icons-material/Edit";
import ArticleIcon from "@mui/icons-material/Article";
import React from "react";
import { useNavigate } from "react-router-dom";
import {
  GridCellRendererProps,
  GridDataType,
  ScrollerListForwardRef
} from "@etsoo/react";
import { app } from "../../../app/MyApp";
import { OrgQueryDto } from "@etsoo/smarterp-core";
import { DataTypes } from "@etsoo/shared";
import { AppUtils } from "../app/components/AppUtils";
import { OrgTiplist } from "@etsoo/smarterp-core/components";

const template = {
  keyword: "string",
  pin: "string",
  parentId: "number"
} as const satisfies DataTypes.BasicTemplate;

export default function AllOrgs() {
  // Route
  const navigate = useNavigate();

  // Permissions
  const editPermission = app.isAdminUser();

  // Labels
  const labels = app.getLabels(
    "actions",
    "brand",
    "companyNo",
    "createNewOrganization",
    "creation",
    "edit",
    "id",
    "orgName",
    "orgPin",
    "orgs",
    "parentOrg",
    "switchOrg",
    "view"
  );

  // Refs
  const ref = React.useRef<ScrollerListForwardRef<OrgQueryDto>>();

  // Load data
  const reloadData = () => ref.current?.reset();

  const margin = MUGlobal.pagePaddings;

  React.useEffect(() => {
    // Page title
    app.setPageKey("orgs");
  }, []);

  return (
    <ResponsivePage<OrgQueryDto, typeof template>
      adjustHeight={24}
      mRef={ref}
      defaultOrderBy={[{ field: "creation", desc: true }]}
      pageProps={{
        onRefresh: reloadData,
        paddings: 0,
        fabButtons: (
          <Fab
            title={labels.createNewOrganization}
            size="medium"
            color="primary"
            onClick={() =>
              navigate("./../../service/all", {
                state: { kind: 2 }
              })
            }
          >
            <AddIcon />
          </Fab>
        )
      }}
      quickAction={(data) => navigate(`./${data.id}`)}
      fieldTemplate={template}
      fields={(data) => [
        <SearchField
          label={labels.orgName}
          name="keyword"
          defaultValue={data.keyword}
        />,
        <OrgTiplist
          label={labels.parentOrg}
          name="parentId"
          search
          idValue={data.parentId}
        />,
        <SearchField
          label={labels.companyNo}
          name="pin"
          minChars={3}
          defaultValue={data.pin}
        />
      ]}
      loadData={(data, lastItem) =>
        app.core.orgApi.query(
          MUUtils.setupPagingKeysets(data, lastItem, "id"),
          {
            defaultValue: [],
            showLoading: false
          }
        )
      }
      columns={[
        {
          field: "name",
          header: labels.orgName,
          sortable: true
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
          field: "creation",
          type: GridDataType.Date,
          width: 116,
          header: labels.creation,
          sortable: true,
          sortAsc: false
        },
        {
          width: 192,
          header: labels.actions,
          cellRenderer: ({
            data,
            cellProps
          }: GridCellRendererProps<OrgQueryDto, BoxProps>) => {
            if (data == null) return undefined;

            cellProps.sx = {
              paddingTop: "6px!important",
              paddingBottom: "6px!important"
            };

            return (
              <React.Fragment>
                {editPermission && (
                  <IconButtonLink
                    title={labels.edit}
                    href={`./../edit/${data.id}`}
                  >
                    <EditIcon />
                  </IconButtonLink>
                )}
                {data.id !== app.userData?.organization && (
                  <IconButton
                    title={labels.switchOrg}
                    onClick={() => AppUtils.switchOrg(data)}
                  >
                    <AccountTreeIcon />
                  </IconButton>
                )}
                <IconButtonLink title={labels.view} href={`./${data.id}`}>
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
              editPermission && {
                label: labels.edit,
                icon: <EditIcon />,
                action: `./../edit/${data.id}`
              },
              data.id !== app.userData?.organization && {
                label: labels.switchOrg,
                icon: <AccountTreeIcon />,
                action: () => AppUtils.switchOrg(data)
              },
              {
                label: labels.view,
                icon: <ArticleIcon />,
                action: `./${data.id}`
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
