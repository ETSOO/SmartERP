import { AssetViewData } from "@etsoo/smarterp-crm";
import { app } from "../../../app/MyApp";
import { ButtonLink, HBox, ViewContainer } from "@etsoo/materialui";
import Typography from "@mui/material/Typography";
import { GridDataType } from "@etsoo/react";
import EditIcon from "@mui/icons-material/Edit";

export type AssetViewUIProps = {
  data: AssetViewData;
  refresh: () => Promise<void>;
};

export function AssetViewUI(props: AssetViewUIProps) {
  // Destruct
  const { data, refresh } = props;

  const labels = app.getLabels("edit");

  return (
    <ViewContainer
      data={data}
      refresh={refresh}
      fields={[
        {
          data: (item) => (
            <HBox
              sx={{
                justifyContent: "center",
                alignItems: "center",
                marginBottom: 2
              }}
            >
              <Typography
                variant="subtitle2"
                sx={{ textAlign: "center", paddingRight: 2 }}
              >
                {item.sn} - {item.productName}
              </Typography>
              <ButtonLink
                startIcon={<EditIcon />}
                variant="outlined"
                href={`./../../edit/${item.id}`}
              >
                {labels.edit}
              </ButtonLink>
            </HBox>
          ),
          singleRow: true
        },
        ["amount", GridDataType.Money],
        {
          data: "description",
          singleRow: true,
          horizontal: true
        },
        {
          data: (item) => app.getStatusLabel(item.status),
          label: "status"
        },
        ["creation", GridDataType.DateTime]
      ]}
    />
  );
}
