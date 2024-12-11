import {
  MUGlobal,
  Tiplist,
  ResponsivePage,
  SearchField,
  IconButtonLink,
  MobileListItemRenderer
} from "@etsoo/materialui";
import { BoxProps, Fab, IconButton, Typography } from "@mui/material";
import AddIcon from "@mui/icons-material/Add";
import DeckIcon from "@mui/icons-material/Deck";
import EditIcon from "@mui/icons-material/Edit";
import PersonAddIcon from "@mui/icons-material/PersonAdd";
import PageviewIcon from "@mui/icons-material/Pageview";
import React from "react";
import { useNavigate } from "react-router-dom";
import {
  GridCellRendererProps,
  GridDataType,
  ScrollerListForwardRef
} from "@etsoo/react";
import { app } from "../../../app/MyApp";
import { OrgListDto, OrgQueryDto } from "@etsoo/smarterp-core";
import { DataTypes } from "@etsoo/shared";

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

  const invitePermission = app.isHRUser();

  // Labels
  const labels = app.getLabels(
    "companyNo",
    "createNewOrganization",
    "organizations",
    "orgName",
    "parentOrg",
    "edit",
    "inviteMember",
    "switchOrganization",
    "id",
    "memberCount",
    "brand",
    "creation",
    "actions",
    "confirmAction",
    "view"
  );

  // Refs
  const ref = React.useRef<ScrollerListForwardRef<OrgQueryDto>>();

  // Load data
  const reloadData = () => ref.current?.reset();

  const margin = MUGlobal.pagePaddings;

  React.useEffect(() => {
    // Page title
    app.setPageKey("organizations");
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
      quickAction={(data) => navigate(`./../view/${data.id}`)}
      fieldTemplate={template}
      fields={(data) => [
        <SearchField
          label={labels.orgName}
          name="keyword"
          defaultValue={data.keyword}
        />,
        <Tiplist<OrgListDto>
          label={labels.parentOrg}
          name="parentId"
          search
          idValue={data.parentId}
          loadData={(keyword, id, maxItems) =>
            app.core.orgApi.list(
              { id, keyword, queryPaging: { batchSize: maxItems } },
              {
                defaultValue: [],
                showLoading: false
              }
            )
          }
        />,
        <SearchField
          label={labels.companyNo}
          name="pin"
          minChars={3}
          defaultValue={data.pin}
        />
      ]}
      loadData={(data) =>
        app.core.orgApi.query(data, {
          defaultValue: [],
          showLoading: false
        })
      }
      columns={[
        {
          field: "name",
          header: labels.orgName,
          sortable: true
        },
        {
          field: "brand",
          width: 120,
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
                {invitePermission && (
                  <IconButton title={labels.inviteMember}>
                    <PersonAddIcon />
                  </IconButton>
                )}
                {data.id !== app.userData?.organization && (
                  <IconButton
                    title={labels.switchOrganization}
                    onClick={() =>
                      app.notifier.confirm(
                        labels.confirmAction.format(labels.switchOrganization),
                        undefined,
                        (confirmed) => {
                          //if (confirmed) app.core.orgApi.switch(data.id);
                        }
                      )
                    }
                  >
                    <DeckIcon />
                  </IconButton>
                )}
                <IconButtonLink
                  title={labels.view}
                  href={`./../view/${data.id}`}
                >
                  <PageviewIcon />
                </IconButtonLink>
              </React.Fragment>
            );
          }
        }
      ]}
      itemSize={[100, margin]}
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
              invitePermission && {
                label: labels.inviteMember,
                icon: <PersonAddIcon />,
                action: () => {}
              },
              data.id !== app.userData?.organization && {
                label: labels.switchOrganization,
                icon: <DeckIcon />,
                action: () =>
                  app.notifier.confirm(
                    labels.confirmAction.format(labels.switchOrganization),
                    undefined,
                    (confirmed) => {
                      //if (confirmed) app.core.orgApi.switch(data.id);
                    }
                  )
              }
            ],
            <React.Fragment>
              {data.brand && (
                <Typography variant="caption" noWrap>
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
