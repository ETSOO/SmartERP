import { ButtonLink, ViewContainer } from "@etsoo/materialui";
import { GridDataType } from "@etsoo/react";
import { OrderViewData } from "@etsoo/smarterp-crm";
import { app } from "../../../app/MyApp";

export type OrderViewUIProps = {
  data: OrderViewData;
  refresh: () => Promise<void>;
};

export function OrderViewUI(props: OrderViewUIProps) {
  // Destruct
  const { data, refresh } = props;

  return (
    <ViewContainer
      data={data}
      refresh={refresh}
      fields={[
        {
          data: "title",
          singleRow: true,
          horizontal: true
        },
        {
          data: "source",
          label: "orderSource"
        },
        {
          data: "sourceId",
          label: "orderSourceId"
        },
        "assignedId",
        {
          data: (item) => (
            <ButtonLink
              href={`./../../../contact/view/${item.userId}`}
              size="small"
              variant="outlined"
            >
              {item.user}
            </ButtonLink>
          ),
          label: "user"
        },
        {
          data: "tags",
          singleRow: "medium",
          horizontal: true
        },
        {
          data: "description",
          singleRow: true,
          horizontal: true
        },
        {
          data: "addressFormatted",
          label: "address",
          singleRow: true
        },
        {
          data: "delivery",
          label: "orderDelivery",
          singleRow: "small"
        },
        {
          data: "deliveryInstruction",
          singleRow: "large"
        },
        {
          data: "payment",
          label: "orderPayment",
          singleRow: "small"
        },
        {
          data: "paymentInstruction",
          singleRow: "large"
        },
        ["startDate", GridDataType.DateTime],
        ["endDate", GridDataType.DateTime],
        {
          data: (item) => app.getStatusLabel(item.status),
          label: "status"
        },
        ["creation", GridDataType.DateTime]
      ]}
    />
  );
}
