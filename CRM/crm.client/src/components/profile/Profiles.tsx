import {
  ButtonLink,
  ComboBox,
  HBox,
  LinkEx,
  MobileListItemRenderer,
  MUGlobal,
  MUUtils,
  ResponsivePage,
  SearchField
} from "@etsoo/materialui";
import { DataTypes } from "@etsoo/shared";
import { DefaultUI } from "@etsoo/smarterp-core/components";
import {
  PersonProfileQueryData,
  PersonProfileQueryRQ
} from "@etsoo/smarterp-crm";
import React from "react";
import { app } from "../../app/MyApp";
import {
  GridCellRendererProps,
  GridDataType,
  ScrollerListForwardRef
} from "@etsoo/react";
import { BoxProps } from "@mui/material/Box";
import { EntityStatus } from "@etsoo/appscript";
import Typography from "@mui/material/Typography";
import { ImportanceText } from "@etsoo/smarterp-crm/components";
import AddIcon from "@mui/icons-material/Add";
import ArticleIcon from "@mui/icons-material/Article";
import Grid from "@mui/material/Grid";
import { ViewInnerProfile, ViewInnerRef } from "./ViewInnerProfile";
import { useNavigate } from "react-router-dom";

const template = {
  keyword: "string",
  kind: "number"
} as const satisfies DataTypes.BasicTemplate;

/**
 * Profiles component
 */
export type ProfilesProps = {
  /**
   * Person ID
   */
  personId: number;
};

/**
 * Profiles component
 * @param props Props
 * @returns Component
 */
export function Profiles(props: ProfilesProps) {
  // Destruct
  const { personId } = props;

  // Route
  const navigate = useNavigate();

  // Labels
  const labels = app.getLabels(
    "add",
    "clickToView",
    "creation",
    "more1",
    "profileTitle",
    "owner",
    "type",
    "view"
  );

  // Refs
  const ref = React.useRef<ScrollerListForwardRef<PersonProfileQueryData>>();
  const mRef = React.useRef<ViewInnerRef>(null);
  const personIdRef = React.useRef(personId);

  React.useEffect(() => {
    personIdRef.current = personId;
  }, [personId]);

  // Load data
  const reloadData = React.useCallback(() => ref.current?.reset(), []);

  const margin = MUGlobal.pagePaddings;

  // Layout
  return (
    <Grid container spacing={1}>
      <Grid size={{ xs: 12, sm: 12, md: 6, lg: 5, xl: 4 }}>
        <ResponsivePage<PersonProfileQueryData, typeof template>
          {...DefaultUI.pageProps({
            onRefresh: reloadData,
            fabRefresh: false
          })}
          {...(app.mdUp
            ? { onClick: (_event, data) => mRef.current?.setData(data) }
            : {
                quickAction: (data) =>
                  navigate(`./../../../profile/view/${data.id}`)
              })}
          mRef={ref}
          defaultOrderBy={[{ field: "creation", desc: true }]}
          fieldTemplate={template}
          fields={(data) => [
            <SearchField
              label={labels.profileTitle}
              name="keyword"
              minChars={2}
              defaultValue={data.keyword}
            />,
            <ComboBox
              options={app.profile.getKinds()}
              name="kind"
              label={labels.type}
              search
              idValue={data.kind}
            />
          ]}
          loadData={async (data, lastItem) => {
            const rq: PersonProfileQueryRQ = {
              ...MUUtils.setupPagingKeysets(data, lastItem, "id"),
              participantId: personIdRef.current
            };
            return await app.profileApi.query(rq, {
              defaultValue: [],
              showLoading: false
            });
          }}
          columns={[
            {
              field: "title",
              header: labels.profileTitle,
              cellRenderer: ({
                data
              }: GridCellRendererProps<PersonProfileQueryData, BoxProps>) => {
                if (data == null) return undefined;

                const statusFlag =
                  data.status === EntityStatus.Inactivated ||
                  data.status === EntityStatus.Deleted;

                const kind = app.profile.getKind(data.kind);

                return (
                  <React.Fragment>
                    {kind && (
                      <Typography component="span" variant="caption">
                        [{kind}]{" "}
                      </Typography>
                    )}
                    <Typography
                      component="span"
                      variant="body2"
                      sx={{
                        textDecoration: statusFlag ? "line-through" : undefined
                      }}
                    >
                      {data.title}
                    </Typography>
                    <ImportanceText
                      importance={data.importance}
                      variant="caption"
                      marginLeft={0.5}
                    />
                  </React.Fragment>
                );
              }
            },
            {
              field: "creation",
              type: GridDataType.Date,
              width: 116,
              header: labels.creation,
              sortable: true,
              sortAsc: false
            }
          ]}
          footerItemRenderer={(_rows, { index, states }) => {
            if (index === 0) {
              const { loadedItems, hasNextPage } = states;
              return (
                <HBox gap={1} alignItems="center">
                  <React.Fragment>
                    {loadedItems.toLocaleString() + (hasNextPage ? "+" : "")}
                  </React.Fragment>
                  <ButtonLink
                    href={`./../../../profile/add?personId=${personId}`}
                    variant="outlined"
                    startIcon={<AddIcon />}
                  >
                    {labels.add}
                  </ButtonLink>
                </HBox>
              );
            } else if (index === 1) {
              return (
                <LinkEx to={`./../../../profile?participantId=${personId}`}>
                  {labels.more1}
                </LinkEx>
              );
            }
          }}
          itemSize={[116, margin, "1px"]}
          innerItemRenderer={(props) =>
            MobileListItemRenderer(props, (data) => {
              return [
                data.title,
                app.formatDate(data.creation, "d"),
                [
                  {
                    label: labels.view,
                    icon: <ArticleIcon />,
                    action: `./../../../profile/view/${data.id}`
                  }
                ],
                <React.Fragment>
                  <HBox gap={1}>
                    <Typography component="span" variant="body2">
                      {app.profile.getKind(data.kind)}
                    </Typography>
                    <ImportanceText importance={data.importance} />
                    <Typography component="span" variant="body2">
                      {data.userName}
                    </Typography>
                  </HBox>
                </React.Fragment>
              ];
            })
          }
        />
      </Grid>
      {app.mdUp && (
        <Grid size={{ md: 6, lg: 7, xl: 8 }}>
          <ViewInnerProfile mRef={mRef} />
        </Grid>
      )}
    </Grid>
  );
}
