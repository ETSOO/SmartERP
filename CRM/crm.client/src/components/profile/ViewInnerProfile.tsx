import AppBar from "@mui/material/AppBar";
import Box from "@mui/material/Box";
import Toolbar from "@mui/material/Toolbar";
import Typography from "@mui/material/Typography";
import ExpandMoreIcon from "@mui/icons-material/ExpandMore";
import AddIcon from "@mui/icons-material/Add";
import EditIcon from "@mui/icons-material/Edit";
import FileUploadIcon from "@mui/icons-material/FileUpload";
import ArticleIcon from "@mui/icons-material/Article";
import EmailIcon from "@mui/icons-material/Email";
import { app } from "../../app/MyApp";
import {
  PersonListItem,
  PersonProfileQueryData,
  PersonProfileViewData
} from "@etsoo/smarterp-crm";
import React from "react";
import {
  ButtonLink,
  FileUploadButton,
  HBox,
  HBoxList,
  HtmlDiv,
  IconButtonLink,
  LinkEx,
  VBox,
  ViewContainer
} from "@etsoo/materialui";
import Accordion from "@mui/material/Accordion";
import AccordionSummary from "@mui/material/AccordionSummary";
import AccordionDetails from "@mui/material/AccordionDetails";
import Divider from "@mui/material/Divider";
import AccordionActions from "@mui/material/AccordionActions";
import Button from "@mui/material/Button";
import Chip from "@mui/material/Chip";
import { Link as RouterLink } from "react-router";
import { MoreLinkActions } from "./MoreLinkActions";
import { useAddLink } from "./useAddLink";
import { MoreAttachmentActions } from "./MoreAttachmentActions";
import Link from "@mui/material/Link";
import { OrgDownloadKind } from "@etsoo/smarterp-core";
import IconButton from "@mui/material/IconButton";
import { useSendEmail } from "./useSendEmail";

function isViewData(
  data: PersonProfileQueryData
): data is PersonProfileViewData {
  return "personId" in data && "attachments" in data;
}

export type ViewInnerRef = {
  /**
   * Set data
   * @param data Data
   */
  setData: (data: PersonProfileQueryData) => void;
};

type ViewInnerProfileProps = {
  /**
   * Methods reference
   */
  mRef: React.Ref<ViewInnerRef>;

  /**
   * Current tab index
   */
  index: number;

  /**
   * Order id
   * 订单编号
   */
  orderId?: number;
};

