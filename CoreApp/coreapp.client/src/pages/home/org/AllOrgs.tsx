import {
  ResponsivePage,
  SearchField,
  IconButtonLink,
  MobileListItemRenderer,
  Switch
} from "@etsoo/materialui";
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
import { OrgQueryDto, usePageDataEmpty } from "@etsoo/smarterp-core";
import { DataTypes } from "@etsoo/shared";
import { AppUtils } from "../components/AppUtils";
import { DefaultUI, OrgTiplist } from "@etsoo/smarterp-core/components";
import { BusinessUtils, EntityStatus } from "@etsoo/appscript";
import Fab from "@mui/material/Fab";
import IconButton from "@mui/material/IconButton";
import Typography from "@mui/material/Typography";
import { BoxProps } from "@mui/material/Box";

const template = {
  keyword: "string",
  pin: "string",
  parentId: "number",
  enabled: "boolean"
} as const satisfies DataTypes.BasicTemplate;

export default function AllOrgs() {
  // Route
  const navigate = useNavigate();

  // Labels
  const labels = app.getLabels(
    "actions",
    "brand",
    "companyNo",
    "createNewOrganization",
    "creation",
    "edit",
    "id",
    "members",
    "orgName",
    "orgPin",
    "orgs",
    "parentOrg",
    "role",
    "statusNormal",
    "switchOrg",
    "view"
  );

  // Refs
  const ref = React.useRef<ScrollerListForwardRef<OrgQueryDto>>(undefined);

  // Load data
  const reloadData = React.useCallback(() => ref.current?.reset(), []);

  // Page data hook
  usePageDataEmpty(app);

  return (
    <ResponsivePage<OrgQueryDto, typeof template>
      {...DefaultUI.pageProps({
        onRefresh: reloadData,
        fabButtons: (
          <Fab
            title={labels.createNewOrganization}
            size="medium"
            color="primary"
            onClick={() =>
              navigate("./../app", {
                state: { kind: 2 }
              })
            }
          >
            <AddIcon />
          </Fab>
        )
      })}
      mRef={ref}
      defaultOrderBy={[{ field: "creation", desc: true }]}
      quickAction={(data) => navigate(`./my/${data.id}`)}
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
        />,
        <Switch
          label={labels.statusNormal}
          name="enabled"
          checked={data.enabled ?? true}
        />
      ]}
      loadData={(data, lastItem) =>
        app.core.orgApi.query(
          BusinessUtils.setupPagingKeysets(data, lastItem, "id"),
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
          sortable: true,
          cellBoxStyle: (data) =>
            data
              ? {
                  textDecoration:
                    data.status > EntityStatus.Approved
                      ? "line-through"
                      : undefined,
                  color:
                    data.userStatus > EntityStatus.Approved ||
                    data.isUserExpired
                      ? "red"
                      : undefined
                }
              : {}
        },
        {
          field: "userRole",
          width: 120,
          header: labels.role,
          valueFormatter: ({ data }) => app.getRoleLabel(data?.userRole),
          sortable: false
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
          field: "users",
          width: 80,
          header: labels.members,
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
          width: DefaultUI.Widths.icon3,
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
                {data.isOwner && (
                  <IconButtonLink
                    title={labels.edit}
                    href={`./edit/${data.id}`}
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
                <IconButtonLink title={labels.view} href={`./my/${data.id}`}>
                  <ArticleIcon />
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
            data.name,
            app.formatDate(data.creation, "d"),
            [
              data.isOwner && {
                label: labels.edit,
                icon: <EditIcon />,
                action: `./edit/${data.id}`
              },
              data.id !== app.userData?.organization && {
                label: labels.switchOrg,
                icon: <AccountTreeIcon />,
                action: () => AppUtils.switchOrg(data)
              },
              {
                label: labels.view,
                icon: <ArticleIcon />,
                action: `./my/${data.id}`
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
