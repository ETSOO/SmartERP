import { CommonPage, UserAvatarEditor, VBox } from "@etsoo/materialui";
import React from "react";
import { app } from "../../../app/MyApp";
import { useNavigate } from "react-router-dom";
import { useLocationState, useParamsEx } from "@etsoo/react";
import { AvatarState, CoreUtils, usePageDataEmpty } from "@etsoo/smarterp-core";
import Stack from "@mui/material/Stack";
import Typography from "@mui/material/Typography";

export default function UpdateAvatar() {
  // Route
  const navigate = useNavigate();
  const { id = 0 } = useParamsEx({ id: "number" });

  const state = useLocationState<AvatarState>();

  // Labels
  const labels = app.getLabels("avatar", "imageSizeTooSmall", "logo");

  // Page data hook
  usePageDataEmpty(app);

  return (
    <CommonPage sx={{ width: "fit-content" }}>
      <Stack direction={{ xs: "column", sm: "column", md: "row" }} spacing={1}>
        <VBox gap={1}>
          {state.avatar == null ? (
            <React.Fragment />
          ) : (
            <img
              src={state.avatar}
              alt={labels.logo}
              style={CoreUtils.avatarStyles(true)}
            />
          )}
          <Typography variant="caption">{state.title}</Typography>
        </VBox>
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

            var result = await app.core.orgApi.updateAvatar(id, form);
            if (result == null) return;

            // To view page
            navigate(`./../../my/${id}`);

            // Reset the UI
            return true;
          }}
          width={320}
          height={160}
          maxWidth={640}
        />
      </Stack>
    </CommonPage>
  );
}