export function ViewInnerProfile(props: ViewInnerProfileProps) {
  // Destruct
  const { mRef, index, orderId } = props;

  // Labels
  const labels = app.getLabels(
    "add",
    "attachments",
    "assignee",
    "browse",
    "clickToView",
    "comments",
    "dateTo",
    "description",
    "dropFilesLabel",
    "edit",
    "order",
    "owner",
    "people",
    "po",
    "sendEmail",
    "view"
  );

  // State
  const [data, setData] = React.useState<PersonProfileViewData>();

  const loadData = React.useCallback(async (data: PersonProfileQueryData) => {
    const innerData = await app.profileApi.readInner(data.id);
    if (innerData == null) {
      setData(undefined);
      return;
    }

    // Cache data
    Object.assign(data, innerData);

    // Combine data
    setData({ ...innerData, ...data });
  }, []);

  const refreshData = () => loadData(data!);

  React.useImperativeHandle(
    mRef,
    () => ({
      setData: (data: PersonProfileQueryData) => {
        if (isViewData(data)) {
          setData(data);
        } else {
          // Load more data
          loadData(data);
        }
      }
    }),
    []
  );

  // Add link
  const addLink = useAddLink(data?.id ?? 0, refreshData);

  // Send email
  const sendEmail = useSendEmail(data?.id ?? 0, data?.personId ?? 0);

  const persons: PersonListItem[] = [];
  if (data) {
    persons.push({
      id: data.userId,
      name: data.userName!,
      owner: labels.owner
    });
    if (data.assigneeId && data.assigneeId !== data.userId) {
      persons.push({
        id: data.assigneeId,
        name: data.assigneeName!,
        owner: labels.assignee
      });
    }
    if (data.persons) {
      data.persons.forEach((item) => {
        if (item.id === data.userId) return;
        if (persons.findIndex((p) => p.id === item.id) < 0) {
          persons.push(item);
        }
      });
    }
  }

  return (
    <React.Fragment>
      <AppBar position="static">
        <Toolbar
          variant="dense"
          disableGutters
          sx={{ px: 1, minHeight: "inherit" }}
        >
          <Typography variant="body2" component="div">
            {data?.title ? data.title : labels.clickToView}
          </Typography>
          <Box sx={{ flexGrow: 1 }} />
          <Box sx={{ display: { xs: "none", md: "flex" } }}>
            <IconButton
              color="inherit"
              title={labels.sendEmail}
              onClick={sendEmail}
              disabled={data == null}
            >
              <EmailIcon />
            </IconButton>
            <IconButtonLink
              href={`./../../../profile/edit/${data?.id}?index=${index}&orderId=${orderId ?? ""}`}
              color="inherit"
              title={labels.edit}
              disabled={data == null}
            >
              <EditIcon />
            </IconButtonLink>
            <IconButtonLink
              href={`./../../../profile/view/${data?.id}`}
              color="inherit"
              title={labels.view}
              disabled={data == null}
            >
              <ArticleIcon />
            </IconButtonLink>
          </Box>
        </Toolbar>
      </AppBar>
      <Box sx={{ height: "calc(100vh - 258px)", overflowY: "auto" }}>
        <Accordion defaultExpanded>
          <AccordionSummary expandIcon={<ExpandMoreIcon />}>
            <Typography component="span">{labels.description}</Typography>
          </AccordionSummary>
          <AccordionDetails>
            <HtmlDiv>{data?.comment}</HtmlDiv>
            <Divider sx={{ my: 1 }} />
            {data && (
              <ViewContainer
                refresh={refreshData}
                data={data}
                fields={[
                  {
                    data: () => (
                      <HBoxList>
                        <Typography variant="caption">
                          {labels.people}:
                        </Typography>
                        {persons.map((p) => (
                          <LinkEx
                            key={p.id}
                            to={`./../${p.id}`}
                            variant="body2"
                          >
                            {p.name + (p.owner ? ` (${p.owner})` : "")}
                          </LinkEx>
                        ))}
                      </HBoxList>
                    ),
                    singleRow: true
                  },
                  { data: "location", singleRow: true, horizontal: true },
                  {
                    data: (item) => (
                      <HBox
                        spacing={0.5}
                        sx={{ alignItems: "center", flexWrap: "wrap" }}
                      >
                        <Typography variant="subtitle2">
                          {app.formatDate(item.happenDate, "dm")}
                        </Typography>
                        <Typography variant="body2">{labels.dateTo}</Typography>
                        <Typography variant="subtitle2">
                          {app.formatDate(item.happenDateEnd, "dm")}
                        </Typography>
                      </HBox>
                    ),
                    label: "happenDate",
                    singleRow: true
                  },
                  {
                    data: (item) =>
                      item.orderTitle && orderId == null ? (
                        <ButtonLink
                          href={`./../../../${item.isOrder ? "order" : "po"}/view/${item.orderId}`}
                          size="small"
                          variant="outlined"
                        >
                          {item.orderTitle}
                        </ButtonLink>
                      ) : undefined,
                    label: (item) =>
                      (item.isOrder ? labels.order : labels.po) + ":",
                    singleRow: true,
                    horizontal: true
                  },
                  {
                    data: (item) => app.getRoleLabel(item.userRole),
                    label: "profileRole",
                    horizontal: true,
                    singleRow: "large"
                  }
                ]}
              ></ViewContainer>
            )}
          </AccordionDetails>
        </Accordion>
        {data && (
          <Accordion>
            <AccordionSummary expandIcon={<ExpandMoreIcon />}>
              <Typography component="span">
                {labels.attachments} ({data.attachments.length})
              </Typography>
            </AccordionSummary>
            <AccordionDetails>
              <HBoxList
                spacing={0.5}
                sx={{ marginBottom: 1, flexWrap: "wrap", alignItems: "center" }}
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
                      <MoreAttachmentActions
                        file={file}
                        callback={refreshData}
                      />
                    )}
                  </React.Fragment>
                ))}
              </HBoxList>
              <FileUploadButton
                dropFilesLabel={labels.dropFilesLabel}
                startIcon={<FileUploadIcon />}
                maxFiles={10}
                onUploadFiles={async (files) => {
                  const action = await app.profileApi.uploadFilesAction(
                    data.id
                  );
                  if (action == null) return;

                  const result = await app.core.orgApi.uploadProfileFiles(
                    data.id,
                    files,
                    action
                  );
                  if (result == null) return;
                  if (result.ok) {
                    refreshData();
                  } else {
                    app.alertResult(result);
                  }
                }}
              >
                {labels.browse}
              </FileUploadButton>
            </AccordionDetails>
          </Accordion>
        )}
        {data && (
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
                    spacing={0.5}
                    sx={{
                      alignItems: "center",
                      padding: 0.5,
                      flexWrap: "wrap",
                      fontSize: 14,
                      backgroundColor: "#f3f3f3"
                    }}
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
                        callback={refreshData}
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
              {app.ownsIdentity(data.personIdentityType, "AddComment") && (
                <Button
                  variant="outlined"
                  startIcon={<AddIcon />}
                  onClick={() => addLink()}
                >
                  {labels.add}
                </Button>
              )}
            </AccordionActions>
          </Accordion>
        )}
      </Box>
    </React.Fragment>
  );
}
