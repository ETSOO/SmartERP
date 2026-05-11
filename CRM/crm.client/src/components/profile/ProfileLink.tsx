import { ComboBox, VBox } from "@etsoo/materialui";
import { ProfileList } from "@etsoo/smarterp-crm/components";
import { EOEditorElement, EOEditorEx } from "@etsoo/reacteditor";
import React from "react";
import { PersonProfileLinkItem } from "@etsoo/smarterp-crm";
import { app } from "../../app/MyApp";

export type ProfileLinkProps = {
  profileId: number;
  data?: PersonProfileLinkItem;
  editorRef: React.RefObject<EOEditorElement | null>;
};

export function ProfileLink(props: ProfileLinkProps) {
  // Destruct
  const { profileId, data, editorRef } = props;

  // Labels
  const labels = app.getLabels("relatedProfile", "type");

  React.useEffect(() => {
    if (editorRef.current) {
      editorRef.current.value = data?.content ?? "";
    }
  }, [data?.content]);

  // Layout
  return (
    <VBox spacing={2} sx={{ paddingTop: 1 }}>
      <ComboBox
        name="kind"
        label={labels.type}
        options={app.profile.getLinkKinds()}
        idValue={data?.kind ?? 1}
        fullWidth
      />
      <ProfileList
        name="targetProfileId"
        label={labels.relatedProfile}
        idValue={data?.targetProfileId}
        rq={{ excludedIds: [profileId] }}
      />
      <EOEditorEx
        ref={editorRef}
        backupKey={`profile-comment${profileId}`}
        language={app.culture}
        height="345px"
      />
    </VBox>
  );
}
