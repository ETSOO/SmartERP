import { GridDataType, useParamsEx } from "@etsoo/react";
import {
  ButtonLink,
  CommonPage,
  FileUploadButton,
  HBox,
  HBoxList,
  LinkEx,
  ViewContainer
} from "@etsoo/materialui";
import EditIcon from "@mui/icons-material/Edit";
import ExpandMoreIcon from "@mui/icons-material/ExpandMore";
import ArrowBackIcon from "@mui/icons-material/ArrowBack";
import FileUploadIcon from "@mui/icons-material/FileUpload";
import { app } from "../../../app/MyApp";
import { OrgDownloadKind, usePageData } from "@etsoo/smarterp-core";
import React from "react";
import { PersonProfileViewData } from "@etsoo/smarterp-crm";
import Typography from "@mui/material/Typography";
import { ImportanceText } from "@etsoo/smarterp-crm/components";
import LinearProgress from "@mui/material/LinearProgress";
import Accordion from "@mui/material/Accordion";
import AccordionSummary from "@mui/material/AccordionSummary";
import AccordionDetails from "@mui/material/AccordionDetails";
import IconButton from "@mui/material/IconButton";
import { useNavigate } from "react-router-dom";
import Button from "@mui/material/Button";
import DOMPurify from "dompurify";
import Link from "@mui/material/Link";

export default function ViewProfile() {
  // Route
  const { id = 0 } = useParamsEx({ id: "number" });
  const navigate = useNavigate();

  // State
  const [data, setData] = React.useState<PersonProfileViewData>();

  // Load data
  const loadData = React.useCallback(async () => {
    const data = await app.profileApi.read(id);
    setData(data);
  }, [id]);

  // Labels
  const labels = app.getLabels(
    "assignee",
    "attachments",
    "back",
    "browse",
    "comments",
    "confirmAction",
    "description",
    "dropFilesLabel",
    "edit",
    "editLogo",
    "leaveOrg",
    "logo",
    "people",
    "profile",
    "view"
  );

  // Page data hook
  usePageData(app, `${labels.view} (${labels.profile})`, [loadData]);

  return (
    <CommonPage paddings={0} onRefresh={loadData}>
      {data == null ? (
        <LinearProgress />
      ) : (
        <React.Fragment>
          <HBox gap={1} alignItems="center" justifyContent="center">
            <Typography textAlign="center" variant="h6">
              {data.title}
            </Typography>
            <IconButton
              title={labels.back}
              disabled={!data.isAdmin && !data.isSelf}
            >
              <EditIcon />
            </IconButton>
            <Button
              variant="outlined"
              startIcon={<ArrowBackIcon />}
              onClick={() => navigate(-1)}
            >
              {labels.back}
            </Button>
          </HBox>
          <Accordion sx={{ marginTop: 0.5 }} defaultExpanded>
            <AccordionSummary expandIcon={<ExpandMoreIcon />}>
              <Typography component="span">
                {app.profile.getKind(data.kind)},{" "}
                <LinkEx to={`./../../../../contact/view/${data.userId}`}>
                  {data.userName}
                </LinkEx>
                , {app.formatDate(data.creation)},{" "}
                <ImportanceText importance={data.importance} />
              </Typography>
            </AccordionSummary>
            <AccordionDetails>
              <ViewContainer
                refresh={loadData}
                data={data}
                fields={[
                  {
                    data: (item) =>
                      item.assigneeId == null &&
                      (item.persons == null ||
                        item.persons.length > 0) ? undefined : (
                        <HBoxList gap={0.5}>
                          <Typography variant="body2">
                            {labels.people}:
                          </Typography>
                          {item.persons != null &&
                            item.persons.map((p) => (
                              <LinkEx
                                key={p.id}
                                to={`./../../../../contact/view/${p.id}`}
                                variant="body2"
                              >
                                {p.name}
                              </LinkEx>
                            ))}
                          {item.assigneeId && (
                            <LinkEx
                              to={`./../../../../contact/view/${item.assigneeId}`}
                              variant="body2"
                            >
                              {item.assigneeName} ({labels.assignee})
                            </LinkEx>
                          )}
                        </HBoxList>
                      ),
                    singleRow: true
                  },
                  "location",
                  ["happenDate", GridDataType.DateTime],
                  {
                    data: "happenDateEnd",
                    label: "dateTo",
                    dataType: GridDataType.DateTime
                  },
                  {
                    data: (item) =>
                      item.orderTitle ? (
                        <ButtonLink
                          href={`./../${item.orderId}`}
                          size="small"
                          variant="outlined"
                        >
                          {item.orderTitle}
                        </ButtonLink>
                      ) : undefined,
                    label: "order",
                    singleRow: true,
                    horizontal: true
                  },
                  {
                    data: (item) => app.getRoleLabel(item.userRole),
                    label: "profileRole",
                    horizontal: true
                  },
                  {
                    data: (item) => app.getStatusLabel(item.status),
                    label: "status",
                    singleRow: "small",
                    horizontal: true
                  },
                  {
                    data: "indexKey",
                    singleRow: "small",
                    horizontal: true
                  }
                ]}
              ></ViewContainer>
            </AccordionDetails>
          </Accordion>
          <Accordion defaultExpanded>
            <AccordionSummary expandIcon={<ExpandMoreIcon />}>
              <Typography component="span">{labels.description}</Typography>
            </AccordionSummary>
            <AccordionDetails
              dangerouslySetInnerHTML={{
                __html: DOMPurify.sanitize(data.comment)
              }}
            ></AccordionDetails>
          </Accordion>
          <Accordion>
            <AccordionSummary expandIcon={<ExpandMoreIcon />}>
              <Typography component="span">
                {labels.attachments} ({data.attachments.length})
              </Typography>
            </AccordionSummary>
            <AccordionDetails>
              <HBoxList gap={0.5} marginBottom={1} flexWrap="wrap">
                {data.attachments.map((file) => (
                  <Link
                    key={file.id}
                    title={file.userName + ", " + app.formatDate(file.creation)}
                    variant="body2"
                    underline="hover"
                    href="#"
                    onClick={(event) => {
                      event.preventDefault();
                      event.stopPropagation();

                      app.core.orgApi.downloadFile(
                        OrgDownloadKind.Profile,
                        file.id
                      );
                    }}
                  >
                    {file.description}
                  </Link>
                ))}
              </HBoxList>
              <FileUploadButton
                dropFilesLabel={labels.dropFilesLabel}
                startIcon={<FileUploadIcon />}
                onUploadFiles={async (files) => {
                  const result = await app.core.orgApi.uploadProfileFiles(
                    id,
                    files
                  );
                  if (result == null) return;
                  if (result.ok) {
                    loadData();
                  } else {
                    app.alertResult(result);
                  }
                }}
              >
                {labels.browse}
              </FileUploadButton>
            </AccordionDetails>
          </Accordion>
          <Accordion defaultExpanded>
            <AccordionSummary expandIcon={<ExpandMoreIcon />}>
              <Typography component="span">
                {labels.comments} ({data.links.length})
              </Typography>
            </AccordionSummary>
            <AccordionDetails
              dangerouslySetInnerHTML={{
                __html: DOMPurify.sanitize(data.comment)
              }}
            ></AccordionDetails>
          </Accordion>
        </React.Fragment>
      )}
    </CommonPage>
  );
}
