import {
  MUGlobal,
  ResponsivePage,
  SearchField,
  IconButtonLink,
  MobileListItemRenderer,
  VBox,
  OptionBool,
  ButtonLink
} from "@etsoo/materialui";
import EditIcon from "@mui/icons-material/Edit";
import AddIcon from "@mui/icons-material/Add";
import MergeIcon from "@mui/icons-material/Merge";
import SortIcon from "@mui/icons-material/Sort";
import React from "react";
import {
  GridCellRendererProps,
  GridDataType,
  ScrollerListForwardRef
} from "@etsoo/react";
import { useNavigate } from "react-router-dom";
import { app } from "../../../app/MyApp";
import { usePageDataEmpty } from "@etsoo/smarterp-core";
import { PersonCategoryQueryData } from "@etsoo/smarterp-crm";
import { DataTypes, DomUtils } from "@etsoo/shared";
import { DefaultUI, IdentityFlagsList } from "@etsoo/smarterp-core/components";
import { BoxProps } from "@mui/material/Box";
import { PersonCategoryTiplist } from "@etsoo/smarterp-crm/components";
import Fab from "@mui/material/Fab";
import IconButton from "@mui/material/IconButton";
import { Typography } from "@mui/material";

const template = {
  keyword: "string",
  identityType: "number",
  parentId: "number"
} as const satisfies DataTypes.BasicTemplate;

export default function AllCategories() {
  // Route
  const navigate = useNavigate();

  // Labels
  const labels = app.getLabels(
    "actions",
    "add",
    "category",
    "changeCaution",
    "confirmAction",
    "creation",
    "edit",
    "identityType",
    "nameB",
    "mergeCategory",
    "mergeCategoryDelete",
    "mergeTo",
    "parentCategory",
    "sortCategory",
    "view"
  );

  // Refs
  const ref = React.useRef<ScrollerListForwardRef<PersonCategoryQueryData>>();
  const refs = React.useRef<{ identityType?: number }>({});

  // Load data
  const reloadData = React.useCallback(() => ref.current?.reset(), []);

  const margin = MUGlobal.pagePaddings;

  const mergeCategory = (data: PersonCategoryQueryData) => {
    app.showInputDialog({
      title: labels.mergeCategory,
      message: labels.changeCaution,
      fullScreen: app.smDown,
      callback: async (form) => {
        // Cancelled
        if (form == null) {
          return;
        }

        // Form data
        const { targetId, deleteSource } = DomUtils.dataAs(new FormData(form), {
          deleteSource: "boolean",
          targetId: "number"
        });

        if (targetId == null) {
          DomUtils.setFocus("targetId", form);
          return false;
        }

        const result = await app.personCategoryApi.merge({
          sourceId: data.id,
          targetId,
          deleteSource
        });

        if (result == null) return false;

        if (result.ok) {
          reloadData();
          return;
        }

        return app.formatResult(result);
      },
      inputs: (
        <VBox gap={2} paddingTop={2}>
          <Typography>
            {labels.category}: {data.names.join(" -> ")}
          </Typography>
          <OptionBool
            name="deleteSource"
            variant="outlined"
            label={labels.mergeCategoryDelete}
            defaultValue={"false"}
          />
          <PersonCategoryTiplist
            label={labels.mergeTo}
            name="targetId"
            rq={{ identityType: data.identityType, excludedIds: [data.id] }}
          />
        </VBox>
      )
    });
  };

  // Page data hook
  usePageDataEmpty(app);

  return (
    <ResponsivePage<PersonCategoryQueryData, typeof template>
      {...DefaultUI.pageProps({
        onRefresh: reloadData,
        fabButtons: (
          <React.Fragment>
            <ButtonLink
              href="./sort"
              size="small"
              variant="outlined"
              startIcon={<SortIcon />}
            >
              {labels.sortCategory}
            </ButtonLink>
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
      quickAction={(data) => navigate(`./view/${data.id}`)}
      fieldTemplate={template}
      fields={(data) => [
        <IdentityFlagsList
          value={data.identityType}
          onItemChange={(item) => (refs.current.identityType = item?.id)}
          search
        />,
        <SearchField
          label={labels.nameB}
          name="keyword"
          defaultValue={data.keyword}
        />,
        <PersonCategoryTiplist
          label={labels.parentCategory}
          name="parentId"
          onLoadData={(rq) =>
            Object.assign(rq, { identityType: refs.current.identityType })
          }
          search
        />
      ]}
      loadData={async (data) => {
        return await app.personCategoryApi.query(data, {
          defaultValue: [],
          showLoading: false
        });
      }}
      columns={[
        {
          field: "identityType",
          width: 120,
          header: labels.identityType,
          valueFormatter: ({ data }) =>
            app.person.getIdentityType(data?.identityType)
        },
        {
          field: "names",
          header: labels.nameB,
          valueFormatter: ({ data }) => data?.names.join(" -> ")
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
          }: GridCellRendererProps<PersonCategoryQueryData, BoxProps>) => {
            if (data == null) return undefined;

            cellProps.sx = {
              paddingTop: "6px!important",
              paddingBottom: "6px!important"
            };

            return (
              <React.Fragment>
                <IconButton
                  title={labels.mergeCategory}
                  onClick={() => mergeCategory(data)}
                  size="small"
                >
                  <MergeIcon />
                </IconButton>
                <IconButtonLink title={labels.edit} href={`./edit/${data.id}`}>
                  <EditIcon />
                </IconButtonLink>
              </React.Fragment>
            );
          }
        }
      ]}
      itemSize={[116, margin, "1px"]}
      innerItemRenderer={(props) =>
        MobileListItemRenderer(props, (data) => {
          return [
            data.names.join(" -> "),
            app.formatDate(data.creation, "d"),
            [
              {
                label: labels.mergeCategory,
                icon: <MergeIcon />,
                action: () => mergeCategory(data)
              },
              {
                label: labels.edit,
                icon: <EditIcon />,
                action: `./edit/${data.id}`
              }
            ],
            <React.Fragment></React.Fragment>
          ];
        })
      }
    />
  );
}
