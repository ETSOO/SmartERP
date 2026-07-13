import {
  CommonPage,
  ImagePreviewButton,
  UserAvatarEditor
} from "@etsoo/materialui";
import React from "react";
import { app } from "../../../app/MyApp";
import { useLocation, useNavigate } from "react-router-dom";
import { useParamsEx } from "@etsoo/react";
import Stack from "@mui/material/Stack";
import { usePageDataEmpty } from "@etsoo/smarterp-core";
import { PersonAvatarState } from "../../../components/person/PersonAvatarState";

export default function ContactAvatar() {
  // Route
  const navigate = useNavigate();
  const { id = 0 } = useParamsEx({ id: "number" });

  const location = useLocation();
  const { avatar, isLegalPerson } = location.state as PersonAvatarState;

  // Labels
  const labels = app.getLabels("avatar", "imageSizeTooSmall", "logo");

  // Page data hook
  usePageDataEmpty(app);

  return (
    <CommonPage sx={{ width: "fit-content" }}>
      <Stack direction={{ xs: "column", sm: "column", md: "row" }} spacing={1}>
        {avatar == null ? (
          <React.Fragment />
        ) : (
          <ImagePreviewButton
            size={isLegalPerson ? 120 : [130, 160]}
            image={avatar}
          />
        )}
        <UserAvatarEditor
          onDone={async (canvas, toBlob, type) => {
            // Check size
            if (canvas.width < 100 || canvas.height < 100) {
              app.notifier.alert(labels.imageSizeTooSmall);
              return;
            }

            // Photo blob
            const blob = await toBlob(canvas, type, 1);

            // Form data
            const form = new FormData();
            form.append("avatar", blob);

            const result = await app.core.memberApi.updateAvatar(id, form);
            if (result == null) return;

            // To view page
            navigate(`./../../view/${id}`);

            // Reset the UI
            return true;
          }}
          width={isLegalPerson ? 320 : 260}
          height={isLegalPerson ? 160 : 320}
          maxWidth={640}
        />
      </Stack>
    </CommonPage>
  );
}
