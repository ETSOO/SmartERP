import React from "react";
import { app } from "../app/MyApp";
import Button from "@mui/material/Button";
import OpenInBrowserIcon from "@mui/icons-material/OpenInBrowser";
import Grid from "@mui/material/Grid";
import {
  CustomFieldUI,
  HBox,
  InputField,
  NotificationMUDataMethods,
  NotificationMUDataProps
} from "@etsoo/materialui";
import { DocumentGenerateRQ, DocumentListData } from "@etsoo/smarterp-core";
import LinearProgress from "@mui/material/LinearProgress";
import ListItemButton from "@mui/material/ListItemButton";
import ListItemText from "@mui/material/ListItemText";
import ListItem from "@mui/material/ListItem";
import List from "@mui/material/List";
import { AppActionData, CustomFieldRef } from "@etsoo/appscript";
import { CultureList } from "./CultureList";
import Checkbox from "@mui/material/Checkbox";
import FormControlLabel from "@mui/material/FormControlLabel";

function DocumentGenerator({
  action,
  kind,
  mRef,
  targetId
}: NotificationMUDataProps & {
  action: (id: number, targetId: number) => Promise<AppActionData | undefined>;
  kind: string;
  targetId: number;
}) {
  // Labels
  const labels = app.getLabels("noCache", "title");

  // State
  const [documents, setDocuments] = React.useState<DocumentListData[]>();
  const [selectedDocument, setSelectedDocument] =
    React.useState<DocumentListData>();

  // Refs
  const parametersRef =
    React.useRef<CustomFieldRef<Record<string, unknown>>>(null);
  const dataRef = React.useRef<{
    subject?: string;
    culture?: string;
    noCache?: boolean;
  }>({});

  React.useImperativeHandle(mRef, () => ({
    getValue: async (): Promise<DocumentGenerateRQ | undefined> => {
      if (selectedDocument == null) {
        return;
      }

      const actionData = await action(selectedDocument.id, targetId);

      if (actionData == null) {
        return;
      }

      return {
        id: selectedDocument.id,
        culture: dataRef.current.culture,
        noCache: dataRef.current.noCache,
        action: actionData,
        targetId,
        data: {
          subject: dataRef.current.subject,
          ...parametersRef.current?.getValue()
        }
      };
    }
  }));

  React.useEffect(() => {
    app.core.documentApi.list({ kind }).then((result) => {
      setDocuments(result);
    });
  }, [kind]);

  if (documents == null) {
    return <LinearProgress />;
  } else {
    return (
      <Grid container spacing={2}>
        <Grid size={{ xs: 12, sm: 5 }}>
          <List sx={{ height: 300, overflow: "auto" }}>
            {documents.map((doc) => (
              <ListItem key={doc.id} disablePadding>
                <ListItemButton
                  onClick={() => setSelectedDocument(doc)}
                  selected={selectedDocument?.id === doc.id}
                >
                  <ListItemText primary={doc.title} />
                </ListItemButton>
              </ListItem>
            ))}
          </List>
        </Grid>
        <Grid
          size={{ xs: 12, sm: 7 }}
          sx={{
            display: "flex",
            flexDirection: "column",
            gap: 2,
            paddingTop: 1
          }}
        >
          <HBox spacing={2}>
            <CultureList
              onItemChange={(item) => (dataRef.current.culture = item?.id)}
            />
            <FormControlLabel
              control={
                <Checkbox
                  onChange={(e) => (dataRef.current.noCache = e.target.checked)}
                />
              }
              label={labels.noCache}
            />
          </HBox>
          <InputField
            fullWidth
            name="title"
            slotProps={{ htmlInput: { maxLength: 128 } }}
            label={labels.title}
            onChange={(e) => (dataRef.current.subject = e.target.value)}
          />
          {selectedDocument?.parameters?.length && (
            <CustomFieldUI
              fields={selectedDocument.parameters}
              mref={parametersRef}
            />
          )}
        </Grid>
      </Grid>
    );
  }
}

export type DocumentGeneratorButtonProps = {
  /**
   * Document generate action
   * @param id Document id
   * @param targetId Target id
   */
  action: (id: number, targetId: number) => Promise<AppActionData | undefined>;

  /**
   * Button icon
   */
  icon?: React.ReactNode;

  /**
   * Kind of document to generate
   */
  kind: string;

  /**
   * Label
   */
  label?: string;

  /**
   * Target ID
   */
  targetId: number;
};

export function DocumentGeneratorButton(props: DocumentGeneratorButtonProps) {
  // Destruct
  const {
    action,
    icon = <OpenInBrowserIcon />,
    kind,
    label = app.get("exportDocument"),
    targetId
  } = props;

  // Click handler
  const handleClick = React.useCallback(() => {
    app.notifier.data<DocumentGenerateRQ | undefined>(
      <DocumentGenerator
        action={action}
        kind={kind}
        mRef={React.createRef<NotificationMUDataMethods>()}
        targetId={targetId}
      />,
      async (data) => {
        if (data == null) return false;

        // Post the data to generate document
        let html = await app.core.documentApi.generate(data);
        if (html == null) return;

        if (typeof html !== "string") {
          app.alertResult(html);
          return false;
        }

        const blob = new Blob([html], { type: "text/html" });
        const url = URL.createObjectURL(blob);

        window.open(url, "_blank")?.print();

        return true;
      },
      label,
      { fullScreen: app.smDown }
    );
  }, []);

  return (
    <Button startIcon={icon} variant="outlined" onClick={handleClick}>
      {label}
    </Button>
  );
}
