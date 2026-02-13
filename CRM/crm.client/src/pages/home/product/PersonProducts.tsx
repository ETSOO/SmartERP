import {
  ResponsivePage,
  SearchField,
  IconButtonLink,
  MobileListItemRenderer,
  VBox
} from "@etsoo/materialui";
import EditIcon from "@mui/icons-material/Edit";
import AddIcon from "@mui/icons-material/Add";
import React from "react";
import { GridCellRendererProps, ScrollerListForwardRef } from "@etsoo/react";
import { useNavigate } from "react-router-dom";
import { app } from "../../../app/MyApp";
import { usePageDataEmpty } from "@etsoo/smarterp-core";
import { PersonProductQueryData, ProductCustomData } from "@etsoo/smarterp-crm";
import { DataTypes } from "@etsoo/shared";
import { DefaultUI } from "@etsoo/smarterp-core/components";
import { BoxProps } from "@mui/material/Box";
import Fab from "@mui/material/Fab";
import { Typography } from "@mui/material";
import { PersonList, ProductList } from "@etsoo/smarterp-crm/components";
import { IdentityTypeFlags } from "@etsoo/appscript";

const template = {
  personId: "number",
  productId: "number",
  assignedId: "string"
} as const satisfies DataTypes.BasicTemplate;

function formatCultures(cultures?: ProductCustomData[]) {
  if (cultures == null || cultures.length == 0) return undefined;
  return (
    <VBox>
      {cultures.map((c) => (
        <Typography variant="caption" key={c.culture}>
          {c.name} ({c.culture})
        </Typography>
      ))}
    </VBox>
  );
}

export default function PersonProducts() {
  // Route
  const navigate = useNavigate();

  // Labels
  const labels = app.getLabels(
    "actions",
    "add",
    "assignedId",
    "customResources",
    "edit",
    "productName",
    "relatedTarget"
  );

  // Refs
  const ref =
    React.useRef<ScrollerListForwardRef<PersonProductQueryData>>(undefined);

  // Load data
  const reloadData = React.useCallback(() => ref.current?.reset(), []);

  // Page data hook
  usePageDataEmpty(app);

  return (
    <ResponsivePage<PersonProductQueryData, typeof template>
      {...DefaultUI.pageProps({
        onRefresh: reloadData,
        fabButtons: (
          <React.Fragment>
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
      fieldTemplate={template}
      fields={(data) => [
        <ProductList inputRequired idValue={data.productId} search />,
        <PersonList
          idValue={data.personId}
          label={labels.relatedTarget}
          search
          rq={{
            identityType:
              IdentityTypeFlags.Customer | IdentityTypeFlags.Supplier
          }}
        />,
        <SearchField
          label={labels.assignedId}
          name="assignedId"
          defaultValue={data.assignedId}
        />
      ]}
      loadData={async (data) => {
        return await app.personProductApi.query(data, {
          defaultValue: [],
          showLoading: false
        });
      }}
      rowHeight={[72, 136]}
      columns={[
        {
          field: "productName",
          header: labels.productName
        },
        {
          field: "assignedId",
          width: 136,
          header: labels.assignedId,
          cellRenderer: ({ data }) => {
            if (data == null) return undefined;
            return (
              <VBox>
                <Typography variant="body2">{data.assignedId}</Typography>
                <Typography variant="caption">
                  {data.productAssignedId}
                </Typography>
              </VBox>
            );
          }
        },
        {
          header: labels.customResources,
          cellRenderer: ({ data }) => {
            if (data == null) return undefined;
            return formatCultures(data.cultures);
          }
        },
        {
          width: DefaultUI.Widths.icon1,
          header: labels.actions,
          cellRenderer: ({
            data,
            cellProps
          }: GridCellRendererProps<PersonProductQueryData, BoxProps>) => {
            if (data == null) return undefined;

            cellProps.sx = {
              paddingTop: "6px!important",
              paddingBottom: "6px!important"
            };

            return (
              <React.Fragment>
                <IconButtonLink
                  title={labels.edit}
                  href={`./edit/${data.productId}/${data.personId}`}
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
            data.productName,
            data.assignedId + ` (${data.productAssignedId})`,
            [
              {
                label: labels.edit,
                icon: <EditIcon />,
                action: `./edit/${data.productId}/${data.personId}`
              }
            ],
            <React.Fragment>
              <Typography variant="body2">
                {formatCultures(data.cultures)}
              </Typography>
            </React.Fragment>
          ];
        })
      }
    />
  );
}
