import {
  ResponsivePage,
  SearchField,
  IconButtonLink,
  MobileListItemRenderer,
  ButtonLink
} from "@etsoo/materialui";
import AddIcon from "@mui/icons-material/Add";
import EditIcon from "@mui/icons-material/Edit";
import ArticleIcon from "@mui/icons-material/Article";
import React from "react";
import {
  GridCellRendererProps,
  GridDeletedCellBoxStyle,
  ScrollerListForwardRef
} from "@etsoo/react";
import { useNavigate } from "react-router-dom";
import { app } from "../../../app/MyApp";
import { usePageDataEmpty } from "@etsoo/smarterp-core";
import { FinanceAccountQueryData } from "@etsoo/smarterp-crm";
import { DataTypes } from "@etsoo/shared";
import { DefaultUI } from "@etsoo/smarterp-core/components";
import { BoxProps } from "@mui/material/Box";
import { Permissions } from "@etsoo/smarterp-crm";
import Fab from "@mui/material/Fab";

const template = {
  keyword: "string",
  leaderId: "number"
} as const satisfies DataTypes.BasicTemplate;

export default function AllAccounts() {
  // Route
  const navigate = useNavigate();

  // Labels
  const labels = app.getLabels(
    "accountNumber",
    "actions",
    "add",
    "bank",
    "bulkCreateAccounts",
    "currency",
    "edit",
    "keywords",
    "owner",
    "type",
    "view"
  );

  // Refs
  const ref =
    React.useRef<ScrollerListForwardRef<FinanceAccountQueryData>>(undefined);

  // Load data
  const reloadData = React.useCallback(() => ref.current?.reset(), []);

  // Page data hook
  usePageDataEmpty(app);

  return (
    <ResponsivePage<FinanceAccountQueryData, typeof template>
      {...DefaultUI.pageProps({
        onRefresh: reloadData,
        fabButtons: (
          <React.Fragment>
            {app.owns(Permissions.Finance.Add) && (
              <React.Fragment>
                <ButtonLink
                  href="./../bulkcreateaccounts"
                  size="small"
                  variant="outlined"
                >
                  {labels.bulkCreateAccounts}
                </ButtonLink>
                <Fab
                  title={labels.add}
                  size="medium"
                  color="primary"
                  onClick={() => navigate(`./../addaccount`)}
                >
                  <AddIcon />
                </Fab>
              </React.Fragment>
            )}
          </React.Fragment>
        )
      })}
      mRef={ref}
      defaultOrderBy={[{ field: "creation", desc: true }]}
      quickAction={(data) => navigate(`./../viewaccount/${data.id}`)}
      fieldTemplate={template}
      fields={(data) => [
        <SearchField
          label={labels.keywords}
          name="keyword"
          defaultValue={data.keyword}
        />
      ]}
      loadData={async (data) => {
        return await app.financeAccountApi.query(data, {
          defaultValue: [],
          showLoading: false
        });
      }}
      columns={[
        {
          field: "personName",
          header: labels.owner,
          width: 240
        },
        {
          field: "kind",
          header: labels.type,
          width: 110,
          valueFormatter: ({ data }) => app.finance.getAccountKind(data?.kind)
        },
        {
          field: "bank",
          header: labels.bank
        },
        {
          field: "currency",
          header: labels.currency,
          width: 88
        },
        {
          field: "accountNumber",
          header: labels.accountNumber,
          width: 180,
          cellBoxStyle: GridDeletedCellBoxStyle
        },
        {
          width: DefaultUI.Widths.icon2,
          header: labels.actions,
          cellBoxStyle: {
            paddingTop: "6px!important",
            paddingBottom: "6px!important"
          },
          cellRenderer: ({
            data
          }: GridCellRendererProps<FinanceAccountQueryData, BoxProps>) => {
            if (data == null) return undefined;

            return (
              <React.Fragment>
                {app.owns(Permissions.Finance.Edit) && (
                  <IconButtonLink
                    title={labels.edit}
                    href={`./../editaccount/${data.id}`}
                  >
                    <EditIcon />
                  </IconButtonLink>
                )}
                {app.owns(Permissions.Finance.View) && (
                  <IconButtonLink
                    title={labels.view}
                    href={`./../viewaccount/${data.id}`}
                  >
                    <ArticleIcon />
                  </IconButtonLink>
                )}
              </React.Fragment>
            );
          }
        }
      ]}
      rowHeight={164}
      itemRenderer={(props) =>
        MobileListItemRenderer(props, (data) => {
          return [
            `[${app.finance.getAccountKind(data?.kind)}] ${data.accountNumber}`,
            data.bank,
            [
              app.owns(Permissions.Finance.Edit) && {
                label: labels.edit,
                icon: <EditIcon />,
                action: `./../editaccount/${data.id}`
              },
              app.owns(Permissions.Finance.View) && {
                label: labels.view,
                icon: <ArticleIcon />,
                action: `./../viewaccount/${data.id}`
              }
            ],
            <React.Fragment></React.Fragment>
          ];
        })
      }
    />
  );
}
