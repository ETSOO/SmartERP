import { app } from "../../app/MyApp";
import { usePageData } from "@etsoo/smarterp-core";
import CalendarMonthIcon from "@mui/icons-material/CalendarMonth";
import HubIcon from "@mui/icons-material/Hub";
import HistoryIcon from "@mui/icons-material/History";
import "reactflow/dist/style.css";
import { Flowchart } from "../../components/Flowchart";
import { LatestTasks } from "../../components/LatestTasks";
import { useNavigate } from "react-router-dom";
import { TabBox } from "@etsoo/materialui";

export default function Home() {
  // Route
  const navigate = useNavigate();

  // Labels
  const labels = app.getLabels("allProfiles", "flowchart", "latestTasks");

  usePageData(app, { noPageHeader: true }, []);

  return (
    <TabBox
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
          children: () => navigate(`./org/profile/all`),
          label: `${labels.allProfiles}...`,
          icon: <HistoryIcon />,
          iconPosition: "start"
        }
      ]}
    />
  );
}
