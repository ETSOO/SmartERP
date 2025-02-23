import {
  MUGlobal,
  ResponsivePage,
  SearchField,
  IconButtonLink,
  MobileListItemRenderer,
  MUUtils,
  Switch
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
import { OrgQueryDto, usePageDataEmpty } from "@etsoo/smarterp-core";
import { DataTypes } from "@etsoo/shared";
import { AppUtils } from "../components/AppUtils";
import { DefaultUI, OrgTiplist } from "@etsoo/smarterp-core/components";
import { EntityStatus } from "@etsoo/appscript";

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
    "statusNormal",
    "switchOrg",
    "view"
  );

  // Refs
  const ref = React.useRef<ScrollerListForwardRef<OrgQueryDto>>();

  // Load data
  const reloadData = () => ref.current?.reset();

  const margin = MUGlobal.pagePaddings;

  // Page data hook
  usePageDataEmpty(app);

  return (
    <ResponsivePage<OrgQueryDto, typeof template>
      {...DefaultUI.createProps({
        onRefresh: reloadData,
        fabButtons: (
          <Fab
            title={labels.createNewOrganization}
            size="medium"
            color="primary"
            onClick={() =>
              navigate("./../../app/all", {
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
        />,
        <Switch
          label={labels.statusNormal}
          name="enabled"
          checked={data.enabled ?? true}
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
              data.isOwner && {
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
