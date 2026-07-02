import {
  CommonPage,
  ImagePreviewButton,
  UserAvatarEditor,
  VBox
} from "@etsoo/materialui";
import React from "react";
import { app } from "../../../app/MyApp";
import { useNavigate } from "react-router-dom";
import { useLocationState, useParamsEx } from "@etsoo/react";
import { AvatarState, usePageDataEmpty } from "@etsoo/smarterp-core";
import Stack from "@mui/material/Stack";
import Typography from "@mui/material/Typography";

export default function CompanySeal() {
  // Route
  const navigate = useNavigate();
  const { id = 0 } = useParamsEx({ id: "number" });

  const state = useLocationState<AvatarState>();

  // Labels
  const labels = app.getLabels("companySeal", "imageSizeTooSmall");

  // Page data hook
  usePageDataEmpty(app);

  return (
    <CommonPage sx={{ width: "fit-content" }}>
      <Stack direction={{ xs: "column", sm: "column", md: "row" }} spacing={1}>
        <VBox spacing={1}>
          {state.avatar == null ? (
            <React.Fragment />
          ) : (
            <ImagePreviewButton
              size={160}
              image={state.avatar}
              buttonProps={{ title: labels.companySeal }}
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
            form.append("companySeal", blob);

            const result = await app.core.orgApi.updateCompanySeal(id, form);
            if (result == null) return;

            // To view page
            navigate(`./../../my/${id}`);

            // Reset the UI
            return true;
          }}
          width={320}
          height={320}
          maxWidth={640}
        />
      </Stack>
    </CommonPage>
  );
}
