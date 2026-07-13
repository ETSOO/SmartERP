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
import Stack from "@mui/material/Stack";
import { AvatarState, usePageDataEmpty } from "@etsoo/smarterp-core";
import Typography from "@mui/material/Typography";

export default function ProductLogo() {
  // Route
  const navigate = useNavigate();
  const { id = 0 } = useParamsEx({ id: "number" });

  const { avatar, title } = useLocationState<AvatarState>();

  // Labels
  const labels = app.getLabels("imageSizeTooSmall", "logo");

  // Page data hook
  usePageDataEmpty(app);

  return (
    <CommonPage sx={{ width: "fit-content" }}>
      <Stack direction={{ xs: "column", sm: "column", md: "row" }} spacing={1}>
        <VBox spacing={1}>
          {avatar == null ? (
            <React.Fragment />
          ) : (
            <ImagePreviewButton
              size={160}
              image={avatar}
              buttonProps={{ title: labels.logo }}
            />
          )}
          <Typography variant="caption">{title}</Typography>
        </VBox>
        <UserAvatarEditor
          onDone={async (canvas, toBlob, type) => {
            // Check size
            if (canvas.width < 100 || canvas.height < 100) {
              app.notifier.alert(labels.imageSizeTooSmall);
              return;
            }

            // Action data
            const action = await app.productApi.uploadLogoAction(id);
            if (action == null) return;

            // Photo blob
            const blob = await toBlob(canvas, type, 1);

            const formData = new FormData();
            formData.append("file", blob);

            // Upload the file
            const result = await app.core.orgApi.uploadFiles(
              id,
              "Products",
              formData,
              action
            );

            if (result == null || !result.data?.urls.length) return;

            const url = result.data.urls[0];

            const logoResult = await app.productApi.updateLogo({ id, url });
            if (logoResult == null) return;

            if (result.ok) {
              navigate(`./../../view/${id}`);
              return true;
            } else {
              app.alertResult(result);
            }
          }}
          maxWidth={800}
        />
      </Stack>
    </CommonPage>
  );
}
