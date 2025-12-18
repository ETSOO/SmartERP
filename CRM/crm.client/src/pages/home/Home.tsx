import { app } from "../../app/MyApp";
import { usePageDataEmpty } from "@etsoo/smarterp-core";
import CalendarMonthIcon from "@mui/icons-material/CalendarMonth";
import HubIcon from "@mui/icons-material/Hub";
import HistoryIcon from "@mui/icons-material/History";
import ContactsIcon from "@mui/icons-material/Contacts";
import "reactflow/dist/style.css";
import { Flowchart } from "../../components/Flowchart";
import { useNavigate } from "react-router-dom";
import { TabBox } from "@etsoo/materialui";
import { DefaultUI } from "@etsoo/smarterp-core/components";
import { LatestTasks } from "../../components/profile/LatestTasks";

export default function Home() {
  // Route
  const navigate = useNavigate();

  // Labels
  const labels = app.getLabels(
    "allProfiles",
    "flowchart",
    "latestTasks",
    "stakeholders"
  );

  usePageDataEmpty(app);

  return (
    <TabBox
      {...DefaultUI.tabsProps(app.smDown)}
      tabs={[
        {
          children: <LatestTasks />,
          label: labels.latestTasks,
          icon: <CalendarMonthIcon />,
          iconPosition: "start"
        },
        {
          children: (visible) => <Flowchart visible={visible} />,
          label: labels.flowchart,
          icon: <HubIcon />,
          iconPosition: "start"
        },
        {
          children: () => navigate(`./profile`),
          label: `${labels.allProfiles}...`,
          icon: <HistoryIcon />,
          iconPosition: "start"
        },
        {
          children: () => navigate(`./contact`),
          label: `${labels.stakeholders}...`,
          icon: <ContactsIcon />,
          iconPosition: "start"
        }
      ]}
    />
  );
}
