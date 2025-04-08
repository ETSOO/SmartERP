import { app } from "../app/MyApp";
import TodayIcon from "@mui/icons-material/Today";
import NavigateBeforeIcon from "@mui/icons-material/NavigateBefore";
import NavigateNextIcon from "@mui/icons-material/NavigateNext";
import AddIcon from "@mui/icons-material/Add";
import { UserTiplist } from "@etsoo/smarterp-core/components";
import React from "react";
import { DateUtils } from "@etsoo/shared";
import { ButtonLink, LinkEx } from "@etsoo/materialui";
import Stack from "@mui/material/Stack";
import Button from "@mui/material/Button";
import IconButton from "@mui/material/IconButton";
import TextField from "@mui/material/TextField";
import Typography from "@mui/material/Typography";
import Chip from "@mui/material/Chip";
import { PersonProfileQueryData } from "@etsoo/smarterp-crm";
import { getImportanceColor } from "@etsoo/smarterp-crm/components";

function removeHours(date: Date) {
  date.setHours(0, 0, 0, 0);
  return date;
}

function formatTen(num: number) {
  return num > 9 ? num : `0${num}`;
}

function formatDate(date: Date) {
  return `${formatTen(date.getMonth() + 1)}-${formatTen(date.getDate())}`;
}

type TaskDate = {
  date: Date;
  outRange: boolean;
  tasks?: PersonProfileQueryData[];
};

export function LatestTasks() {
  // Labels
  const labels = app.getLabels("nextWeek", "newTask", "previousWeek", "today");

  // Week days
  const weekDays = app.get<string[]>("weekDays") ?? [];

  // State
  const today = removeHours(new Date());
  const [date, setDate] = React.useState(today);
  const [userId, setUserId] = React.useState<number>();
  const [tasks, setTasks] = React.useState<PersonProfileQueryData[]>();

  // Set week
  const setWeek = React.useCallback(
    (week: number) => {
      const d = new Date(date);
      d.setDate(d.getDate() + week * 7);
      setDate(d);
    },
    [date]
  );

  // Dates
  const [dates, maxDate] = React.useMemo(() => {
    const dates: TaskDate[][] = [];
    let currentDay = date.getDay();
    if (currentDay === 0) currentDay = 7;
    const weeks = 3;
    const maxDate = new Date(date);
    maxDate.setDate(maxDate.getDate() + 13);
    for (let i = 0; i < weeks; i++) {
      const weekDates: TaskDate[] = [];
      for (let j = 0; j < 7; j++) {
        const d = new Date(date);
        d.setDate(d.getDate() + i * 7 + j - currentDay + 1);

        if (d < date || d > maxDate) {
          weekDates.push({ date: d, outRange: true });
        } else {
          const dateTasks = tasks?.filter((task) => {
            const taskStart = DateUtils.parse(task.happenDate);
            taskStart?.setHours(0, 0, 0, 0);
            const taskEnd = DateUtils.parse(task.happenDateEnd);
            taskEnd?.setHours(0, 0, 0, 0);
            return taskStart && taskEnd && d >= taskStart && d <= taskEnd;
          });
          weekDates.push({ date: d, outRange: false, tasks: dateTasks });
        }
      }
      dates.push(weekDates);
    }
    return [dates, maxDate];
  }, [date, tasks]);

  React.useEffect(() => {
    app.profileApi
      .query(
        {
          isTask: true,
          enabled: true,
          participantId: userId ?? 0,
          happenDateStart: date,
          happenDateEnd: maxDate
        },
        { showLoading: false }
      )
      .then((result) => {
        if (!result) return;
        setTasks(result);
      });
  }, [date, maxDate.valueOf(), userId]);

  // Layout
  return (
    <Stack spacing={1}>
      <Stack direction="row" gap={0.5} flexWrap="wrap">
        <Button
          startIcon={<TodayIcon />}
          disabled={today.valueOf() === date.valueOf()}
          onClick={() => setDate(today)}
        >
          {labels.today}
        </Button>
        <IconButton title={labels.previousWeek} onClick={() => setWeek(-1)}>
          <NavigateBeforeIcon />
        </IconButton>
        <TextField
          type="date"
          size="small"
          value={DateUtils.formatForInput(date)}
          onChange={(e) =>
            setDate(removeHours(DateUtils.parse(e.target.value) ?? today))
          }
        />
        <IconButton title={labels.nextWeek} onClick={() => setWeek(1)}>
          <NavigateNextIcon />
        </IconButton>
        <UserTiplist search onValueChange={(value) => setUserId(value?.id)} />
        <ButtonLink
          variant="outlined"
          color="primary"
          startIcon={<AddIcon />}
          href="org/profile/addTask"
        >
          {labels.newTask}
        </ButtonLink>
      </Stack>
      <table className="responsive-table">
        <thead>
          <tr>
            {weekDays.map((day, index) => (
              <th key={index}>
                <Typography component="span">{day}</Typography>
              </th>
            ))}
          </tr>
        </thead>
        <tbody>
          {dates.map((row, rowIndex) => (
            <tr key={rowIndex}>
              {row.map((d, cellIndex) => {
                if (d.outRange) {
                  return (
                    <td key={`${rowIndex}-${cellIndex}`} className="small"></td>
                  );
                }

                let weekDayIndex = d.date.getDay();
                if (weekDayIndex === 0) weekDayIndex = 7;

                const isToday = d.date.valueOf() === today.valueOf();

                return (
                  <td key={d.date.toDateString()} valign="top">
                    <Stack direction="row" spacing={1}>
                      <Chip
                        label={formatDate(d.date)}
                        size="small"
                        color={isToday ? "primary" : undefined}
                      />
                      <Chip
                        label={weekDays[weekDayIndex - 1]}
                        size="small"
                        className="cell"
                      />
                    </Stack>
                    <Stack
                      direction="column"
                      className="tasks"
                      spacing={0.5}
                      flexWrap="wrap"
                      flexGrow={3}
                    >
                      {d.tasks?.map((task, index) => (
                        <LinkEx
                          key={task.id}
                          to={`./org/profile/view/${task.id}`}
                          variant="caption"
                          color={getImportanceColor(task.importance)}
                        >
                          {index + 1}{" "}
                          {`${task.title}${
                            task.isSelf ? "" : ` (${task.userName})`
                          }`}
                        </LinkEx>
                      ))}
                    </Stack>
                  </td>
                );
              })}
            </tr>
          ))}
        </tbody>
      </table>
    </Stack>
  );
}
