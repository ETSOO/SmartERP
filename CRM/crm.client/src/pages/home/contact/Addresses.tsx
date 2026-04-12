import {
  ResponsivePage,
  SearchField,
  IconButtonLink,
  MobileListItemRenderer,
  SelectBool,
  VBox,
  InputField
} from "@etsoo/materialui";
import EditIcon from "@mui/icons-material/Edit";
import AddIcon from "@mui/icons-material/Add";
import RoomIcon from "@mui/icons-material/Room";
import React from "react";
import {
  GridCellRendererProps,
  GridDataType,
  ScrollerListForwardRef,
  useParamsEx
} from "@etsoo/react";
import { useNavigate } from "react-router-dom";
import { app } from "../../../app/MyApp";
import { usePageDataEmpty } from "@etsoo/smarterp-core";
import { AddressQueryData } from "@etsoo/smarterp-crm";
import { DataTypes, DomUtils } from "@etsoo/shared";
import { DefaultUI } from "@etsoo/smarterp-core/components";
import { BoxProps } from "@mui/material/Box";
import Fab from "@mui/material/Fab";
import { IconButton, Typography } from "@mui/material";

const template = {
  keyword: "string",
  isLocation: "boolean"
} as const satisfies DataTypes.BasicTemplate;

export default function Addresses() {
  // Route
  const navigate = useNavigate();

  const { id: personId = 0 } = useParamsEx({
    id: "number"
  });

  // Labels
  const labels = app.getLabels(
    "actions",
    "add",
    "addLocation",
    "addressFormatted",
    "changeCaution",
    "creation",
    "edit",
    "id",
    "isLocation",
    "keywords",
    "nameB",
    "type",
    "view"
  );

  // Refs
  const ref = React.useRef<ScrollerListForwardRef<AddressQueryData>>(undefined);

  // Load data
  const reloadData = React.useCallback(() => ref.current?.reset(), []);

  const addLocation = (addressId: number) => {
    app.showInputDialog({
      title: labels.addLocation,
      message: "",
      callback: async (form) => {
        // Cancelled
        if (form == null) return;

        // Validate form
        if (!form.reportValidity()) {
          return false;
        }

        // Form data
        const { name, placeId } = DomUtils.dataAs(new FormData(form), {
          name: "string",
          placeId: "string"
        });

        if (!name) {
          DomUtils.setFocus("name", form);
          return false;
        }

        const result = await app.personAddressApi.createLocation({
          parentId: addressId,
          name,
          placeId
        });

        if (result == null) return;

        if (result.ok) {
          reloadData();
          return true;
        } else {
          return app.formatResult(result);
        }
      },
      inputs: (
        <VBox spacing={2} sx={{ marginTop: 1 }}>
          <InputField label={labels.nameB} name="name" required />
          <InputField label={labels.id} name="placeId" />
        </VBox>
      )
    });
  };

  // Page data hook
  usePageDataEmpty(app);

  return (
    <ResponsivePage<AddressQueryData, typeof template>
      {...DefaultUI.pageProps({
        onRefresh: reloadData,
        fabButtons: (
          <React.Fragment>
            <Fab
              title={labels.add}
              size="medium"
              color="primary"
              onClick={() => navigate(`./../../address/${personId}`)}
            >
              <AddIcon />
            </Fab>
          </React.Fragment>
        )
      })}
      mRef={ref}
      fieldTemplate={template}
      fields={(data) => [
        <SearchField
          label={labels.keywords}
          name="keyword"
          defaultValue={data.keyword}
        />,
        <SelectBool
          search
          name="isLocation"
          label={labels.isLocation}
          value={data.isLocation}
        />
      ]}
      loadData={async (data) => {
        return await app.personAddressApi.query(
          { personId, ...data },
          {
            defaultValue: [],
            showLoading: false
          }
        );
      }}
      columns={[
        {
          field: "kind",
          width: 120,
          header: labels.type,
          sortable: true,
          valueFormatter: ({ data }) =>
            app.personAddress.getAddressKind(data?.kind)
        },
        {
          field: "name",
          width: 250,
          header: labels.nameB,
          valueFormatter: ({ data }) =>
            data?.parentName ? `${data.name} - ${data.parentName}` : data?.name
        },
        {
          field: "formattedAddress",
          header: labels.addressFormatted
        },
        {
          field: "creation",
          type: GridDataType.Date,
          width: 116,
          header: labels.creation,
          sortable: true
        },
        {
          width: DefaultUI.Widths.icon2,
          header: labels.actions,
          cellRenderer: ({
            data,
            cellProps
          }: GridCellRendererProps<AddressQueryData, BoxProps>) => {
            if (data == null) return undefined;

            cellProps.sx = {
              paddingTop: "6px!important",
              paddingBottom: "6px!important"
            };

            return (
              <React.Fragment>
                <IconButtonLink
                  title={labels.edit}
                  href={`./../../address/${personId}?id=${data.id}`}
                >
                  <EditIcon />
                </IconButtonLink>
                {data.parentName == null && (
                  <IconButton
                    title={labels.addLocation}
                    onClick={() => addLocation(data.id)}
                  >
                    <RoomIcon />
                  </IconButton>
                )}
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
                action: `./../../address/${personId}?id=${data.id}`
              },
              data.parentName == null && {
                label: labels.addLocation,
                icon: <RoomIcon />,
                action: () => addLocation(data.id)
              }
            ],
            <React.Fragment>
              <Typography variant="body2">{data.formattedAddress}</Typography>
            </React.Fragment>
          ];
        })
      }
    />
  );
}
