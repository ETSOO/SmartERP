import { GridDataType, useParamsEx } from "@etsoo/react";
import {
  ButtonLink,
  CommonPage,
  FileUploadButton,
  HBox,
  HBoxList,
  HtmlDiv,
  IconButtonLink,
  LinkEx,
  VBox,
  ViewContainer
} from "@etsoo/materialui";
import EditIcon from "@mui/icons-material/Edit";
import ExpandMoreIcon from "@mui/icons-material/ExpandMore";
import ArrowBackIcon from "@mui/icons-material/ArrowBack";
import FileUploadIcon from "@mui/icons-material/FileUpload";
import AddIcon from "@mui/icons-material/Add";
import EmailIcon from "@mui/icons-material/Email";
import { app } from "../../../app/MyApp";
import { OrgDownloadKind, usePageDataEmpty } from "@etsoo/smarterp-core";
import React from "react";
import { PersonProfileViewData } from "@etsoo/smarterp-crm";
import Typography from "@mui/material/Typography";
import { ImportanceText } from "@etsoo/smarterp-crm/components";
import LinearProgress from "@mui/material/LinearProgress";
import Accordion from "@mui/material/Accordion";
import AccordionSummary from "@mui/material/AccordionSummary";
import AccordionDetails from "@mui/material/AccordionDetails";
import { useNavigate } from "react-router-dom";
import Button from "@mui/material/Button";
import Link from "@mui/material/Link";
import AccordionActions from "@mui/material/AccordionActions";
import Chip from "@mui/material/Chip";
import { Link as RouterLink } from "react-router";
import { MoreLinkActions } from "../../../components/profile/MoreLinkActions";
import { MoreAttachmentActions } from "../../../components/profile/MoreAttachmentActions";
import { useAddLink } from "../../../components/profile/useAddLink";
import IconButton from "@mui/material/IconButton";
import { useSendEmail } from "../../../components/profile/useSendEmail";

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
    "add",
    "addComment",
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
    "noChanges",
    "people",
    "profile",
    "sendEmail"
  );

  // Page data hook
  usePageDataEmpty(app);

  // Add link
  const addLink = useAddLink(id, loadData);

  // Send email
  const sendEmail = useSendEmail(data?.id ?? 0, data?.personId ?? 0);

  return (
    <CommonPage paddings={0} onRefresh={loadData}>
      {data == null ? (
        <LinearProgress />
      ) : (
        <React.Fragment>
          <HBox gap={1} alignItems="center" justifyContent="center">
            <Typography textAlign="center" variant="h6">
              [{app.core.getIdentityFlagsLabel(data.personIdentityType)}]{" "}
              {data.title}
            </Typography>
            <IconButton title={labels.sendEmail} onClick={sendEmail}>
              <EmailIcon />
            </IconButton>
            <IconButtonLink
              title={labels.back}
              disabled={!data.isAdmin && !data.isSelf}
              href={`./../../edit/${id}`}
            >
              <EditIcon />
            </IconButtonLink>
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
                          <Typography variant="caption">
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
                    data: "personName",
                    label: "relatedTarget"
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
            <AccordionDetails>
              <HtmlDiv>{data.comment}</HtmlDiv>
            </AccordionDetails>
          </Accordion>
          <Accordion>
            <AccordionSummary expandIcon={<ExpandMoreIcon />}>
              <Typography component="span">
                {labels.attachments} ({data.attachments.length})
              </Typography>
            </AccordionSummary>
            <AccordionDetails>
              <HBoxList
                gap={0.5}
                marginBottom={1}
                flexWrap="wrap"
                alignItems="center"
              >
                {data.attachments.map((file, index) => (
                  <React.Fragment key={file.id}>
                    <Link
                      key={file.id}
                      title={
                        file.userName + ", " + app.formatDate(file.creation)
                      }
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
                      {index + 1} {file.description}
                      {file.extension}
                    </Link>
                    {(data.isAdmin || data.isSelf || file.isSelf) && (
                      <MoreAttachmentActions file={file} callback={loadData} />
                    )}
                  </React.Fragment>
                ))}
              </HBoxList>
              <FileUploadButton
                dropFilesLabel={labels.dropFilesLabel}
                startIcon={<FileUploadIcon />}
                maxFiles={10}
                onUploadFiles={async (files) => {
                  const action = await app.profileApi.uploadFilesAction(id);
                  if (action == null) return;

                  const result = await app.core.orgApi.uploadProfileFiles(
                    id,
                    files,
                    action
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
            <AccordionDetails>
              {data.links.map((link, index) => (
                <VBox key={`${link.id}${link.content}`}>
                  <HBox
                    gap={0.5}
                    alignItems="center"
                    padding={0.5}
                    flexWrap="wrap"
                    sx={{ fontSize: 14, backgroundColor: "#f3f3f3" }}
                  >
                    {index + 1}.
                    <LinkEx to={`./../../../../contact/view/${link.userId}`}>
                      {link.userName}
                    </LinkEx>
                    , {app.formatDate(link.creation)},{" "}
                    {app.profile.getLinkKind(link.kind)}
                    {(data.isAdmin || data.isSelf || link.isSelf) && (
                      <MoreLinkActions
                        link={link}
                        callback={loadData}
                        onEdit={() => addLink(link)}
                      />
                    )}
                    {link.targetProfileTitle && (
                      <Chip
                        label={link.targetProfileTitle}
                        component={RouterLink}
                        to={`./../${link.targetProfileId}`}
                        variant="outlined"
                        clickable
                      />
                    )}
                  </HBox>
                  <HtmlDiv>{link.content}</HtmlDiv>
                </VBox>
              ))}
            </AccordionDetails>
            <AccordionActions>
              <Button
                variant="outlined"
                startIcon={<AddIcon />}
                onClick={() => addLink()}
              >
                {labels.add}
              </Button>
            </AccordionActions>
          </Accordion>
        </React.Fragment>
      )}
    </CommonPage>
  );
}
