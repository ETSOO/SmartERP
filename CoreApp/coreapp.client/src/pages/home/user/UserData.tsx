import { ComboBox, SearchBar, SearchField, Tiplist } from "@etsoo/materialui";
import { DateUtils } from "@etsoo/shared";
import { app } from "../../../app/MyApp";
import { DeviceListDto } from "@etsoo/smarterp-core";

export default function UserData() {
  // Labels
  const labels = app.getLabels(
    "actions",
    "creation",
    "device",
    "endDate",
    "startDate",
    "title"
  );

  return (
    <SearchBar
      onSubmit={(data) => {
        console.log("data", data);
      }}
      fields={[
        <SearchField label={labels.title} name="keyword" />,
        <ComboBox
          name="paymentStatus"
          label={"Status"}
          search
          options={app.getStatusList()}
        />,
        <Tiplist<DeviceListDto>
          label={labels.device}
          name="deviceId"
          search
          loadData={(keyword, id) =>
            app.core.userApi.deviceList(
              { id, keyword },
              { defaultValue: [], showLoading: false }
            )
          }
        />,
        <SearchField
          label={labels.startDate}
          name="creationStart"
          type="date"
          slotProps={{
            htmlInput: { max: DateUtils.formatForInput(new Date()) }
          }}
        />,
        <SearchField
          label={labels.endDate}
          name="creationEnd"
          type="date"
          slotProps={{
            htmlInput: { max: DateUtils.formatForInput(new Date()) }
          }}
        />
      ]}
    />
  );
}
